// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Titanium.Web.Proxy
open Titanium.Web.Proxy.Models
open System.Net
open Titanium.Web.Proxy.EventArguments
open System.Threading.Tasks
open FSharp.Control.Tasks.V2
open System.IO

let blankImg = File.ReadAllBytes("Images/blank.png")
// Define a function to construct a message to print
let onRequest sender (e:SessionEventArgs): Task =
    task {
        let responseHeaders = e.HttpClient.Response.Headers;
        if (e.HttpClient.Request.Method = "GET" || e.HttpClient.Request.Method = "POST") then
            if (e.HttpClient.Response.StatusCode = 200) then
                let contentType = e.HttpClient.Response.ContentType
                if (contentType |> isNull |> not) then
                    if contentType.ToLower().Contains("image") || 
                        contentType.ToLower().Contains("video") then
                        //let! body = e.GetResponseBodyAsString();
                        e.SetResponseBody(blankImg);
                        //Console.WriteLine (contentType + " -> " + (e.HttpClient.Request.Url))
    } 
    :> Task

[<EntryPoint>]
let main argv =
    use proxyServer = new ProxyServer();
   
    // locally trust root certificate used by this proxy 
    proxyServer.CertificateManager.CertificateEngine <- Network.CertificateEngine.DefaultWindows; 
    proxyServer.CertificateManager.EnsureRootCertificate();
    //proxyServer.CertificateManager.TrustRootCertificate(true)
   
    // optionally set the Certificate Engine
    // Under Mono only BouncyCastle will be supported
    //proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;
    let h = AsyncEventHandler(onRequest)
    //proxyServer.add_BeforeRequest h;
    proxyServer.add_BeforeResponse h;
    //proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
    //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;
   
   
    let explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);
    //{
        // Use self-issued generic certificate on all https requests
        // Optimizes performance by not creating a certificate for each https-enabled domain
        // Useful when certificate trust is not required by proxy clients
        //GenericCertificate = new X509Certificate2(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "genericcert.pfx"), "password")
    //};
   
    // Fired when a CONNECT request is received
    //explicitEndPoint.BeforeTunnelConnect += OnBeforeTunnelConnect;
   
    // An explicit endpoint is where the client knows about the existence of a proxy
    // So client sends request in a proxy friendly manner
    proxyServer.AddEndPoint(explicitEndPoint);
    proxyServer.Start();
   
    // Transparent endpoint is useful for reverse proxy (client is not aware of the existence of proxy)
    // A transparent endpoint usually requires a network router port forwarding HTTP(S) packets or DNS
    // to send data to this endPoint
    
    for endPoint in proxyServer.ProxyEndPoints do
        Console.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
            endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
   
    // Only explicit proxies can be set as system proxy!
    proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
    proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);
   
    // wait here (You can use something else as a wait function, I am using this as a demo)
    Console.Read();
   
    // Unsubscribe & Quit
    //explicitEndPoint.BeforeTunnelConnect -= OnBeforeTunnelConnect;
    //proxyServer.BeforeRequest -= OnRequest;
    proxyServer.remove_BeforeResponse h;

   
    proxyServer.Stop();
    0 // return an integer exit code