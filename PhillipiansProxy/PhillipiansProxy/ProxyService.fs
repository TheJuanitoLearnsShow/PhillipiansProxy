namespace PhillipiansProxy

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open System.Threading
open Titanium.Web.Proxy
open Titanium.Web.Proxy.EventArguments
open Titanium.Web.Proxy.Models
open System
open System.Net
open FSharp.Control.Tasks.V2
open System.IO
open System.Drawing
open System.Diagnostics
open System.IO.Pipes

type ProxyService(logger: ILogger<ProxyService>) = 
    let mutable proxyServer:ProxyServer = null
    let predictorFolderPath = @"E:\OneDrive\sources\PhillipiansProxy\JS\out\"
    let predictorPath = @"index.js"
    let blankImg = File.ReadAllBytes("Images/blank.png")
    let bitmapTypes = [| "jpeg"; "bmp"; "tiff"; "bitmap"; "png" |]

    
    let StartReadingAsync(pipeReader: AnonymousPipeServerStream) =
        task {
            try
                use sr = new StreamReader(pipeReader);
            
                // This method should get a CancellationToken so we use that instead of a while true.
                // But this will work now.
                while (true) do
            
                    let! message = sr.ReadLineAsync();
    
                    if (message <> null) then
                        Console.WriteLine(message);
            with ex -> 
                Console.WriteLine(ex);
        }

    let setupPredictor () =
        // We are going to create two pipes, one writer and one reader.
        let pipeWriter = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        let pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

        // We create a child process passing the pipes handles as string.
        let client = new Process();

        client.StartInfo.FileName <- "node";
        client.StartInfo.Arguments <- predictorFolderPath + predictorPath + " " + pipeWriter.GetClientHandleAsString() + " " + pipeReader.GetClientHandleAsString();
        client.StartInfo.UseShellExecute <- false;
        client.StartInfo.WorkingDirectory <- predictorFolderPath
        client.Start() |> ignore
          
        // If microsoft docs tells me to call this method I will.
        pipeWriter.DisposeLocalCopyOfClientHandle();
        pipeReader.DisposeLocalCopyOfClientHandle();

        // We start listening to messages
        let _ = StartReadingAsync(pipeReader);

        // We create a stream writer, and we will write messages on that stream.
        use sw = new StreamWriter(pipeWriter)
        sw.AutoFlush <- true
        sw

    let predictorInput = setupPredictor()
    let onRequest sender (e:SessionEventArgs): Task =
        task {
            let responseHeaders = e.HttpClient.Response.Headers;
            if (e.HttpClient.Request.Method = "GET" || e.HttpClient.Request.Method = "POST") then
                if (e.HttpClient.Response.StatusCode = 200) then
                    let contentType = e.HttpClient.Response.ContentType
                    if (contentType |> isNull |> not) then
                        let ct = contentType.ToLower()
                        
                        if ct.ToLower().Contains("image") && bitmapTypes |> Array.tryFind (ct.Contains) |> Option.isSome then
                            let! rawBytes = e.GetResponseBody()
                            predictorInput.Write(rawBytes)
                            //Convert to Bitmap
                            //let bitmapImage = new MemoryStream(rawBytes) |> System.Drawing.Image.FromStream :?> Bitmap;
                            e.SetResponseBody(blankImg);

                            //Set the specific image data into the ImageInputData type used in the DataView
                            //let imageInputData: ImageInputData =  { Image = bitmapImage };
                            //let prediction = predictionEnginePool.Predict("ImageModel", imageInputData) 
                            //let predictedResult = prediction.ToHelper()
                            ////let! body = e.GetResponseBodyAsString();
                            //if (predictedResult.H > 0.7f) || (predictedResult.P > 0.7f) || (predictedResult.S > 0.9f) then
                            //    Console.WriteLine(predictedResult.ToString() + " -> " + (e.HttpClient.Request.Url));
                            //    e.SetResponseBody(blankImg);
        } 
        :> Task
    let h = AsyncEventHandler(onRequest)

    interface  IHostedService with 
        member x.StartAsync(cancellationToken: CancellationToken)=
            proxyServer <- new ProxyServer();
            // locally trust root certificate used by this proxy 
            proxyServer.CertificateManager.CertificateEngine <- Network.CertificateEngine.DefaultWindows; 
            proxyServer.CertificateManager.EnsureRootCertificate();
            //proxyServer.add_BeforeRequest h;
            proxyServer.add_BeforeResponse h;
            let explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();
            
            for endPoint in proxyServer.ProxyEndPoints do
                logger.LogInformation("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                    endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
       
            // Only explicit proxies can be set as system proxy!
            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            Task.CompletedTask

        member x.StopAsync(cancellationToken: CancellationToken ) =
            // Unsubscribe & Quit
           
            proxyServer.remove_BeforeResponse h;
    
            proxyServer.DisableAllSystemProxies()
            proxyServer.Stop();
            Task.CompletedTask

