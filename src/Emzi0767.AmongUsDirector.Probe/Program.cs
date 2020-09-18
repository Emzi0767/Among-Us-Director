// This file is part of Among Us Director project.
// 
// Copyright 2020 Emzi0767
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Emzi0767.AmongUsDirector.Services;
using Emzi0767.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Emzi0767.AmongUsDirector
{
    public static class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args)
                .Build()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(ConfigureHostConfiguration(args))
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);

        public static Action<IConfigurationBuilder> ConfigureHostConfiguration(string[] args)
        {
            return configuration => configuration.AddJsonFile("config.json", optional: false)
                .AddEnvironmentVariables("AMONGUS:DIRECTOR:")
                .AddCommandLine(args);
        }

        public static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            services.AddOptions<ProbeConfiguration>()
                .Bind(ctx.Configuration)
                .ValidateDataAnnotations();

            services.AddSingleton<DiscoveryClient>();
            services.AddSingleton<AmongUsGame>();
            services.AddSingleton<ProbeCommArray>();
            services.AddTransient<PayloadSerializer>();

            services.AddTransient<AsyncExecutor>();

            services.AddHostedService<ProbeHostedService>();
        }

        public static void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.SetMinimumLevel(LogLevel.Debug);

            logging.AddConsole(console =>
            {
                console.Format = ConsoleLoggerFormat.Default;
                console.TimestampFormat = "yyyy-MM-dd HH:mm:ss zzz ";
                console.LogToStandardErrorThreshold = LogLevel.Error;
            });
        }
    }
}
