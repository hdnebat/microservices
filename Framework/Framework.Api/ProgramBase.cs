﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using Framework.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Winton.Extensions.Configuration.Consul;

namespace Framework.Api {
    public static class ProgramBase<TStart> where TStart : StartupBase<TStart> {
        public static readonly CancellationTokenSource ConsulConfigCancellationTokenSource =
            new CancellationTokenSource();

        private static IMetricsRoot Metrics { get; set; }

        public static int Run(string[] args) {
            try {
                BuildWebHost(args).Run();
                return 0;
            }
            catch (Exception ex) {
                const string str = "Host terminated unexpectedly";
                Log.Fatal(ex, str);
                return 1;
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private static IWebHost BuildWebHost(string[] args) {
            Metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusProtobuf()
                .Build();
            var builder = new WebHostBuilder()
                .UseKestrel(options => options.AddServerHeader = false)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseMetrics(options => {
                    options.EndpointOptions = endpointsOptions => {
                        endpointsOptions.MetricsTextEndpointEnabled = false;
                        endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters
                            .GetType<MetricsPrometheusProtobufOutputFormatter>();
                    };
                })
                .ConfigureAppConfiguration((hostingContext, config) => {
                    var hostingEnvironment = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", true, true).AddJsonFile(
                        string.Format("appsettings.{0}.json", hostingEnvironment.EnvironmentName), true, true);

                    if (hostingEnvironment.IsDevelopment()) {
                        var assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                        if (assembly != null)
                            config.AddUserSecrets(assembly, true);
                    }

                    config.AddEnvironmentVariables();
                    config.AddDockerSecrets(cfg => { cfg.Optional = true; });
                    if (args != null)
                        config.AddCommandLine(args);
                    var jsonCfg = config.Build();
                    if (!string.IsNullOrWhiteSpace(jsonCfg["ConsulOptions:ServiceName"]))
                        config.AddConsul(jsonCfg["ConsulOptions:ServiceName"], ConsulConfigCancellationTokenSource.Token,
                            opt => {
                                opt.ReloadOnChange = true;
                                opt.Optional = true;
                                opt.ConsulConfigurationOptions = cfg => {
                                    cfg.Address = new Uri(jsonCfg["ConsulOptions:HttpEndpoint"]);
                                    cfg.Datacenter = jsonCfg["ConsulOptions:Datacenter"];
                                };
                            }
                        );
                    InitializeLogs(config.Build());
                })
                .UseDefaultServiceProvider((ctx, opt) => { })
                .UseHealthChecks("/hc")
                .UseStartup<TStart>()
                .UseSerilog();

            return builder.Build();
        }

        private static void InitializeLogs(IConfiguration config) {
            var format = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] [" + Environment.MachineName +
                         "] {SourceContext}  [{Address}] [{RequestId}] {Message:lj}{NewLine}{Exception}{NewLine}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Default", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: format)
                .CreateLogger();
        }
    }
}