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
open NsfwSpyNS

type ProxyService(logger: ILogger<ProxyService>, nsfwEngine: INsfwSpy) = 
    let mutable proxyServer:ProxyServer = null
    let predictorFolderPath = @"E:\OneDrive\sources\PhillipiansProxy\JS\out\"
    let predictorPath = @"index.js"
    let blankImg = File.ReadAllBytes("Images/blank.png")
    let bitmapTypes = [| "jpeg"; "bmp"; "tiff"; "bitmap"; "png" |]

    
    
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
                            let prediction = nsfwEngine.ClassifyImage(rawBytes)
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

