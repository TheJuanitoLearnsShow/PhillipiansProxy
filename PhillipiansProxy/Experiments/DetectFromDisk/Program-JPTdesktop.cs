// See https://aka.ms/new-console-template for more information

using DetectFromDisk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using NsfwSpyNS;

var _modelPath = Path.Combine(AppContext.BaseDirectory, "NsfwSpyModel.zip");


Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) => {
        services.AddLogging( configure =>configure.AddConsole() );
        services.AddScoped<INsfwSpy, NsfwSpy>();

        services.AddPredictionEnginePool<NsfwSpyNS.ModelInput, NsfwSpyNS.ModelOutput>().FromFile("ImageModel", _modelPath, true);
        services.AddHostedService<HostedService>();
    }
    ).Build().Run();
