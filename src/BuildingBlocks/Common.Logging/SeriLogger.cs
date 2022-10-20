﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
    public class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
        (context, configuration) =>
        {
            var elasticUrl = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");

            configuration
                            .Enrich.FromLogContext()
                            .Enrich.WithMachineName()
                            .WriteTo.Debug()
                            .WriteTo.Console()
                            .WriteTo.Elasticsearch(
                                new ElasticsearchSinkOptions(
                                    new Uri(elasticUrl)
                                )
                                {
                                    IndexFormat = $"applogs-{context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-")}-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                                    AutoRegisterTemplate = true,
                                    NumberOfShards = 2,
                                    NumberOfReplicas = 1
                                    // Find out what is the meaning of each of these properties
                                    // ModifyConnectionSettings = x => x.BasicAuthentication("elastic", "eaRD8IHdpGn6vqpdugPE")
                                }
                            )
                            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                            .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                            .ReadFrom.Configuration(context.Configuration)
                            ;
        };
    }
}
