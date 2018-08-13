namespace EnhancedMapServerNetCore.Network
{
    //public class SocketServer : SocketServerBase<SocketClientSession>
    //{
    //    public SocketServer(Configuration.Config config, int buffsize)
    //        : this(config.Port, config.MaxSimultaneConnections, config.MaxActiveConnections, buffsize)
    //    {

    //    }

    //    public SocketServer(ushort port, int maxSimultanAcceptConnections, int maxActiveConnections, int bufferSize) :
    //        base(port, maxSimultanAcceptConnections, maxActiveConnections, bufferSize)
    //    {

    //    }


    //    public void Flush()
    //    {
    //        for (int i = 0; i < Sessions.Count; i++)
    //        {
    //            var session = Sessions[i];
    //            if (session != null && !session.IsDisposed)
    //            {
    //                session.Flush();
    //            }
    //        }
    //    }

    //    public void CheckSessionsAlive()
    //    {
    //        for (int i = 0; i < Sessions.Count; i++)
    //        {
    //            var session = Sessions[i];
    //            if (session != null && !session.IsDisposed)
    //            {

    //            }
    //        }
    //    }


    //}
}