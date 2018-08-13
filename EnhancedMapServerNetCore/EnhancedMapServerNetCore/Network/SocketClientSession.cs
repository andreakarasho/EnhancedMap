using System;

namespace EnhancedMapServerNetCore.Network
{
    [Flags]
    public enum SocketStatus
    {
        Pending = 0x01,
        Pause = 0x02
    }

    //public class SocketClientSession : SocketSessionBase, IDisposable
    //{
    //    public bool IsAccepted { get; internal set; }

    //    public override void Accept()
    //    {
    //        IsDisposed = false;

    //        if (Sockets[0].AcceptSocket == null)
    //            return;

    //        for (int i = 0; i < Sockets.Length; i++)
    //            Sockets[i].Completed += Process;

    //        Sockets[0].UserToken = new AsyncUserToken(Sockets[0].AcceptSocket);

    //        if (IsAccepted)
    //            _lastUpdate = DateTime.Now.AddMinutes(1);

    //        InternalRecv();
    //    }

    //    public override void Process(object sender, SocketAsyncEventArgs e)
    //    {
    //        switch (e.LastOperation)
    //        {

    //            case SocketAsyncOperation.Receive:
    //                ProcessReceive(e);
    //                if (!IsDisposed)
    //                    InternalRecv();

    //                break;

    //            case SocketAsyncOperation.Send:
    //                ProcessSend(e);

    //                if (IsDisposed)
    //                    return;

    //                Gram gram;

    //                lock (_sendQueue)
    //                {
    //                    gram = _sendQueue.Dequeue();
    //                    if (gram == null && _sendQueue.IsFlushReady)
    //                        gram = _sendQueue.CheckFlushReady();
    //                }

    //                if (gram != null)
    //                {
    //                    e.SetBuffer(gram.Buffer, 0, gram.Length);
    //                    InternalSend();
    //                }
    //                else
    //                {
    //                    lock (_send)
    //                        _isSending = false;
    //                }

    //                break;

    //            default:
    //                throw new ArgumentException("The last operation completed on the socket was not a receive or send");
    //        }
    //    }

    //    internal void ProcessReceive(SocketAsyncEventArgs e)
    //    {
    //        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
    //        {
    //            if (IsAccepted)
    //                _lastUpdate = DateTime.Now.AddMinutes(1);

    //            AsyncUserToken token = (AsyncUserToken)e.UserToken;

    //            ProcessReceivedData(token.DataStartOffset, token.NextReceiveOffset - token.DataStartOffset + e.BytesTransferred, 0, token, e);

    //            token.NextReceiveOffset += e.BytesTransferred;

    //            if (token.NextReceiveOffset == e.Buffer.Length)
    //            {
    //                token.NextReceiveOffset = 0;

    //                if (token.DataStartOffset < e.Buffer.Length)
    //                {
    //                    var notYesProcessDataSize = e.Buffer.Length - token.DataStartOffset;
    //                    Buffer.BlockCopy(e.Buffer, token.DataStartOffset, e.Buffer, 0, notYesProcessDataSize);

    //                    token.NextReceiveOffset = notYesProcessDataSize;
    //                }

    //                token.DataStartOffset = 0;
    //            }

    //            e.SetBuffer(token.NextReceiveOffset, e.Buffer.Length - token.NextReceiveOffset);

    //            lock (_obj)
    //                _status &= ~SocketStatus.Pending;              
    //        }
    //        else
    //        {
    //            Dispose();
    //        }
    //    }

    //    private void ProcessReceivedData(int dataStartOffset, int totalReceivedDataSize, int alreadyProcessedDataSize, AsyncUserToken token, SocketAsyncEventArgs e)
    //    {
    //        if (alreadyProcessedDataSize >= totalReceivedDataSize)
    //        {
    //            return;
    //        }

    //        if (token.MessageID == null || token.MessageSize == null)
    //        {
    //            if (totalReceivedDataSize > HEADER_SIZE)
    //            {
    //                Buffer.BlockCopy(e.Buffer, dataStartOffset, _headerBuffer, 0, HEADER_SIZE);
    //                var messageID = _headerBuffer[0];
    //                var messageSize = (ushort)((_headerBuffer[2] << 8) | _headerBuffer[1]);

    //                token.MessageID = messageID;
    //                token.MessageSize = messageSize - HEADER_SIZE;
    //                token.DataStartOffset = dataStartOffset + HEADER_SIZE;

    //                ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + HEADER_SIZE, token, e);
    //            }
    //        }
    //        else
    //        {
    //            int messageSize = token.MessageSize.Value;
    //            if (totalReceivedDataSize - alreadyProcessedDataSize >= messageSize)
    //            {
    //                byte[] data = new byte[messageSize];
    //                Buffer.BlockCopy(e.Buffer, dataStartOffset, data, 0, messageSize);

    //                // process message
    //                Packet packet = new Packet(data, token.MessageID.Value, messageSize);
    //                PacketHandlers.Handlers.OnPacket(this, packet);

    //                token.DataStartOffset = dataStartOffset + messageSize;
    //                token.MessageID = null;
    //                token.MessageSize = null;

    //                ProcessReceivedData(token.DataStartOffset, totalReceivedDataSize, alreadyProcessedDataSize + messageSize, token, e);
    //            }
    //        }
    //    }

    //    private void InternalRecv()
    //    {

    //        lock (_obj)
    //        {
    //            if ((_status & (SocketStatus.Pending | SocketStatus.Pause)) == 0)
    //            {
    //                _status |= SocketStatus.Pending;
    //                if (!Sockets[0].AcceptSocket.ReceiveAsync(Sockets[0]))
    //                    Process(null, Sockets[0]);
    //            }
    //        }
    //    }

    //    private void ProcessSend(SocketAsyncEventArgs e)
    //    {
    //        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
    //        {
    //            if (IsAccepted)
    //                _lastUpdate = DateTime.Now.AddMinutes(1);
    //        }
    //        else
    //            Dispose();
    //    }

    //    public void Send(PacketWriter packet) => Send(packet.ToArray());

    //    private void Send(byte[] data)
    //    {
    //        try
    //        {
    //            Gram gram = null;

    //            lock (_send)
    //            {
    //                lock (_sendQueue)
    //                    gram = _sendQueue.Enqueue(data, 0, data.Length);
    //                if (gram != null && !_isSending)
    //                {
    //                    _isSending = true;
    //                    Sockets[1].SetBuffer(gram.Buffer, 0, gram.Length);
    //                    InternalSend();
    //                }
    //            }
    //        }
    //        catch (CapacityExceededException)
    //        {
    //            Log.Message(LogTypes.Warning, $"Too muck data pending for {Guid}");
    //            Dispose();
    //        }
    //    }

    //    private void InternalSend()
    //    {
    //        bool result = false;

    //        result = !Sockets[1].AcceptSocket.SendAsync(Sockets[1]);
    //        if (result)
    //            Process(null, Sockets[1]);
    //    }

    //    public void Flush()
    //    {
    //        if (Sockets[0].AcceptSocket == null)
    //            return;

    //        lock (_send)
    //        {
    //            if (_isSending)
    //                return;

    //            Gram gram = null;

    //            lock (_sendQueue)
    //            {
    //                if (!_sendQueue.IsFlushReady)
    //                    return;
    //                gram = _sendQueue.CheckFlushReady();
    //            }

    //            if (gram != null)
    //            {
    //                _isSending = true;
    //                Sockets[1].SetBuffer(gram.Buffer, 0, gram.Length);
    //                InternalSend();
    //            }
    //        }
    //    }

    //    internal void SetPaused()
    //    {
    //        lock (_obj)
    //            _status |= SocketStatus.Pause;
    //    }

    //    internal void SetNormal()
    //    {
    //        lock (_obj)
    //        {
    //            _status &= ~SocketStatus.Pause;

    //            if ((_status & SocketStatus.Pending) == 0)
    //            {
    //                InternalRecv();
    //            }
    //        }
    //    }

    //    public void IsAlive()
    //    {
    //        if (Sockets[0]?.AcceptSocket == null)
    //            return;

    //        if (_lastUpdate > DateTime.Now)
    //            return;

    //        Log.Message(LogTypes.Info, $"Session {Guid} disconnected for inactivity.");
    //        Dispose();
    //    }

    //    private void Dispose(bool disposing)
    //    {
    //        if (disposing)
    //        {
    //            if (!IsDisposed)
    //            {
    //                UserManager.Remove(Guid);

    //                IsDisposed = true;

    //                var token = ((AsyncUserToken)Sockets[0].UserToken);
    //                token.Dispose();
    //                Sockets[0].UserToken = null;

    //                Sockets[0].Completed -= Process;
    //                Sockets[1].Completed -= Process;

    //                Disconnect(this);
    //            }
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        Dispose(true);
    //    }
    //}


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
    //            this.Socket.Shutdown(SocketShutdown.Both);
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