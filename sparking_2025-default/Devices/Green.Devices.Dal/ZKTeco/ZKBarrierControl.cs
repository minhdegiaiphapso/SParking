using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static System.Windows.Forms.AxHost;

namespace Green.Devices.Dal.ZKTeco
{
    public class ZKBarrierControl
    {
        public string TcpIp { get; set; }
        public ushort Port { get; set; }
        public string Door { get; set; }

        public CardState State { get; set; }

        public bool Connect()
        {
            if (State != CardState.IsReady)
            {
                var processor = ZKControllerProcessor.GetInstance(TcpIp, Port);
                if (processor.Connect())
                {
                    this.State = CardState.IsReady;
                }
            }

            return this.State == CardState.IsReady;
        }

        public void DisConnect()
        {
            var processor = ZKControllerProcessor.GetInstance(TcpIp, Port);
            processor.DisConnect();
            this.State = CardState.IsStop;
        }

        public object GetController()
        {
            return ZKControllerProcessor.GetInstance(TcpIp, Port);
        }
    }

    public class ZKBarrierControlProcessor
    {
        public static Dictionary<string, ZKBarrierControlProcessor> _instances = new Dictionary<string, ZKBarrierControlProcessor>();
        Dictionary<string, GreenCardReaderEventHandler> _takeOffCompletedEvents = new Dictionary<string, GreenCardReaderEventHandler>();
        Dictionary<string, GreenCardReaderEventHandler> _readingCompletedEvents = new Dictionary<string, GreenCardReaderEventHandler>();

        IntPtr h = IntPtr.Zero;
        Timer _timer = new Timer();


        public string Ip { get; set; }
        public ushort Port { get; set; }

        private ZKBarrierControlProcessor() { }
        public static ZKBarrierControlProcessor GetInstance(string ip, ushort port)
        {
            var key = $"{ip}:{port}";
            if (!_instances.TryGetValue(key, out ZKBarrierControlProcessor processor))
            {
                processor = new ZKBarrierControlProcessor() { Ip = ip, Port = port };
                _instances.Add(key, processor);
            }

            return processor;
        }

        public void AddTakeOffHandler(string reader, GreenCardReaderEventHandler handler)
        {
            if (!_takeOffCompletedEvents.ContainsKey(reader))
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

            return result;
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
}
