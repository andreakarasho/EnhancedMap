using EnhancedMapServerNetCore.Logging;
using EnhancedMapServerNetCore.Managers;
using System;
using System.Net;
using System.Net.Sockets;

namespace EnhancedMapServerNetCore.Network
{
    public sealed class Session : IDisposable
    {
        private static readonly ReusableArray<byte> _bufferPool = new ReusableArray<byte>(1000, 2048);

        private readonly object _asyncLock = new object();
        private readonly ServerHandler _manager;
        private readonly object _sendLock = new object();
        private SocketStatus _asyncState;
        private DateTime _lastUpdate;
        private readonly byte[] _recvBuffer;
        private bool _sending;
        private readonly SendQueue _sendQueue;
        public Action<Session> Disconnect;


        public Session(Socket socket, ServerHandler manager)
        {
            _socket = socket;
            _manager = manager;
            _sendQueue = new SendQueue();
            Buffer = new CircularBuffer();
            _recvBuffer = _bufferPool.GetSegment();
            Guid = GuidGenerator.GenerateNew();

            _lastUpdate = DateTime.Now.AddMinutes(1);
        }

        private SocketAsyncEventArgs _recvEventArgs;
        private SocketAsyncEventArgs _sendEventArgs;
        private Socket _socket { get; }


        public bool IsDisposed { get; private set; }
        public Guid Guid { get; }
        public CircularBuffer Buffer { get; }

        public bool IsConnected => _socket != null && _socket.Connected;
        public bool IsRunning { get; private set; }
        public bool IsAccepted { get; internal set; }
        public IPEndPoint Address => _socket?.RemoteEndPoint as IPEndPoint;

        public void Dispose()
        {
            Dispose(true);
        }


        public void Accept()
        {
            IsRunning = true;

            _recvEventArgs = new SocketAsyncEventArgs();
            _recvEventArgs.Completed += RecvCompleted;
            _recvEventArgs.SetBuffer(_recvBuffer, 0, _recvBuffer.Length);

            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.Completed += SendCompleted;

            if (IsAccepted)
                _lastUpdate = DateTime.Now.AddMinutes(1);

            StartReceive();
        }

        private void RecvCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
            if (!IsDisposed)
                StartReceive();
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);

            if (IsDisposed)
                return;

            SendQueue.Gram gram;

            lock (_sendQueue)
            {
                gram = _sendQueue.Dequeue();
                if (gram == null && _sendQueue.IsFlushReady)
                    gram = _sendQueue.CheckFlushReady();
            }

            if (gram != null)
            {
                _sendEventArgs.SetBuffer(gram.Buffer, 0, gram.Length);
                StartSend();
            }
            else
            {
                lock (_sendLock)
                {
                    _sending = false;
                }
            }
        }

        public void Send(PacketWriter writer)
        {
            Send(writer.ToArray());
        }

        public void Flush()
        {
            if (_socket == null)
                return;

            lock (_sendLock)
            {
                if (_sending)
                    return;

                SendQueue.Gram gram;

                lock (_sendQueue)
                {
                    if (!_sendQueue.IsFlushReady)
                        return;
                    gram = _sendQueue.CheckFlushReady();
                }

                if (gram != null)
                {
                    _sending = true;
                    _sendEventArgs.SetBuffer(gram.Buffer, 0, gram.Length);
                    StartSend();
                }
            }
        }


        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                byte[] buffer = _recvBuffer;

                if (IsAccepted)
                    _lastUpdate = DateTime.Now.AddMinutes(1);

                lock (Buffer)
                {
                    Buffer.Enqueue(buffer, 0, e.BytesTransferred);
                }

                _manager.Enqueue(this);

                lock (_asyncLock)
                {
                    _asyncState &= ~SocketStatus.Pending;
                }
            }
            else
                Dispose();
        }

        private void StartReceive()
        {
            lock (_asyncLock)
            {
                if ((_asyncState & (SocketStatus.Pending | SocketStatus.Pause)) == 0)
                {
                    _asyncState |= SocketStatus.Pending;
                    if (!_socket.ReceiveAsync(_recvEventArgs))
                        RecvCompleted(null, _recvEventArgs);
                }
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                if (IsAccepted)
                    _lastUpdate = DateTime.Now.AddMinutes(1);
            }
            else
                Dispose();
        }

        private void StartSend()
        {
            if (!_socket.SendAsync(_sendEventArgs))
                SendCompleted(null, _sendEventArgs);
        }

        private void Send(byte[] data)
        {
            if (_socket == null)
                return;

            if (data != null)
            {
                if (data.Length <= 0)
                    return;

                try
                {
                    SendQueue.Gram gram;

                    lock (_sendLock)
                    {
                        lock (_sendQueue)
                        {
                            gram = _sendQueue.Enqueue(data, 0, data.Length);
                        }

                        if (gram != null && !_sending)
                        {
                            _sending = true;
                            _sendEventArgs.SetBuffer(gram.Buffer, 0, gram.Length);
                            StartSend();
                        }
                    }
                }
                catch (CapacityExceededException)
                {
                    Log.Message(LogTypes.Warning, $"Too muck data pending for {Guid}");
                    Dispose();
                }

                Core.Set();
            }
            else
                Dispose();
        }

        public void Pause()
        {
            lock (_asyncLock)
            {
                _asyncState |= SocketStatus.Pause;
            }
        }

        public void Resume()
        {
            lock (_asyncLock)
            {
                _asyncState &= ~SocketStatus.Pause;
                if ((_asyncState & SocketStatus.Pending) == 0)
                    StartReceive();
            }
        }

        public void CheckAlive()
        {
            if (_socket == null)
                return;

            if (_lastUpdate > DateTime.Now)
                return;

            Log.Message(LogTypes.Info, $"Session {Guid} disconnected for inactivity.");
            Dispose();
        }

        private void IO_Socket(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    if (!IsDisposed)
                        StartReceive();
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);

                    if (IsDisposed)
                        return;

                    SendQueue.Gram gram;

                    lock (_sendQueue)
                    {
                        gram = _sendQueue.Dequeue();
                        if (gram == null && _sendQueue.IsFlushReady)
                            gram = _sendQueue.CheckFlushReady();
                    }

                    if (gram != null)
                    {
                        _sendEventArgs.SetBuffer(gram.Buffer, 0, gram.Length);
                        StartSend();
                    }
                    else
                    {
                        lock (_sendLock)
                        {
                            _sending = false;
                        }
                    }

                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void Dispose(bool flush)
        {
            if (IsDisposed)
                return;

            UserManager.Remove(Guid);

            IsDisposed = true;
            IsRunning = false;

            if (flush)
                Flush();

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            try
            {
                _socket.Close();
            }
            catch
            {
            }

            if (_recvBuffer != null)
            {
                lock (_bufferPool)
                {
                    _bufferPool.Free(_recvBuffer);
                }
            }

            lock (_sendQueue)
            {
                if (!_sendQueue.IsEmpty)
                    _sendQueue.Clear();
            }

            _recvEventArgs = null;
            _sendEventArgs = null;


            Disconnect(this);
        }
    }
}