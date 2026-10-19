using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using SubmitServ.Helpers;
using SubmitServ.IRepository.ETA;
using SubmitServ.Repository.ETA;
using System;
using System.Reflection;

namespace SubmitServ
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Bootstrap DI container before MainWindow is created
            if (!ServiceProvider.IsBuilt)
            {
                // Load appsettings.json — must contain ToolkitConfig:LocalConnectionsString
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();

                // Register ETA toolkit (registers MediatR handlers internally)
                ServiceProvider.AddToolkit(configuration);

                // Fix DI Lifetime Mismatch for toolkit services
                ServiceProvider.ReplaceServiceLifetimeByName("ICodesService", Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);

                // Register MediatR with the toolkit's assembly so handlers are found
                ServiceProvider.AddMediatR(
                    typeof(ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateUuid.Command).Assembly,
                    Assembly.GetExecutingAssembly()
                );

                // Register our own EReceiptToolsService
                ServiceProvider.AddScoped<IEReceiptToolsService, EReceiptToolsService>();

                // Build the provider — after this IsBuilt == true
                ServiceProvider.Build();
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
