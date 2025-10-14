using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.Devices.Dal.CardControler
{
    public enum DoorType
    {
        Door1,
        Door2,
        Door3,
        Door4
    }
    public enum AlarmType
    {
        Alarm1,
        Alarm2,
        Alarm3,
        Alarm4
    }
    public enum Msg485Type
    {
        Msg485_1,
        Msg485_2,
        Msg485_3,
        Msg485_4
    }
    public enum MsgType
    {
        Msg1,
        Msg2,
        Msg3,
        Msg4
    }
    public enum TypeInUse
    {
        Door,
        Alarm,
        Msg,
        Msg485
    }
    public enum ExitType
    {
        Exit1,
        Exit2,
        Exit3,
        Exit4
    }
    public interface IGreenControllerInfo
    {
        TypeInUse TypeInUse { get; set; }
        DoorType DoorType { get; set; }
        AlarmType AlarmType { get; set; }
        MsgType MsgType { get; set; }
        Msg485Type Msg485Type { get; set; }
        ExitType ExitType { get; set; }
        string Ip { get; set; }
        ushort Port { get; set; }
        
    }
    public interface IGreenController
    {
        IGreenControllerInfo Info { get; set; }
        bool Connect();
        bool DisConnect();
        bool OpenElectricGun(string source, int timeTick);
        bool CloseElectricGun(string source);
        bool OpenFire();
        bool CloseFire();
        GreenTcpIpControllerInfo GetController();
    }
}
