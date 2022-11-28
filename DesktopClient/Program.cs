using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopClient
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Logger = GetSerilogLogger();

            try
            {
                Log.Information("Starting .NET client");
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unexpected error");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

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
                    IndexFormat = "test-client-index-{0:yyyy.MM}",
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
