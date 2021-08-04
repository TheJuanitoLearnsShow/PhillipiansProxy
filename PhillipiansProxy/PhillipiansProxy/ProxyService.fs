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
open Microsoft.Extensions.ML
open System.Drawing

type ProxyService(logger: ILogger<ProxyService>, predictionEnginePool: PredictionEnginePool<ImageInputData, ImageLabelPredictions>) = 
    let mutable proxyServer:ProxyServer = null
    let blankImg = File.ReadAllBytes("Images/blank.png")
    // Define a function to construct a message to print
    let onRequest sender (e:SessionEventArgs): Task =
        task {
            let responseHeaders = e.HttpClient.Response.Headers;
            if (e.HttpClient.Request.Method = "GET" || e.HttpClient.Request.Method = "POST") then
                if (e.HttpClient.Response.StatusCode = 200) then
                    let contentType = e.HttpClient.Response.ContentType
                    if (contentType |> isNull |> not) then
                        //if contentType.ToLower().Contains("image") || 
                        //    contentType.ToLower().Contains("video") then
                        if contentType.ToLower().Contains("image") then
                            let! rawBytes = e.GetResponseBody()
                            //Convert to Bitmap
                            let bitmapImage = new MemoryStream(rawBytes) |> Image.FromStream :?> Bitmap;

                            //Set the specific image data into the ImageInputData type used in the DataView
                            let imageInputData: ImageInputData =  { Image = bitmapImage };
                            let prediction = predictionEnginePool.Predict("ImageModel", imageInputData)
                            Console.WriteLine(prediction);
                            //let! body = e.GetResponseBodyAsString();
                            e.SetResponseBody(blankImg);
                            //Console.WriteLine (contentType + " -> " + (e.HttpClient.Request.Url))
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
                Console.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                    endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
       
            // Only explicit proxies can be set as system proxy!
            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            Task.CompletedTask

        member x.StopAsync(cancellationToken: CancellationToken ) =
            // Unsubscribe & Quit
            //explicitEndPoint.BeforeTunnelConnect -= OnBeforeTunnelConnect;
            //proxyServer.BeforeRequest -= OnRequest;
            proxyServer.remove_BeforeResponse h;
    
            proxyServer.DisableAllSystemProxies()
            proxyServer.Stop();
            Task.CompletedTask

