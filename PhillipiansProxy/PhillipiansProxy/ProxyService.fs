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
open Microsoft.Extensions.Configuration

type ProxyServiceConfiguration()=
    member val PornBlockerImageFilePath = "Images/blank.png" with get, set
    member val SexyBlockerImageFilePath = "Images/blank.png" with get, set
    member val HentaiBlockerImageFilePath = "Images/blank.png" with get, set
    member val PornBlockerThreshold = 0.30f  with get, set
    member val SexyBlockerThreshold = 0.40f with get, set
    member val HentaiBlockerThreshold = 0.30f with get, set
    member val WhiteListUrlPrefixes:string[] = [||] with get, set
    member val WhiteListUrlRegexes:string[] = [||] with get, set
    member val ProxyCertPath:string = "" with get, set

    

type ProxyService(logger: ILogger<ProxyService>, nsfwEngine: INsfwSpy, configuration: IConfiguration) = 
    let mutable proxyServer:ProxyServer = null
    let proxyConf = configuration.GetSection(nameof ProxyServiceConfiguration).Get<ProxyServiceConfiguration>()
    let proxyCertPassword = configuration.GetValue<string>("ProxyCertPassword")
    let pornBlockerImageFile = File.ReadAllBytes(proxyConf.PornBlockerImageFilePath)
    let sexyBlockerImageFile = File.ReadAllBytes(proxyConf.SexyBlockerImageFilePath)
    let hentaiBlockerImageFile = File.ReadAllBytes(proxyConf.HentaiBlockerImageFilePath)
    let bitmapTypes = [| "jpeg"; "bmp"; "tiff"; "bitmap"; "png" |]
    let minFileSize = 1024 * 4
    
    
    let onRequest sender (e:SessionEventArgs): Task =
        task {
            let responseHeaders = e.HttpClient.Response.Headers;
            if (e.HttpClient.Request.Method = "GET" || e.HttpClient.Request.Method = "POST") then
                if (e.HttpClient.Response.StatusCode = 200) then
                    let response = e.HttpClient.Response
                    let contentType = response.ContentType
                    if (contentType |> isNull |> not && response.ContentLength >= minFileSize) then
                        let ct = contentType.ToLower()
                        let url = e.HttpClient.Request.Url
                        let isWhitelistUrl =
                            proxyConf.WhiteListUrlPrefixes 
                            |> Array.tryFind (fun u -> url.StartsWith(u, StringComparison.OrdinalIgnoreCase)) 
                            |> Option.isSome
                        if isWhitelistUrl |> not then
                            if ct.Contains("image") && bitmapTypes |> Array.tryFind (ct.Contains) |> Option.isSome then
                                try
                                    let! rawBytes = e.GetResponseBody()
                                    let prediction = nsfwEngine.ClassifyImage(rawBytes)
                                
                                    if prediction.Pornography > proxyConf.PornBlockerThreshold then
                                        e.SetResponseBody(pornBlockerImageFile)
                                    elif prediction.Hentai > proxyConf.HentaiBlockerThreshold then 
                                        e.SetResponseBody(hentaiBlockerImageFile)
                                    elif prediction.Sexy > proxyConf.SexyBlockerThreshold then 
                                        e.SetResponseBody(sexyBlockerImageFile)
                                with
                                | exn ->
                                    e.SetResponseBody(sexyBlockerImageFile);

                            if ct.Contains("/gif")  then
                                try
                                    let! rawBytes = e.GetResponseBody()
                                    let prediction = nsfwEngine.ClassifyGif(rawBytes, null)
                                                           
                                    if prediction.TopPornographyScore > proxyConf.PornBlockerThreshold then
                                        e.SetResponseBody(pornBlockerImageFile)
                                    elif prediction.TopHentaiScore > proxyConf.HentaiBlockerThreshold then 
                                        e.SetResponseBody(hentaiBlockerImageFile)
                                    elif prediction.TopSexyScore > proxyConf.SexyBlockerThreshold then 
                                        e.SetResponseBody(sexyBlockerImageFile)
                                with
                                | exn ->
                                    e.SetResponseBody(sexyBlockerImageFile);
                            
        } 
        :> Task
    let h = AsyncEventHandler(onRequest)

    interface  IHostedService with 
        member x.StartAsync(cancellationToken: CancellationToken)=
        // TODo shpudl those be false for a one time trust? or separate app for trust??
            proxyServer <- new ProxyServer(proxyConf.ProxyCertPath, "phillipiansproxy",false,false,false)
            proxyServer.CertificateManager.PfxPassword <- proxyCertPassword
            // locally trust root certificate used by this proxy 
            proxyServer.CertificateManager.CertificateEngine <- Network.CertificateEngine.DefaultWindows; 
            //proxyServer.CertificateManager.EnsureRootCertificate();
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

