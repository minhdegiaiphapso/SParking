using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Sharp7;
namespace Green.Devices.Dal.Siemens
{
    public class Port4
    {  
        private Task _task;
        private static Port4 _instance = null;
        public static Port4 GetInstance()
        {
            if (_instance == null)
                _instance = new Port4();
            return _instance;
        }
        private List<SiemenInfo> CommandsOut;
        private List<SiemenInfo> CommandsIn;
        public Action<LogoTypeIn4> HandleButtonIn1;
        public Action<LogoTypeIn4> HandleButtonIn2;
        public Action<LogoTypeIn4> HandleButtonIn3;
        public Action<LogoTypeIn4> HandleButtonIn4;
        private bool active = false;
        public bool Active { get { return active; } }
        private bool InDoing = false;
        private DateTime time1;
        private DateTime time2;
        private DateTime time3;
        private DateTime time4;
        private void WatcherProcess()
        {
            while (true)
            {
                if (active)
                {
                    if(CommandsOut!=null && CommandsOut.Count>0)
                    {
                        active = false;
                        var cmd = CommandsOut[0];
                        for (var i=0;i<3;i++)
                        {
                            if (CallComandOut(cmd))
                                break;
                        }
                        CommandsOut.RemoveAt(0);
                        active = true;
                    }
                    else if(!InDoing  && CommandsIn != null && CommandsIn.Count>0)
                    {
                        active = false;  
                        var tmp = CommandsIn.ToList();
                        foreach (var cmd in tmp)
                        {
                            if (CallComandIn(cmd))
                            {
                                switch (cmd.Lane)
                                {
                                    case 1:
                                        if (HandleButtonIn1 != null && (DateTime.Now - time1).TotalSeconds >= cmd.KeepButtonInSeconds)
                                        {
                                            time1 = DateTime.Now;
                                            HandleButtonIn1(cmd.TypeIn);
                                        }
                                        break;
                                    case 2:
                                        if (HandleButtonIn2 != null && (DateTime.Now - time2).TotalSeconds >= cmd.KeepButtonInSeconds)
                                        {
                                            time2 = DateTime.Now;
                                            HandleButtonIn2(cmd.TypeIn);
                                        }
                                        break;
                                    case 3:
                                        if (HandleButtonIn3 != null && (DateTime.Now - time3).TotalSeconds >= cmd.KeepButtonInSeconds)
                                        {
                                            time3 = DateTime.Now;
                                            HandleButtonIn3(cmd.TypeIn);
                                        }
                                        break;
                                    case 4:
                                        if (HandleButtonIn4 != null && (DateTime.Now - time4).TotalSeconds >= cmd.KeepButtonInSeconds)
                                        {
                                            time4 = DateTime.Now;
                                            HandleButtonIn1(cmd.TypeIn);
                                        }
                                        break;
                                }
                                break;
                            }
                        }   
                        active = true;
                    }
                }
                else
                    Thread.Sleep(100);
            }
        } 
        private bool CallComandOut(SiemenInfo info)
        {
            if (info!=null && info.TypeOut != LogoTypeOut4.None &&! string.IsNullOrEmpty(info.TcpIp))
            {
                S7Client _client;
                _client = new S7Client();
                _client.SetConnectionParams(info.TcpIp, 0x0200, 0x0300);
                _client.Connect();
                if (_client != null && _client.Connected)
                {
                    bool successDBWrite = true;
                    byte[] buffer = new byte[8];
                    var param = Logo.GetOutParam(info.TypeOut);
                    S7.SetBitAt(ref buffer, 0, param.AddressBit, true);
                    int check0 = _client.DBWrite(1, param.Address, buffer.Length, buffer);
                    Thread.Sleep(300);
                    S7.SetBitAt(ref buffer, 0, param.AddressBit, false);
                    int check1 = _client.DBWrite(1, param.Address, buffer.Length, buffer);
                    if (check0 == 0 && check1 == 0)
                    {
                        return successDBWrite;
                    }
                    else
                    {
                        return !successDBWrite;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }
        private bool CallComandIn(SiemenInfo info)
        {
            if ((info != null && info.TypeIn != LogoTypeIn4.None && !string.IsNullOrEmpty(info.TcpIp)))
            {
                S7Client _client;
                _client = new S7Client();
                _client.SetConnectionParams(info.TcpIp, 0x0200, 0x0300);
                _client.Connect();
                if (_client != null && _client.Connected)
                {
                    byte[] buff = new byte[8];
                    var param = Logo.GetInParam(info.TypeIn);
                    _client.DBRead(1, param.Address, buff.Length, buff);
                    return S7.GetBitAt(buff, 0, param.AddressBit);
                }
                else
                    return false;
            }
            else
                return false;
        }
        private Port4()
        {
            time1 = time2 = time3 = time4 = DateTime.Now;
            _task = Task.Factory.StartNew(() => WatcherProcess(), TaskCreationOptions.LongRunning);
        }
        public void AddCommandOut(SiemenInfo info)
        {
            active = false;
            if (CommandsOut == null)
                CommandsOut = new List<SiemenInfo>();
            CommandsOut.Add(info);
            active = true;
        }
        public void AddCommandIn(SiemenInfo info)
        {
            active = false;
            InDoing = true;
            if (CommandsIn == null)
                CommandsIn = new List<SiemenInfo>();
            var cm = CommandsIn.FirstOrDefault(c => c.Lane == info.Lane);
            if (cm == null)
                CommandsIn.Add(info);
            else
            {
                cm.TcpIp = info.TcpIp;
                cm.TypeIn = info.TypeIn;
            }
            active = true;
            InDoing = false;
        }
        private static class Logo
        {
            public static LogoParam GetOutParam(LogoTypeOut4 type)
            {
                LogoParam param = new LogoParam();
                switch (type)
                {
                    case LogoTypeOut4.Out1:              //M1 - V1104.0
                        param.AddressBit = 0;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out12:             //M2 - V1104.1
                        param.AddressBit = 1;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out13:             //M3 - V1104.2
                        param.AddressBit = 2;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out14:             //M4 - V1104.3
                        param.AddressBit = 3;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out123:            //M5 - V1104.4
                        param.AddressBit = 4;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out124:            //M6 - V1104.5
                        param.AddressBit = 5;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out134:            //M7 - V1104.6
                        param.AddressBit = 6;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out1234:           //M8 - V1104.1
                        param.AddressBit = 7;
                        param.Address = 1104;
                        break;
                    case LogoTypeOut4.Out2:              //M9 - V1105.0
                        param.AddressBit = 0;
                        param.Address = 1105;
                        break;
                    case LogoTypeOut4.Out23:             //M10 - V1105.1
                        param.AddressBit = 1;
                        param.Address = 1105;
                        break;
                    case LogoTypeOut4.Out24:             //M11 - 1105.2
                        param.AddressBit = 2;
                        param.Address = 1105;
                        break;
                    case LogoTypeOut4.Out234:            //M12 - 1105.3
                        param.AddressBit = 3;
                        param.Address = 1105;
                        break;
                    case LogoTypeOut4.Out3:              //M13 - 1105.4
                        param.AddressBit = 4;
                        param.Address = 1105;
                        break;
                    case LogoTypeOut4.Out34:             //M14 - 1105.5
                        param.AddressBit = 5;
                        param.Address = 1105;
                        break;
                    case LogoTypeOut4.Out4:              //M15 - 1105.6
                        param.AddressBit = 6;
                        param.Address = 1105;
                        break;
                }
                return param;
            }
            public static LogoParam GetInParam(LogoTypeIn4 type)
            {
                LogoParam param = new LogoParam();
                switch (type)
                {
                    case LogoTypeIn4.In1:                  //M16 - V1105.7
                        param.AddressBit = 7;
                        param.Address = 1105;
                        break;
                    case LogoTypeIn4.In2:                 //M17 - V1106.0
                        param.AddressBit = 0;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In3:                 //M18 - V1106.1
                        param.AddressBit = 1;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In4:                 //M19 - V1106.2
                        param.AddressBit = 2;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In12:                //M20 - V1106.3
                        param.AddressBit = 3;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In13:                //M21 - V1106.4
                        param.AddressBit = 4;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In14:                //M22 - V1106.5
                        param.AddressBit = 5;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In123:               //M23 - V1106.6
                        param.AddressBit = 6;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In124:              //M24 - V1106.7
                        param.AddressBit = 7;
                        param.Address = 1106;
                        break;
                    case LogoTypeIn4.In134:              //M25 - V1107.0
                        param.AddressBit = 0;
                        param.Address = 1107;
                        break;
                    case LogoTypeIn4.In1234:             //M26 - 1107.1
                        param.AddressBit = 1;
                        param.Address = 1107;
                        break;
                    case LogoTypeIn4.In23:               //M27 - V1107.2
                        param.AddressBit = 2;
                        param.Address = 1107;
                        break;
                    case LogoTypeIn4.In24:              //M28 - 1107.3
                        param.AddressBit = 3;
                        param.Address = 1107;
                        break;
                    case LogoTypeIn4.In234:            //M29 - 1107.4
                        param.AddressBit = 4;
                        param.Address = 1107;
                        break;
                    case LogoTypeIn4.In34:            //M30 - 1107.5
                        param.AddressBit = 5;
                        param.Address = 1107;
                        break;
                }
                return param;
            }
        }
        private class LogoParam
        {
            public int AddressBit { get; set; }
            public int Address { get; set; }
        }
    }
    public class SiemenInfo
    {
        public string TcpIp { get; set; }
        public int KeepButtonInSeconds { get; set; } = 3;
        public LogoTypeIn4 TypeIn { get; set; }
        public LogoTypeOut4 TypeOut { get; set; }
        public int Lane { get; set; }
    }
    public enum LogoTypeOut4
    {
        Out1,
        Out12,
        Out13,
        Out14,
        Out123,
        Out124,
        Out134,
        Out1234,
        Out2,
        Out23,
        Out24,
        Out234,
        Out3,
        Out34,
        Out4,
        None
    }
    public enum LogoTypeIn4
    {
        In1,
        In12,
        In13,
        In14,
        In123,
        In124,
        In134,
        In1234,
        In2,
        In23,
        In24,
        In234,
        In3,
        In34,
        In4,
        None
    }
}
