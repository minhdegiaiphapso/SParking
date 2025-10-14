using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
using System.Timers;
using System.Runtime.InteropServices;
using Green.Devices.Dal.CardControler;

namespace Green.Devices.Dal.CardControler
{
    public class AcsTcpClass
    {
        #region constdata
        public const byte TcpErr_OK = 0;
        public const byte TcpErr_NotExists = 1; // 对象不存在
        public const byte TcpErr_DataErr = 2; // 数据超出边界
        public const byte TcpErr_OutTime = 3; // 操作超时
        public const byte TcpErr_UnLink = 4; //
        public const byte TcpErr_ReData = 5; // 返回数据错误
        public const byte TcpErr_Working = 6; //
        public const byte TcpErr_Unknow = 7; //
        #endregion


        public byte TCPLastError = 0;

        #region 内部变量
        private Boolean Busy;
        private volatile Boolean FisWaiting;
        private TcpPackge TcpPackge;
        private Boolean HasTcpObj;
        public TcpClientClass TcpIpObj;
        #endregion

        #region 委托事件声明

        public delegate void TOnEventHandler(string SerialNo, byte EType, DateTime Datetime, byte Second, byte Minute, byte Hour, byte Day, byte Month, int Year, byte DoorStatus,
            byte Ver, byte FuntionByte, Boolean Online, byte CardsofPackage, string CardNo, string QRCode, byte Door, byte EventType,
            UInt16 CardIndex, byte CardStatus, byte reader, out byte relay, out Boolean OpenDoor, out Boolean Ack);   //声明委托 
        public event TOnEventHandler OnEventHandler;        //声明事件

        //   public delegate void TOnCardStatusHandler(UInt32 CardIndex, byte CardStatus, out Boolean Ack);   //声明委托 
        //   public event TOnCardStatusHandler OnCardStatusHandler;        //声明事件

        public delegate void TOnEventChinaHandler(string SerialNo, DateTime Datetime, byte Event, byte Reader, string Name, string Sex, string Nation, DateTime Birthday,
            string Address, string Card, string Dept, DateTime DateFrm, DateTime DateTo, byte[] photo, out Boolean OpenDoor, out Boolean Ack);   //声明委托 
        public event TOnEventChinaHandler OnEventChinaHandler;        //声明事件

        public delegate void TOnDisconnect();
        public event TOnDisconnect OnDisconnect;        //声明事件

        //发送和接收事件，用于调试
        public delegate void TOnDataDebug(byte[] buffRX, int len, string str);   //声明委托
        public event TOnDataDebug OnDataDebug;        //声明事件 
        #endregion

        public AcsTcpClass(bool hasTcpObj)
        {
            TcpPackge = new TcpPackge();
            HasTcpObj = hasTcpObj;
            //if (asClient)
            {
                TcpIpObj = new TcpClientClass(HasTcpObj);
                TcpIpObj.OnRxTxDataEvent += DoOnDataDebug;// 调试
                TcpIpObj.OnRxTxDataEvent += TcpPackge.HandleMessage;
                TcpIpObj.OnDisconnected += EventDisConnect;

                TcpPackge.OnSetTcpTick += TcpIpObj.SetTcpTick;
                TcpPackge.OnClearWait += this.ClearWait;
                TcpPackge.OnEventHandler += EventHandler;
                TcpPackge.OnChinaEventHandler += EventChinaHandler;
                TcpPackge.OnDoSenddata += this.SendAndNOReturn;
            }
        }

        public Boolean IsconnectSuccess()
        {
            return TcpIpObj.IsconnectSuccess;
        }

        protected Boolean isWorking()
        {
            if (Busy) TCPLastError = TcpErr_Working;
            return Busy;
        }

        private void ClearWait()
        {
            FisWaiting = false;
        }

        //===============================================================
        private Boolean WaitReturn(int delay)
        {
            Boolean te, result;

            while (FisWaiting)
            {

                Thread.Sleep(2);

                int StartTick = Environment.TickCount - TcpIpObj.StartTick;

                te = StartTick > (200 + delay);
                if (te)
                {
                    break;
                }
            }
            result = (!FisWaiting);
            if (result)
            {
                FisWaiting = false;
            }
            else
                TCPLastError = TcpErr_OutTime;
            return result;
        }

        private Boolean WaitReturnx(int delay)
        {
            Boolean te, result;
            int t1 = 0;
            while (FisWaiting)
            {
                Thread.Sleep(2);
                t1++;
                te = t1 > (300 + delay);
                if (te)
                {
                    break;
                }
            }
            result = (!FisWaiting);
            if (result)
            {
                FisWaiting = false;
            }
            else
                TCPLastError = TcpErr_OutTime;
            return result;
        }


        private Boolean SendAndNOReturn()
        {
            byte re;
            TcpPackge.BeforeSend();
            re = TcpIpObj.DoSendData(TcpPackge.BufferTX, TcpPackge.WriteNum);
            TCPLastError = re;
            return (re == 0);
        }

        protected Boolean SendAndReturn(int delay)
        {
            Boolean result = false;
            byte re;
            Busy = true;
            try
            {
                FisWaiting = true;
                TcpPackge.BeforeSend();
                re = TcpIpObj.DoSendData(TcpPackge.BufferTX, TcpPackge.WriteNum);
                TCPLastError = re;
                if (re == 0)
                    result = WaitReturn(delay);
                return result;
            }
            finally
            {
                Busy = false;
            }
        }

        // =====================================================================================================
        public void EventHandler(Green.Devices.Dal.CardControler.RAcsEvent Event, out byte relay, out Boolean OpenDoor, out Boolean Ack)
        {
            OpenDoor = false;
            Ack = true;
            relay = Event.Reader;
            if (Event.EType == 0)
            {
                TimeSpan ds = DateTime.Now - Event.Datetime;
                if (System.Math.Abs(ds.Seconds) >= 5)
                {
                    TcpPackge.SetTime(DateTime.Now);
                    SendAndNOReturn();
                }
            }
            if (OnEventHandler != null)
                OnEventHandler(Event.SerialNo, Event.EType, Event.Datetime, Event.Second, Event.Minute, Event.Hour, Event.Day, Event.Month, Event.Year, Event.DoorStatus, Event.Ver, Event.FuntionByte,
                    Event.Online, Event.CardNumInPack, Event.CardNo, Event.QRCode, Event.Door, Event.EventType, Event.CardIndex, Event.CardStatus, Event.Reader, out relay, out OpenDoor, out Ack);
        }

        public void EventChinaHandler(Green.Devices.Dal.CardControler.RChinaCard Event, out Boolean OpenDoor, out Boolean Ack)
        {
            OpenDoor = false;
            Ack = true;
            if (OnEventChinaHandler != null)
                OnEventChinaHandler(Event.SerialNo, Event.Datetime, Event.Event, Event.Reader, Event.Name, Event.Sex, Event.Nation, Event.Birthday,
                Event.Address, Event.Card, Event.Dept, Event.DateFrm, Event.DateTo, Event.Photo, out OpenDoor, out Ack);
        }

        public void EventDisConnect()
        {
            if (OnDisconnect != null) OnDisconnect();
        }

        public void DoOnDataDebug(byte rt, byte[] buff, int len)
        {
            // 调试用，用于显示传输的实际数据 
            if (OnDataDebug != null)
            {
                byte[] returnBytes = new byte[len];
                Array.ConstrainedCopy(buff, 0, returnBytes, 0, len);
                string str = BitConverter.ToString(returnBytes).Replace("-", " ");

                if (rt == 0)
                { str = string.Concat("Recv ", str); }
                else
                { str = string.Concat("Send ", str); }
                OnDataDebug(buff, len, str); /* */
            }
        }

        #region 网络指令
        public bool OpenIP(string ip, int port, int oemcode = 23456)
        {
            TcpPackge.OEMCode = oemcode;
            return TcpIpObj.OpenIP(ip, port);
        }

        public bool CloseTcpip()
        {
            return TcpIpObj.CloseTcpip();
        }
        #endregion

        #region 参数类指令
        public bool SetTime(DateTime datetime)
        {
            if (isWorking()) return false;
            TcpPackge.SetTime(datetime);
            return SendAndReturn(100);
        }

        public Boolean SetDoor(byte Door, UInt16 OpenTime, UInt16 OpenOutTime, Boolean TooLongAlarm, UInt16 AlarmMast, UInt16 AlarmTime, Boolean DoublePath, byte CardsOpen, byte CardsOpenInOut)
        {
            if (isWorking()) return false;
            TcpPackge.SetDoor(Door, OpenTime, OpenOutTime, TooLongAlarm, AlarmMast, AlarmTime, DoublePath, CardsOpen, CardsOpenInOut);
            return SendAndReturn(100);
        }

        public Boolean SetControl(UInt16 FireTime, UInt16 AlarmTime, string DuressPIN, byte LockEach)
        {
            if (isWorking()) return false;
            TcpPackge.SetControl(FireTime, AlarmTime, DuressPIN, LockEach);
            return SendAndReturn(100);
        }

        public Boolean DelTimeZone(byte Door)
        {
            if (isWorking()) return false;
            TcpPackge.DelTimeZone(Door);
            return SendAndReturn(100);
        }

        public Boolean AddTimeZone(UInt16 Door, byte Index, DateTime frmtime, DateTime totime, byte Week, Boolean PassBack, byte Indetify, DateTime Enddatetime, byte Group)
        {
            if (isWorking()) return false;
            TcpPackge.AddTimeZone(Door, Index, frmtime, totime, Week, PassBack, Indetify, Enddatetime, Group);
            return SendAndReturn(100);
        }

        public Boolean DelHoliday()
        {
            if (isWorking()) return false;
            TcpPackge.DelHoliday();
            return SendAndReturn(100);
        }

        public Boolean AddHoliday(byte Index, DateTime frmdate, DateTime todate)
        {
            if (isWorking()) return false;
            TcpPackge.AddHoliday(Index, frmdate, todate);
            return SendAndReturn(100);
        }

        #endregion

        #region 系统类指令
        public Boolean Reset()
        {
            if (isWorking()) return false;
            TcpPackge.Reset();
            return SendAndReturn(3000);
        }

        public Boolean Restart()
        {
            if (isWorking()) return false;
            TcpPackge.Restart();
            return SendAndReturn(100);
        }
        #endregion

        #region 下载卡
        public Boolean AddCard(UInt32 Index, UInt64 CardNo, string pin, string name, byte TZ1, byte TZ2, byte TZ3, byte TZ4, byte Status, DateTime enddatetime)
        {
            if (isWorking()) return false;
            TcpPackge.AddCard(Index, CardNo, pin, name, TZ1, TZ2, TZ3, TZ4, Status, enddatetime);
            return SendAndReturn(400);
        }

        // isLastRecord 是否为最后一个包
        UInt16 PackIndex = 0;// 包序号 0..
        byte CardofPack = 0; // 卡在包中的序号 0..CardNumInPack-1
        public Boolean AddCards(Boolean isLastRecord, UInt16 CardIndex, UInt64 CardNo, string pin, string Name, byte TZ1, byte TZ2, byte TZ3, byte TZ4, byte Status, DateTime enddatetime)
        {
            Boolean result = false;
            if (isWorking()) return false;

            if (!TcpPackge.CheckCardNumInPack()) { TCPLastError = TcpErr_DataErr; return false; }

            PackIndex = Convert.ToUInt16(Math.Floor(Convert.ToDouble(CardIndex / TcpPackge.CardNumInPack)));
            CardofPack = Convert.ToByte(CardIndex % TcpPackge.CardNumInPack);

            TcpPackge.AddCards(PackIndex, CardofPack, CardIndex, CardNo, pin, TZ1, TZ2, TZ3, TZ4, Status, Name, enddatetime);
            if (((CardofPack + 1) == TcpPackge.CardNumInPack) || (isLastRecord))
            {
                result = SendAndReturn(1500);
                if (result)
                {
                    result = TcpPackge.CheckAddCardsResult(PackIndex);
                    if (!result) TCPLastError = TcpErr_ReData;
                }
                return result;
            }
            return true;
        }

        public Boolean DelCard(UInt16 Index)
        {
            if (isWorking()) return false;
            TcpPackge.DelCard(Index);
            return SendAndReturn(200);
        }

        public Boolean SetCardStatus(UInt16 Index, byte status)
        {
            if (isWorking()) return false;
            TcpPackge.SetCardStatus(Index, status);
            return SendAndReturn(200);
        }

        public Boolean ClearAllCards()
        {
            if (isWorking()) return false;
            TcpPackge.ClearAllCards();
            return SendAndReturn(1500);
        }
        #endregion

        #region 控制类指令

        public Boolean SetPass(byte Door, byte Reader, Boolean Pass)
        {
            if (isWorking()) return false;
            TcpPackge.SetPass(Door, Reader, Pass);
            return SendAndReturn(50);
        }

        public Boolean Opendoor(byte Door)
        {
            if (isWorking()) return false;
            TcpPackge.Opendoor(Door);
            return SendAndReturn(50);
        }

        public Boolean CloseDoor(byte Door)
        {
            if (isWorking()) return false;
            TcpPackge.Closedoor(Door);
            return SendAndReturn(50);
        }

        public Boolean LockDoor(byte Door, Boolean Lock)
        {
            if (isWorking()) return false;
            TcpPackge.LockDoor(Door, Lock);
            return SendAndReturn(50);
        }

        public Boolean OpenDoorLong(byte Door)
        {
            if (isWorking()) return false;
            TcpPackge.OpenDoorLong(Door);
            return SendAndReturn(50);
        }

        public Boolean SetAlarm(Boolean AClose, Boolean ALong)
        {
            if (isWorking()) return false;
            TcpPackge.SetAlarm(AClose, ALong);
            return SendAndReturn(50);
        }

        public Boolean Send485(byte[] data)
        {
            if (isWorking()) return false;
            TcpPackge.Send485(data);
            return SendAndReturn(50);
        }

        public Boolean SetFire(Boolean AClose, Boolean ALong)
        {
            if (isWorking()) return false;
            TcpPackge.SetFire(AClose, ALong);
            return SendAndReturn(50);
        }
        #endregion
    }
}
