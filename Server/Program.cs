using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = GetSerilogLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();

        private static ILogger GetSerilogLogger()
        {
            var configuration = new LoggerConfiguration()
                .WriteTo.File(path: "Logs\\log-.txt",
                              retainedFileCountLimit: 31,
                              shared: true,
                              rollingInterval: RollingInterval.Day,
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message} [{Properties}]{NewLine}{Exception}")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    IndexFormat = "test-server-index-{0:yyyy.MM}",
                    BufferBaseFilename = "Logs\\ElasticBuffer",
                    BufferLogShippingInterval = TimeSpan.FromSeconds(5),
                    BufferRetainedInvalidPayloadsLimitBytes = 5000,
                    BufferFileCountLimit = 31,
                    MinimumLogEventLevel = LogEventLevel.Debug
                });

            return configuration.CreateLogger();
        }
    }
}
