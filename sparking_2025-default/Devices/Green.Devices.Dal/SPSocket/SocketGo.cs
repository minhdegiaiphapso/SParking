using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Green.Devices.Dal.SPSocket
{
    class TCPSocket
    {
        private Socket tcpSocket;
        private int _ReadTimeout = 2000;
        private int _WriteTimeout = 2000;
        private int _ConnectTimeout = 1000;
        public int LastError = 0;
        public bool ReceivedState { get; private set; } = false;
        public TCPSocket()
        {

        }
        ~TCPSocket()
        {
            Close();
           
        }
        public void Close()
        {
            if (tcpSocket != null)
            {
               
                tcpSocket.Dispose();
                tcpSocket = null;
            }
        }
        private void CreateSocket()
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.NoDelay = true;
        }
        public int Connect(string Host, int Port)
        {
            LastError = 0;
            if (!Connected)
            {
                try
                {
                    if (tcpSocket == null)
                        CreateSocket();
                    IAsyncResult result = tcpSocket.BeginConnect(Host, Port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(_ConnectTimeout, true);
                    if(!success)
                    {
                        LastError = TCPConsts.errTCPConnectionFailed;
                    }
                    else if(!Connected)
                    {
                        LastError = TCPConsts.errTCPNotConnected;
                    }
                }
                catch
                {
                    Close();
                    Thread.Sleep(500);
                    reconnect(Host, Port);
                }
            }
            return LastError;
        }
        private void reconnect(string Host, int Port)
        {
            LastError = 0;
            if (!Connected)
            {
                try
                {
                    if (tcpSocket == null)
                        CreateSocket();
                    IAsyncResult result = tcpSocket.BeginConnect(Host, Port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(_ConnectTimeout, true);
                    if (!success)
                    {
                        LastError = TCPConsts.errTCPConnectionFailed;
                    }
                    else if (!Connected)
                    {
                        LastError = TCPConsts.errTCPNotConnected;
                    }
                }
                catch
                {
                    LastError = TCPConsts.errTCPConnectionFailed;
                }
            }
        }
        public int Disconnect()
        {
            LastError = 0;
            if (Connected)
            {
                try
                {
                    tcpSocket.Shutdown(SocketShutdown.Both);
                    tcpSocket.Close();
                }
                catch
                {
                    LastError = TCPConsts.errTCPConnectionReset;
                }
            }
            return LastError;
        }    
        public bool Connected {get{ return tcpSocket != null && tcpSocket.Connected; } }
        public bool Write(byte[] buffers, int size) 
        {
            if(Connected)
            {
                LastError = 0;
                try
                {
                    var result = tcpSocket.BeginSend(buffers, 0, size, SocketFlags.None, new AsyncCallback(WriteCallback), tcpSocket);
                    var success = result.AsyncWaitHandle.WaitOne(_WriteTimeout, true);
                   
                    if (!success || LastError!=0)
                    {
                        LastError = TCPConsts.errTCPDataSend;
                       
                        return false;
                    }
                    else
                        return true;
                }
                catch
                {
                    LastError = TCPConsts.errTCPDataSend;
                    return false;
                }
            }
            else
            {
                LastError = TCPConsts.errTCPNotConnected;
                return false;
            }
        }
        public bool WriteAndStop(byte[] buffers, int size)
        {  
            if (Connected)
            {
                LastError = 0;
                try
                {
                    var result = tcpSocket.BeginSend(buffers, 0, size, SocketFlags.None, new AsyncCallback(WriteCallback), tcpSocket);
                    var success = result.AsyncWaitHandle.WaitOne(_WriteTimeout, true);

                    if (!success || LastError != 0)
                    {    
                        Disconnect();
                        LastError = TCPConsts.errTCPDataSend;
                        return false;
                    }
                    else
                    {
                        Disconnect();
                        return true;
                    }
                }
                catch
                {
                    Disconnect();
                    LastError = TCPConsts.errTCPDataSend;
                    return false;
                }
            }
            else
            {
                LastError = TCPConsts.errTCPNotConnected;
                return false;
            }
        }
        private void WriteCallback(IAsyncResult ar)
        {
            if (!(ar.AsyncState as Socket).Connected)
                LastError = SPConsts.errTCPDataSend;
        }
        public StateObject Read()
        {
            if (Connected)
            {
                LastError = 0;
                ReceivedState = false;
                try
                {
                    StateObject state = new StateObject();
                    state.workSocket = tcpSocket;
                    var result = tcpSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                    var success = result.AsyncWaitHandle.WaitOne(_ReadTimeout, true);
                    if (!success || LastError != 0)
                    {
                        LastError = TCPConsts.errTCPDataReceive;
                        return null;
                    }
                    else
                        return state;
                }
                catch
                {
                    LastError = TCPConsts.errTCPDataReceive;
                    return null;
                }
            }
            else
            {
                LastError = TCPConsts.errTCPNotConnected;
                return null;
            }
        }
        public StateObject ReadAndStop()
        {
            if (Connected)
            {
                LastError = 0;
                ReceivedState = false;
                try
                {
                    StateObject state = new StateObject();
                    state.workSocket = tcpSocket;
                    var result = tcpSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                    var success = result.AsyncWaitHandle.WaitOne(_ReadTimeout, true);
                    if (!success || LastError != 0)
                    {
                        Disconnect();
                        LastError = TCPConsts.errTCPDataReceive;
                        return null;
                    }
                    else
                    {
                        Disconnect();
                        return state;
                    }
                }
                catch
                {
                    Disconnect();
                    LastError = TCPConsts.errTCPDataReceive;
                    return null;
                }
            }
            else
            {
                LastError = TCPConsts.errTCPNotConnected;
                return null;
            }
        }
        private void ReadCallback(IAsyncResult ar)
        {
            //StateObject state = (StateObject)ar.AsyncState;
            //Socket handler = state.workSocket;
            //try
            //{
            //    //if (!handler.Connected)
            //    //{
            //    //    ReceivedState = true;
            //    //}
            //    //else if (handler.EndReceive(ar) == 0)
            //    //{

            //    //}
            //    ReceivedState = true;
            //    handler.Shutdown(SocketShutdown.Both);
            //    handler.Close();
            //}
            //catch
            //{; }
        }
        public StateObject TransactionGo(byte[] buffers, int size)
        {
            if(Write(buffers,size))
            {
                return Read();
            }
            else
            {
                return null;
            }
        }
        public StateObject TransactionGoAndStop(byte[] buffers, int size)
        {
            if (Write(buffers, size))
            {
                return ReadAndStop();
            }
            else
            {
                return null;
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
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        public int ReceivedBytes = 0;
    }
    public static class TCPConsts
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
    public class SocketGo
    {
        private static int DefaultTimeout = 2000;
        private int _RecvTimeout = DefaultTimeout;
        private int _SendTimeout = DefaultTimeout;
        private int _ConnTimeout = DefaultTimeout;
        private int _LastError = 0;
        private int _Port;
        private string _IPAddress;
        private TCPSocket Socket = null;
        private void CreateSocket()
        {
            try
            {
                Socket = new TCPSocket();
                Socket.ConnectTimeout = _ConnTimeout;
                Socket.ReadTimeout = _RecvTimeout;
                Socket.WriteTimeout = _SendTimeout;

            }
            catch
            {
            }
        }
        public SocketGo()
        {
            CreateSocket();
        }
        ~SocketGo()
        {
            DisConnect();
        }
        public int TCPConnect()
        {
            if (!Connected)
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
        public StateObject TransactionGo(byte[] cmd)
        {
            if(Connected)
            {
                return Socket.TransactionGo(cmd, cmd.Length);
            }
            else
            {
                _LastError = TCPConsts.errTCPNotConnected;
                return null;
            }
        }
        public StateObject TransactionGoAndStop(byte[] cmd)
        {
            if (Connected)
            {
                return Socket.TransactionGoAndStop(cmd, cmd.Length);
            }
            else
            {
                _LastError = TCPConsts.errTCPNotConnected;
                return null;
            }
        }
        public bool Write(byte[] cmd)
        {
            if (Connected)
            {
                return Socket.Write(cmd, cmd.Length);
            }
            else
            {
                _LastError = TCPConsts.errTCPNotConnected;
                return false;
            }
        }
        public bool WriteAndStop(byte[] cmd)
        {
            if (Connected)
            {
                return Socket.WriteAndStop(cmd, cmd.Length);
            }
            else
            {
                _LastError = TCPConsts.errTCPNotConnected;
                return false;
            }
        }
        public StateObject Read()
        {
            if (Connected)
            {
                return Socket.Read();
            }
            else
            {
                _LastError = TCPConsts.errTCPNotConnected;
                return null;
            }
        }
        public StateObject ReadAndStop()
        {
            if (Connected)
            {
                return Socket.ReadAndStop();
            }
            else
            {
                _LastError = TCPConsts.errTCPNotConnected;
                return null;
            }
        }
        public void DisConnect()
        {
            if (Socket.Connected)
                Socket.Disconnect();
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
        public string IPAddress
        {
            get
            {
                return _IPAddress;
            }
            set
            {
                _IPAddress = value;
            }
        }
        public int LastError { get { return _LastError; } }
        public int ConnTimeout
        {
            get
            {
                return _ConnTimeout;
            }
            set
            {
                _ConnTimeout = value;
                if (Socket != null)
                    Socket.ConnectTimeout = _ConnTimeout;
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
                if (Socket != null)
                    Socket.ReadTimeout = _RecvTimeout;
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
                if (Socket != null)
                    Socket.WriteTimeout = _RecvTimeout;
            }
        }
        public bool Connected
        {
            get
            {
                return (Socket != null) && (Socket.Connected);
            }
        }
        public bool ReceivedState { get { return Socket != null && Socket.ReceivedState; } } 
    }
}
