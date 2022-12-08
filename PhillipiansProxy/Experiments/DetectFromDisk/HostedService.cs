using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NsfwSpyNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DetectFromDisk
{
    internal class HostedService : IHostedService
    {
        private readonly INsfwSpy _nsfwEngine;
        private readonly ILogger<HostedService> _logger;
        private readonly IHostApplicationLifetime  _hostExecutionContext;

        public HostedService(ILogger<HostedService>  logger , INsfwSpy nsfwEngine, IConfiguration configuration, IHostApplicationLifetime  hostExecutionContext )
        {
            _nsfwEngine = nsfwEngine;
            _logger = logger;
            _hostExecutionContext = hostExecutionContext;
        }

        
        private bool ProcessImage(string fileName)
        {
            var rawBytes = File.ReadAllBytes(fileName);
            var prediction = _nsfwEngine.ClassifyImage(rawBytes);
            return (prediction.Sexy >=  0.8 || prediction.Pornography >= 0.5);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var basePath = @"F:\OneDrive\SkyDrive camera roll";
            var folders = Directory.EnumerateDirectories(basePath);
            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(folder, "*.jpg", SearchOption.AllDirectories);
                var filesCaptured = files.Where(ProcessImage).ToList();
                foreach (var fileCaptured in filesCaptured)
                {
                    Console.WriteLine(fileCaptured);
                }
            }
            Console.WriteLine("This is the end!");
            _hostExecutionContext.StopApplication();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    } 
}
