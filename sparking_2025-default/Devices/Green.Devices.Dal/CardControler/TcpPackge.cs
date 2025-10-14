using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
/*
    tcp包裹 打包处理部分
    * 处理数据的组包，和解包
    * 各个控制器指令的组包和应答包 
*/

namespace Green.Devices.Dal.CardControler
{
    #region 变量结构类型
    public struct RChinaCard
    {
        public string SerialNo;
        public byte Ver;
        public DateTime Datetime;
        public byte Reader;
        public byte ReturnIndex;

        public string Name;
        public string Sex;
        public string Nation;
        public byte NationIndex;
        public DateTime Birthday;
        public string Address;
        public string Card;
        public string Dept;
        public DateTime DateFrm;
        public DateTime DateTo;
        public byte Event;
        public byte[] Photo;
    }

    public struct RAcsEvent
    {
        public byte EType;
        public byte Second, Minute, Hour, Day, Month;
        public DateTime Datetime;
        public int Year;
        public byte DoorStatus;
        public byte Ver;
        public Boolean Online;
        public byte FuntionByte, CardNumInPack;
        public string CardNo;
        public byte Door, EventType;
        public UInt16 CardIndex;
        public byte CardStatus;
        public byte Reader;
        public string SerialNo;
        public string QRCode;
        public byte TModel;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RTCPCardEvent
    {
        public byte stx;
        public byte temp;
        public byte cmd;
        public byte addr;
        public byte doorAdr;
        public UInt16 len;
        public UInt32 Card;
        public byte second;
        public byte minute;
        public byte hour;
        public byte day;
        public byte month;
        public byte year;
        public byte Event;
        public byte Door;
        public byte hasEvent;
        public byte index;
    }
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RTCPAlarmEvent
    {
        public byte stx;
        public byte temp;
        public byte cmd;
        public byte addr;
        public byte doorAdr;
        public UInt16 len;
        public byte second;
        public byte minute;
        public byte hour;
        public byte day;
        public byte month;
        public byte year;
        public byte Event;
        public byte Door;
        public byte hasEvent;
        public byte index;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RTCPCardStatus // 卡状态
    {
        public byte stx;
        public byte temp;
        public byte cmd;
        public byte addr;
        public byte doorAdr;
        public UInt16 len;
        public UInt16 EmployeeID;
        public byte CardStatus;
        public byte index;
        public byte hasEvent;
    }

    #endregion

    class TcpPackge
    {
        #region 常量
        private const byte Loc_Begin = 0;
        private const byte Loc_Temp = 1;
        public const byte Loc_Command = 2;
        private const byte Loc_Address = 3;
        private const byte Loc_DoorAddr = 4;
        private const byte Loc_Len = 5;
        private const byte Loc_Data = 7;

        public int OEMCode = 23456;//8569;//
        private byte LastCmd;
        #endregion

        #region 内部变量
        private byte[] BufferRX = new byte[1024];
        public byte[] BufferTX = new byte[1024];
        private byte ChinaImageIndex = 0;
        private RChinaCard ChinaCardEvent;

        private int nBytesWrite = 0;
        public int WriteNum;

        public byte CardNumInPack;
        protected byte FuntionByte;
        public string Serial;
        protected Boolean isEndDate, isOrPIN, isShowName, isCardorPIN;
        public byte Ver;
        #endregion

        #region 事件声明
        public delegate void TOnEventHandler(RAcsEvent Event, out byte relay, out Boolean OpenDoor, out Boolean Ack);   //Khai báo một delegate 
        public event TOnEventHandler OnEventHandler;        //Khai báo sự kiện 

        public delegate void TOnChinaEventHandler(RChinaCard Event, out Boolean OpenDoor, out Boolean Ack);   //声明委托 
        public event TOnChinaEventHandler OnChinaEventHandler;        //Khai báo sự kiện

        public delegate void TOnSetTcpTick();   //声明委托 
        public event TOnSetTcpTick OnSetTcpTick;        //声明事件

        public delegate void TOnClearWait(); //声明委托
        public event TOnClearWait OnClearWait; //声明事件

        public delegate Boolean TOnDoSenddata(); //声明委托
        public event TOnDoSenddata OnDoSenddata; //声明事件
        #endregion

        #region 数据包装
        public static object ByteToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, structPtr, size);
            object obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        protected void SetBufCommand(byte command)
        {
            BufferTX[Loc_Begin] = 0x02;
            nBytesWrite = Loc_Data;
            BufferTX[Loc_Command] = command;
            BufferTX[Loc_DoorAddr] = 0;
            BufferTX[Loc_Len] = 0;
            BufferTX[Loc_Len + 1] = 0;
            BufferTX[Loc_Address] = 0xff;
        }

        protected void SetBufDoorAddr(byte ADoorAddr)
        {
            BufferTX[Loc_DoorAddr] = ADoorAddr;
        }

        protected void PutBuf(byte AData)
        {
            BufferTX[nBytesWrite] = AData;
            nBytesWrite++;
        }

        protected void PutBuf(DateTime AData)
        {
            PutBuf(Convert.ToByte(AData.Hour));
            PutBuf(Convert.ToByte(AData.Minute));
        }

        protected void PutBufDate(DateTime AData)
        {
            if (AData.Year >= 2000)
                PutBuf(Convert.ToByte((AData.Year - 2000) & 0xff));
            else
                PutBuf(Convert.ToByte(AData.Year & 0xff));

            PutBuf(Convert.ToByte(AData.Month));
            PutBuf(Convert.ToByte(AData.Day));
        }

        protected void PutBufCard(UInt64 AData)
        {
            PutBuf(Convert.ToByte((AData) & 0xff));
            PutBuf(Convert.ToByte((AData >> 8) & 0xff));
            PutBuf(Convert.ToByte((AData >> 16) & 0xff));
            PutBuf(Convert.ToByte((AData >> 24) & 0xff));
        }

        protected void PutBufPin2(string AData)
        {
            UInt64 vPin = Convert.ToUInt64(AData);

            PutBuf(Convert.ToByte((vPin >> 8) & 0xff));
            PutBuf(Convert.ToByte(vPin & 0xff));
        }

        protected void PutBufPin4(string AData)
        {
            int i, len;
            byte[] p = new byte[8];
            byte[] v = new byte[4];

            byte[] ap = UTF8Encoding.UTF8.GetBytes(AData);

            try
            {
                len = ap.Length;
                for (i = 0; i < 8; i++) p[i] = 0xFF;

                if (len > 8) len = 8;
                for (i = 0; i < len; i++)
                    p[i] = Convert.ToByte(ap[i] - 0x30);

                for (i = 0; i < 4; i++)
                    v[i] = Convert.ToByte(((p[i * 2] << 4) & 0xF0) + (p[i * 2 + 1] & 0x0F));

                PutBuf(Convert.ToByte(v[0]));
                PutBuf(Convert.ToByte(v[1]));
                PutBuf(Convert.ToByte(v[2]));
                PutBuf(Convert.ToByte(v[3]));

            }
            catch
            {
            }
        }

        protected void PutBufCardName(string AData)
        {
            int i, len;
            byte[] aname = UTF8Encoding.Default.GetBytes(AData);
            byte[] p = new byte[8];
            try
            {
                len = aname.Length;
                if (len > 8) len = 8;

                for (i = 0; i < 8; i++) p[i] = 0;

                for (i = 0; i < len; i++)
                    p[i] = Convert.ToByte(aname[i]);

                for (i = 0; i < 8; i++)
                    PutBuf(Convert.ToByte(p[i]));   // 178
            }
            catch
            {
            }
        }

        public DateTime GetDatetime(byte Second, byte Minute, byte Hour, byte Day, byte Month, int Year)
        {
            try
            {
                return new DateTime(Year, Month, Day, Hour, Minute, Second);
            }
            catch { return new DateTime(); }
        }

        public String bcd2Str(byte[] bytes)
        {
            StringBuilder temp = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                temp.Append((byte)((bytes[i] & 0xf0) >> 4));
                temp.Append((byte)(bytes[i] & 0x0f));
            }
            return temp.ToString();//.Substring(0, 1).Equals("0") ? temp.ToString().Substring(1) : temp.ToString();
        }

        public String ASCii2Char(byte[] bytes)
        {
            byte c;
            StringBuilder temp = new StringBuilder(bytes.Length);

            for (int i = 0; i < bytes.Length; i++)
            {
                c = bytes[i];
                if ((c >= 0x30) && (c <= 0x39))
                    temp.Append((byte)(c & 0x0f));
                else
                    temp.Append((byte)(c));
            }
            return temp.ToString(); //.Substring(0, 1).Equals("0") ? temp.ToString().Substring(1) : temp.ToString();
        }

        public static DateTime StringToDateTime(string date)
        {
            try
            {
                return DateTime.ParseExact(date, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            }
            catch
            {
                return DateTime.Now;
            }

        }

        private byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        #endregion

        #region Gửi dữ liệu
        private Boolean CheckCs(byte[] buff, int loc)
        {
            int i;

            if (buff[loc] != 0x02) return false;
            if (buff[loc + 3] == 0x03)
                buff[loc + 3] = Convert.ToByte(0x03 + loc);

            int Bufferlen = buff[Loc_Len + 1 + loc] + buff[Loc_Len + loc] * 256 + Loc_Data + 2;
            if (Bufferlen > 1024) return false;
            if (buff[Bufferlen - 1 + loc] != 0x03) return false;

            Boolean result = false;
            byte cs = 0;
            int len = Bufferlen - 2;
            for (i = 0; i < len; i++)
            {
                cs ^= buff[i + loc];
            }
            result = (cs == buff[Bufferlen + loc - 2]);
            return result;
        }

        private Boolean CheckRxDataCS(byte[] buffRX, int len)
        {
            int i;
            if (len < 4) return false;

            int L = 0;
            Boolean re = false;

            for (i = 0; i < 20; i++)
            {
                re = CheckCs(buffRX, i);
                if (re) { L = i; break; }
            }

            if (L > 0)
            {
                for (i = 0; i < len; i++)
                {
                    buffRX[i] = buffRX[i + L];
                }
                len = len - L;
            }

            return re;
        }

        public void BeforeSend()
        {
            int i, datalen;
            byte OutBufferCS, cmd;

            datalen = nBytesWrite - Loc_Data;
            BufferTX[Loc_Len] = Convert.ToByte(datalen & 0xFF);
            BufferTX[Loc_Len + 1] = Convert.ToByte((datalen >> 8) & 0xFF);
            BufferTX[Loc_Temp] = Convert.ToByte(OEMCode & 0xff);

            OutBufferCS = 0;
            for (i = 0; i < nBytesWrite; i++)
                OutBufferCS = Convert.ToByte(OutBufferCS ^ BufferTX[i]);

            BufferTX[nBytesWrite] = OutBufferCS;
            BufferTX[nBytesWrite + 1] = 0x03;
            WriteNum = nBytesWrite + 2;

            cmd = BufferTX[Loc_Command];
            LastCmd = BufferTX[Loc_Command];
        }

        //===================================================================================================
        private void AnsEvent(byte Command, byte index, byte Door, Boolean opendoor)
        {
            SetBufCommand(Command);
            SetBufDoorAddr(Door);
            PutBuf(index);
            PutBuf(Convert.ToByte(opendoor));
            OnDoSenddata();
        }

        private void AnsEvent(byte Command, byte index)
        {
            SetBufCommand(Command);
            SetBufDoorAddr(0);
            PutBuf(index);
            OnDoSenddata();
        }

        private void AskHeart()
        {
            SetBufCommand(0x56);
            PutBuf(Convert.ToByte(OEMCode >> 8));
            PutBuf(Convert.ToByte(OEMCode & 0xFF));
            OnDoSenddata();
        }
        #endregion

        #region Hướng dẫn lớp tham số
        public void SetTime(DateTime datetime)
        {
            DateTime dt = datetime;
            SetBufCommand(0x07);
            PutBuf(Convert.ToByte(dt.Second));
            PutBuf(Convert.ToByte(dt.Minute));
            PutBuf(Convert.ToByte(dt.Hour));
            PutBuf(Convert.ToByte(dt.DayOfWeek + 1));
            PutBuf(Convert.ToByte(dt.Day));
            PutBuf(Convert.ToByte(dt.Month));
            if (dt.Year >= 2000)
                PutBuf(Convert.ToByte((dt.Year - 2000) & 0xff));
            else
                PutBuf(Convert.ToByte(dt.Year & 0xff));

        }

        public void SetDoor(byte Door, UInt16 OpenTime, UInt16 OpenOutTime, Boolean TooLongAlarm, UInt16 AlarmMast, UInt16 AlarmTime,
            Boolean DoublePath, byte CardsOpen, byte CardsOpenInOut)
        {
            SetBufCommand(0x61);
            SetBufDoorAddr(Convert.ToByte(Door + 1));

            PutBuf(Convert.ToByte(OpenTime));
            PutBuf(Convert.ToByte(OpenOutTime));
            PutBuf(Convert.ToByte(DoublePath));
            PutBuf(Convert.ToByte(TooLongAlarm));
            PutBuf(Convert.ToByte(OpenTime >> 8));
            PutBuf(Convert.ToByte(AlarmMast));
            PutBuf(Convert.ToByte(AlarmTime));
            PutBuf(Convert.ToByte(AlarmTime >> 8));
            PutBuf(Convert.ToByte(CardsOpen));
            PutBuf(Convert.ToByte(CardsOpenInOut));
        }

        public void SetControl(UInt16 FireTime, UInt16 AlarmTime, string DuressPIN, byte LockEach)
        {
            SetBufCommand(0x63);
            PutBuf(Convert.ToByte(LockEach));
            PutBuf(Convert.ToByte(FireTime));
            PutBuf(Convert.ToByte(FireTime >> 8));
            PutBuf(Convert.ToByte(AlarmTime));
            PutBuf(Convert.ToByte(AlarmTime >> 8));

            if (isOrPIN)
            {
                PutBufPin2(DuressPIN);
            }
            else if (isCardorPIN)
            {
                PutBufPin4(DuressPIN);
            }
            else
                PutBufPin2(DuressPIN);
            /*
            PutBuf(Convert.ToByte(pin[0]));
            PutBuf(Convert.ToByte(pin[1]));
            PutBuf(Convert.ToByte(pin[2]));
            PutBuf(Convert.ToByte(pin[3]));*/
        }

        public void DelTimeZone(byte Door)
        {
            SetBufCommand(0x0f);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
        }

        public void AddTimeZone(UInt16 Door, byte Index, DateTime frmtime, DateTime totime, byte Week, Boolean PassBack, byte Indetify, DateTime Enddatetime, byte Group)
        {
            byte vIndetify = Indetify;
            SetBufCommand(0x0d);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
            PutBuf(Convert.ToByte(Index));
            PutBuf(frmtime);
            PutBuf(totime);
            PutBuf(Convert.ToByte(Week));
            if (PassBack)
                vIndetify |= 0x80;
            PutBuf(Convert.ToByte(vIndetify));
            PutBufDate(Enddatetime);
            PutBuf(Convert.ToByte(Group));
        }

        public void DelHoliday()
        {
            SetBufCommand(0x0c);
        }

        public void AddHoliday(byte Index, DateTime frmdate, DateTime todate)
        {
            SetBufCommand(0x09);

            PutBuf(Convert.ToByte(Index));
            PutBufDate(frmdate);
            PutBufDate(todate);
        }

        #endregion

        #region Hướng dẫn lớp phụ

        public void Reset()
        {
            SetBufCommand(0x04);
        }

        public void Restart()
        {
            SetBufCommand(0x05);
        }
        #endregion

        #region Tải xuống thẻ
        public void AddCard(UInt32 Index, UInt64 CardNo, string pin, string name, byte TZ1, byte TZ2, byte TZ3, byte TZ4, byte Status, DateTime enddatetime)
        {
            SetBufCommand(0x62);
            PutBuf(Convert.ToByte(Index & 0xff));
            PutBuf(Convert.ToByte(Index >> 8));
            PutBufCard(CardNo);

            if (isOrPIN)
            {
                PutBufPin2(pin);
            }
            else if (isCardorPIN)
            {
                PutBufPin4(pin);
            }
            else
                PutBufPin2(pin);

            PutBuf(Convert.ToByte(TZ1));
            PutBuf(Convert.ToByte(TZ2));
            PutBuf(Convert.ToByte(TZ3));
            PutBuf(Convert.ToByte(TZ4));

            if (isEndDate)
            {
                PutBufDate(enddatetime);
                PutBuf(Convert.ToByte(enddatetime.Hour));
                PutBuf(Convert.ToByte(enddatetime.Minute));
            }
            else
            {
                PutBuf(Convert.ToByte(0));
                PutBuf(Convert.ToByte(0));
                PutBuf(Convert.ToByte(0));
                PutBuf(Convert.ToByte(0));
                PutBuf(Convert.ToByte(Status));
            }

            if (isShowName)
                PutBufCardName(name);
        }

        public void DelCard(UInt16 Index)
        {
            SetBufCommand(0x16);
            PutBuf(Convert.ToByte(Index & 0xff));
            PutBuf(Convert.ToByte(Index >> 8));
        }

        public void SetCardStatus(UInt16 Index, byte status)
        {
            SetBufCommand(0x66);
            PutBuf(Convert.ToByte(Index & 0xff));
            PutBuf(Convert.ToByte(Index >> 8));
            PutBuf(Convert.ToByte(status));
        }

        public Boolean CheckCardNumInPack()
        {
            Boolean result = false;
            if (CardNumInPack < 15)
                return result;
            if (CardNumInPack > 45)
                return result;
            return true;
        }

        public Boolean CheckAddCardsResult(UInt16 PackIndex)
        {
            return BufferRX[Loc_Data + 0] * 256 + BufferRX[Loc_Data + 1] == (PackIndex + 1);
        }

        public Boolean AddCards(UInt16 PackIndex, byte CardofPack, UInt16 CardIndex, UInt64 CardNo, string pin,
            byte TZ1, byte TZ2, byte TZ3, byte TZ4, byte Status, string Name, DateTime enddatetime)
        {
            this.BufferTX[Loc_Command] = 0x88;
            BufferTX[Loc_Data + 0] = Convert.ToByte(((PackIndex + 1) & 0xFF)); //
            BufferTX[Loc_Data + 1] = Convert.ToByte(((PackIndex + 1) >> 8));
            BufferTX[Loc_Data + 2] = Convert.ToByte(CardofPack + 1); //
            if (CardofPack == 0)
            {
                SetBufCommand(0x88);
                nBytesWrite = Loc_Data + 3;
                PutBuf(Convert.ToByte(CardIndex & 0xFF));
                PutBuf(Convert.ToByte((CardIndex >> 8) & 0xFF));
            }
            SendEmpTcpOne(CardNo, pin, TZ1, TZ2, TZ3, TZ4, Status, Name, enddatetime);
            return true;
        }

        private void SendEmpTcpOne(UInt64 CardNo, string pin, byte TZ1, byte TZ2, byte TZ3, byte TZ4, byte Status, string Name, DateTime enddatetime)
        {
            PutBufCard(CardNo);
            if (isOrPIN)
            {
                PutBufPin2(pin);
            }
            else if (isCardorPIN)
            {
                PutBufPin4(pin);
            }
            else
                PutBufPin2(pin);

            PutBuf(Convert.ToByte(TZ1));
            PutBuf(Convert.ToByte(TZ2));
            PutBuf(Convert.ToByte(TZ3));
            PutBuf(Convert.ToByte(TZ4));

            if (isEndDate)
            {
                PutBufDate(enddatetime);
                PutBuf(Convert.ToByte(enddatetime.Hour));
                PutBuf(Convert.ToByte(enddatetime.Minute));
            }
            if (isShowName)
                PutBufCardName(Name);
        }

        public void ClearAllCards()
        {
            SetBufCommand(0x17);
        }
        #endregion

        #region Kiểm soát hướng dẫn lớp
        public void Opendoor(byte Door)
        {
            SetBufCommand(0x2C);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
        }

        public void Closedoor(byte Door)
        {
            SetBufCommand(0x2e);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
        }
        public void SetPass(byte Door, byte Reader, Boolean Pass)
        {
            SetBufCommand(0x5A);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
            PutBuf(Convert.ToByte(0));
            PutBuf(Convert.ToByte(Reader));
            PutBuf(Convert.ToByte(0));
            PutBuf(Convert.ToByte(!Pass));
            PutBuf(Convert.ToByte(0));
        }
        public void LockDoor(byte Door, Boolean Lock)
        {
            SetBufCommand(0x2f);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
            PutBuf(Convert.ToByte(Lock));
            PutBuf(Convert.ToByte(Lock));
        }

        public void OpenDoorLong(byte Door)
        {
            SetBufCommand(0x2d);
            SetBufDoorAddr(Convert.ToByte(Door + 1));
        }

        public void SetAlarm(Boolean AClose, Boolean ALong)
        {
            SetBufCommand(0x18);
            PutBuf(Convert.ToByte(AClose));
            PutBuf(Convert.ToByte(ALong));
        }

        public void SetFire(Boolean AClose, Boolean ALong)
        {
            SetBufCommand(0x19);
            PutBuf(Convert.ToByte(AClose));
            PutBuf(Convert.ToByte(ALong));
        }

        public void Send485(byte[] data)
        {
            SetBufCommand(0xB1);
            int len = data.Length;
            int i;
            for (i = 0; i < len; i++)
                PutBuf(Convert.ToByte(data[i]));
        }
        #endregion

        #region Xử lý tiếp nhận dữ liệu

        // 控制器发来的心跳
        private void DoFormatStatusvent()
        {
            byte vOE;
            int vOEM;
            Boolean Ack, vopenDoor;
            RAcsEvent Event;

            Event = new RAcsEvent();

            Event.Second = (BufferRX[Loc_Data + 6]);
            Event.Minute = BufferRX[Loc_Data + 5];
            Event.Hour = BufferRX[Loc_Data + 4];
            Event.Day = BufferRX[Loc_Data + 3];
            Event.Month = BufferRX[Loc_Data + 2];
            Event.Year = BufferRX[Loc_Data + 1] + 2000;

            vOE = Convert.ToByte((~BufferRX[Loc_Data + 19]) & 0xff);
            vOEM = (vOE * 256) + Convert.ToByte((~BufferRX[Loc_Data + 20]) & 0xff);

            if (OEMCode != 23456)
            {
                if (vOEM != OEMCode)
                {
                    //sock.Close();
                    return;
                }
            }
            else
                if (vOE == 0x23) return;

            Event.DoorStatus = BufferRX[Loc_Data + 7];

            Event.Ver = BufferRX[Loc_Data + 18];
            FuntionByte = BufferRX[Loc_Data + 10];
            Event.FuntionByte = FuntionByte;

            Ver = Event.Ver;

            isEndDate = (Ver >= 81) & ((FuntionByte & 0x01) == 0x01);
            isOrPIN = (Ver >= 81) & ((FuntionByte & 0x04) == 0x04);
            isShowName = (Ver >= 81) & ((FuntionByte & 0x08) == 0x08);
            isCardorPIN = (Ver >= 81) & ((FuntionByte & 0x10) == 0x10);

            if (Ver >= 90)
            {
                CardNumInPack = (BufferRX[Loc_Data + 8]);
            }
            else
            {
                if (isEndDate)
                {
                    if (isShowName)
                        CardNumInPack = 20;
                    else
                        CardNumInPack = 30;
                }
                else
                {
                    if (isShowName)
                        CardNumInPack = 30;
                    else
                        CardNumInPack = 45;
                }
            }
            Event.Datetime = GetDatetime(Event.Second, Event.Minute, Event.Hour, Event.Day, Event.Month, Event.Year);

            byte[] returnBytes = new byte[6];
            Array.ConstrainedCopy(BufferRX, Loc_Data + 21, returnBytes, 0, 6);
            Event.SerialNo = Encoding.ASCII.GetString(returnBytes);
            Serial = Event.SerialNo;
            Event.CardNumInPack = CardNumInPack;

            Event.EType = 0;
            Event.Online = true;
            Ack = true;
            byte relay = 0;
            OnEventHandler(Event, out relay, out vopenDoor, out Ack);
            if (Ack)
                AskHeart();
        }

        // 刷卡记录
        private void DoFormCardevent()
        {
            byte ReturnIndex;
            byte[] vCard = new byte[4];
            Boolean Ack, vopenDoor;
            RAcsEvent Event = new RAcsEvent();
            RTCPCardEvent CardEvent = (RTCPCardEvent)ByteToStruct(BufferRX, typeof(RTCPCardEvent));

            Event.Second = CardEvent.second;
            Event.Minute = CardEvent.minute;
            Event.Hour = CardEvent.hour;
            Event.Day = CardEvent.day;
            Event.Month = CardEvent.month;
            Event.Year = CardEvent.year + 2000;

            Event.CardNo = Convert.ToString(CardEvent.Card);
            Event.EventType = Convert.ToByte(CardEvent.Event & 0x7F);
            Event.Reader = Convert.ToByte((CardEvent.Event & 0x80) >> 7);
            Event.Door = CardEvent.Door;
            ReturnIndex = CardEvent.index;

            Ack = true;
            vopenDoor = false;
            Event.Online = true;
            Event.Datetime = GetDatetime(Event.Second, Event.Minute, Event.Hour, Event.Day, Event.Month, Event.Year);
            Event.SerialNo = Serial;
            Event.EType = 1;

            byte relay = Event.Reader;
            OnEventHandler(Event, out relay, out vopenDoor, out Ack);
            if (Ack)
                AnsEvent(0x53, ReturnIndex, relay, vopenDoor);

            //  Opendoor(1); OnDoSenddata();
        }

        // 接收卡状态，用于防潜返
        private void DoFormCardStatus()
        {
            byte ReturnIndex;
            Boolean Ack, vopenDoor;
            RAcsEvent Event = new RAcsEvent();
            RTCPCardStatus CardStatus = (RTCPCardStatus)ByteToStruct(BufferRX, typeof(RTCPCardStatus));
            /*
            Event.CardIndex = Convert.ToUInt16(BufferRX[Loc_Data + 0] + BufferRX[Loc_Data + 1] * 256);
            Event.CardStatus = BufferRX[Loc_Data + 2];
            ReturnIndex = BufferRX[Loc_Data + 3];*/

            Event.CardIndex = CardStatus.EmployeeID;
            Event.CardStatus = CardStatus.CardStatus;
            ReturnIndex = CardStatus.index;

            Event.Online = true;
            Event.Datetime = GetDatetime(Event.Second, Event.Minute, Event.Hour, Event.Day, Event.Month, Event.Year);
            Event.SerialNo = Serial;
            Event.EType = 3;
            vopenDoor = false;
            Ack = true;
            byte relay = 0;//
            OnEventHandler(Event, out relay, out vopenDoor, out Ack);
            if (Ack)
                AnsEvent(0x52, ReturnIndex);
        }

        // 接收报警信息
        private void DoForMatAlarmevent()
        {
            byte ReturnIndex;
            Boolean Ack, vopenDoor;
            RAcsEvent Event = new RAcsEvent();
            RTCPAlarmEvent AlarmEvent = (RTCPAlarmEvent)ByteToStruct(BufferRX, typeof(RTCPAlarmEvent));

            Event.Second = AlarmEvent.second;
            Event.Minute = AlarmEvent.minute;
            Event.Hour = AlarmEvent.hour;
            Event.Day = AlarmEvent.day;
            Event.Month = AlarmEvent.month;
            Event.Year = AlarmEvent.year + 2000;

            Event.EventType = Convert.ToByte(AlarmEvent.Event & 0x7F);
            Event.Reader = Convert.ToByte((AlarmEvent.Event & 0x80) >> 7);
            Event.Door = AlarmEvent.Door;
            ReturnIndex = AlarmEvent.index;

            Event.Online = true;
            Ack = true;
            vopenDoor = false;
            Event.Datetime = GetDatetime(Event.Second, Event.Minute, Event.Hour, Event.Day, Event.Month, Event.Year);
            Event.SerialNo = Serial;
            Event.EType = 2;
            byte relay = (byte)((byte)Event.Reader + (byte)Event.Door);
            OnEventHandler(Event, out relay, out vopenDoor, out Ack);
            if (Ack)
                AnsEvent(0x54, ReturnIndex, relay, vopenDoor);
        }

        // 接收身份证信息
        private void DoForMatChinaCard()
        {
            byte ReturnIndex;
            byte[] vCard = new byte[4];
            Boolean Ack, vopenDoor;
            RChinaCard Event;

            ChinaImageIndex = 0;

            byte[] Name = new byte[12];     //30
            // byte Sex=0;          //2
            byte[] Nation = new byte[4];    //4
            byte[] Birthday = new byte[8];  //16
            byte[] Address = new byte[70];  //70
            byte[] Card = new byte[18];     //36
            byte[] Dept = new byte[30];     //30
            byte[] DateFrm = new byte[8];   //16
            byte[] DateTo = new byte[8];    //16    

            Event = new RChinaCard();

            byte HasImage = BufferRX[Loc_Data + 0];
            Event.Reader = BufferRX[Loc_Data + 1];
            Event.Event = BufferRX[Loc_Data + 2];

            Array.Copy(BufferRX, Loc_Data + 6, Name, 0, 12);
            string name = Encoding.Unicode.GetString(Name);
            Event.Name = name;
            Event.Sex = "true";
            //if (BufferRX[Loc_Data +6 + 12] == 0x31)
            // Event.Sex = "男";
            if (BufferRX[Loc_Data + 6 + 12] == 0x30)
                Event.Sex = "false"; // "女";

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1, Nation, 0, 4);
            string value = Encoding.Unicode.GetString(Nation);

            string[] nationality ={"汉","蒙古","回","藏","维吾尔","苗","彝","壮","布依",
                                  "朝鲜","满","侗","瑶","白","土家","哈尼","哈萨克","傣","黎","傈僳","佤","畲","高山","拉祜",
                                  "水","东乡","纳西","景颇","柯尔克孜","土","达斡尔","仫佬","羌","布朗","撒拉","毛南","仡佬",
                                  "锡伯","阿昌","普米","塔吉克","怒","乌孜别克","俄罗斯","鄂温克","德昂","保安","裕固","京",
                                  "塔塔尔","独龙","鄂伦春","赫哲","门巴","珞巴","基诺"};

            int c = Convert.ToInt16(value);
            if (c > 0) c--;
            if (c < nationality.Length)
            {
                Event.NationIndex = Convert.ToByte(c);
                Event.Nation = nationality[c];
            }

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1 + 4, Birthday, 0, 8);
            value = ASCii2Char(Birthday);
            // Event.Birthday = value;
            Event.Birthday = StringToDateTime(value);

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1 + 4 + 8, Address, 0, 70);
            value = Encoding.Unicode.GetString(Address);
            Event.Address = value;

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1 + 4 + 8 + 70, Card, 0, 18);
            value = ASCii2Char(Card);
            Event.Card = value;

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1 + 4 + 8 + 70 + 18, Dept, 0, 30);
            value = Encoding.Unicode.GetString(Dept);
            Event.Dept = value;

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1 + 4 + 8 + 70 + 18 + 30, DateFrm, 0, 8);
            value = ASCii2Char(DateFrm);
            Event.DateFrm = StringToDateTime(value);

            Array.Copy(BufferRX, Loc_Data + 6 + 12 + 1 + 4 + 8 + 70 + 18 + 30 + 8, DateTo, 0, 8);
            value = ASCii2Char(DateTo);
            Event.DateTo = StringToDateTime(value);

            ReturnIndex = 0;

            Ack = true;
            vopenDoor = false;
            Event.Datetime = DateTime.Now;
            Event.Photo = new byte[1024];
            Event.SerialNo = Serial;
            ChinaCardEvent = Event;

            if (HasImage == 0)
            {
                OnChinaEventHandler(Event, out vopenDoor, out Ack);
                if (Ack)
                    AnsEvent(0x58, ReturnIndex, Event.Reader, vopenDoor);
            }
        }

        // 接收身份证照片
        private void DoForMatChinaCardImage()
        {
            Boolean Ack, vopenDoor;
            byte page;
            int len;

            len = BufferRX[5] * 256 + BufferRX[6];
            page = BufferRX[3];

            if (len > 256) return;
            if (page > 3) return;
            //if (page * 256 + len > 1024) return;

            Array.Copy(BufferRX, Loc_Data, ChinaCardEvent.Photo, page * 256, len);

            ChinaImageIndex++;
            if (ChinaImageIndex >= 4)
            {
                OnChinaEventHandler(ChinaCardEvent, out vopenDoor, out Ack);
                if (Ack)
                    AnsEvent(0x58, 0, ChinaCardEvent.Reader, vopenDoor);
            }
        }

        // 接收字符串如二维码 
        private void DoForMatOrCode()
        {
            RAcsEvent Event = new RAcsEvent();
            byte ReturnIndex;
            Boolean Ack, vopenDoor;

            int len = BufferRX[5] * 256 + BufferRX[6];
            if (len > 5) len = len - 5;

            char[] data = new char[len];
            Array.Copy(BufferRX, Loc_Data + 5, data, 0, len);

            Ack = true;
            vopenDoor = false;

            Event.QRCode = new string(data);
            if (!string.IsNullOrEmpty(Event.QRCode))
            {
                Event.QRCode = Event.QRCode.Trim().Replace("\n", "").Replace("\r", "").Replace("\0", "");
            }

            Event.Reader = BufferRX[Loc_Data + 1];
            Event.Door = BufferRX[Loc_Data];

            Event.Online = true;
            Event.Datetime = DateTime.Now;
            Event.SerialNo = Serial;

            if ((Event.Reader & 0x01) == 1) Event.EventType = 33; else Event.EventType = 32;

            Event.EventType = 34;

            Event.EType = 4;
            ReturnIndex = 0;
            byte relay = Event.Reader;
            OnEventHandler(Event, out relay, out vopenDoor, out Ack);
            if (Ack)
                AnsEvent(0x58, ReturnIndex, relay, vopenDoor);
        }

        public void HandleMessage(byte rt, byte[] buffRX, int len)
        {
            //buffRX.CopyTo(this.BufferRX, 0);
            if (rt != 0) return; // 不是接收数据
            try
            {
                Array.ConstrainedCopy(buffRX, 0, BufferRX, 0, len);
                if (CheckRxDataCS(buffRX, buffRX.Length))
                {
                    OnSetTcpTick();
                    switch (buffRX[Loc_Command])
                    {
                        case 0x56: DoFormatStatusvent(); break;
                        case 0x52: DoFormCardStatus(); break;   // card status

                        case 0x53: DoFormCardevent(); break;   // card event
                        case 0x54: DoForMatAlarmevent(); break;   // alarm
                        case 0x55: DoForMatOrCode(); break;
                        case 0x58: DoForMatChinaCard(); break;   // 
                        case 0x59: DoForMatChinaCardImage(); break;   // 

                        default:
                            if (buffRX[Loc_Command] == LastCmd) OnClearWait();
                            break;
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
