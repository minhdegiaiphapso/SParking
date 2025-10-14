

using MR6100Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Green.Devices.Dal.CardControler;
using System.Management;
using Green.Devices.Dal.SPSocket;

namespace Green.Devices.Dal
{
    public class GreenCardReaderInfo : IGreenCardReaderInfo
    {
        /// <summary>
        /// Type can be 'ModWinsCard', 'Tcp Ip Client', 'Tcp Ip Server', 'Remode Card'
        /// </summary>
        public string Type { get; set; }
        public string CallName { get; set; }
        /// <summary>
        /// Using when Type ="ModWinsCard"
        /// </summary>
        public string DeviceName { get; set; }
        public string SerialNumber { get; set; }
        /// <summary>
        /// Using when Type in ['Tcp Ip Client', 'Tcp Ip Server']
        /// </summary>
        public string TcpIp { get; set; }
        public ushort Port { get; set; }
        public byte ActiveCode { get; set; }
        public byte InactiveCode { get; set; }
        public bool IsReset { get; set; }
        public int TimeReset { get; set; }
        public string Antenna { get; set; }
        public string Reader { get; set; }
		public bool UsageAsTheSameFarCard { get; set; }
	}
    public class TcpIpRemodeCardReader : IGreenCardReader
    {
        MR6100Api.MR6100Api Api = new MR6100Api.MR6100Api();
        private const int BUFF_SIZE = 32;
        private int cur = 0;
        private DateTime connectTime = DateTime.Now;
        public string CurrentCardID { get; private set; }
        public TcpIpRemodeCardReader(IGreenCardReaderInfo info)
        {
            this._info = info;
            this._ID_old = string.Empty;
            this.State = CardState.IsDisable;
            this.CurrentCardID = string.Empty;
            this._state = CardState.IsDisable;

            _task = null;
        }

        private Task _task;
        private string _ID_old;
        private IGreenCardReaderInfo _info;
        public IGreenCardReaderInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (_info == null)
                {
                    _info = new GreenCardReaderInfo()
                    {
                        Type = "TRemode Card",
                        TcpIp = "",
                        Port = 80
                    };
                    this.State = CardState.IsDisable;
                }
            }
        }
        private CardState _state;
        public CardState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this._state = value;
            }
        }
        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;
        public bool Connect()
        {
            var reConnect = 5;
            bool b = false;
            while (reConnect > 0 && this._state != CardState.IsReady)
            {
                if (Api.isNetWorkConnect(Info.TcpIp))
                {
                    int status = Api.TcpConnectReader(Info.TcpIp, Info.Port);
                    if (status == MR6100Api.MR6100Api.SUCCESS_RETURN)
                    {
                        //reConnect = 0;
                        State = CardState.IsReady;
                        b = true;
                        connectTime = DateTime.Now;
                        this.Info.SerialNumber = GetSerialNumber();
                        Run();
                    }
                    else
                        reConnect--;
                }
                else
                    reConnect--;
            }
            if (reConnect <= 0)
                State = CardState.IsDisable;
            return b;
        }
        public string GetSerialNumber()
        {
            string serialNo = string.Empty;
            int status = Api.GetSerialNo(255, ref serialNo);
            return serialNo;
        }
        private int lastReadTime = 0;
        private int _Status = 0;
        public async void ReadingThread()
        {
            while (this._state == CardState.IsReady)
            {
                DateTime now = DateTime.Now;
                if (connectTime <= now)
                {
                    connectTime = now.AddMilliseconds(10);
                    var getCardIdTask = Task.Factory.StartNew<string>(() => { return GetCardIdBlockFunc(); });
                    string result = await getCardIdTask;

                    if (ReadingCompleted != null)
                    {
                        ReadingCompleted(this, new GreenCardReaderEventArgs { CardID = result, CardReader = this });

                    }
                    //var curr = Environment.TickCount;
                    //int chk = curr - lastReadTime;
                    //if (curr - lastReadTime > 3000)
                    //{
                    //    lastReadTime = curr;
                    //    if (ReadingCompleted != null)
                    //        ReadingCompleted(this, new GreenCardReaderEventArgs { CardID = result, CardReader = this });
                    //}
                    //Console.WriteLine(result);
                }
            }
        }
        public string GetCardIdBlockFunc()
        {
            string cardId = string.Empty;
            while (true)
            {
                cardId = GetCardInfo();
                if (!string.IsNullOrEmpty(cardId))
                    return cardId;

                else if (_Status == 2009)
                {
                    //TakingOffCompleted(this, new CardReaderEventArgs { CardReader = this });
                    //ProlificCardReaderInfo info = CardReaderInfo as ProlificCardReaderInfo;
                    if (Info != null)
                    {
                        _Status = Api.TcpConnectReader(Info.TcpIp, Info.Port);
                        if (_Status == MR6100Api.MR6100Api.SUCCESS_RETURN)
                        {
                            if (TakingOffCompleted != null)
                                TakingOffCompleted(this, new GreenCardReaderEventArgs { CardReader = this });
                            continue;
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                }

                System.Threading.Thread.Sleep(10);
            }
        }
        public string GetCardInfo()
        {
            byte tag_flag = 0;
            byte[,] tagData = new byte[500, 14];
            int tagCount = 0;
            tagCount = 0;
            Api.ClearIdBuf(255);
            System.Threading.Thread.Sleep(10);

            _Status = Api.EpcMultiTagIdentify(255, ref tagData, ref tagCount, ref tag_flag);

            if (_Status == MR6100Api.MR6100Api.SUCCESS_RETURN)
            {
                if (tagCount > 1) tagCount = 1;

                string strAnteNo = "", strID = "", strTemp = "";
                for (int i = 0; i < tagCount; i++)
                {
                    int j = 0;
                    strID = "";
                    strAnteNo = string.Format("{0:X2}", tagData[i, 1]);
                    for (j = 2; j < 14; j++) // update: 0->2, 12->14
                    {
                        strTemp = string.Format("{0:X2}", tagData[i, j]);
                        strID += strTemp;
                    }
                    if (strID == "000000000000000000000000")
                        continue;

                    return strID;
                }
            }

            return null;
        }

        private bool _isReading;

        public void Run()
        {
            if (!_isReading)
            {
                _isReading = true;
                _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
            }
        }
        public void DisConnect()
        {

        }
        public Object GetController()
        {
            return null;
        }
    }
    public class TcpIpServerCardReader_OLD : IGreenCardReader
    {
        private const int BUFF_SIZE = 35;
        private int cur = 0;
        private DateTime connectTime = DateTime.Now;
        private DateTime waitefrom = DateTime.Now;
        public string CurrentCardID { get; private set; }
        public TcpIpServerCardReader_OLD(IGreenCardReaderInfo info)
        {
            this._info = info;
            this._info.IsReset = true;
            this._info.TimeReset = 18;
            this._ID_old = string.Empty;
            this.State = CardState.IsDisable;
            this.CurrentCardID = string.Empty;
            this._state = CardState.IsDisable;
        }
        private Stream _stream;
        private TcpClient _client;
        private Task _task;
        private string _ID_old;
        private IGreenCardReaderInfo _info;
        public IGreenCardReaderInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (_info == null)
                {
                    _info = new GreenCardReaderInfo()
                    {
                        Type = "Tcp Ip Server",
                        TcpIp = "",
                        Port = 80
                    };
                    this.State = CardState.IsDisable;
                    //SemacV14.Define.CommandType.GetBF50CardNo;
                }
            }
        }
        private CardState _state;
        public CardState State
        {
            get
            {
                return this._state;
            }
            set
            {
                if (this._state != CardState.IsReady)
                    this._state = CardState.IsDisable;
            }
        }
        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;
        public bool Connect()
        {
            try
            {
                if (_client == null)
                {
                    _client = new TcpClient();
                    _client.Connect(Info.TcpIp, Info.Port);
                    if (_client.Connected)
                    {
                        _stream = _client.GetStream();
                        _ID_old = "";
                        this._state = CardState.IsReady;
                        connectTime = DateTime.Now;
                        _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
                        return true;
                    }
                    else
                    {
                        DisConnect();
                        return false;
                    }
                }
                else
                {
                    if (_client.Connected && this._state == CardState.IsReady)
                    {
                        return true;
                    }
                    else
                    {
                        DisConnect();
                        _client.Connect(Info.TcpIp, Info.Port);
                        if (_client.Connected)
                        {
                            _stream = _client.GetStream();
                            _ID_old = "";
                            this._state = CardState.IsReady;
                            connectTime = DateTime.Now;
                            _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
                            return true;
                        }
                        else
                        {
                            DisConnect();
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DisConnect();
                return false;
            }
        }
        private void ReadingThread()
        {
            while (_client != null && _client.Connected && this._state == CardState.IsReady)
            {
                DateTime now = DateTime.Now;
                if (connectTime <= now)
                {
                    connectTime = now.AddMilliseconds(100);
                    try
                    {

                        byte[] rev = new byte[BUFF_SIZE];
                        try
                        {
                            if (_stream.CanRead)
                            {
                                _stream.Read(rev, 0, BUFF_SIZE);
                                if (rev[0] != 2 || rev[33] != 13 || rev[34] != 10)
                                    continue;
                            }
                            else
                                continue;
                        }
                        catch (Exception e)
                        {
                            DisConnect();
                            continue;
                        }
                        string CardID = "";
                        //byte[] r1 = { 2, 67, 56, 48, 68, 48, 48, 69, 50, 48, 48, 53, 48, 50, 52, 57, 56, 48, 56, 48, 49, 57, 52, 48, 54, 48, 48, 68, 52, 57, 70, 69, 70, 10, 13 };
                        byte[] r = new byte[24];
                        Array.Copy(rev, 7, r, 0, 24);
                        byte[] SendBuff = new byte[12];
                        for (int i = 0; i < 24; i++)
                        {
                            if (r[i] >= 48 && r[i] <= 57) r[i] -= 0x30;
                            if (r[i] == 65) r[i] = 0x0A;
                            if (r[i] == 66) r[i] = 0x0B;
                            if (r[i] == 67) r[i] = 0x0C;
                            if (r[i] == 68) r[i] = 0x0D;
                            if (r[i] == 69) r[i] = 0x0E;
                            if (r[i] == 70) r[i] = 0x0F;
                        }
                        for (int i = 0; i < 12; i++)
                        {
                            SendBuff[i] = Convert.ToByte(r[2 * i] * 0x10 | r[2 * i + 1]);
                        }

                        for (int i = 0; i < SendBuff.Length; i++)
                        {
                            CardID += string.Format("{0:x2}", SendBuff[i]).ToUpper();
                        }

                        if (CardID != "000000000000000000000000")// && CardID.Substring(0,5)=="E2005")
                        {
                            this.CurrentCardID = CardID;
                            if (CurrentCardID != _ID_old && CurrentCardID != "")
                            {
                                _ID_old = CardID;
                                waitefrom = DateTime.Now;
                                //Task.Factory.StartNew(() => SfactorsCardreaderLog.Log(
                                //    new CardInfoLog()
                                //    {
                                //        CardId = CardID,
                                //        CardType = "TCPIPServer",
                                //        TimeReceived = waitefrom.ToString("dd-MM-yyy HH:mm:ss"),
                                //        Message = "Đọc mới"
                                //    }));
                                this.Info.IsReset = false;
                                if (ReadingCompleted != null)
                                {
                                    ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CardID, CardReader = this });

                                }
                                if (TakingOffCompleted != null)
                                {
                                    TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                }
                            }
                            else
                            {
                                if ((this.Info.IsReset || (DateTime.Now - waitefrom).Seconds >= this.Info.TimeReset) && CurrentCardID != "")
                                {
                                    _ID_old = CardID;
                                    waitefrom = DateTime.Now;
                                    this.Info.IsReset = false;
                                    //Task.Factory.StartNew(() => SfactorsCardreaderLog.Log(
                                    //new CardInfoLog()
                                    //{
                                    //    CardId = CardID,
                                    //    CardType = "TCPIPServer",
                                    //    TimeReceived = waitefrom.ToString("dd-MM-yyy HH:mm:ss"),
                                    //    Message = "Đọc lại"
                                    //}));
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CardID, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                            }
                        }
                        else
                        {
                            var id = CardID;
                        }
                    }
                    catch (Exception e)
                    {
                        DisConnect();
                    }
                }
            }
        }
        public void DisConnect()
        {
            this._state = CardState.IsDisable;
            this._info.IsReset = true;
            if (_stream != null)
                _stream.Dispose();
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }
        public Object GetController()
        {
            return null;
        }
    }
    public class TcpIpClientCardReader_OLD : IGreenCardReader
    {
        private const int BUFF_SIZE = 1024;
        private int cur = 0;
        private DateTime connectTime = DateTime.Now;
        private DateTime waitefrom = DateTime.Now;
        // private Socket socket;
        public string CurrentCardID { get; private set; }
        public TcpIpClientCardReader_OLD(IGreenCardReaderInfo info)
        {
            this._info = info;
            this._info.IsReset = true;
            this._info.TimeReset = 18;
            this._ID_old = string.Empty;
            this.State = CardState.IsDisable;
            this.CurrentCardID = string.Empty;
            this._state = CardState.IsDisable;
            //socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
        }
        private NetworkStream _stream;
        private TcpClient _client;
        private Task _task;
        private string _ID_old;
        private IGreenCardReaderInfo _info;
        public IGreenCardReaderInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (_info == null)
                {
                    _info = new GreenCardReaderInfo()
                    {
                        Type = "Tcp Ip Client",
                        TcpIp = "",
                        Port = 80
                    };
                    this.State = CardState.IsDisable;
                    //SemacV14.Define.CommandType.GetBF50CardNo;
                }
            }
        }
        private CardState _state;
        public CardState State
        {
            get
            {
                return this._state;
            }
            set
            {
                if (this._state != CardState.IsReady)
                    this._state = CardState.IsDisable;
            }
        }
        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;

        public bool Connect()
        {
            return false;
            //try
            //{
            //    DisConnect();

            //    _client = new TcpClient();
            //    _client.Connect(Info.TcpIp, Info.Port);
            //    if (_client.Connected)
            //    {
            //        _stream = _client.GetStream();
            //        _ID_old = "";
            //        this._state = CardState.IsReady;
            //        connectTime = DateTime.Now;
            //        //if (_task == null)
            //        //    _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
            //        //else
            //        //    if (_task.Status != TaskStatus.Running)
            //        //    _task.Start();
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //catch
            //{
            //    //DisConnect();
            //    return false;
            //}

        }
        private bool IsDoing = false;
        private void ReadingThread_OLD()
        {
            while (true)
            {
                if (_client != null && _client.Connected && this._state == CardState.IsReady)
                {

                    DateTime now = DateTime.Now;
                    if (!IsDoing && connectTime <= now)
                    {
                        IsDoing = true;
                        connectTime = now.AddMilliseconds(100);
                        try
                        {
                            byte[] cmd = { 0x7e, 0x05, 0x01, 0x31, 0x13, 0xdc, 0x21 };
                            byte[] rev = new byte[BUFF_SIZE];
                            byte xor = 0xFF;
                            byte sum = 0x00;
                            try
                            {
                                _stream.Write(cmd, 0, cmd.Length);

                                Thread.Sleep(300);
                                if (_stream.CanRead)
                                {
                                    int c = 0;
                                    c = _stream.Read(rev, 0, BUFF_SIZE);
                                    if (c == 0)
                                    {
                                        IsDoing = false;
                                        continue;
                                    }
                                }
                                else
                                {
                                    Connect();
                                    IsDoing = false;
                                    continue;
                                }

                            }
                            catch (Exception e)
                            {
                                _stream.Write(cmd, 0, cmd.Length);
                                Thread.Sleep(300);
                                IsDoing = false;
                                continue;
                            }
                            int length = rev[1];
                            if (rev[0] != 126 || length < 10 || (int)(rev[length - 1]) != 2)
                            {
                                IsDoing = false;
                                continue;
                            }
                            for (int i = 2; i < length; i++)
                            {
                                xor ^= rev[i];
                                sum += rev[i];
                            }
                            sum += xor;
                            if (xor == rev[length] && sum == rev[length + 1])
                            {
                                IsDoing = false;
                                string id1 = string.Format("{0:x}", rev[length - 2]);
                                if (id1.Length < 2)
                                    id1 = "0" + id1;
                                string id2 = string.Format("{0:x}", rev[length - 3]);
                                if (id2.Length < 2)
                                    id2 = "0" + id2;
                                string id3 = string.Format("{0:x}", rev[length - 4]);
                                if (id3.Length < 2)
                                    id3 = "0" + id3;
                                string id4 = string.Format("{0:x}", rev[length - 5]);
                                if (id4.Length < 2)
                                    id4 = "0" + id4;
                                string CardID = string.Format("{0}{1}{2}{3}", id1, id2, id3, id4);
                                if (CardID.Contains("00000000") || "00000000000000000000000000000000".Contains(CardID))
                                    CardID = string.Empty;
                                this.CurrentCardID = CardID;
                                if (CurrentCardID != "")
                                {
                                    _ID_old = CardID;
                                    waitefrom = DateTime.Now;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CardID, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }

                                }
                            }
                            else if (rev[14] != 0 && rev[13] != 0)
                            //else if (rev[15] != 0 && rev[16] != 0)
                            {
                                _ID_old = "";
                                IsDoing = false;
                            }
                            IsDoing = false;
                        }
                        catch (Exception e)
                        {
                            Connect();
                            IsDoing = false;
                            continue;
                        }
                    }
                }
                else
                {
                    Connect();
                    IsDoing = false;
                }
            }
        }
        private string read()
        {
            string ID = string.Empty;
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(this._info.TcpIp, this._info.Port);
                if (client.Connected)
                {
                    this.State = CardState.IsReady;
                    byte[] cmd = { 0x7e, 0x05, 0x01, 0x31, 0x13, 0xdc, 0x21 };
                    byte[] rev = new byte[BUFF_SIZE];
                    byte xor = 0xFF;
                    byte sum = 0x00;
                    client.GetStream().Write(cmd, 0, cmd.Length);
                    Thread.Sleep(300);
                    client.GetStream().Read(rev, 0, BUFF_SIZE);
                    client.Close();

                    client = null;
                    int length = rev[1];
                    if (rev[0] != 126 || length < 10 || (int)(rev[length - 1]) != 2)
                    {
                        return string.Empty;
                    }
                    for (int i = 2; i < length; i++)
                    {
                        xor ^= rev[i];
                        sum += rev[i];
                    }
                    sum += xor;
                    if (xor == rev[length] && sum == rev[length + 1])
                    {
                        IsDoing = false;
                        string id1 = string.Format("{0:x}", rev[length - 2]);
                        if (id1.Length < 2)
                            id1 = "0" + id1;
                        string id2 = string.Format("{0:x}", rev[length - 3]);
                        if (id2.Length < 2)
                            id2 = "0" + id2;
                        string id3 = string.Format("{0:x}", rev[length - 4]);
                        if (id3.Length < 2)
                            id3 = "0" + id3;
                        string id4 = string.Format("{0:x}", rev[length - 5]);
                        if (id4.Length < 2)
                            id4 = "0" + id4;
                        string CardID = string.Format("{0}{1}{2}{3}", id1, id2, id3, id4);
                        if (CardID.Contains("00000000") || "00000000000000000000000000000000".Contains(CardID))
                            CardID = string.Empty;
                        this.CurrentCardID = CardID;
                        return CardID;
                    }
                    else if (rev[14] != 0 && rev[13] != 0)
                    //else if (rev[15] != 0 && rev[16] != 0)
                    {
                        _ID_old = "";
                    }
                    return "";
                }
                else
                {
                    this.State = CardState.IsDisable;
                    return "";
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        private void ReadingThread()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                if (!IsDoing && connectTime <= now)
                {
                    connectTime = now.AddMilliseconds(300);
                    IsDoing = true;
                    Task.Run(() =>
                    {
                        CurrentCardID = read();
                    });
                    if (CurrentCardID != "" && CurrentCardID != _ID_old)
                    {
                        _ID_old = CurrentCardID;
                        waitefrom = DateTime.Now;
                        this.Info.IsReset = false;
                        if (ReadingCompleted != null)
                        {
                            ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CurrentCardID, CardReader = this });

                        }
                        if (TakingOffCompleted != null)
                        {
                            TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                        }

                    }
                    else
                    {
                        if (CurrentCardID != "" && (DateTime.Now - waitefrom).TotalSeconds >= 3)
                        {
                            _ID_old = CurrentCardID;
                            waitefrom = DateTime.Now;
                            this.Info.IsReset = false;
                            if (ReadingCompleted != null)
                            {
                                ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CurrentCardID, CardReader = this });

                            }
                            if (TakingOffCompleted != null)
                            {
                                TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                            }
                        }
                    }
                    IsDoing = false;
                }
                IsDoing = false;
            }
        }

        public void DisConnect()
        {
            //this._state = CardState.IsDisable;
            //this._info.IsReset = true;
            //if (_stream != null)
            //{
            //    _stream.Close();
            //    _stream.Dispose();
            //}
            //if (_client != null)
            //{
            //    _client.Close();
            //    _client = null;
            //}

        }
        public Object GetController()
        {
            return null;
        }
    }
    public class ModWinsCardReader_OLD : IGreenCardReader
    {
        private int _context;
        private int _cardHandle;
        private bool _canRead;
        private DateTime connectTime = DateTime.Now;
        private int _activeProtocol;
        SCARD_IO_REQUEST _sendRequest = new SCARD_IO_REQUEST();
        SCARD_IO_REQUEST _recvRequest = new SCARD_IO_REQUEST();
        byte[] _recvBuff;
        byte[] _sendBuff;
        Task _task;
        public ModWinsCardReader_OLD(IGreenCardReaderInfo info)
        {
            this._info = new GreenCardReaderInfo()
            {
                Type = "ModWinsCard",
                SerialNumber = info.SerialNumber
            };
            _cardHandle = -1;
            _activeProtocol = -1;
            _recvBuff = new byte[128];
            _sendBuff = new byte[128];
            this._state = CardState.IsDisable;
            GetReady();
            ReleaseContext();
        }
        private void ReleaseContext()
        {
            int retCode = ModWinsCard.SCardCancel(_context);
            //if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            //    Console.WriteLine("Cancel failed");

            retCode = ModWinsCard.SCardReleaseContext(_context);
            //if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            //    Console.WriteLine("Release failed");
        }
        public static List<string> ListModWinsCards()
        {
            int _context = -1;
            List<string> lst = new List<string>();
            int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                return null;
            }
            if (_context == -1)
            {
                return null;
            }
            int readerCount = 255;

            Byte[] bytes = new Byte[readerCount];

            retCode = ModWinsCard.SCardListReaders(_context, null, bytes, ref readerCount);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                return null;
            }

            try
            {
                string[] readerArr = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 0, readerCount).Split('\0');
                foreach (string readerName in readerArr)
                {
                    if (!string.IsNullOrEmpty(readerName) && readerName.Length > 1)
                    {
                        // http://stackoverflow.com/questions/6940824/getting-pcsc-reader-serial-number-with-winscard
                        int readerHandle = 0;

                        int protocol = 0;
                        int ret = ModWinsCard.SCardConnect(_context, readerName, ModWinsCard.SCARD_SHARE_DIRECT, ModWinsCard.SCARD_PROTOCOL_UNDEFINED, ref readerHandle, ref protocol);

                        byte[] data = new byte[128];
                        int leng = 128;
                        ret = ModWinsCard.SCardGetAttrib(readerHandle, ModWinsCard.SCARD_ATTR_VENDOR_IFD_SERIAL_NO, data, ref leng);

                        string serialNo = System.Text.ASCIIEncoding.ASCII.GetString(data, 0, leng);

                        //int b = ModWinsCard.SCardFreeMemory(_context, data);

                        ModWinsCard.SCardDisconnect(readerHandle, ModWinsCard.SCARD_LEAVE_CARD);

                        lst.Add(serialNo);
                    }
                }
            }
            catch
            {
                return null;
            }
            return lst;
        }
        private void GetReady()
        {
            int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                this._canRead = false;
                return;
            }
            if (_context == -1)
            {
                this._canRead = false;
                return;
            }
            int readerCount = 255;

            Byte[] bytes = new Byte[readerCount];

            retCode = ModWinsCard.SCardListReaders(_context, null, bytes, ref readerCount);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                this._canRead = false;
                return;
            }

            try
            {
                string[] readerArr = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 0, readerCount).Split('\0');
                foreach (string readerName in readerArr)
                {
                    GetSerialNumber(readerName, (res, b) =>
                    {
                        if (b && res == this._info.SerialNumber)
                        {
                            this._info.DeviceName = readerName;
                            this._canRead = true;
                            return;
                        }
                    });
                }
            }
            catch
            {
                this._canRead = false;
                return;
            }

        }
        private void GetSerialNumber(string ModWinsDeviceName, Action<string, bool> complete)
        {
            if (!string.IsNullOrEmpty(ModWinsDeviceName) && ModWinsDeviceName.Length > 1)
            {
                // http://stackoverflow.com/questions/6940824/getting-pcsc-reader-serial-number-with-winscard
                int readerHandle = 0;

                int protocol = 0;
                int ret = ModWinsCard.SCardConnect(_context, ModWinsDeviceName, ModWinsCard.SCARD_SHARE_DIRECT, ModWinsCard.SCARD_PROTOCOL_UNDEFINED, ref readerHandle, ref protocol);

                byte[] data = new byte[128];
                int leng = 128;
                ret = ModWinsCard.SCardGetAttrib(readerHandle, ModWinsCard.SCARD_ATTR_VENDOR_IFD_SERIAL_NO, data, ref leng);

                string serialNo = System.Text.ASCIIEncoding.ASCII.GetString(data, 0, leng);

                //int b = ModWinsCard.SCardFreeMemory(_context, data);

                ModWinsCard.SCardDisconnect(readerHandle, ModWinsCard.SCARD_LEAVE_CARD);

                if (complete != null)
                    if (ret != ModWinsCard.SCARD_S_SUCCESS)
                        complete(string.Empty, false);
                    else
                        complete(serialNo, true);
            }
            else
                complete(string.Empty, false);
        }
        private IGreenCardReaderInfo _info;
        public IGreenCardReaderInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (this._info == null)
                {
                    this._info = new GreenCardReaderInfo()
                    {
                        Type = "ModWinsCard",
                        DeviceName = string.Empty,
                        SerialNumber = string.Empty
                    };
                    this._state = CardState.IsDisable;
                }
            }
        }
        private CardState _state;
        public CardState State
        {
            get
            {
                return this._state;
            }
            set
            {
                if (this._state != CardState.IsReady)
                    this._state = CardState.IsDisable;
            }
        }

        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;
        public bool Connect()
        {
            try
            {
                if (_canRead)
                {
                    this._state = CardState.IsReady;
                    _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
                    return true;
                }
                else
                {
                    GetReady();
                    Thread.Sleep(1000);
                    if (_canRead)
                    {
                        this._state = CardState.IsReady;
                        _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
                    }
                    return true;
                }
            }
            catch
            {
                this._state = CardState.IsDisable;
                int retCode = ModWinsCard.SCardCancel(_context);
                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    Console.WriteLine(string.Format("{0} cancel failed", this._info.SerialNumber));
                retCode = ModWinsCard.SCardReleaseContext(_context);
                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    Console.WriteLine(string.Format("{0} release failed", this._info.SerialNumber));
                return false;
            }
        }
        private void ReadingThread()
        {

            try
            {
                SCARD_READERSTATE readerState;
                readerState.RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
                readerState.RdrEventState = ModWinsCard.SCARD_STATE_UNKNOWN;
                readerState.UserData = new IntPtr(0);
                readerState.ATRLength = 0;
                readerState.ATRValue = new byte[36];
                readerState.RdrName = this._info.DeviceName;
                while (true)
                {
                    if (_canRead && _state == CardState.IsReady)
                    {
                        DateTime tmp = DateTime.Now;
                        if (connectTime <= tmp)
                        {
                            connectTime = tmp.AddMilliseconds(2);
                            try
                            {
                                int retCode = ModWinsCard.SCardGetStatusChange(_context, ModWinsCard.INFINITE, ref readerState, 1);

                                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                                {
                                    readerState.RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
                                    readerState.RdrEventState = ModWinsCard.SCARD_STATE_UNKNOWN;
                                    readerState.UserData = new IntPtr(0);
                                    readerState.ATRLength = 0;
                                    readerState.ATRValue = new byte[36];
                                    readerState.RdrName = this._info.DeviceName;

                                    ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
                                    Thread.Sleep(1000);

                                    //ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this, ex = new Exception("Reading failed") });
                                    continue;
                                }

                                if ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_CHANGED) == ModWinsCard.SCARD_STATE_CHANGED)
                                {
                                    if ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_EMPTY) == ModWinsCard.SCARD_STATE_EMPTY)
                                    {
                                        if (TakingOffCompleted != null)
                                            TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                    else if (((readerState.RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) == ModWinsCard.SCARD_STATE_PRESENT)
                                        && ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) != (readerState.RdrCurrState & ModWinsCard.SCARD_STATE_PRESENT)))
                                    {
                                        GetCardId();
                                        //GetCardIdLisa();
                                    }
                                }

                                readerState.RdrCurrState = readerState.RdrEventState;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("BUGGG: {0}", ex.Message);
                                this.DisConnect();
                                GetReady();
                                Thread.Sleep(1000);
                                this.Connect();
                            }

                        }
                    }
                    else
                    {
                        this.DisConnect();
                        GetReady();
                        Thread.Sleep(1000);
                        this.Connect();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("BUGGG: {0}", exception.Message);
                this.DisConnect();
                GetReady();
                System.Threading.Thread.Sleep(1000);
                this.Connect();
            }

        }
        public void DisConnect()
        {
            this._state = CardState.IsDisable;
            int retCode = ModWinsCard.SCardCancel(_context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                Console.WriteLine(string.Format("{0} cancel failed", this._info.SerialNumber));
            retCode = ModWinsCard.SCardReleaseContext(_context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                Console.WriteLine(string.Format("{0} release failed", this._info.SerialNumber));
        }
        private int ConnectCard()
        {
            return ModWinsCard.SCardConnect(_context, this._info.DeviceName, ModWinsCard.SCARD_SHARE_SHARED, ModWinsCard.SCARD_PROTOCOL_T0 | ModWinsCard.SCARD_PROTOCOL_T1, ref _cardHandle, ref _activeProtocol);
        }
        #region Felica
        public string IdDm { get; set; }
        private string GetCardIdFelica()
        {
            if (ConnectCard() != ModWinsCard.SCARD_S_SUCCESS)
            {
                return string.Empty;
            }

            Array.Clear(_sendBuff, 0, _sendBuff.Length);
            Array.Clear(_recvBuff, 0, _recvBuff.Length);
            int sendBuffLen = 0x0B;
            int RecvBuffLen = 0x2D;
            string CodeData = "FF46010206CB1880008001";
            OpcodeConv(CodeData);
            _sendRequest.dwProtocol = _activeProtocol;
            _sendRequest.cbPciLength = Marshal.SizeOf(_sendRequest);
            _recvRequest.dwProtocol = _activeProtocol;
            _recvRequest.cbPciLength = Marshal.SizeOf(_recvRequest);

            int retCode = ModWinsCard.SCardTransmit(_cardHandle, ref _sendRequest, ref _sendBuff[0], sendBuffLen, ref _recvRequest, ref _recvBuff[0], ref RecvBuffLen);

            string sCardID = string.Empty;
            for (int i = 0; i < RecvBuffLen - 2; i++)
                sCardID = sCardID + String.Format("{0:X2}", _recvBuff[i]);

            string StrDateTime = "";
            if (!string.IsNullOrWhiteSpace(sCardID))
            {
                this.IdDm = !string.IsNullOrWhiteSpace(sCardID) ? sCardID.Substring(0, 16) : "";
                string TimeRide = "";
                if (sCardID.Length > 85)
                {
                    TimeRide = !string.IsNullOrWhiteSpace(sCardID) ? ConvertHex(sCardID.Substring(54, 32)) : "";
                    //"2017011610:47:03"

                    //dd/MM/yyyy HH:mm:ss
                    StrDateTime = string.Format("{0}/{1}/{2} {3}", TimeRide.Substring(6, 2), TimeRide.Substring(4, 2), TimeRide.Substring(0, 4), TimeRide.Substring(8, 8));
                }
                //sCardID = "zxcvbnmasd";
                if (sCardID.Length < 16)
                    sCardID = string.Empty;
                else
                    sCardID = sCardID.Substring(0, 16);
                //sCardID = ConvertHex(sCardID.Substring(22, 32));
            }
            if (ReadingCompleted != null)
                ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = sCardID, TimeRide = StrDateTime, CardReader = this });

            // Disconnect card after reading completed
            retCode = ModWinsCard.SCardDisconnect(_cardHandle, ModWinsCard.SCARD_LEAVE_CARD);

            return sCardID;
        }
        private void OpcodeConv(String opcode)
        {
            Byte[] toBytes = Encoding.ASCII.GetBytes(opcode);
            for (int i = 0; i < opcode.Length; i++)
            {
                switch (toBytes[i])
                {
                    case 65:
                        toBytes[i] = (Byte)0x0A;
                        break;
                    case 97:
                        toBytes[i] = (Byte)0x0A;
                        break;
                    case 66:
                        toBytes[i] = (Byte)0x0B;
                        break;
                    case 98:
                        toBytes[i] = (Byte)0x0B;
                        break;
                    case 67:
                        toBytes[i] = (Byte)0x0C;
                        break;
                    case 99:
                        toBytes[i] = (Byte)0x0C;
                        break;
                    case 68:
                        toBytes[i] = (Byte)0x0D;
                        break;
                    case 100:
                        toBytes[i] = (Byte)0x0D;
                        break;
                    case 69:
                        toBytes[i] = (Byte)0x0E;
                        break;
                    case 101:
                        toBytes[i] = (Byte)0x0E;
                        break;
                    case 70:
                        toBytes[i] = (Byte)0x0F;
                        break;
                    case 102:
                        toBytes[i] = (Byte)0x0F;
                        break;
                    default:
                        toBytes[i] -= (Byte)0x30;
                        break;
                }
            }
            for (Byte i = 0; i < opcode.Length / 2; i++)
            {
                _sendBuff[i] = Convert.ToByte(toBytes[2 * i] * 0x10 | toBytes[2 * i + 1]);
            }
        }
        public string ConvertHex(String hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }
        #endregion
        private string GetCardId()
        {
            lock (this)
            {
                _recvBuff = new byte[128];
                _sendBuff = new byte[128];
                if (ConnectCard() != ModWinsCard.SCARD_S_SUCCESS)
                {
                    return string.Empty;
                }
                int RecvBuffLen = 0x6;
                Array.Clear(_sendBuff, 0, _sendBuff.Length);

                _sendBuff[0] = 0xFF;      //CLA
                _sendBuff[1] = 0xCA;      //P1 : Same for all source type
                _sendBuff[2] = 0x0;       //INS : for stored key input
                _sendBuff[3] = 0x0;       //P2  : for stored key input
                _sendBuff[4] = 0x0;          //P3  : for stored key input
                int sendBuffLen = 0x5;

                _sendRequest.dwProtocol = _activeProtocol;
                _sendRequest.cbPciLength = Marshal.SizeOf(_sendRequest);

                _recvRequest.dwProtocol = _activeProtocol;
                _recvRequest.cbPciLength = Marshal.SizeOf(_recvRequest);

                int retCode = ModWinsCard.SCardTransmit(_cardHandle, ref _sendRequest, ref _sendBuff[0], sendBuffLen, ref _recvRequest, ref _recvBuff[0], ref RecvBuffLen);

                string sCardID = string.Empty;
                for (int i = 0; i < RecvBuffLen - 2; i++)
                {
                    sCardID = sCardID + String.Format("{0:X2}", _recvBuff[i]);
                }
                if (string.IsNullOrEmpty(sCardID) || sCardID.Contains("0000000") || "00000000000000000000000000000000".Contains(sCardID))
                    //return GetCardIdFelica();
                    sCardID = string.Empty;
                if (ReadingCompleted != null)
                    ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = sCardID, CardReader = this });
                // Disconnect card after reading completed
                retCode = ModWinsCard.SCardDisconnect(_cardHandle, ModWinsCard.SCARD_LEAVE_CARD);
                return sCardID;
            }
        }
        public Object GetController()
        {
            return null;
        }
    }
	public class TcpIpServerCardReader : IGreenCardReader
	{
		private const int BUFF_SIZE = 35;
		private int cur = 0;
		private DateTime connectTime = DateTime.Now;
		private DateTime waitefrom = DateTime.Now;
		public string CurrentCardID { get; private set; }
		public TcpIpServerCardReader(IGreenCardReaderInfo info)
		{
			this._info = info;
			this._info.IsReset = true;
			this._info.TimeReset = 18;
			this._ID_old = string.Empty;
			this.State = CardState.IsDisable;
			this.CurrentCardID = string.Empty;
			this._state = CardState.IsDisable;
			Connect();
		}
		~TcpIpServerCardReader()
		{
			DisConnect();
			if (_client != null)
				_client.DisConnect();
		}
		private SocketGo _client;
		private Task _task;
		private string _ID_old;
		private IGreenCardReaderInfo _info;
		public IGreenCardReaderInfo Info
		{
			get
			{
				return _info;
			}
			set
			{
				if (value == null)
				{
					_info = new GreenCardReaderInfo()
					{
						Type = "Tcp Ip Server",
						TcpIp = "",
						Port = 80
					};
					this.State = CardState.IsDisable;
					//SemacV14.Define.CommandType.GetBF50CardNo;
				}
				else
				{
					_info = new GreenCardReaderInfo()
					{
						Type = "Tcp Ip Server",
						TcpIp = value.TcpIp,
						CallName = value.CallName,
						Port = value.Port
					};
					if (_client != null)
					{
						_client.IPAddress = value.TcpIp;
						_client.Port = value.Port;

					}
				}
			}
		}
		private CardState _state;
		public CardState State
		{
			get
			{
				return this._state;
			}
			set
			{
				this._state = value;
			}
		}
		public event GreenCardReaderEventHandler ReadingCompleted;
		public event GreenCardReaderEventHandler TakingOffCompleted;
		public bool Connect()
		{
			if (State != CardState.IsReady)
			{
				State = CardState.IsReady;
				connectTime = DateTime.Now;
				_task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
			}
			return true;
		}
		private void ReadingThread()
		{
			while (State == CardState.IsReady)
			{
				DateTime now = DateTime.Now;
				if (connectTime <= now)
				{
					connectTime = now.AddMilliseconds(200);
					try
					{
						byte[] rev = new byte[BUFF_SIZE];
						if (_client == null)
							_client = new SocketGo() { IPAddress = this._info.TcpIp, Port = this._info.Port };
						_client.TCPConnect();
						if (_client.Connected)
						{
							var obj = _client.ReadAndStop();
							if (obj != null)
							{
								Array.Copy(obj.buffer, 0, rev, 0, BUFF_SIZE);
								if (rev[0] != 2 || rev[33] != 13 || rev[34] != 10)
									continue;
								string CardID = "";
								byte[] r = new byte[24];
								Array.Copy(rev, 7, r, 0, 24);
								byte[] SendBuff = new byte[12];
								for (int i = 0; i < 24; i++)
								{
									if (r[i] >= 48 && r[i] <= 57) r[i] -= 0x30;
									if (r[i] == 65) r[i] = 0x0A;
									if (r[i] == 66) r[i] = 0x0B;
									if (r[i] == 67) r[i] = 0x0C;
									if (r[i] == 68) r[i] = 0x0D;
									if (r[i] == 69) r[i] = 0x0E;
									if (r[i] == 70) r[i] = 0x0F;
								}
								for (int i = 0; i < 12; i++)
								{
									SendBuff[i] = Convert.ToByte(r[2 * i] * 0x10 | r[2 * i + 1]);
								}

								for (int i = 0; i < SendBuff.Length; i++)
								{
									CardID += string.Format("{0:x2}", SendBuff[i]).ToUpper();
								}

								if (CardID != "000000000000000000000000")
								{
									this.CurrentCardID = CardID;
									if (CurrentCardID != _ID_old && CurrentCardID != "")
									{
										_ID_old = CardID;
										waitefrom = DateTime.Now;

										this.Info.IsReset = false;
										if (ReadingCompleted != null)
										{
											ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CardID, CardReader = this });

										}
										if (TakingOffCompleted != null)
										{
											TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
										}
									}
									else
									{
										if ((this.Info.IsReset || (DateTime.Now - waitefrom).Seconds >= this.Info.TimeReset) && CurrentCardID != "")
										{
											_ID_old = CardID;
											waitefrom = DateTime.Now;
											this.Info.IsReset = false;
											if (ReadingCompleted != null)
											{
												ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CardID, CardReader = this });

											}
											if (TakingOffCompleted != null)
											{
												TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
											}
										}
									}
								}
								else
								{
									var id = CardID;
								}
							}
						}
						else
						{
							State = CardState.IsDisable;
							continue;
						}
					}
					catch
					{
						State = CardState.IsDisable;
					}

				}
				else
				{
					Thread.Sleep(200);
				}
			}
		}
		public void DisConnect()
		{
			State = CardState.IsDisable;
		}
		public Object GetController()
		{
			return null;
		}
	}
	public class TcpIpClientCardReader : IGreenCardReader
	{
		private const int BUFF_SIZE = 1024;
		private int cur = 0;
		private DateTime connectTime = DateTime.Now;
		private DateTime waitefrom = DateTime.Now;
		public string CurrentCardID { get; private set; }
		public TcpIpClientCardReader(IGreenCardReaderInfo info)
		{
			this._info = info;
			this._info.IsReset = true;
			this._info.TimeReset = 18;
			this._ID_old = string.Empty;
			this.State = CardState.IsDisable;
			this.CurrentCardID = string.Empty;
			this._state = CardState.IsDisable;
			Connect();
		}
		~TcpIpClientCardReader()
		{
			DisConnect();
		}
		private SocketGo _client;
		private Task _task;
		private string _ID_old;
		private IGreenCardReaderInfo _info;
		public IGreenCardReaderInfo Info
		{
			get
			{
				return _info;
			}
			set
			{
				if (value == null)
				{
					_info = new GreenCardReaderInfo()
					{
						Type = "Tcp Ip Client",
						TcpIp = "",
						Port = 80
					};
					this.State = CardState.IsDisable;
					//SemacV14.Define.CommandType.GetBF50CardNo;
				}
				else
				{
					_info = new GreenCardReaderInfo()
					{
						Type = "Tcp Ip Client",
						TcpIp = value.TcpIp,
						CallName = value.CallName,
						Port = value.Port
					};
					if (_client != null)
					{
						_client.IPAddress = value.TcpIp;
						_client.Port = value.Port;
					}
				}
			}
		}
		private CardState _state;
		public CardState State
		{
			get
			{
				return this._state;
			}
			set
			{
				this._state = value;
			}
		}
		public event GreenCardReaderEventHandler ReadingCompleted;
		public event GreenCardReaderEventHandler TakingOffCompleted;

		public bool Connect()
		{
			if (State != CardState.IsReady)
			{
				State = CardState.IsReady;
				connectTime = DateTime.Now;
				_task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
			}
			return true;
		}
		private bool IsDoing = false;

		private string read()
		{
			string ID = string.Empty;
			try
			{
				if (_client == null)
					_client = new SocketGo() { IPAddress = this._info.TcpIp, Port = this._info.Port };
				_client.TCPConnect();
				if (_client.Connected)
				{
					this.State = CardState.IsReady;
					byte[] cmd = { 0x7e, 0x05, 0x01, 0x31, 0x13, 0xdc, 0x21 };
					byte[] rev = new byte[BUFF_SIZE];
					byte xor = 0xFF;
					byte sum = 0x00;
					var obj = _client.TransactionGoAndStop(cmd);
					if (obj != null)
					{
						Array.Copy(obj.buffer, 0, rev, 0, BUFF_SIZE);

						int length = rev[1];
						if (rev[0] != 126 || length < 10 || (int)(rev[length - 1]) != 2)
						{
							return string.Empty;
						}
						for (int i = 2; i < length; i++)
						{
							xor ^= rev[i];
							sum += rev[i];
						}
						sum += xor;
						if (xor == rev[length] && sum == rev[length + 1])
						{
							IsDoing = false;
							string id1 = string.Format("{0:x}", rev[length - 2]);
							if (id1.Length < 2)
								id1 = "0" + id1;
							string id2 = string.Format("{0:x}", rev[length - 3]);
							if (id2.Length < 2)
								id2 = "0" + id2;
							string id3 = string.Format("{0:x}", rev[length - 4]);
							if (id3.Length < 2)
								id3 = "0" + id3;
							string id4 = string.Format("{0:x}", rev[length - 5]);
							if (id4.Length < 2)
								id4 = "0" + id4;
							string CardID = string.Format("{0}{1}{2}{3}", id1, id2, id3, id4);
							if (CardID.Contains("00000000") || "00000000000000000000000000000000".Contains(CardID))
								CardID = string.Empty;
							this.CurrentCardID = CardID;
							return CardID;
						}
						else if (rev[14] != 0 && rev[13] != 0)
						{
							_ID_old = "";
						}
						return "";
					}
					return "";
				}
				else
				{
					this.State = CardState.IsDisable;
					return "";
				}
			}
			catch (Exception e)
			{
				this.State = CardState.IsDisable;
				return string.Empty;
			}
		}
		private void ReadingThread()
		{
			while (State == CardState.IsReady)
			{
				DateTime now = DateTime.Now;
				if (!IsDoing && connectTime <= now)
				{
					connectTime = now.AddMilliseconds(300);
					IsDoing = true;
					CurrentCardID = read();
					if (CurrentCardID != "" && CurrentCardID != _ID_old)
					{
						_ID_old = CurrentCardID;
						waitefrom = DateTime.Now;
						this.Info.IsReset = false;
						if (ReadingCompleted != null)
						{
							ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CurrentCardID, CardReader = this });

						}
						if (TakingOffCompleted != null)
						{
							TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
						}

					}
					else
					{
						if (CurrentCardID != "" && (DateTime.Now - waitefrom).TotalSeconds >= 3)
						{
							_ID_old = CurrentCardID;
							waitefrom = DateTime.Now;
							this.Info.IsReset = false;
							if (ReadingCompleted != null)
							{
								ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = CurrentCardID, CardReader = this });

							}
							if (TakingOffCompleted != null)
							{
								TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
							}
						}
					}
					IsDoing = false;
				}
				else
				{
					Thread.Sleep(300);
					IsDoing = false;
				}

			}
		}
		public void DisConnect()
		{

			State = CardState.IsDisable;
			if (_client != null)
				_client.DisConnect();
		}
		public Object GetController()
		{
			return null;
		}
	}
	public class ModWinsCardReader : IGreenCardReader
	{
		private int _context;
		private int _cardHandle;
		private bool _canRead;
		private DateTime connectTime = DateTime.Now;
		private int _activeProtocol;
		SCARD_IO_REQUEST _sendRequest = new SCARD_IO_REQUEST();
		SCARD_IO_REQUEST _recvRequest = new SCARD_IO_REQUEST();
		byte[] _recvBuff;
		byte[] _sendBuff;
		Task _task;
		public ModWinsCardReader(IGreenCardReaderInfo info)
		{
			this._info = new GreenCardReaderInfo()
			{
				Type = "ModWinsCard",
				SerialNumber = info.SerialNumber,
				CallName = info.CallName,
			};
			_cardHandle = -1;
			_activeProtocol = -1;
			_recvBuff = new byte[128];
			_sendBuff = new byte[128];
			this._state = CardState.IsDisable;
			GetReady();
			ReleaseContext();
		}
		private void ReleaseContext()
		{
			int retCode = ModWinsCard.SCardCancel(_context);
			//if (retCode != ModWinsCard.SCARD_S_SUCCESS)
			//    Console.WriteLine("Cancel failed");

			retCode = ModWinsCard.SCardReleaseContext(_context);
			//if (retCode != ModWinsCard.SCARD_S_SUCCESS)
			//    Console.WriteLine("Release failed");
		}
		public static List<string> ListModWinsCards()
		{
			int _context = -1;
			List<string> lst = new List<string>();
			int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
			if (retCode != ModWinsCard.SCARD_S_SUCCESS)
			{
				return null;
			}
			if (_context == -1)
			{
				return null;
			}
			int readerCount = 255;

			Byte[] bytes = new Byte[readerCount];

			retCode = ModWinsCard.SCardListReaders(_context, null, bytes, ref readerCount);
			if (retCode != ModWinsCard.SCARD_S_SUCCESS)
			{
				return null;
			}

			try
			{
				string[] readerArr = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 0, readerCount).Split('\0');
				foreach (string readerName in readerArr)
				{
					if (!string.IsNullOrEmpty(readerName) && readerName.Length > 1)
					{
						// http://stackoverflow.com/questions/6940824/getting-pcsc-reader-serial-number-with-winscard
						int readerHandle = 0;

						int protocol = 0;
						int ret = ModWinsCard.SCardConnect(_context, readerName, ModWinsCard.SCARD_SHARE_DIRECT, ModWinsCard.SCARD_PROTOCOL_UNDEFINED, ref readerHandle, ref protocol);

						byte[] data = new byte[128];
						int leng = 128;
						ret = ModWinsCard.SCardGetAttrib(readerHandle, ModWinsCard.SCARD_ATTR_VENDOR_IFD_SERIAL_NO, data, ref leng);

						string serialNo = System.Text.ASCIIEncoding.ASCII.GetString(data, 0, leng);

						//int b = ModWinsCard.SCardFreeMemory(_context, data);

						ModWinsCard.SCardDisconnect(readerHandle, ModWinsCard.SCARD_LEAVE_CARD);

						lst.Add(serialNo);
					}
				}
			}
			catch
			{
				return null;
			}
			return lst;
		}
		private void GetReady()
		{
			int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
			if (retCode != ModWinsCard.SCARD_S_SUCCESS)
			{
				this._canRead = false;
				return;
			}
			if (_context == -1)
			{
				this._canRead = false;
				return;
			}
			int readerCount = 255;

			Byte[] bytes = new Byte[readerCount];

			retCode = ModWinsCard.SCardListReaders(_context, null, bytes, ref readerCount);
			if (retCode != ModWinsCard.SCARD_S_SUCCESS)
			{
				this._canRead = false;
				return;
			}

			try
			{
				string[] readerArr = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 0, readerCount).Split('\0');
				foreach (string readerName in readerArr)
				{
					GetSerialNumber(readerName, (res, b) =>
					{
						if (b && res == this._info.SerialNumber)
						{
							this._info.DeviceName = readerName;
							this._canRead = true;
							return;
						}
					});
				}
			}
			catch
			{
				this._canRead = false;
				return;
			}

		}
		private void GetSerialNumber(string ModWinsDeviceName, Action<string, bool> complete)
		{
			if (!string.IsNullOrEmpty(ModWinsDeviceName) && ModWinsDeviceName.Length > 1)
			{
				// http://stackoverflow.com/questions/6940824/getting-pcsc-reader-serial-number-with-winscard
				int readerHandle = 0;

				int protocol = 0;
				int ret = ModWinsCard.SCardConnect(_context, ModWinsDeviceName, ModWinsCard.SCARD_SHARE_DIRECT, ModWinsCard.SCARD_PROTOCOL_UNDEFINED, ref readerHandle, ref protocol);

				byte[] data = new byte[128];
				int leng = 128;
				ret = ModWinsCard.SCardGetAttrib(readerHandle, ModWinsCard.SCARD_ATTR_VENDOR_IFD_SERIAL_NO, data, ref leng);

				string serialNo = System.Text.ASCIIEncoding.ASCII.GetString(data, 0, leng);

				//int b = ModWinsCard.SCardFreeMemory(_context, data);

				ModWinsCard.SCardDisconnect(readerHandle, ModWinsCard.SCARD_LEAVE_CARD);

				if (complete != null)
					if (ret != ModWinsCard.SCARD_S_SUCCESS)
						complete(string.Empty, false);
					else
						complete(serialNo, true);
			}
			else
				complete(string.Empty, false);
		}
		private IGreenCardReaderInfo _info;
		public IGreenCardReaderInfo Info
		{
			get
			{
				return _info;
			}
			set
			{
				if (value == null)
				{
					this._info = new GreenCardReaderInfo()
					{
						Type = "ModWinsCard",
						DeviceName = string.Empty,
						SerialNumber = string.Empty
					};
					this._state = CardState.IsDisable;
				}
				else
				{
					this._info = value;
					this._info.Type = "ModWinsCard";
				}
			}
		}
		private CardState _state;
		public CardState State
		{
			get
			{
				return this._state;
			}
			set
			{
				if (this._state != CardState.IsReady)
					this._state = CardState.IsDisable;
			}
		}

		public event GreenCardReaderEventHandler ReadingCompleted;
		public event GreenCardReaderEventHandler TakingOffCompleted;
		public bool Connect()
		{
			try
			{
				if (_canRead)
				{
					this._state = CardState.IsReady;
					_task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
					return true;
				}
				else
				{
					GetReady();
					Thread.Sleep(1000);
					if (_canRead)
					{
						this._state = CardState.IsReady;
						_task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
					}
					return true;
				}
			}
			catch
			{
				this._state = CardState.IsDisable;
				int retCode = ModWinsCard.SCardCancel(_context);
				if (retCode != ModWinsCard.SCARD_S_SUCCESS)
					Console.WriteLine(string.Format("{0} cancel failed", this._info.SerialNumber));
				retCode = ModWinsCard.SCardReleaseContext(_context);
				if (retCode != ModWinsCard.SCARD_S_SUCCESS)
					Console.WriteLine(string.Format("{0} release failed", this._info.SerialNumber));
				return false;
			}
		}
		private void ReadingThread()
		{

			try
			{
				SCARD_READERSTATE readerState;
				readerState.RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
				readerState.RdrEventState = ModWinsCard.SCARD_STATE_UNKNOWN;
				readerState.UserData = new IntPtr(0);
				readerState.ATRLength = 0;
				readerState.ATRValue = new byte[36];
				readerState.RdrName = this._info.DeviceName;
				while (true)
				{
					if (_canRead && _state == CardState.IsReady)
					{
						DateTime tmp = DateTime.Now;
						if (connectTime <= tmp)
						{
							connectTime = tmp.AddMilliseconds(2);
							try
							{
								int retCode = ModWinsCard.SCardGetStatusChange(_context, ModWinsCard.INFINITE, ref readerState, 1);

								if (retCode != ModWinsCard.SCARD_S_SUCCESS)
								{
									readerState.RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
									readerState.RdrEventState = ModWinsCard.SCARD_STATE_UNKNOWN;
									readerState.UserData = new IntPtr(0);
									readerState.ATRLength = 0;
									readerState.ATRValue = new byte[36];
									readerState.RdrName = this._info.DeviceName;

									ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
									Thread.Sleep(1000);

									//ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this, ex = new Exception("Reading failed") });
									continue;
								}

								if ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_CHANGED) == ModWinsCard.SCARD_STATE_CHANGED)
								{
									if ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_EMPTY) == ModWinsCard.SCARD_STATE_EMPTY)
									{
										if (TakingOffCompleted != null)
											TakingOffCompleted(this, new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
									}
									else if (((readerState.RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) == ModWinsCard.SCARD_STATE_PRESENT)
										&& ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) != (readerState.RdrCurrState & ModWinsCard.SCARD_STATE_PRESENT)))
									{
										GetCardId();
										//GetCardIdLisa();
									}
								}

								readerState.RdrCurrState = readerState.RdrEventState;
							}
							catch (Exception ex)
							{
								Console.WriteLine("BUGGG: {0}", ex.Message);
								this.DisConnect();
								GetReady();
								Thread.Sleep(1000);
								this.Connect();
							}

						}
					}
					else
					{
						this.DisConnect();
						GetReady();
						Thread.Sleep(1000);
						this.Connect();
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("BUGGG: {0}", exception.Message);
				this.DisConnect();
				GetReady();
				System.Threading.Thread.Sleep(1000);
				this.Connect();
			}

		}
		public void DisConnect()
		{
			this._state = CardState.IsDisable;
			int retCode = ModWinsCard.SCardCancel(_context);
			if (retCode != ModWinsCard.SCARD_S_SUCCESS)
				Console.WriteLine(string.Format("{0} cancel failed", this._info.SerialNumber));
			retCode = ModWinsCard.SCardReleaseContext(_context);
			if (retCode != ModWinsCard.SCARD_S_SUCCESS)
				Console.WriteLine(string.Format("{0} release failed", this._info.SerialNumber));
		}
		private int ConnectCard()
		{
			return ModWinsCard.SCardConnect(_context, this._info.DeviceName, ModWinsCard.SCARD_SHARE_SHARED, ModWinsCard.SCARD_PROTOCOL_T0 | ModWinsCard.SCARD_PROTOCOL_T1, ref _cardHandle, ref _activeProtocol);
		}
		#region Felica
		public string IdDm { get; set; }
		private string GetCardIdFelica()
		{
			if (ConnectCard() != ModWinsCard.SCARD_S_SUCCESS)
			{
				return string.Empty;
			}

			Array.Clear(_sendBuff, 0, _sendBuff.Length);
			Array.Clear(_recvBuff, 0, _recvBuff.Length);
			int sendBuffLen = 0x0B;
			int RecvBuffLen = 0x2D;
			string CodeData = "FF46010206CB1880008001";
			OpcodeConv(CodeData);
			_sendRequest.dwProtocol = _activeProtocol;
			_sendRequest.cbPciLength = Marshal.SizeOf(_sendRequest);
			_recvRequest.dwProtocol = _activeProtocol;
			_recvRequest.cbPciLength = Marshal.SizeOf(_recvRequest);

			int retCode = ModWinsCard.SCardTransmit(_cardHandle, ref _sendRequest, ref _sendBuff[0], sendBuffLen, ref _recvRequest, ref _recvBuff[0], ref RecvBuffLen);

			string sCardID = string.Empty;
			for (int i = 0; i < RecvBuffLen - 2; i++)
				sCardID = sCardID + String.Format("{0:X2}", _recvBuff[i]);

			string StrDateTime = "";
			if (!string.IsNullOrWhiteSpace(sCardID))
			{
				this.IdDm = !string.IsNullOrWhiteSpace(sCardID) ? sCardID.Substring(0, 16) : "";
				string TimeRide = "";
				if (sCardID.Length > 85)
				{
					TimeRide = !string.IsNullOrWhiteSpace(sCardID) ? ConvertHex(sCardID.Substring(54, 32)) : "";
					//"2017011610:47:03"

					//dd/MM/yyyy HH:mm:ss
					StrDateTime = string.Format("{0}/{1}/{2} {3}", TimeRide.Substring(6, 2), TimeRide.Substring(4, 2), TimeRide.Substring(0, 4), TimeRide.Substring(8, 8));
				}
				//sCardID = "zxcvbnmasd";
				if (sCardID.Length < 16)
					sCardID = string.Empty;
				else
					sCardID = sCardID.Substring(0, 16);
				//sCardID = ConvertHex(sCardID.Substring(22, 32));
			}
			if (ReadingCompleted != null)
				ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = sCardID, TimeRide = StrDateTime, CardReader = this });

			// Disconnect card after reading completed
			retCode = ModWinsCard.SCardDisconnect(_cardHandle, ModWinsCard.SCARD_LEAVE_CARD);

			return sCardID;
		}
		private void OpcodeConv(String opcode)
		{
			Byte[] toBytes = Encoding.ASCII.GetBytes(opcode);
			for (int i = 0; i < opcode.Length; i++)
			{
				switch (toBytes[i])
				{
					case 65:
						toBytes[i] = (Byte)0x0A;
						break;
					case 97:
						toBytes[i] = (Byte)0x0A;
						break;
					case 66:
						toBytes[i] = (Byte)0x0B;
						break;
					case 98:
						toBytes[i] = (Byte)0x0B;
						break;
					case 67:
						toBytes[i] = (Byte)0x0C;
						break;
					case 99:
						toBytes[i] = (Byte)0x0C;
						break;
					case 68:
						toBytes[i] = (Byte)0x0D;
						break;
					case 100:
						toBytes[i] = (Byte)0x0D;
						break;
					case 69:
						toBytes[i] = (Byte)0x0E;
						break;
					case 101:
						toBytes[i] = (Byte)0x0E;
						break;
					case 70:
						toBytes[i] = (Byte)0x0F;
						break;
					case 102:
						toBytes[i] = (Byte)0x0F;
						break;
					default:
						toBytes[i] -= (Byte)0x30;
						break;
				}
			}
			for (Byte i = 0; i < opcode.Length / 2; i++)
			{
				_sendBuff[i] = Convert.ToByte(toBytes[2 * i] * 0x10 | toBytes[2 * i + 1]);
			}
		}
		public string ConvertHex(String hexString)
		{
			try
			{
				string ascii = string.Empty;

				for (int i = 0; i < hexString.Length; i += 2)
				{
					String hs = string.Empty;

					hs = hexString.Substring(i, 2);
					uint decval = System.Convert.ToUInt32(hs, 16);
					char character = System.Convert.ToChar(decval);
					ascii += character;

				}

				return ascii;
			}
			catch (Exception ex) { Console.WriteLine(ex.Message); }

			return string.Empty;
		}
		#endregion
		private string GetCardId()
		{
			lock (this)
			{
				_recvBuff = new byte[128];
				_sendBuff = new byte[128];
				if (ConnectCard() != ModWinsCard.SCARD_S_SUCCESS)
				{
					return string.Empty;
				}
				int RecvBuffLen = 0x6;
				Array.Clear(_sendBuff, 0, _sendBuff.Length);

				_sendBuff[0] = 0xFF;      //CLA
				_sendBuff[1] = 0xCA;      //P1 : Same for all source type
				_sendBuff[2] = 0x0;       //INS : for stored key input
				_sendBuff[3] = 0x0;       //P2  : for stored key input
				_sendBuff[4] = 0x0;          //P3  : for stored key input
				int sendBuffLen = 0x5;

				_sendRequest.dwProtocol = _activeProtocol;
				_sendRequest.cbPciLength = Marshal.SizeOf(_sendRequest);

				_recvRequest.dwProtocol = _activeProtocol;
				_recvRequest.cbPciLength = Marshal.SizeOf(_recvRequest);

				int retCode = ModWinsCard.SCardTransmit(_cardHandle, ref _sendRequest, ref _sendBuff[0], sendBuffLen, ref _recvRequest, ref _recvBuff[0], ref RecvBuffLen);

				string sCardID = string.Empty;
				for (int i = 0; i < RecvBuffLen - 2; i++)
				{
					sCardID = sCardID + String.Format("{0:X2}", _recvBuff[i]);
				}
				if (string.IsNullOrEmpty(sCardID) || sCardID.Contains("0000000") || "00000000000000000000000000000000".Contains(sCardID))
					//return GetCardIdFelica();
					sCardID = string.Empty;
				if (ReadingCompleted != null)
					ReadingCompleted(this, new GreenCardReaderEventArgs() { CardID = sCardID, CardReader = this });
				// Disconnect card after reading completed
				retCode = ModWinsCard.SCardDisconnect(_cardHandle, ModWinsCard.SCARD_LEAVE_CARD);
				return sCardID;
			}
		}
		public Object GetController()
		{
			return null;
		}
	}
	public class TcpIpCardController : IGreenCardReader
    {
        public TcpIpCardController(IGreenCardReaderInfo info)
        {
            this._info = info;
            this.State = CardState.IsDisable;
            ctr = GreenTcpIpControllerSingleton.GetInstance(info.TcpIp, info.Port);
        }
        private GreenTcpIpControllerSingleton ctr = null;

        private IGreenCardReaderInfo _info;
        public IGreenCardReaderInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (_info == null)
                {
                    _info = new GreenCardReaderInfo()
                    {
                        Type = "Tcp Ip Controller",
                        TcpIp = "",
                        Port = 80
                    };
                    this.State = CardState.IsDisable;
                }
            }
        }
        private CardState _state;
        public CardState State
        {
            get { return _state; }
            set
            {
                if (value != CardState.IsReady)
                    _state = CardState.IsDisable;
                else
                    _state = CardState.IsReady;
            }
        }

        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;

        public bool Connect()
        {
            bool b = ctr.Connect(this.Info.TcpIp, this.Info.Port);
            if (b)
            {
                State = CardState.IsReady;
            }
            else
            {
                State = CardState.IsDisable;
            }
            return b;

        }

        public void DisConnect()
        {

            State = CardState.IsDisable;
        }
        public Object GetController()
        {
            return ctr[this.Info.TcpIp, this.Info.Port];
        }
    }
    public class ScannelCardReader : IGreenCardReader
    {
        private const int BUFF_SIZE = 1024;
        private IGreenCardReaderInfo _info;
        public IGreenCardReaderInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (_info == null)
                {
                    _info = new GreenCardReaderInfo()
                    {
                        Type = "Scannel",
                        TcpIp = "",
                        Port = 80
                    };
                    this.State = CardState.IsDisable;
                    //SemacV14.Define.CommandType.GetBF50CardNo;
                }
            }
        }
        private CardState _state;
        public CardState State
        {
            get
            {
                return this._state;
            }
            set
            {
                if (this._state != CardState.IsReady)
                    this._state = CardState.IsDisable;
            }
        }

        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;
        private ScannelInfo Antenna_old1;
        private ScannelInfo Antenna_old2;
        private ScannelInfo Antenna_old3;
        private ScannelInfo Antenna_old4;
        private bool IsDoing = false;
        DateTime connectTime;
        private NetworkStream _stream;
        private TcpClient _client;
        private Task _task;
        public ScannelCardReader(IGreenCardReaderInfo info)
        {
            this._info = info;
            this._info.Type = "Scannel";
            this._info.IsReset = true;
            this._info.TimeReset = 18;
            this.State = CardState.IsDisable;
            Antenna_old1 = new ScannelInfo { CardId = string.Empty, Anntenna = "1", TimeReceived = DateTime.Now };
            Antenna_old2 = new ScannelInfo { CardId = string.Empty, Anntenna = "2", TimeReceived = DateTime.Now };
            Antenna_old3 = new ScannelInfo { CardId = string.Empty, Anntenna = "3", TimeReceived = DateTime.Now };
            Antenna_old4 = new ScannelInfo { CardId = string.Empty, Anntenna = "4", TimeReceived = DateTime.Now };
            connectTime = DateTime.Now;
            //_task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
        }


        private ScannelInfo GetId(string data)
        {
            try
            {
                var Results = data.Split('*');
                var mystr = Results[2].Substring(0, 19);
                var timeReceived = DateTime.ParseExact(mystr, "yyyy-MM-dd HH:mm:ss", null);
                return new ScannelInfo { CardId = Results[0], Anntenna = Results[1], TimeReceived = timeReceived };
            }
            catch (Exception e)
            {
                tasking = false;
                return null;
            }
        }
        private bool tasking = false;
        ScannelInfo CurrentInfo = null;
        private int count = 0;
        private void ReadingThread()
        {
            while (_client != null && _client.Connected && this._state == CardState.IsReady)
            {
                DateTime now = DateTime.Now;

                if (!IsDoing && connectTime <= now)
                {
                    IsDoing = true;
                    connectTime = now.AddMilliseconds(300);
                    try
                    {

                        byte[] rev = new byte[BUFF_SIZE];
                        try
                        {
                            if (_stream.CanRead)
                            {
                                var data_size = _stream.Read(rev, 0, BUFF_SIZE);
                                string data = string.Empty;
                                if (data_size > 0)
                                    data = System.Text.Encoding.ASCII.GetString(rev, 0, BUFF_SIZE);
                                else
                                {
                                    IsDoing = false;
                                    count++;
                                    if (count > 50)
                                        DisConnect();
                                    continue;
                                }
                                if (string.IsNullOrEmpty(data))
                                {
                                    IsDoing = false;
                                    continue;
                                }
                                else
                                {
                                    CurrentInfo = GetId(data);
                                }
                            }
                            else
                            {
                                IsDoing = false;
                                continue;
                            }

                        }
                        catch (Exception e)
                        {
                            DisConnect();
                            IsDoing = false;
                            continue;
                        }


                    }
                    catch (Exception e)
                    {
                        DisConnect();
                        IsDoing = false;
                        continue;
                    }
                    if (CurrentInfo != null)
                    {
                        switch (CurrentInfo.Anntenna)
                        {
                            case "1":
                                if (CurrentInfo.CardId != Antenna_old1.CardId && !string.IsNullOrEmpty(CurrentInfo.CardId))
                                {
                                    Antenna_old1.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;

                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("1", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("1", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                else if (!string.IsNullOrEmpty(CurrentInfo.CardId) && (this.Info.IsReset || (CurrentInfo.TimeReceived - Antenna_old1.TimeReceived).TotalSeconds >= Info.TimeReset))
                                {

                                    Antenna_old1.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("1", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("1", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                break;
                            case "2":
                                if (CurrentInfo.CardId != Antenna_old2.CardId && !string.IsNullOrEmpty(CurrentInfo.CardId))
                                {

                                    Antenna_old2.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("2", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("2", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                else if (!string.IsNullOrEmpty(CurrentInfo.CardId) && (this.Info.IsReset || (CurrentInfo.TimeReceived - Antenna_old2.TimeReceived).TotalSeconds >= Info.TimeReset))
                                {

                                    Antenna_old2.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("2", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });
                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("2", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                break;
                            case "3":
                                if (CurrentInfo.CardId != Antenna_old3.CardId && !string.IsNullOrEmpty(CurrentInfo.CardId))
                                {

                                    Antenna_old3.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("3", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("3", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                else if (!string.IsNullOrEmpty(CurrentInfo.CardId) && (this.Info.IsReset || (CurrentInfo.TimeReceived - Antenna_old3.TimeReceived).TotalSeconds >= Info.TimeReset))
                                {

                                    Antenna_old3.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("3", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });
                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("3", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                break;
                            case "4":
                                if (CurrentInfo.CardId != Antenna_old4.CardId && !string.IsNullOrEmpty(CurrentInfo.CardId))
                                {

                                    Antenna_old4.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("4", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });

                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("4", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                else if (!string.IsNullOrEmpty(CurrentInfo.CardId) && (this.Info.IsReset || (CurrentInfo.TimeReceived - Antenna_old4.TimeReceived).TotalSeconds >= Info.TimeReset))
                                {

                                    Antenna_old4.TimeReceived = CurrentInfo.TimeReceived;
                                    this.Info.IsReset = false;
                                    if (ReadingCompleted != null)
                                    {
                                        ReadingCompleted("4", new GreenCardReaderEventArgs() { CardID = CurrentInfo.CardId, CardReader = this });
                                    }
                                    if (TakingOffCompleted != null)
                                    {
                                        TakingOffCompleted("4", new GreenCardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                                    }
                                }
                                break;
                            default:
                                Thread.Sleep(300);
                                break;

                        }
                        tasking = false;
                    }
                    IsDoing = false;
                }
                IsDoing = false;
            }
        }
        public bool Connect()
        {
            try
            {
                if (_client == null)
                {
                    _client = new TcpClient();
                    _client.Connect(Info.TcpIp, Info.Port);
                    if (_client.Connected)
                    {
                        _stream = _client.GetStream();
                        CurrentInfo = null;
                        this._state = CardState.IsReady;
                        connectTime = DateTime.Now;
                        _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
                        count = 0;
                        return true;
                    }
                    else
                    {
                        DisConnect();
                        return false;
                    }
                }
                else
                {
                    if (_client.Connected && this._state == CardState.IsReady)
                    {
                        return true;
                    }
                    else
                    {
                        DisConnect();
                        _client.Connect(Info.TcpIp, Info.Port);
                        if (_client.Connected)
                        {
                            _stream = _client.GetStream();
                            CurrentInfo = null;
                            this._state = CardState.IsReady;
                            connectTime = DateTime.Now;
                            _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
                            count = 0;
                            return true;
                        }
                        else
                        {
                            DisConnect();
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DisConnect();
                return false;
            }
        }
        public void DisConnect()
        {
            this._state = CardState.IsDisable;
            this._info.IsReset = true;
            if (_stream != null)
                _stream.Dispose();
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        public object GetController()
        {
            return null;
        }
        class ScannelInfo
        {
            public string CardId { get; set; }
            public DateTime TimeReceived { get; set; }
            public string Anntenna { get; set; }
        }
    }
    public static class CurrentListCardReader
    {
        public static List<IGreenCardReaderInfo> ListCardInfo { get; set; }
        public static List<IGreenCardReader> ListCard { get; set; }
        public static IGreenCardReader AddCardInfo(IGreenCardReaderInfo info)
        {
            if (ListCardInfo == null)
                ListCardInfo = new List<IGreenCardReaderInfo>();
            var cif = ListCardInfo.FirstOrDefault(c => c.Type == info.Type &&
                       ((c.Type == "ModWinsCard" && c.SerialNumber == info.SerialNumber)
                           || (c.Type != "ModWinsCard" && c.TcpIp == info.TcpIp && c.Port == info.Port)
                       )
                   );
            if (cif == null)
                ListCardInfo.Add(info);
            if (ListCard == null)
                ListCard = new List<IGreenCardReader>();
            var crd = ListCard.FirstOrDefault(c => c.Info.Type == info.Type &&
                        ((c.Info.Type == "ModWinsCard" && c.Info.SerialNumber == info.SerialNumber)
                            || (c.Info.Type == "ZKFarCard" && c.Info.TcpIp == info.TcpIp && c.Info.Port == info.Port && c.Info.Reader == info.Reader)
                            || (c.Info.Type != "ZKFarCard" && c.Info.TcpIp == info.TcpIp && c.Info.Port == info.Port)
                        )
                    );
            if (crd == null)
            {
                switch (info.Type)
                {
                    //'ModWinsCard', 'Tcp Ip Client', 'Tcp Ip Server', 'Remode Card'
                    case "ModWinsCard":
                        crd = new ModWinsCardReader(info);
                        break;
                    case "Tcp Ip Client":
                        crd = new TcpIpClientCardReader(info);
                        break;
                    case "Tcp Ip Server":
                        crd = new TcpIpServerCardReader(info);
                        break;
                    case "Remode Card":
                        crd = new TcpIpRemodeCardReader(info);
                        break;
                    case "Scannel":
                        crd = new ScannelCardReader(info);
                        break;
                    case "Tcp Ip Controller":
                        crd = new TcpIpCardController(info);
                        break;
                    case "NFC":
                        {
                            crd = new NFCCardReader() { Reader = info.DeviceName, Info = info };
                            break;
                        }
                    case "Proxies":
                        {
                            crd = new ProxiesCardReader() { Port = info.DeviceName, Info = info };
                            break;
                        }
                    case "ZKFarCard":
                        {
                            crd = new ZKFarCardReader { Info = info};
                            break;
                        }
                }

                ListCard.Add(crd);
            }

            return crd;
        }
        public static void RemoveCards()
        {
            if (ListCard != null)
            {
                foreach (var c in ListCard)
                {
                    c.DisConnect();
                }
                ListCard.Clear();
            }
            if (ListCardInfo != null)
                ListCardInfo.Clear();
        }
        public static void RemoveCardInfo(IGreenCardReaderInfo info)
        {
            if (ListCardInfo != null)
            {
                var cif = ListCardInfo.FirstOrDefault(c => c.Type == info.Type &&
                        ((c.Type == "ModWinsCard" && c.SerialNumber == info.SerialNumber)
                            || (c.Type != "ModWinsCard" && c.TcpIp == info.TcpIp && c.Port == info.Port)
                        )
                    );
                if (cif != null)
                {
                    ListCardInfo.Remove(cif);
                }
                cif = ListCardInfo.FirstOrDefault(c => c.Type == info.Type &&
                        ((c.Type == "ModWinsCard" && c.SerialNumber == info.SerialNumber)
                            || (c.Type != "ModWinsCard" && c.TcpIp == info.TcpIp && c.Port == info.Port)
                        )
                    );
                if (cif == null && ListCard != null)
                {
                    var crd = ListCard.FirstOrDefault(c => c.Info.Type == info.Type &&
                        ((c.Info.Type == "ModWinsCard" && c.Info.SerialNumber == info.SerialNumber)
                            || (c.Info.Type != "ModWinsCard" && c.Info.TcpIp == info.TcpIp && c.Info.Port == info.Port)
                        )
                    );
                    if (crd != null)
                    {
                        crd.DisConnect();
                        ListCard.Remove(crd);
                    }
                }
            }
        }
        public static string RefreshListCard()
        {
            if (ListCard == null)
                return "Can't find out card reader";
            string res = string.Empty;
            foreach (var crd in ListCard.ToList())
            {
                if (crd.State != CardState.IsReady)
                {
                    if (crd.Info.Type != "Tcp Ip Client")
                        crd.Connect();
                }

                string rowString = string.Empty;
                if (crd.State == CardState.IsReady)
                {
                    res += string.Format("{0}:{1} -- already {2}", crd.Info.Type, crd.Info.Type == "ModWinsCard" ? crd.Info.SerialNumber : crd.Info.TcpIp + ":" + crd.Info.Port, Environment.NewLine);
                }
                else
                {
                    res += string.Format("{0}:{1} -- can't connect {2}", crd.Info.Type, crd.Info.Type == "ModWinsCard" ? crd.Info.SerialNumber : crd.Info.TcpIp + ":" + crd.Info.Port, Environment.NewLine);
                }

            }
            return res;
        }
        public static bool StartGreenCardReader(List<IGreenCardReaderInfo> lstInfo, GreenCardReaderEventHandler read, GreenCardReaderEventHandler takeoff)
        {
            if (lstInfo == null)
                return false;
            bool b = false;
            foreach (var info in lstInfo)
            {
                var crd = AddCardInfo(info);
                RefreshListCard();
                
                if (crd != null)
                {
                    if (crd.Info.Type == "Tcp Ip Controller" )
                    {
                        GreenTcpIpControllerInfo ctr = crd.GetController() as GreenTcpIpControllerInfo;
                        if (ctr == null)
                            return false;
                        ctr.ReadingCompleted += read;
                        ctr.TakingOffCompleted += takeoff;
                    }
                    else if (crd.Info.Type == Constants.CardType.ZKFarCard.ToString())
                    {
                        ZKControllerProcessor ctr = crd.GetController() as ZKControllerProcessor;
                        if (ctr == null)
                            return false;
                        ctr.AddReadingCompletedHandler(crd.Info.Reader, read);
                        ctr.AddTakeOffHandler(crd.Info.Reader, takeoff);
                    }
                    else
                    {
                        crd.ReadingCompleted += read;
                        crd.TakingOffCompleted += takeoff;
                        b = true;
                    }
                }
            }
            return b;
        }
        public static bool StoptGreenCardReader(List<IGreenCardReaderInfo> lstInfo, GreenCardReaderEventHandler read, GreenCardReaderEventHandler takeoff)
        {
            if (lstInfo == null || ListCard == null || ListCard.Count == 0)
                return false;
            bool b = false;
            foreach (var info in lstInfo)
            {
                var crd = ListCard.FirstOrDefault(c => c.Info.Type == info.Type &&
                       ((c.Info.Type == "ModWinsCard" && c.Info.SerialNumber == info.SerialNumber)
                           || (c.Info.Type != "ModWinsCard" && c.Info.TcpIp == info.TcpIp && c.Info.Port == info.Port)
                       )
                   );
                if (crd != null)
                {
                    if (crd.Info.Type == "Tcp Ip Controller")
                    {
                        GreenTcpIpControllerInfo ctr = crd.GetController() as GreenTcpIpControllerInfo;
                        if (ctr == null)
                            return false;
                        ctr.ReadingCompleted -= read;
                        ctr.TakingOffCompleted -= takeoff;
                    }
                    else
                    {
                        crd.ReadingCompleted -= read;
                        crd.TakingOffCompleted -= takeoff;
                        b = true;
                    }
                }
            }
            return b;
        }
        public static Object FindController(string Ip, ushort Port)
        {
            if (ListCard != null)
            {
                var GrrenCard = ListCard.FirstOrDefault(c => c.Info.Type == "Tcp Ip Controller" && c.Info.TcpIp == Ip && c.Info.Port == Port);
                if (GrrenCard != null)
                {
                    var ControllerCard = GrrenCard as TcpIpCardController;
                    return ControllerCard.GetController();
                }
            }
            return null;
        }

    }
}
