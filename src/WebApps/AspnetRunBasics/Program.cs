using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                // For integrating Serilog, we are going to edit the default builder which implements the default aspnet logger
                .UseSerilog(
                    (context, configuration) =>
                    {
                        configuration
                            .Enrich.FromLogContext()
                            .Enrich.WithMachineName()
                            .WriteTo.Console()
                            .WriteTo.Elasticsearch(
                                new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
                                    new Uri(context.Configuration["ElasticConfiguration:Uri"])
                                )
                                {
                                    IndexFormat = $"applogs-{context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-")}-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                                    AutoRegisterTemplate = true,
                                    NumberOfShards = 2,
                                    NumberOfReplicas = 1
                                    // Find out what is the meaning of each of these properties
                                }
                            )
                            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                            .ReadFrom.Configuration(context.Configuration)
                            ;
                    }
                )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
