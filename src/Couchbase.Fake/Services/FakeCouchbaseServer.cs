using System;
using System.Net;
using System.Net.Sockets;
using Couchbase.Fake.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Couchbase.Fake.Services;

class FakeCouchbaseServer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FakeCouchbaseOptions _options;
    private readonly ILogger _logger;

    private Task? _acceptTask;

    public FakeCouchbaseServer(
        IServiceProvider serviceProvider,
        IOptions<FakeCouchbaseOptions> options,
        ILogger<FakeCouchbaseServer> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting listener on {Host}:{Port}", _options.ListenHost, _options.ListenPort);

        var listener = new TcpListener(IPAddress.Parse(_options.ListenHost), _options.ListenPort);
        listener.Start();
        _acceptTask = AcceptLoopAsync(listener, cancellationToken);

        _logger.LogInformation("Listener started");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping listener");
        return Task.CompletedTask;
    }

    private async Task AcceptLoopAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        await Task.Yield();

        long count = 0;

        for (;;)
        {
            var socket = await listener.AcceptSocketAsync(cancellationToken);
            _ = CommunicateAsync(socket, ++count);
        }
    }

    private async Task CommunicateAsync(Socket socket, long connectionId)
    {
        var localEp = socket.LocalEndPoint;
        var remoteEp = socket.RemoteEndPoint;

        try
        {
            using var loggerScope = _logger.BeginScope(new KeyValuePair<string, object?>[]
            {
                new("ConnectionId", connectionId),
                new("LocalEndPoint", localEp),
                new("RemoteEndPoint", remoteEp),
            });

            _logger.LogInformation("New connection {ConnectionId} from {RemoteEndPoint}", connectionId, remoteEp);

            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var networkStream = new NetworkStream(socket, true);
            await using var bufferedStream = new BufferedStream(networkStream);
            await scope.ServiceProvider.GetRequiredService<ICouchbaseProtocol>().RunAsync(bufferedStream);

            _logger.LogInformation("Closing connection {ConnectionId} from {RemoteEndPoint}", connectionId, remoteEp);
        }
        catch (Exception err)
        {
            _logger.LogError(err, "Failed connection {ConnectionId} from {RemoteEndPoint}", connectionId, remoteEp);
        }
    }
}
