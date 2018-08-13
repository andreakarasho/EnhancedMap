namespace EnhancedMapServerNetCore.Network
{
    //public abstract class SocketSessionBase
    //{
    //    protected const int HEADER_SIZE = 3;
    //    protected readonly byte[] _headerBuffer = new byte[HEADER_SIZE];
    //    protected SendQueue _sendQueue = new SendQueue();
    //    protected readonly object _obj = new object();
    //    protected SocketStatus _status;
    //    protected readonly object _send = new object();
    //    protected bool _isSending;

    //    protected DateTime _lastUpdate;

    //    public bool IsDisposed { get; protected set; }
    //    public SocketAsyncEventArgs[] Sockets { get; set; }
    //    public Action<SocketSessionBase> Disconnect { get; set; }

    //    public IPAddress Address =>Sockets != null && Sockets[0].AcceptSocket != null ? ((IPEndPoint)Sockets[0].AcceptSocket.RemoteEndPoint).Address : IPAddress.None;

    //    // Used to identify the current session.
    //    public Guid Guid { get; set; }

    //    // AddressFamily, IPAddress and Port.
    //    public IPEndPoint AddressInfo => Sockets?[0].AcceptSocket.RemoteEndPoint as IPEndPoint;

    //    public abstract void Accept();
    //    public abstract void Process(object sender, SocketAsyncEventArgs e);
    //}
}