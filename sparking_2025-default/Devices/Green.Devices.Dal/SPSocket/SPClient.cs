using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

//------------------------------------------------------------------------------
// If you are compiling for UWP verify that WINDOWS_UWP or NETFX_CORE are 
// defined into Project Properties->Build->Conditional compilation symbols
//------------------------------------------------------------------------------
#if WINDOWS_UWP || NETFX_CORE
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else // <-- Including MONO
using System.Net.Sockets;
#endif
namespace Green.Devices.Dal.SPSocket
{

    #region [Async Sockets UWP(W10,IoT,Phone)/Windows 8/Windows 8 Phone]
#if WINDOWS_UWP || NETFX_CORE
    class SPSocket
    {
        private DataReader Reader = null;
        private DataWriter Writer = null;
        private StreamSocket TCPSocket;

        private bool _Connected;

        private int _ReadTimeout = 2000;
        private int _WriteTimeout = 2000;
        private int _ConnectTimeout = 1000;

        public static int LastError = 0;


        private void CreateSocket()
        {
            TCPSocket = new StreamSocket();
            TCPSocket.Control.NoDelay = true;
            _Connected = false;
        }

        public SPSocket()
        {
        }

        public void Close()
        {
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
            if (Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }
            if (TCPSocket != null)
            {
                TCPSocket.Dispose();
                TCPSocket = null;
            }
            _Connected = false;
        }

        private async Task AsConnect(string Host, string port, CancellationTokenSource cts)
        {
            HostName ServerHost = new HostName(Host);
            try
            {
                await TCPSocket.ConnectAsync(ServerHost, port).AsTask(cts.Token);
                _Connected = true;
            }
            catch (TaskCanceledException)
            {
                LastError = SPConsts.errTCPConnectionTimeout;
            }
            catch
            {
                LastError = SPConsts.errTCPConnectionFailed; // Maybe unreachable peer
            }
        }

        public int Connect(string Host, int Port)
        {
            LastError = 0;
            if (!Connected)
            {
                CreateSocket();
                CancellationTokenSource cts = new CancellationTokenSource();
                try
                {
                    try
                    {
                        cts.CancelAfter(_ConnectTimeout);
                        Task.WaitAny(Task.Run(async () => await AsConnect(Host, Port.ToString(), cts)));
                    }
                    catch
                    {
                        LastError = SPConsts.errTCPConnectionFailed;
                    }
                }
                finally
                {
                    if (cts != null)
                    {
                        try
                        {
                            cts.Cancel();
                            cts.Dispose();
                            cts = null;
                        }
                        catch { }
                    }

                }
                if (LastError == 0)
                {
                    Reader = new DataReader(TCPSocket.InputStream);
                    Reader.InputStreamOptions = InputStreamOptions.Partial;
                    Writer = new DataWriter(TCPSocket.OutputStream);
                    _Connected = true;
                }
                else
                    Close();
            }
            return LastError;
        }

        private async Task AsReadBuffer(byte[] Buffer, int Size, CancellationTokenSource cts)
        {
            try
            {
                await Reader.LoadAsync((uint)Size).AsTask(cts.Token);
                Reader.ReadBytes(Buffer);
            }
            catch
            {
                LastError = SPConsts.errTCPDataReceive;
            }
        }

        public int Receive(byte[] Buffer, int Start, int Size)
        {
            byte[] InBuffer = new byte[Size];
            CancellationTokenSource cts = new CancellationTokenSource();
            LastError = 0;
            try
            {
                try
                {
                    cts.CancelAfter(_ReadTimeout);
                    Task.WaitAny(Task.Run(async () => await AsReadBuffer(InBuffer, Size, cts)));
                }
                catch
                {
                    LastError = SPConsts.errTCPDataReceive;
                }
            }
            finally
            {
                if (cts != null)
                {
                    try
                    {
                        cts.Cancel();
                        cts.Dispose();
                        cts = null;
                    }
                    catch { }
                }
            }
            if (LastError == 0)
                Array.Copy(InBuffer, 0, Buffer, Start, Size);
            else
                Close();
            return LastError;
        }

        private async Task WriteBuffer(byte[] Buffer, CancellationTokenSource cts)
        {
            try
            {
                Writer.WriteBytes(Buffer);
                await Writer.StoreAsync().AsTask(cts.Token);
            }
            catch
            {
                LastError = SPConsts.errTCPDataSend;
            }
        }

        public int Send(byte[] Buffer, int Size)
        {
            byte[] OutBuffer = new byte[Size];
            CancellationTokenSource cts = new CancellationTokenSource();
            Array.Copy(Buffer, 0, OutBuffer, 0, Size);
            LastError = 0;
            try
            {
                try
                {
                    cts.CancelAfter(_WriteTimeout);
                    Task.WaitAny(Task.Run(async () => await WriteBuffer(OutBuffer, cts)));
                }
                catch
                {
                    LastError = SPConsts.errTCPDataSend;
                }
            }
            finally
            {
                if (cts != null)
                {
                    try
                    {
                        cts.Cancel();
                        cts.Dispose();
                        cts = null;
                    }
                    catch { }
                }
            }
            if (LastError != 0)
                Close();
            return LastError;
        }

        ~SPSocket()
        {
            Close();
        }

        public bool Connected
        {
            get
            {
                return (TCPSocket != null) && _Connected;
            }
        }

        public int ReadTimeout
        {
            get
            {
                return _ReadTimeout;
            }
            set
            {
                _ReadTimeout = value;
            }
        }

        public int WriteTimeout
        {
            get
            {
                return _WriteTimeout;
            }
            set
            {
                _WriteTimeout = value;
            }
        }
        public int ConnectTimeout
        {
            get
            {
                return _ConnectTimeout;
            }
            set
            {
                _ConnectTimeout = value;
            }
        }
    }
#endif
    #endregion
    #region [Sync Sockets Win32/Win64 Desktop Application]
#if !WINDOWS_UWP && !NETFX_CORE
    class SPSocket
    {
        private Socket TCPSocket;
        private int _ReadTimeout = 2000;
        private int _WriteTimeout = 2000;
        private int _ConnectTimeout = 1000;
        public int LastError = 0;

        public SPSocket()
        {
        }

        ~SPSocket()
        {
            Close();
        }
        public void Close()
        {
            if (TCPSocket != null)
            {
                TCPSocket.Dispose();
                TCPSocket = null;
            }
        }
        private void CreateSocket()
        {
            TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TCPSocket.NoDelay = true;
        }

        private void TCPPing(string Host, int Port)
        {
            // To Ping the PLC an Asynchronous socket is used rather then an ICMP packet.
            // This allows the use also across Internet and Firewalls (obviously the port must be opened)           
            LastError = 0;
            Socket PingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IAsyncResult result = PingSocket.BeginConnect(Host, Port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(_ConnectTimeout, true);
                if (!success)
                {
                    LastError = SPConsts.errTCPConnectionFailed;
                }
            }
            catch
            {
                LastError = SPConsts.errTCPConnectionFailed;
            };
            PingSocket.Close();
        }

        public int Connect(string Host, int Port)
        {
            LastError = 0;
            if (!Connected)
            {
                TCPPing(Host, Port);
                if (LastError == 0)
                    try
                    {
                        CreateSocket();
                        TCPSocket.Connect(Host, Port);
                    }
                    catch
                    {
                        LastError = SPConsts.errTCPConnectionFailed;
                    }
            }
            return LastError;
        }

        private int WaitForData(int Size, int Timeout)
        {
            bool Expired = false;
            int SizeAvail;
            int Elapsed = Environment.TickCount;
            LastError = 0;
            try
            {
                SizeAvail = TCPSocket.Available;
                while ((SizeAvail < Size) && (!Expired))
                {
                    Thread.Sleep(2);
                    SizeAvail = TCPSocket.Available;
                    Expired = Environment.TickCount - Elapsed > Timeout;
                    // If timeout we clean the buffer
                    if (Expired && (SizeAvail > 0))
                        try
                        {
                            byte[] Flush = new byte[SizeAvail];
                            TCPSocket.Receive(Flush, 0, SizeAvail, SocketFlags.None);
                        }
                        catch { }
                }
            }
            catch
            {
                LastError = SPConsts.errTCPDataReceive;
            }
            if (Expired)
            {
                LastError = SPConsts.errTCPDataReceive;
            }
            return LastError;
        }

        public int Receive(byte[] Buffer, int Start, int Size)
        {

            int BytesRead = 0;
            LastError = WaitForData(Size, _ReadTimeout);
            if (LastError == 0)
            {
                try
                {
                    BytesRead = TCPSocket.Receive(Buffer, Start, Size, SocketFlags.None);
                }
                catch
                {
                    LastError = SPConsts.errTCPDataReceive;
                }
                if (BytesRead == 0) // Connection Reset by the peer
                {
                    LastError = SPConsts.errTCPDataReceive;
                    Close();
                }
            }
            return LastError;
        }

        public int Send(byte[] Buffer, int Size)
        {
            LastError = 0;
            try
            {
                int BytesSent = TCPSocket.Send(Buffer, Size, SocketFlags.None);
            }
            catch
            {
                LastError = SPConsts.errTCPDataSend;
                Close();
            }
            return LastError;
        }

        public bool Connected
        {
            get
            {
                return (TCPSocket != null) && (TCPSocket.Connected);
            }
        }

        public int ReadTimeout
        {
            get
            {
                return _ReadTimeout;
            }
            set
            {
                _ReadTimeout = value;
            }
        }

        public int WriteTimeout
        {
            get
            {
                return _WriteTimeout;
            }
            set
            {
                _WriteTimeout = value;
            }

        }
        public int ConnectTimeout
        {
            get
            {
                return _ConnectTimeout;
            }
            set
            {
                _ConnectTimeout = value;
            }
        }
    }
#endif
    #endregion
    public static class SPConsts
    {
        #region [Exported Consts]
        // Error codes
        //------------------------------------------------------------------------------
        //                                     ERRORS                 
        //------------------------------------------------------------------------------
        public const int errTCPSocketCreation = 0x00000001;
        public const int errTCPConnectionTimeout = 0x00000002;
        public const int errTCPConnectionFailed = 0x00000003;
        public const int errTCPReceiveTimeout = 0x00000004;
        public const int errTCPDataReceive = 0x00000005;
        public const int errTCPSendTimeout = 0x00000006;
        public const int errTCPDataSend = 0x00000007;
        public const int errTCPConnectionReset = 0x00000008;
        public const int errTCPNotConnected = 0x00000009;
        public const int errTCPUnreachableHost = 0x00002751;

        public const int errIsoConnect = 0x00010000; // Connection error
        public const int errIsoInvalidPDU = 0x00030000; // Bad format
        public const int errIsoInvalidDataSize = 0x00040000; // Bad Datasize passed to send/recv : buffer is invalid

        public const int errCliNegotiatingPDU = 0x00100000;
        public const int errCliInvalidParams = 0x00200000;
        public const int errCliJobPending = 0x00300000;
        public const int errCliTooManyItems = 0x00400000;
        public const int errCliInvalidWordLen = 0x00500000;
        public const int errCliPartialDataWritten = 0x00600000;
        public const int errCliSizeOverPDU = 0x00700000;
        public const int errCliInvalidPlcAnswer = 0x00800000;
        public const int errCliAddressOutOfRange = 0x00900000;
        public const int errCliInvalidTransportSize = 0x00A00000;
        public const int errCliWriteDataSizeMismatch = 0x00B00000;
        public const int errCliItemNotAvailable = 0x00C00000;
        public const int errCliInvalidValue = 0x00D00000;
        public const int errCliCannotStartPLC = 0x00E00000;
        public const int errCliAlreadyRun = 0x00F00000;
        public const int errCliCannotStopPLC = 0x01000000;
        public const int errCliCannotCopyRamToRom = 0x01100000;
        public const int errCliCannotCompress = 0x01200000;
        public const int errCliAlreadyStop = 0x01300000;
        public const int errCliFunNotAvailable = 0x01400000;
        public const int errCliUploadSequenceFailed = 0x01500000;
        public const int errCliInvalidDataSizeRecvd = 0x01600000;
        public const int errCliInvalidBlockType = 0x01700000;
        public const int errCliInvalidBlockNumber = 0x01800000;
        public const int errCliInvalidBlockSize = 0x01900000;
        public const int errCliNeedPassword = 0x01D00000;
        public const int errCliInvalidPassword = 0x01E00000;
        public const int errCliNoPasswordToSetOrClear = 0x01F00000;
        public const int errCliJobTimeout = 0x02000000;
        public const int errCliPartialDataRead = 0x02100000;
        public const int errCliBufferTooSmall = 0x02200000;
        public const int errCliFunctionRefused = 0x02300000;
        public const int errCliDestroying = 0x02400000;
        public const int errCliInvalidParamNumber = 0x02500000;
        public const int errCliCannotChangeParam = 0x02600000;
        public const int errCliFunctionNotImplemented = 0x02700000;
        //------------------------------------------------------------------------------
        //        PARAMS LIST FOR COMPATIBILITY WITH Snap7.net.cs           
        //------------------------------------------------------------------------------
        public const Int32 p_u16_LocalPort = 1;  // Not applicable here
        public const Int32 p_u16_RemotePort = 2;
        public const Int32 p_i32_PingTimeout = 3;
        public const Int32 p_i32_SendTimeout = 4;
        public const Int32 p_i32_RecvTimeout = 5;
        public const Int32 p_i32_WorkInterval = 6;  // Not applicable here
        public const Int32 p_u16_SrcRef = 7;  // Not applicable here
        public const Int32 p_u16_DstRef = 8;  // Not applicable here
        public const Int32 p_u16_SrcTSap = 9;  // Not applicable here
        public const Int32 p_i32_PDURequest = 10;
        public const Int32 p_i32_MaxClients = 11; // Not applicable here
        public const Int32 p_i32_BSendTimeout = 12; // Not applicable here
        public const Int32 p_i32_BRecvTimeout = 13; // Not applicable here
        public const Int32 p_u32_RecoveryTime = 14; // Not applicable here
        public const Int32 p_u32_KeepAliveTime = 15; // Not applicable here
        // Area ID
        public const byte S7AreaPE = 0x81;
        public const byte S7AreaPA = 0x82;
        public const byte S7AreaMK = 0x83;
        public const byte S7AreaDB = 0x84;
        public const byte S7AreaCT = 0x1C;
        public const byte S7AreaTM = 0x1D;
        // Word Length
        public const int S7WLBit = 0x01;
        public const int S7WLByte = 0x02;
        public const int S7WLChar = 0x03;
        public const int S7WLWord = 0x04;
        public const int S7WLInt = 0x05;
        public const int S7WLDWord = 0x06;
        public const int S7WLDInt = 0x07;
        public const int S7WLReal = 0x08;
        public const int S7WLCounter = 0x1C;
        public const int S7WLTimer = 0x1D;
        // PLC Status
        public const int S7CpuStatusUnknown = 0x00;
        public const int S7CpuStatusRun = 0x08;
        public const int S7CpuStatusStop = 0x04;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct S7Tag
        {
            public Int32 Area;
            public Int32 DBNumber;
            public Int32 Start;
            public Int32 Elements;
            public Int32 WordLen;
        }
        #endregion
    }
    public class SPClient
    {
        private static int DefaultTimeout = 2000;
        private int _RecvTimeout = DefaultTimeout;
        private int _SendTimeout = DefaultTimeout;
        private int _ConnTimeout = DefaultTimeout;
        private int _LastError = 0;
        private int _Port;
        private string _IPAddress;
        private SPSocket Socket = null;
       
        private void CreateSocket()
        {
            try
            {
                Socket = new SPSocket();
                Socket.ConnectTimeout = _ConnTimeout;
                Socket.ReadTimeout = _RecvTimeout;
                Socket.WriteTimeout = _SendTimeout;
               
            }
            catch
            {
            }
        }
        private int TCPConnect()
        {
            if (_LastError == 0)
                try
                {
                    _LastError = Socket.Connect(IPAddress, Port);
                }
                catch
                {
                    _LastError = SPConsts.errTCPConnectionFailed;
                }
            return _LastError;
        }
        public SPClient()
        {
            CreateSocket();
        }
        ~SPClient()
        {
            Disconnect();
        }
        public int Disconnect()
        {
            if (Socket != null)
                Socket.Close();
            return 0;
        }
        public void SendBytes(byte[] Buffer)
        {
            if (Connected)
                _LastError = Socket.Send(Buffer, Buffer.Length);
            else
                _LastError = SPConsts.errTCPNotConnected;
        }
        public void ReceiveBytes(byte[] Buffer)
        {
            if (Connected)
                _LastError = Socket.Receive(Buffer, 0, Buffer.Length);
            else
                _LastError = SPConsts.errTCPNotConnected;
        }
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
            }
        }

        public string IPAddress {
            get
            {
                return _IPAddress;
            }
            set
            {
                _IPAddress = value;
            }
        }


        public int ConnTimeout
        {
            get
            {
                return _ConnTimeout;
            }
            set
            {
                _ConnTimeout = value;
            }
        }

        public int RecvTimeout
        {
            get
            {
                return _RecvTimeout;
            }
            set
            {
                _RecvTimeout = value;
            }
        }

        public int SendTimeout
        {
            get
            {
                return _SendTimeout;
            }
            set
            {
                _SendTimeout = value;
            }
        }

        public bool Connected
        {
            get
            {
                return (Socket != null) && (Socket.Connected);
            }
        }

    }
}
