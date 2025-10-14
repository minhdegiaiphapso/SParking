using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Green.Devices.Dal.CardControler;

namespace Green.Devices.Dal.CardControler
{
    public class GreenBarrierIpController : IGreenController
    {
        public GreenBarrierIpController(string Ip, ushort Port)
        {
            Info = new GreenBarrierInfo()
            {
                Ip = Ip,
                Port = Port,
                TypeInUse = TypeInUse.Door
            };
            ctr = GreenTcpIpControllerSingleton.GetInstance(Ip, Port);     
        }  
        public IGreenControllerInfo Info { get; set; }
        private GreenTcpIpControllerSingleton ctr = null;  
        public bool CloseElectricGun(string source)
        {
            return ctr.CloseElectricGun(this.Info.Ip, this.Info.Port, source);
           
        }
        public bool CloseFire()
        {
            return ctr.CloseFire(this.Info.Ip, this.Info.Port);
         
        }
        public bool Connect()
        {
            return ctr.Connect(this.Info.Ip, this.Info.Port);
            
        }
        public bool DisConnect()
        {
            return ctr.DisConnect(this.Info.Ip, this.Info.Port);
           
        }
        public GreenTcpIpControllerInfo GetController()
        {
            return ctr[this.Info.Ip, this.Info.Port]; 
        }
        public bool SendMessage()
        {
            //TcpipObj.
            return true;
        }
        public bool OpenElectricGun(string source, int timeTick)
        {  
            return ctr.OpenElectricGun(this.Info.Ip, this.Info.Port, source, timeTick);
            
        }
        public bool OpenFire()
        {
            return ctr.OpenFire(this.Info.Ip, this.Info.Port);
           
        }
    }
    public class GreenBarrierInfo : IGreenControllerInfo
    {
        private TypeInUse _typeInUse;
        public TypeInUse TypeInUse {
            get {
                return _typeInUse;
            }
            set
            {
                if (_typeInUse != TypeInUse.Door)
                    _typeInUse = TypeInUse.Door;
            }
        }
        public DoorType DoorType { get ; set; }
        public AlarmType AlarmType { get; set; }
        public MsgType MsgType { get; set ; }
        public Msg485Type Msg485Type { get; set; }
        public ExitType ExitType { get; set; }
        public string Ip { get ; set ; }
        public ushort Port { get ; set; }
    }
    public static class CurrentListBarrierIp
    {
        public static List<IGreenControllerInfo> ListBarrierIpInfo { get; set; }
        public static List<IGreenController> ListBarrierIp { get; set; }
        private static void AddBarrierIp(string ip, ushort port)
        {
            if (ListBarrierIpInfo == null)
                ListBarrierIpInfo = new List<IGreenControllerInfo>();
            var item = ListBarrierIpInfo.FirstOrDefault(i => i.Ip == ip && i.Port == port && i.TypeInUse == TypeInUse.Door);
            if (item==null)
            {
                ListBarrierIpInfo.Add(new GreenBarrierInfo() { Ip = ip, Port = port, TypeInUse = TypeInUse.Door });
            }
            if(ListBarrierIp==null)
            {
                ListBarrierIp = new List<IGreenController>();
            }
            var brr = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
            if(brr==null)
            {
                ListBarrierIp.Add(new GreenBarrierIpController(ip, port));
            }
        }
        public static void RemoveBarrier(string ip, ushort port)
        {
            if(ListBarrierIpInfo!=null)
            {
                var item = ListBarrierIpInfo.FirstOrDefault(i => i.Ip == ip && i.Port == port && i.TypeInUse == TypeInUse.Door);
                if (item != null)
                    ListBarrierIpInfo.Remove(item);
            }
            if(ListBarrierIp!=null)
            {
                var item = ListBarrierIpInfo.FirstOrDefault(i => i.Ip == ip && i.Port == port);
                if (item == null)
                {
                    var brr = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
                    if (brr != null)
                    {
                        ListBarrierIp.Remove(brr);
                    }
                }
            }
        }
        public static string RefreshListController()
        {
            if (ListBarrierIp == null)
                return "Không tìm thấy Controller";
            string res = string.Empty;
            foreach (var brr in ListBarrierIp)
            {
                bool b = brr.Connect(); 
                string rowString = string.Empty;
                if (b)
                {
                    res += string.Format("Controller: {0}:{1} -- đã sẵn sàng {2}", brr.Info.Ip, brr.Info.Port, Environment.NewLine);
                }
                else
                {
                    res += string.Format("Controller: {0}:{1} -- không thể kết nối {2}", brr.Info.Ip, brr.Info.Port, Environment.NewLine);
                }

            }
            return res;
        }
        public static bool OpenBarrier(string ip, ushort port, string source, int timeTick)
        {
            if (ListBarrierIp == null)
            {
                AddBarrierIp(ip, port);
            }
            var brr = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
            if(brr!=null && brr.Connect())
                return brr.OpenElectricGun(source, timeTick);
            else
            {
                AddBarrierIp(ip, port);
                var brr1 = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
                if (brr1 != null && brr1.Connect())
                {
                    return brr1.OpenElectricGun(source, timeTick);
                }
                else
                    return false;
            }
        }
        public static Object FindController(string Ip, ushort Port)
        {
            if (ListBarrierIp != null)
            {
                var brr = ListBarrierIp.FirstOrDefault(c => c.Info.Ip == Ip && c.Info.Port == Port);
                if (brr != null)
                {
                    var ControllerBrr = brr as GreenBarrierIpController;
                    return ControllerBrr.GetController();
                }
            }
            return null;
        }
        public static bool StartHandButtonClick(string ip, ushort port, GreenHandButtonEventHandler clicked)
        {
            if (ListBarrierIp == null)
            {
                AddBarrierIp(ip, port);
            }
            var brr = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
            if (brr != null && brr.Connect())
            {
                var b =  brr as GreenBarrierIpController;
                b.GetController().HandButtonClicked += clicked;
                return true;
            }
            else
            {
                AddBarrierIp(ip, port);
                var brr1 = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
                if (brr1 != null && brr1.Connect())
                {
                    var b = brr as GreenBarrierIpController;
                    b.GetController().HandButtonClicked += clicked;
                    return true;
                }
                else
                    return false;
            }
        }
        public static bool StoptHandButtonClick(string ip, ushort port, GreenHandButtonEventHandler clicked)
        {
            if (ListBarrierIp == null)
            {
                return false;
            }
            var brr = ListBarrierIp.FirstOrDefault(b => b.Info.Ip == ip && b.Info.Port == port);
            if (brr != null)
            {
                var b = brr as GreenBarrierIpController;
                b.GetController().HandButtonClicked -= clicked;
                return true;
            }
            return false;
        }
    }
}
