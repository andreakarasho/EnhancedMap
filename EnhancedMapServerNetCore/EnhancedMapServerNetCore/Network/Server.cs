using EnhancedMapServerNetCore.Configuration;
using EnhancedMapServerNetCore.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EnhancedMapServerNetCore.Network
{
    public class Server : IDisposable
    {
        private static readonly Socket[] _emptySockets = new Socket[0];

        private static readonly SocketAsyncEventArgs[] _emptyArgs = new SocketAsyncEventArgs[0];
        private readonly Queue<Socket> _acceptedSockets;
        private readonly object _acceptedSync;

        private readonly Queue<Session> _disposedSessionsQueue = new Queue<Session>();
        private readonly SocketAsyncEventArgs _acceptEventArgs;
        private int _activeConnectionsCount;

        //private readonly Stack<SocketAsyncEventArgs> _readwritePoolEventArgs;
        private Socket _serverSocket;
        private Config _config;

        private readonly List<Session> _sessions;

        public Server(Config config)
        {
            _config = config;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, config.Port);

            _sessions = new List<Session>();
            _acceptedSockets = new Queue<Socket>();
            _acceptedSync = ((ICollection)_acceptedSockets).SyncRoot;
            _serverSocket = Bind(ipep);
            if (_serverSocket == null)
                return;

            _acceptEventArgs = new SocketAsyncEventArgs();
            _acceptEventArgs.Completed += (sender, e) =>
            {
                ProcessAccept(e);
                StartAccept();
            };

            //_readwritePoolEventArgs = new Stack<SocketAsyncEventArgs>();

            //for (int i = 0; i < config.MaxActiveConnections * 2; i++)
            //{
            //    SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            //    _readwritePoolEventArgs.Push(arg);
            //}

            StartAccept();
        }

        public int TotalSocketsAlive => _activeConnectionsCount;
        public bool IsFull => _activeConnectionsCount >= _config.MaxActiveConnections;

        public IReadOnlyList<Session> Sessions => _sessions;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Socket[] GetAwaitingSockets()
        {
            Socket[] sockets;

            lock (_acceptedSync)
            {
                if (_acceptedSockets.Count == 0)
                    return _emptySockets;
                sockets = _acceptedSockets.ToArray();
                _acceptedSockets.Clear();
            }

            return sockets;
        }

        //public SocketAsyncEventArgs[] GetAvailableArgs()
        //{
        //    SocketAsyncEventArgs[] args;

        //    if (_readwritePoolEventArgs.Count > 1)
        //    {
        //        args = new SocketAsyncEventArgs[2];

        //        args[0] = _readwritePoolEventArgs.Pop();
        //        args[1] = _readwritePoolEventArgs.Pop();
        //    }
        //    else
        //        args = _emptyArgs;

        //    return args;
        //}

        public void Increase(Session session)
        {
            _sessions.Add(session);
            Interlocked.Increment(ref _activeConnectionsCount);
        }

        public void Decrease(Session session)
        {
            lock (_disposedSessionsQueue)
            {
                _disposedSessionsQueue.Enqueue(session);
            }

            Interlocked.Decrement(ref _activeConnectionsCount);
        }

        public void Disconnect(Session session)
        {
            //for (int i = 0; i < session.Args.Length; i++)
            //    _readwritePoolEventArgs.Push(session.Args[i]);

            Decrease(session);

            Log.Message(LogTypes.Info, "Session terminated.");
        }

        public void Flush()
        {
            for (int i = 0; i < _sessions.Count; i++) _sessions[i].Flush();
        }

        public void CheckSessionsAlive()
        {
            try
            {
                for (int i = 0; i < _sessions.Count; i++) _sessions[i].CheckAlive();
            }
            catch
            {
            }
        }

        public void Pause()
        {
            for (int i = 0; i < _sessions.Count; i++) _sessions[i].Pause();
        }

        public void Resume()
        {
            for (int i = 0; i < _sessions.Count; i++) _sessions[i].Resume();
        }

        public void ReleaseDisposed()
        {
            lock (_disposedSessionsQueue)
            {
                int i = 0;

                while (i < 200 && _disposedSessionsQueue.Count > 0)
                {
                    i++;

                    Session s = _disposedSessionsQueue.Dequeue();
                    if (s == null)
                        continue;

                    _sessions.Remove(s);
                }
            }
        }

        private void StartAccept()
        {
            bool result = false;

            do
            {
                try
                {
                    result = !_serverSocket.AcceptAsync(_acceptEventArgs);
                }
                catch (SocketException e)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                if (result)
                    ProcessAccept(_acceptEventArgs);
            } while (result);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && VerifySocket(e.AcceptSocket))
                Enqueue(e.AcceptSocket);
            else
                Release(e.AcceptSocket);
            e.AcceptSocket = null;
        }

        private void Enqueue(Socket s)
        {
            lock (_acceptedSync)
            {
                _acceptedSockets.Enqueue(s);
            }

            Core.Set();
        }

        private bool VerifySocket(Socket s)
        {
            try
            {
                s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
                return true;
            }
            catch
            {
                Log.Message(LogTypes.Warning, "Something wrong with this socket");
                return false;
            }
        }

        private void Release(Socket s)
        {
            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException e)
            {
            }

            s.Close();
        }

        private Socket Bind(IPEndPoint local)
        {
            Socket s = new Socket(local.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                s.LingerState.Enabled = false;
                s.ExclusiveAddressUse = false;
                s.Bind(local);
                s.Listen(10);
                return s;
            }
            catch (Exception e)
            {
                if (e is SocketException se)
                {
                }

                return null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Socket socket = Interlocked.Exchange(ref _serverSocket, null);
                if (socket != null)
                    socket.Close();
            }
        }
    }
}