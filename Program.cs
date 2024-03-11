using NLog;
using UDP_MQTT_Gateway;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<MyService>();
        });
