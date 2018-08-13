namespace EnhancedMapServerNetCore.Network
{
    //https://github.com/Arctium/WoW-Core/blob/master/src/Arctium.Core/src/Network/Sockets/SocketServerBase.cs

    //public class SocketServerBase<TSession> where TSession : SocketClientSession, IDisposable, new()
    //{
    //    private bool _disposed;
    //    private readonly ConcurrentStack<SocketAsyncEventArgs> _readWriteSockets, _acceptedSockets;
    //    private Socket _serverSocket;
    //    private readonly IPAddress _ipAddress;
    //    private readonly ushort _port;
    //    private bool _acceptConnections;
    //    private int _activeConnections;
    //    private SocketAsyncEventArgs _acceptEventArgs;
    //    private DateTime _latestUpdate;

    //    private Queue<SocketSessionBase> _disposedSessions = new Queue<SocketSessionBase>();

    //    public SocketServerBase(ushort port, int maxSimultanAcceptConnections, int maxActiveConnections, int bufferSize)
    //    {
    //        MaxSimultaneConnections = maxSimultanAcceptConnections;
    //        MaxActiveConnections = maxActiveConnections;
    //        BufferSize = bufferSize;

    //        _ipAddress = IPAddress.Any;
    //        _readWriteSockets = new ConcurrentStack<SocketAsyncEventArgs>();
    //        _acceptedSockets = new ConcurrentStack<SocketAsyncEventArgs>();
    //        _port = port;

    //        Create(_ipAddress, port);
    //    }

    //    public int MaxSimultaneConnections { get; }
    //    public int MaxActiveConnections { get; }
    //    public int BufferSize { get; }
    //    public bool IsListening { get; private set; }
    //    public int ActiveConnections => _activeConnections;
    //    public List<SocketClientSession> Sessions { get; } = new List<SocketClientSession>();


    //    private void Create(IPAddress ipAddress, ushort port)
    //    {
    //        _disposedSessions.Clear();

    //        for (int i = 0; i < MaxSimultaneConnections; i++)
    //        {
    //            var args = new SocketAsyncEventArgs();
    //            args.Completed += async (sender, e) => await ProcessAcceptAsync(args);
    //            _acceptedSockets.Push(args);
    //        }

    //        for (int i = 0; i < MaxActiveConnections * 2; i++)
    //        {
    //            var args = new SocketAsyncEventArgs();
    //            args.SetBuffer(new byte[BufferSize], 0, BufferSize);
    //            _readWriteSockets.Push(args);
    //        }

    //        _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    //        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
    //            _serverSocket.DualMode = true;

    //        var ipep = new IPEndPoint(ipAddress, port);
    //        _serverSocket.Bind(ipep);

    //        Log.Message(LogTypes.Info, "Resolving addresses...");

    //        if (ipep.Address.Equals(IPAddress.Any) || ipep.Address.Equals(IPAddress.IPv6Any))
    //        {
    //            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
    //            foreach (NetworkInterface adapter in adapters)
    //            {
    //                IPInterfaceProperties properties = adapter.GetIPProperties();
    //                foreach (IPAddressInformation unicast in properties.UnicastAddresses)
    //                {
    //                    if (ipep.AddressFamily == unicast.Address.AddressFamily)
    //                    {
    //                        Log.Message(LogTypes.Info, $"Address: {unicast.Address} - Port: {ipep.Port}");
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Log.Message(LogTypes.Info, $"Address: {ipep.Address} - Port: {ipep.Port}");
    //        }
    //    }

    //    public bool Listen()
    //    {
    //        try
    //        {
    //            _serverSocket.Listen(MaxActiveConnections);
    //            new Thread(AcceptAsync) { IsBackground = true }.Start();

    //            IsListening = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Message(LogTypes.Error, ex.Message);

    //            IsListening = false;
    //        }
    //        return IsListening;
    //    }

    //    private async void AcceptAsync()
    //    {
    //        try
    //        {
    //            _acceptConnections = true;

    //            while (_acceptConnections)
    //            {
    //                await Task.Delay(10);

    //                if (_serverSocket.Poll(0, SelectMode.SelectRead))
    //                {
    //                    if (!_acceptedSockets.TryPop(out var arg))
    //                    {
    //                        arg = new SocketAsyncEventArgs();
    //                        arg.Completed += async (sender, e) => await ProcessAcceptAsync(arg);
    //                    }

    //                    if (!_serverSocket.AcceptAsync(arg))
    //                        await ProcessAcceptAsync(arg);
    //                }
    //            }

    //            if (!_disposed && IsListening)
    //            {
    //                while (!_acceptConnections)
    //                    await Task.Delay(10);

    //                AcceptAsync();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Message(LogTypes.Error, ex.Message);
    //        }
    //    }

    //    private async Task ProcessAcceptAsync(SocketAsyncEventArgs e)
    //    {
    //        if (e.AcceptSocket?.Connected == true)
    //        {
    //            var readWriteEventArgs = new SocketAsyncEventArgs[2];

    //            if (_readWriteSockets.TryPopRange(readWriteEventArgs) == readWriteEventArgs.Length)
    //            {
    //                Interlocked.Increment(ref _activeConnections);

    //                readWriteEventArgs[0].AcceptSocket = e.AcceptSocket;
    //                readWriteEventArgs[1].AcceptSocket = e.AcceptSocket;

    //                await CreateSession(readWriteEventArgs);
    //            }
    //            else
    //            {
    //                e.AcceptSocket.Dispose();

    //                Log.Message(LogTypes.Info, "Maximum number of active client connections has been reached.");
    //            }
    //        }

    //        e.AcceptSocket = null;

    //        _acceptedSockets.Push(e);
    //    }

    //    protected virtual Task CreateSession(SocketAsyncEventArgs[] sockets)
    //    {
    //        return Task.Run(() =>
    //       {
    //           var session = new TSession
    //           {
    //               Guid = GuidGenerator.GenerateNew(),
    //               Disconnect = DeleteSession,
    //               Sockets = sockets
    //           };

    //           Sessions.Add(session);
    //           Log.Message(LogTypes.Warning, $">> CREAZIONE SESSIONE {_activeConnections}");
    //           session.Accept();
    //       });           
    //    }

    //    protected void DeleteSession(SocketSessionBase session)
    //    {
    //        session.Sockets[0].SetBuffer(new byte[BufferSize], 0, BufferSize);
    //        session.Sockets[1].SetBuffer(new byte[BufferSize], 0, BufferSize);

    //        session.Sockets[0].AcceptSocket?.Dispose();
    //        session.Sockets[1].AcceptSocket?.Dispose();

    //        _readWriteSockets.PushRange(session.Sockets);

    //        Interlocked.Decrement(ref _activeConnections);

    //        lock (_disposedSessions)
    //            _disposedSessions.Enqueue(session);

    //        Log.Message(LogTypes.Warning, $"<< ELIMINAZIONE SESSIONE {_activeConnections}");
    //    }

    //    public void Pause()
    //    {
    //        for (int i = 0; i < Sessions.Count; i++)
    //        {
    //            var client = Sessions[i];
    //            if (client != null && !client.IsDisposed)
    //                client.SetPaused();
    //        }
    //    }

    //    public void Resume()
    //    {
    //        for (int i = 0; i < Sessions.Count; i++)
    //        {
    //            var client = Sessions[i];
    //            if (client != null && !client.IsDisposed)
    //            {
    //                client.SetNormal();
    //            }
    //        }
    //    }

    //    public void FreeDisposedSessions()
    //    {
    //        lock (_disposedSessions)
    //        {
    //            int a = 0;

    //            while (a < 200 && _disposedSessions.Count > 0)
    //            {
    //                a++;

    //                var session = _disposedSessions.Dequeue();
    //                Sessions.Remove((SocketClientSession)session);
    //            }
    //        }
    //    }


    //     public void StartAcceptConnections() => _acceptConnections = true;
    //     public void StopAcceptConnections() => _acceptConnections = false;

    //    public void Restart()
    //    {
    //        if (IsListening || _acceptConnections)
    //            return;

    //        Create(_ipAddress, _port);
    //        Listen();
    //    }

    //    public void Stop()
    //    {
    //        StopAcceptConnections();

    //        _serverSocket.Close();

    //        _acceptEventArgs = null;
    //        _readWriteSockets.Clear();

    //        IsListening = false;
    //    }


    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!_disposed)
    //        {
    //            if (!disposing)
    //            {
    //                Stop();
    //            }
    //            _disposed = true;
    //        }
    //    }

    //    public void Dispose() => Dispose(true);
    //}
}