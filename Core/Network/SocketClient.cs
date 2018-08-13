using System;
using System.Net;
using System.Net.Sockets;
using EnhancedMap.Core.Network.Packets;

namespace EnhancedMap.Core.Network
{
    public enum ConnectionStatus
    {
        Offline,
        Online,
        Waiting
    }

    public class SocketClient
    {
        private const int BUFFER_SIZE = 4096;
        private const int HEADER_SIZE = 3;

        private static readonly BufferPool _pool = new BufferPool(10, BUFFER_SIZE);

        private readonly byte[] _headerBuffer = new byte[3];
        private readonly object _send = new object();
        private CircularBuffer _buffer;

        private Socket _clientSocket;
        private byte[] _dataBuffer;
        private IPEndPoint _hostEndPoint;

        private bool _isSending;
        private SocketAsyncEventArgs _recvEventArgs;
        private SocketAsyncEventArgs _sendEventArgs;
        private SendQueue _sendQueue;
        private DateTime _updateTime;


        public bool IsConnected => _clientSocket != null && _clientSocket.Connected;
        public bool IsRunning { get; private set; }

        public bool ReceivedPackets { get; private set; }

        public AccessLevel AccessLevel { get; internal set; }
        public Protocol Protocol { get; internal set; }

        public ConnectionStatus Status { get; internal set; }
        public uint TotalIn { get; private set; }
        public uint TotalOut { get; private set; }

        public bool CanSend { get; set; }
        public static event EventHandler Connected, Disconnected, Waiting;

        public static void InvokeConnect()
        {
            NetworkManager.SocketClient.Status = ConnectionStatus.Online;
            Connected.Raise();
        }

        public static void InvokeDisconnected()
        {
            NetworkManager.SocketClient.Status = ConnectionStatus.Offline;
            Disconnected.Raise();
        }

        public static void InvokeWaiting()
        {
            NetworkManager.SocketClient.Status = ConnectionStatus.Waiting;
            Waiting.Raise();
        }

        public void Connect(IPEndPoint host)
        {
            _buffer = new CircularBuffer();
            _sendQueue = new SendQueue();
            _dataBuffer = _pool.GetFreeSegment();
            CanSend = false;
            AccessLevel = AccessLevel.Normal;
            Protocol = Protocol.Unknown;
            IsRunning = true;

            InvokeWaiting();

            _hostEndPoint = host;
            _clientSocket = new Socket(_hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.UserToken = _clientSocket;
            _sendEventArgs.RemoteEndPoint = _hostEndPoint;
            _sendEventArgs.Completed += _sendEventArgs_Completed;

            _recvEventArgs = new SocketAsyncEventArgs();
            //_recvEventArgs.UserToken = new AsyncUserToken(_clientSocket);
            _recvEventArgs.RemoteEndPoint = _hostEndPoint;
            _recvEventArgs.SetBuffer(_dataBuffer, 0, _dataBuffer.Length);
            _recvEventArgs.Completed += _recvEventArgs_Completed;


            SocketAsyncEventArgs connectEventArgs = new SocketAsyncEventArgs {UserToken = _clientSocket, RemoteEndPoint = _hostEndPoint};
            connectEventArgs.Completed += _connectEventArgs_Completed;

            if (!_clientSocket.ConnectAsync(connectEventArgs)) Connect_Process(connectEventArgs);
        }

        private void _connectEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            Connect_Process(e);
        }

        private void Connect_Process(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (!IsRunning)
                    return;

                if (!_clientSocket.ReceiveAsync(_recvEventArgs))
                    ProcessRecv(_recvEventArgs);

                Send(new PProtocolRequest());
            }
            else
                NetworkManager.Disconnect(false);
        }

        public void Sync()
        {
            HandleReceive();

            if (DateTime.Now > _updateTime && CanSend)
                Global.PlayerInstance.SendData();

            Flush();
        }

        private void ProcessRecv(SocketAsyncEventArgs e)
        {
            if (!IsRunning)
                return;

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                byte[] buffer = _dataBuffer;

                lock (_buffer)
                {
                    _buffer.Enqueue(buffer, 0, e.BytesTransferred);
                }

                TotalIn += (uint) e.BytesTransferred;

                /*AsyncUserToken token = e.UserToken as AsyncUserToken;

                ProcessRecvData(token.DataStartOffset, token.NextReceiveOffset - token.DataStartOffset + e.BytesTransferred, 0, token, e);

                token.NextReceiveOffset += e.BytesTransferred;

                if (token.NextReceiveOffset == e.Buffer.Length)
                {
                    token.NextReceiveOffset = 0;

                    if (token.DataStartOffset < e.Buffer.Length)
                    {
                        var notYesProcessDataSize = e.Buffer.Length - token.DataStartOffset;
                        Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notYesProcessDataSize);
                        token.NextReceiveOffset = notYesProcessDataSize;
                    }

                    token.DataStartOffset = 0;
                }

                e.SetBuffer(token.NextReceiveOffset, e.Buffer.Length - token.NextReceiveOffset);

                TotalIn += (uint)e.BytesTransferred;

                if (IsRunning)
                    if (!token.Socket.ReceiveAsync(e))
                        ProcessRecv(e);
                ReceivedPackets = true;*/
            }
            else
                NetworkManager.Disconnect(false);
        }

        //private void ProcessRecvData(int dataStartOffset, int totalReceivedDataSize, int alreadyProcessedDataSize, AsyncUserToken token, SocketAsyncEventArgs e)
        //{
        //    if (ReceivedPackets)
        //        ReceivedPackets = false;

        //    if (alreadyProcessedDataSize >= totalReceivedDataSize)
        //    {
        //        return;
        //    }

        //    if (token.MessageSize == null || token.MessageID == null)
        //    {
        //        if (totalReceivedDataSize > HEADER_SIZE)
        //        {
        //            Buffer.BlockCopy(e.Buffer, dataStartOffset, _headerBuffer, 0, HEADER_SIZE);

        //            var messageID = _headerBuffer[0];
        //            var messageSize = (ushort)((_headerBuffer[2] << 8) | _headerBuffer[1]);

        //            token.MessageID = messageID;
        //            token.MessageSize = messageSize - HEADER_SIZE;
        //            token.DataStartOffset = dataStartOffset + HEADER_SIZE;

        //            ProcessRecvData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + HEADER_SIZE, token, e);
        //        }
        //    }
        //    else
        //    {
        //        int messageSize = token.MessageSize.Value;
        //        if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize)
        //        {
        //            byte[] data = new byte[messageSize];
        //            Buffer.BlockCopy(e.Buffer, dataStartOffset, data, 0, messageSize);

        //            // process message
        //            Packet packet = new Packet(data, token.MessageID.Value, messageSize);
        //            PacketHandlers.Handlers.OnPacket(packet);

        //            token.DataStartOffset = dataStartOffset + messageSize;
        //            token.MessageID = null;
        //            token.MessageSize = null;

        //            //_pool.AddFreeSegment(data);

        //            ProcessRecvData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + messageSize, token, e);
        //        }
        //    }
        //}


        private void HandleReceive()
        {
            if (!IsRunning || _buffer == null || _buffer.Length <= 0)
                return;

            lock (_buffer)
            {
                ReceivedPackets = false;

                int length = _buffer.Length;

                while (length > 0 && IsRunning)
                {
                    byte id = _buffer.GetId();
                    int size = _buffer.GetLength();

                    if (size < 3)
                    {
                        NetworkManager.Disconnect(false);
                        break;
                    }

                    if (length < size)
                        break;

                    byte[] packet = 4096 >= size ? _pool.GetFreeSegment() : new byte[size];
                    size = _buffer.Dequeue(packet, 0, size);

                    Packet p = new Packet(packet, id, size);
                    PacketHandlers.Handlers.OnPacket(p);

                    length = _buffer.Length;

                    if (4096 >= size)
                        _pool.AddFreeSegment(packet);

                    ReceivedPackets = true;
                }
            }
        }

        public void Disconnect()
        {
            if (_clientSocket == null || !IsRunning)
                return;

            IsRunning = false;
            InvokeDisconnected();

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                _clientSocket.Close();
            }
            catch
            {
            }

            // (_recvEventArgs?.UserToken as AsyncUserToken)?.Dispose();

            _clientSocket = null;
            _sendEventArgs = null;
            _recvEventArgs = null;

            Protocol = Protocol.Unknown;
            AccessLevel = AccessLevel.Normal;
            ReceivedPackets = false;

            if (_dataBuffer != null)
                _pool.AddFreeSegment(_dataBuffer);
            _dataBuffer = null;
            _buffer.Clear();

            lock (_sendQueue)
            {
                if (!_sendQueue.IsEmpty)
                    _sendQueue.Clear();
            }
        }

        public void Send(PacketWriter packet)
        {
            Send(packet.ToArray());
        }

        private void Send(byte[] msg)
        {
            try
            {
                SendQueue.Gram buff = null;

                lock (_send)
                {
                    lock (_sendQueue)
                    {
                        buff = _sendQueue.Enqueue(msg, 0, msg.Length);
                    }

                    if (buff != null && !_isSending)
                    {
                        _isSending = true;
                        _sendEventArgs.SetBuffer(buff.Buffer, 0, buff.Length);

                        if (!_clientSocket.SendAsync(_sendEventArgs))
                            _sendEventArgs_Completed(null, _sendEventArgs);
                    }
                }
            }
            catch (CapacityExceededException)
            {
                NetworkManager.Disconnect(false);
            }
        }


        private void Flush()
        {
            if (_clientSocket == null)
                return;

            lock (_send)
            {
                if (_isSending)
                    return;

                SendQueue.Gram buffer = null;
                lock (_sendQueue)
                {
                    if (!_sendQueue.IsFlushReady) return;
                    buffer = _sendQueue.CheckFlushReady();
                }

                if (buffer != null)
                {
                    _isSending = true;
                    _sendEventArgs.SetBuffer(buffer.Buffer, 0, buffer.Length);

                    if (!_clientSocket.SendAsync(_sendEventArgs))
                        _sendEventArgs_Completed(null, _sendEventArgs);
                }
            }
        }

        private void _recvEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessRecv(e);
            if (IsRunning)
            {
                if (!_clientSocket.ReceiveAsync(_recvEventArgs))
                    _recvEventArgs_Completed(null, _recvEventArgs);
            }
        }

        private void _sendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);

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
                if (!_clientSocket.SendAsync(_sendEventArgs))
                    _sendEventArgs_Completed(null, _sendEventArgs);
            }
            else
            {
                lock (_send)
                {
                    _isSending = false;
                }
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                TotalOut += (uint) e.BytesTransferred;
                _updateTime = DateTime.Now.AddSeconds(1);

                /*if (_isSending)
                    _isSending = false;*/
            }
            else
                NetworkManager.Disconnect(true);
        }
    }

    //public sealed class AsyncUserToken : IDisposable
    //{
    //    public Socket Socket { get; private set; }
    //    public byte? MessageID { get; set; }
    //    public int? MessageSize { get; set; }
    //    public int DataStartOffset { get; set; }
    //    public int NextReceiveOffset { get; set; }

    //    public AsyncUserToken(Socket socket)
    //    {
    //        this.Socket = socket;
    //    }

    //    #region IDisposable Members

    //    public void Dispose()
    //    {
    //        try
    //        {
    //            this.Socket.Shutdown(SocketShutdown.Send);
    //        }
    //        catch (Exception)
    //        { }

    //        try
    //        {
    //            this.Socket.Close();
    //        }
    //        catch (Exception)
    //        { }
    //    }

    //    #endregion
    //}
}