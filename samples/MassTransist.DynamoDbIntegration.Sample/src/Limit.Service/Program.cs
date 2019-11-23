using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Limit.Service.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orquestrator.Saga.Contracts;
using Orquestrator.Saga.Contracts.Commands;
using Serilog;

namespace Limit.Service
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static async Task Main()
        {
            await new HostBuilder()
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder
                        .SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var brokerSettings = new BrokerSettings();
                    hostContext.Configuration.GetSection("RabbitSettings").Bind(brokerSettings);

                    services.AddSingleton<IConsumer<ReserveCustomerBankDepositLimitAmount>, ReserveCustomerBankDepositLimitAmountConsumer>();

                    services.AddMassTransit(configure =>
                    {
                        configure.AddConsumer<ReserveCustomerBankDepositLimitAmountConsumer>();

                        configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configureBus =>
                        {
                            var hostBus = configureBus.Host(new Uri(brokerSettings.Host), configureHost =>
                            {
                                configureHost.Username(brokerSettings.User);
                                configureHost.Password(brokerSettings.Password);
                            });

                            configureBus.ReceiveEndpoint(hostBus, brokerSettings.InputQueue, configureEndpoint =>
                            {
                                configureEndpoint.Consumer<ReserveCustomerBankDepositLimitAmountConsumer>(provider);
                            });

                            configureBus.UseSerilog();
                        }));
                    });

                    services.AddMassTransitHostedService();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging
                        .AddSerilog()
                        .AddConsole()
                        .AddDebug()
                        .SetMinimumLevel(LogLevel.Debug);
                })
                .UseConsoleLifetime()
                .Build().StartAsync();


        }
    }
}
