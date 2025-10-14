using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.Devices.Dal.CardControler
{
    public class GreenTcpIpControllerInfo
    {
        public AcsTcpClass TcpipObj { get; set; }
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public DateTime ConnectTime { get; set; }
        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;
        public event GreenHandButtonEventHandler HandButtonClicked;
        public CardState State { get; set; }
        public void AddEvent()
        {
            TcpipObj.OnEventHandler += TcpipObj_OnEventHandler;
            TcpipObj.OnDisconnect += TcpipObj_OnDisconnect;
        }
        public void RemoveEvent()
        {
            TcpipObj.OnEventHandler -= TcpipObj_OnEventHandler;
            TcpipObj.OnDisconnect -= TcpipObj_OnDisconnect;
        }
        private void TcpipObj_OnDisconnect()
        {
            //throw new NotImplementedException();
        }
        private string HexaCardCode(string cardCode)
        {
            long c = 0;
            string card = "";
            if (long.TryParse(cardCode, out c))
            {
                var tmpcard = c.ToString("X");
                if (tmpcard.Length % 2 != 0)
                    tmpcard = "0" + tmpcard;
                for (int i = tmpcard.Length - 1; i > 0; i = i - 2)
                {
                    card += tmpcard[i - 1].ToString() + tmpcard[i].ToString();
                }
            }
            else
            {
                card = cardCode;
            }
            return card;
        }
        private void TcpipObj_OnEventHandler(string SerialNo, byte EType, DateTime Datetime, byte Second, byte Minute, byte Hour, byte Day, byte Month, int Year, byte DoorStatus, byte Ver, byte FuntionByte, bool Online, byte CardsofPackage, string CardNo, string QRCode, byte Door, byte EventType, ushort CardIndex, byte CardStatus, byte reader, out byte relay, out bool OpenDoor, out bool Ack)
        {
            Ack = true;
            OpenDoor = false;
            relay = 0;
            if (EType != 2)
            {                
                if (Datetime <= ConnectTime || EType != 1 || State != CardState.IsReady)
                    return;  
                ConnectTime = Datetime.AddMilliseconds(2);
                string card = HexaCardCode(CardNo);
                if (ReadingCompleted != null)
                    ReadingCompleted("Tcp Ip Controller card", new GreenCardReaderEventArgs() { CardID = card, CardReader = null, Door = Door.ToString(), Reader = reader.ToString() });
                if (TakingOffCompleted != null)
                    TakingOffCompleted("Tcp Ip Controller card", new GreenCardReaderEventArgs() { CardID = card, CardReader = null, Door = Door.ToString(), Reader = reader.ToString(), Time = Datetime });
            }
            else
            {
                if (Datetime <= ConnectTime || State != CardState.IsReady)
                    return;
                ConnectTime = Datetime.AddMilliseconds(2);
                if (HandButtonClicked != null)
                    HandButtonClicked("Tcp Ip Controller button", new GreenHandButtonEventArgs() { Ip = this.Ip, Port = this.Port, Time = Datetime, EventType = EventType });
            }
        }

    }
    public class GreenTcpIpControllerSingleton
    {
        private static List<KeyValuePair<GreenTcpIpControllerInfo, GreenTcpIpControllerSingleton>> lstSingleton = null;
        private GreenTcpIpControllerSingleton()
        {

        }
       
        public GreenTcpIpControllerInfo this[string ip, ushort port]
        {
            get
            {
                if (lstSingleton != null)
                {
                    var key = lstSingleton.FirstOrDefault(i => i.Key.Ip == ip && i.Key.Port == port).Key;
                    if (key != null)
                        return key;
                    return null;
                }
                return null;
            }
        }  
        public static GreenTcpIpControllerSingleton GetInstance(string ip, ushort port)
        {
            if (lstSingleton == null)
            {
                lstSingleton = new List<KeyValuePair<GreenTcpIpControllerInfo, GreenTcpIpControllerSingleton>>();
                KeyValuePair<GreenTcpIpControllerInfo, GreenTcpIpControllerSingleton> item =
                    new KeyValuePair<GreenTcpIpControllerInfo, GreenTcpIpControllerSingleton>(
                        new GreenTcpIpControllerInfo()
                        {
                            Ip = ip,
                            Port = port,
                            TcpipObj = new AcsTcpClass(true),
                            ConnectTime = DateTime.Now 
                        }, 
                    new GreenTcpIpControllerSingleton());

                item.Key.AddEvent();   
                lstSingleton.Add(item);
                return lstSingleton.FirstOrDefault(i => i.Key.Ip == ip && i.Key.Port == port).Value;
            }
            else
            {
                var instane = lstSingleton.FirstOrDefault(i => i.Key.Ip == ip && i.Key.Port == port).Value;
                if (instane != null)
                    return instane;
                KeyValuePair<GreenTcpIpControllerInfo, GreenTcpIpControllerSingleton> item =
                    new KeyValuePair<GreenTcpIpControllerInfo, GreenTcpIpControllerSingleton>(
                        new GreenTcpIpControllerInfo()
                        {
                            Ip = ip,
                            Port = port,
                            TcpipObj = new AcsTcpClass(true)    
                        },
                    new GreenTcpIpControllerSingleton());
                item.Key.AddEvent();
                lstSingleton.Add(item);
                return lstSingleton.FirstOrDefault(i => i.Key.Ip == ip && i.Key.Port == port).Value;
            }
        }
        public bool OpenElectricGun(string ip, ushort port, string source, int timeTick)
        {
            var TcpipObj = this[ip, port].TcpipObj;
            if (TcpipObj == null)
                return false;
            bool b = false;
            if (source.Contains("1"))
            {
                TcpipObj.CloseDoor(0);
                b = TcpipObj.Opendoor(0) || b;
                //TcpipObj.CloseDoor(0);
            }
            if (source.Contains("2"))
            {
                TcpipObj.CloseDoor(1);
                b = TcpipObj.Opendoor(1) || b;
                //TcpipObj.CloseDoor(1);
            }
            if (source.Contains("3"))
            {
                TcpipObj.CloseDoor(2);
                b = TcpipObj.Opendoor(2) || b;
                //TcpipObj.CloseDoor(2);
            }
            if (source.Contains("4"))
            {
                TcpipObj.CloseDoor(3);
                b = TcpipObj.Opendoor(3) || b;
                //TcpipObj.CloseDoor(3);
            }
            if (timeTick > 0)
                System.Threading.Thread.Sleep(timeTick);
            if (source.Contains("1"))
            {
                TcpipObj.CloseDoor(0);
                
            }
            if (source.Contains("2"))
            {
                TcpipObj.CloseDoor(1);
              
            }
            if (source.Contains("3"))
            {
                TcpipObj.CloseDoor(2);
               
            }
            if (source.Contains("4"))
            {
                TcpipObj.CloseDoor(3);
               
            }
            return b;
        }
        public bool OpenFire(string ip, ushort port)
        {
            var TcpipObj = this[ip, port].TcpipObj;
            if (TcpipObj == null)
                return false;
            return TcpipObj.SetFire(false, false);
        }
        public bool CloseElectricGun(string ip, ushort port, string source)
        {
            var TcpipObj = this[ip, port].TcpipObj;
            if (TcpipObj == null)
                return false;
            bool b = false;
            if (source.Contains("1"))
                b = TcpipObj.CloseDoor(0) || b;
            if (source.Contains("2"))
                b = TcpipObj.CloseDoor(1) || b;
            if (source.Contains("3"))
                b = TcpipObj.CloseDoor(2) || b;
            if (source.Contains("4"))
                b = TcpipObj.CloseDoor(3) || b;
            return b;
        }
        public bool CloseFire(string ip, ushort port)
        {
            var TcpipObj = this[ip, port].TcpipObj;
            if (TcpipObj == null)
                return false;
            return TcpipObj.SetFire(true, false);
        }
        public bool OpenMessage485(string ip, ushort port, string source)
        {
            var TcpipObj = this[ip, port].TcpipObj;
            if (TcpipObj == null)
                return false;
            bool b = TcpipObj.Send485(Encoding.Unicode.GetBytes(source));
            return b;
        }
        public bool Connect(string ip, ushort port)
        {
            var key = this[ip, port];
            var TcpipObj = key.TcpipObj;
            if (TcpipObj == null)
                return false;
            if (TcpipObj.IsconnectSuccess())
            {
                key.State = CardState.IsReady;
                return true;
            }
            bool b = TcpipObj.OpenIP(ip, port);
            if (b)
            {
                TcpipObj.Reset();
                key.State = CardState.IsReady;
            }
            else
            {
                key.State = CardState.IsDisable;
            }
            return b;
        }
        public bool DisConnect(string ip, ushort port)
        {
            var key = this[ip, port];
            var TcpipObj = key.TcpipObj;
            key.State = CardState.IsDisable;
            if (TcpipObj == null)
                return false;
            return TcpipObj.CloseTcpip();
        }
        public bool RemoveAt(string ip, ushort port)
        {
            if (lstSingleton != null)
            {
                var instane = lstSingleton.FirstOrDefault(i => i.Key.Ip == ip && i.Key.Port == port);
                if (instane.Key != null)
                {
                    lstSingleton.Remove(instane);
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
    }
}
