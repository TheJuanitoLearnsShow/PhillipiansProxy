// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Titanium.Web.Proxy
open Titanium.Web.Proxy.Models
open System.Net
open Titanium.Web.Proxy.EventArguments
open System.Threading.Tasks
open FSharp.Control.Tasks.V2
open System.IO
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open PhillipiansProxy

let _modelPath = "MLModel/nsfw_net.zip"
let CreateHostBuilder(args)  =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(fun (hostContext:HostBuilderContext) (services) ->
            //services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>().FromFile( "ImageModel", _modelPath, true) |> ignore
            services.AddHostedService<ProxyService>() |> ignore
        );

[<EntryPoint>]
let main argv =
    CreateHostBuilder(argv).Build().Run()
    0


    //phllipians_proxy_service_acct