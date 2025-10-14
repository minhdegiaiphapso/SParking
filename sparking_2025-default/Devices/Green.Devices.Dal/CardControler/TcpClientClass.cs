using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Runtime.InteropServices;

namespace Green.Devices.Dal.CardControler
{
    public class TcpClientClass:TcpBaseClass
    {

        #region 内部变量
        private Socket sock;
        private IPEndPoint iep;
        private System.Timers.Timer timer;
        private Boolean Reconnet = false;
        public string remoteHost = "192.168.0.71";
        public int remotePort = 8000;
        private Boolean Enable = true;
        #endregion

        public TcpClientClass(Boolean enable = true)
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
            timer.Interval = 500;
            timer.Enabled = false;
            Enable = enable;
        }

        private bool IsSocketConnected()
        {
            // This is how you can determine whether a socket is still connected.
            bool connectState = true;
            bool blockingState = sock.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                sock.Blocking = false;
                sock.Send(tmp, 1, 0);
                Console.WriteLine("Connected!");
                connectState = true; //若Send错误会跳去执行catch体，而不会执行其try体里其之后的代码
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    //Console.WriteLine("Still Connected, but the Send would block");
                    connectState = true;
                }
                else
                {
                    SockErrorStr = e.ToString();
                    //Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    connectState = false;
                    IsconnectSuccess = false;
                    //Console.WriteLine("IsSocketConnected ");
                }
            }
            finally
            {
                sock.Blocking = blockingState;
            }
            return connectState;
        }

        /// 另一种判断connected的方法，但未检测对端网线断开或ungraceful的情况 
        private bool IsSocketConnected(Socket s)
        {
            #region remarks
            /* As zendar wrote, it is nice to use the Socket.Poll and Socket.Available, but you need to take into consideration 
             * that the socket might not have been initialized in the first place. 
             * This is the last (I believe) piece of information and it is supplied by the Socket.Connected property. 
             * The revised version of the method would looks something like this: 
             * from：http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c */
            #endregion

            try
            {
                if (s == null)
                    return false;
                return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
            }
            catch (SocketException e)
            {
                IsconnectSuccess = false;
                SockErrorStr = e.ToString();
                return false;
            }
        }
        //================================================================================================================================
        /// 创建套接字+异步连接函数
        private bool socket_create_connect()
        {
            if (remoteHost == "") return false;
            try
            {
                IPAddress serverIp = IPAddress.Parse(remoteHost);
                int serverPort = Convert.ToInt32(remotePort);
                iep = new IPEndPoint(serverIp, serverPort);

                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.SendTimeout = 500;
                sock.BeginConnect(iep, new AsyncCallback(connectedCallback), sock);
            }
            catch (Exception err)
            {
                SockErrorStr = err.ToString();
                //  Console.WriteLine("socket_create_connect" + err.Message);
                return false;
            }
            return true;
        }

        /// 异步连接回调函数
        private void connectedCallback(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                timer.Enabled = true;
                if (sock.Connected)
                {
                    sock.EndConnect(iar);
                    IsconnectSuccess = true;
                    sock.BeginReceive(BufferRX, 0, BufferRX.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), sock);
                }
                else
                {
                    IsconnectSuccess = false;
                    //sock.BeginConnect(iep, new AsyncCallback(connectedCallback), sock);
                }
            }
            catch (Exception e)
            {
                SockErrorStr = e.ToString();
                IsconnectSuccess = false;
                // Console.WriteLine("connectedCallback ");
            }
        }

        // 接收数据回调函数
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                lock (BufferRX)
                {
                    Socket peerSock = (Socket)ar.AsyncState;
                    int BytesRead = peerSock.EndReceive(ar);
                    if (BytesRead > 0)
                    {
                        if (BytesRead > 1024)
                            BytesRead = 1024;
                        DoOnRxTxDataEvent(0, BufferRX, BytesRead);
                    }
                    else//对端gracefully关闭一个连接
                    {
                        if (sock.Connected)//上次socket的状态
                        {
                            DosocketDisconnected();
                            return;
                        }
                    }
                    sock.BeginReceive(BufferRX, 0, BufferRX.Length, 0, new AsyncCallback(ReceiveCallBack), sock);
                }
            }
            catch (Exception ex)
            {
                SockErrorStr = ex.ToString();
                DosocketDisconnected();
                return;
            }
        }

        private bool Reconnect()
        {
            try
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Disconnect(true);
                IsconnectSuccess = false;
                sock.Close();
            }
            catch (Exception ex)
            {
                SockErrorStr = ex.ToString();
            }
            return socket_create_connect();
        }

        override public bool OpenIP(string ip, int port)
        {
            if (ip == "") return false;
            Reconnet = true;
            if (IsconnectSuccess) return true;

            remoteHost = ip;
            remotePort = port;
            return socket_create_connect();
        }

        override public bool CloseTcpip()
        {
            timer.Enabled = false;
            Reconnet = false;
            DosocketDisconnected();

            lock (this)
            {
                if (sock != null)
                    if (IsconnectSuccess)
                    {
                        try
                        {
                            //关闭socket 
                            timer.Enabled = false;
                            sock.Disconnect(false);
                            IsconnectSuccess = false;
                        }
                        catch (Exception ex)
                        {
                            SockErrorStr = ex.ToString();
                        }
                    }
            }
            timer.Enabled = false;
            return true;
        }

        private void socketDisconnectedHandler()
        {
            //IsconnectSuccess = false;
            // if (timer.Enabled)
            //    Reconnect();
        }

        private void timer_Tick(object sender, ElapsedEventArgs e)
        {
            if (!Enable) { timer.Enabled = false; return; }
            if (!Reconnet) { timer.Enabled = Reconnet; return; }
            try
            {
                isHeartTime++;
                if (isHeartTime > 10)
                {
                    isHeartTime = 0;

                    if (sock == null)
                    {
                        timer.Enabled = false;
                        DosocketDisconnected();
                        socket_create_connect();
                    }
                    else // if
                    {
                        if ((!sock.Connected) || (!IsSocketConnected(sock)) || (!IsSocketConnected()))
                        {
                            DosocketDisconnected();
                            timer.Enabled = false;
                            IsconnectSuccess = false;
                            Reconnect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SockErrorStr = ex.ToString();
                IsconnectSuccess = false;
                //    Console.WriteLine(string.Format("timer_Tick {0}", ex.Message));
                timer.Enabled = true;
            }
        }

        private void SendDataEnd(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
        }

        override public byte DoSendData(byte[] buffTX, int WriteNum)
        {
            StartTick = Environment.TickCount;
            try
            {
                if (IsconnectSuccess)
                    lock (buffTX)
                    {
                        if (WriteNum > 1024) WriteNum = 1024;
                        Array.ConstrainedCopy(buffTX, 0, BufferTX, 0, WriteNum);

                        DoOnRxTxDataEvent(1, BufferTX, WriteNum);

                        lock (sock)
                            sock.BeginSend(buffTX, 0, WriteNum, SocketFlags.None, new AsyncCallback(SendDataEnd), sock);

                        return 0;
                    }
                return 4;
            }
            catch (Exception ex)
            {
                SockErrorStr = ex.ToString();
                return 7;
            }
        }

        override public void SetTcpTick()
        {
            isHeartTime = 0;
        }
    }
}
