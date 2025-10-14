using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using Green.Devices.Dal.ZKTeco;
using PCSC.Monitoring;
using static System.Windows.Forms.AxHost;

namespace Green.Devices.Dal
{
    public class ZKControllerProcessor
    {
        public static Dictionary<string, ZKControllerProcessor> _instances = new Dictionary<string, ZKControllerProcessor>();
        Dictionary<string, GreenCardReaderEventHandler> _takeOffCompletedEvents = new Dictionary<string, GreenCardReaderEventHandler>();
        Dictionary<string, GreenCardReaderEventHandler> _readingCompletedEvents = new Dictionary<string, GreenCardReaderEventHandler>();

        IntPtr h = IntPtr.Zero;
        Timer _timer = new Timer();

        public string Ip { get; set; }
        public ushort Port { get; set; }

        private ZKControllerProcessor() { }
        public static ZKControllerProcessor GetInstance(string ip, ushort port)
        {
            var key = $"{ip}:{port}";
            if (!_instances.TryGetValue(key, out ZKControllerProcessor processor))
            {
                processor = new ZKControllerProcessor() { Ip = ip, Port = port };
                _instances.Add(key, processor);
            }

            return processor;
        }

        public void AddTakeOffHandler(string reader, GreenCardReaderEventHandler handler)
        {
            if(!_takeOffCompletedEvents.ContainsKey(reader))
            {
                _takeOffCompletedEvents.Add(reader, handler);
            }
            else
            {
                _takeOffCompletedEvents[reader] = handler;
            }
        }

        public void AddReadingCompletedHandler(string reader, GreenCardReaderEventHandler handler)
        {
            if (!_readingCompletedEvents.ContainsKey(reader))
            {
                _readingCompletedEvents.Add(reader, handler);
            }
            else
            {
                _readingCompletedEvents[reader] = handler;
            }
        }

        public bool Connect()
        {

            var str = $"protocol=TCP,ipaddress={Ip},port={Port},timeout=2000,passwd=";
            h = PLComPro.Connect(str);
            var result = h != IntPtr.Zero;
            if (result)
            {
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Elapsed += _timer_Elapsed;
                //_timer.Interval = 1000;
                _timer.Enabled = true;
            }

            return result;
        }

        /// <summary>
        /// Đổi sang Hexa để check các đầu số chuẩn
        /// </summary>
        /// <param name="CardNo"></param>
        /// <returns></returns>
		private string ConvetCardNo(long CardNo)
		{
			if (CardNo < 1)
				return "";
			var hex = CardNo.ToString("X");
			if (hex.Length % 2 == 1)
			{
				hex = "0" + hex;
			}
			var l = hex.Length;
			if (l == 4)
				hex = "0000" + hex;
			else if (l == 6)
				hex = "00" + hex;
			string res = "";
			while (hex.Length > 0)
			{
				var tmp = hex.Substring(0, 2);
				hex = hex.Substring(2);
				if (tmp.Length == 2)
				{
					res = $"{tmp}{res}";
				}
				else
				{
					res = $"0{tmp}{res}";
				}
			}
			return res;
		}
		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int ret = 0, i = 0, buffersize = 256;
            string str = "";
            string[] tmp = null;
            byte[] buffer = new byte[256];

            if (IntPtr.Zero != h)
            {
                var result = new GreenCardReaderEventArgs { CardReader = new ZKFarCardReader { Info = new GreenCardReaderInfo { TcpIp = Ip, Port = Port } } };
                ret = PLComPro.GetRTLog(h, ref buffer[0], buffersize);
                if (ret >= 0)
                {
                    str = Encoding.Default.GetString(buffer);
                    tmp = str.Split(',');

                    if (tmp.Length > 5 && tmp[4] == "27")
                    {
						long outCard = 0;
						Int64.TryParse(tmp[2], out outCard);
                        result.CardID = ConvetCardNo(outCard);
						//result.CardID = tmp[2];
                        var reader = tmp[3];
                        result.CardReader.Info.Reader = reader;
                        var isExisted = _takeOffCompletedEvents.TryGetValue(reader, out GreenCardReaderEventHandler takeOffCompletedHandler);
                        if (isExisted)
                        {
                            takeOffCompletedHandler(this, result);
                        }

                        isExisted = _readingCompletedEvents.TryGetValue(reader, out GreenCardReaderEventHandler readingCompletedHandler);
                        if (isExisted)
                        {
                            readingCompletedHandler(this, result);
                        }
                    }
                }
            }
        }

        public void SendOutputCommand(string door)
        {
            int outputadr = 1;
            int doorstate = 1;
            if (IntPtr.Zero == h)
            {
                Connect();
                Console.WriteLine("Login!!");
            }

            if (IntPtr.Zero != h)
            {
                var ret = PLComPro.ControlDevice(h, 1, int.Parse(door), outputadr, doorstate, 0, "");     //引用PullSDK控制设备操作函数
                if (ret < 0)
                {
                    Console.WriteLine("Cant operate control");
                }
            }
        }

        public void DisConnect()
        {
            if (IntPtr.Zero != h)
            {
                _timer.Enabled = false;
                PLComPro.Disconnect(h);
                h = IntPtr.Zero;
            }
        }
    }

    public class ZKFarCardReader : IGreenCardReader
    {
        public string Reader { get; set; }
        public bool IsConnected { get; private set; }
 

        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;

        public IGreenCardReaderInfo Info { get; set; }
        public CardState State { get; set; }

        

        public static ICollection<string> GetReaders()
        {
            string[] ComList = new string[] { };

            return ComList;
        }

        public bool Connect()
        {
            if (State != CardState.IsReady)
            {
                var processor = ZKControllerProcessor.GetInstance(Info.TcpIp, Info.Port);
                if(processor.Connect())
                {
                    this.State = CardState.IsReady;
                }

            }

            return this.State == CardState.IsReady;
        }

        public void DisConnect()
        {
            var processor = ZKControllerProcessor.GetInstance(Info.TcpIp, Info.Port);
            processor.DisConnect();
            this.State = CardState.IsStop;
        }

        public object GetController()
        {
            return ZKControllerProcessor.GetInstance(Info.TcpIp, Info.Port);
        }
    }
}
