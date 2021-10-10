using Couchbase.Fake;
using Couchbase.Fake.Services;
using Serilog;

await Host.CreateDefaultBuilder()
    .UseSerilog()
    .ConfigureLogging((host, logging) =>
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(host.Configuration)
            .CreateLogger();
    })
    .ConfigureWebHostDefaults(web =>
    {
        web.UseStartup<Startup>();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<FakeCouchbaseServer>();
    })
    .RunConsoleAsync();
