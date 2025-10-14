using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Green.Devices.Dal.CashierCounter
{
    public class ItcCashier
    {
        byte[] bufferSendReject = { (byte)CmdType.Reject };
        byte[] bufferSendAccept = { (byte)CmdType.Accept };
        byte[] bufferSendEnable = { (byte)CmdType.Enable };
        byte[] bufferSendDisable = { (byte)CmdType.Disable };
        byte[] bufferSendReset = { (byte)CmdType.Reset };
        byte[] bufferSendStatus = { (byte)CmdType.Status };
        private SerialPort serial;
        private int tmp;
        private Task _task;
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[(sender as SerialPort).ReadBufferSize];
            int bytesRead = (sender as SerialPort).Read(buffer, 0, buffer.Length);
            if (bytesRead >= 2 && buffer[0] == 0x81 && buffer[1] >= 0x40 && buffer[1] <= 0x48)
            {
                SetTemp(buffer[1]);
                serial.Write(bufferSendAccept, 0, 1);
            }
            else if (bytesRead == 1 && buffer[0] >= 0x40 && buffer[0] <= 0x48)
            {
                SetTemp(buffer[0]);
                serial.Write(bufferSendAccept, 0, 1);
            }
            else if (buffer[0] == 0x10)
            {
                DoAccept(tmp);
                tmp = 0;
            }    
            
        }
        private void DoAccept(int number)
        {
            if (number > 0 && Transaction !=null)
            {
                Transaction.Amount++;
                Transaction.Total += number;  
            }
        }
        private void SetTemp(byte btemp)
        {
            switch (btemp)
            {
                case (byte)AmountMoney.M1000:
                    tmp = 1000;
                    break;
                case (byte)AmountMoney.M2000:
                    tmp = 2000;
                    break;
                case (byte)AmountMoney.M5000:
                    tmp = 5000;
                    break;
                case (byte)AmountMoney.M10000:
                    tmp = 10000;
                    break;
                case (byte)AmountMoney.M20000:
                    tmp = 20000;
                    break;
                case (byte)AmountMoney.M50000:
                    tmp = 50000;
                    break;
                case (byte)AmountMoney.M100000:
                    tmp = 100000;
                    break;
                case (byte)AmountMoney.M200000:
                    tmp = 200000;
                    break;
                case (byte)AmountMoney.M500000:
                    tmp = 500000;
                    break;
                default:
                    tmp = 0;
                    break;
            }
        }
        private enum CmdType
        {
            Reset = 0x30,
            Enable = 0x3E,
            Disable = 0x5E,
            Accept = 0x02,
            Reject = 0x0F,
            Status = 0x0C
        }
        private enum AmountMoney
        {
            M1000 = 0x40,
            M2000 = 0x41,
            M5000 = 0x42,
            M10000 = 0x43,
            M20000 = 0x44,
            M50000 = 0x45,
            M100000 = 0x46,
            M200000 = 0x47,
            M500000 = 0x48
        }
        private enum StatusCashMoney
        {
            MotorFailure = 0x20,        //Lỗi động cơ
            ChecksumError = 0x21,       //Lỗi checksum
            BillJam = 0x22,             //Bill Jam 
            BillRemove = 0x23,          //Bill Remove
            StackerOpen = 0x24,         //Stacker Open
            SensorProblem = 0x25,       //Sensor Problem
            BillFish = 0x27,            //Bill Fish
            StackerProblem = 0x28,      //Stacker Problem 
            BillReject = 0x29,          //Bill Reject 
            InvalidCommand = 0x2A,      //Invalid Command
            Reserved = 0x2E,            //Reserved 
            ResponseError = 0x2F,       //Response when Error Status is Exclusion
            BillEnable = 0x3E,          //Bill Acceptor Enable 
            BillDisable = 0x5E,         //Bill Acceptor Disable 
        }
        private void Connect()
        {
            if(serial.PortName!=ComName)
            {
                Close();
                serial.PortName = ComName;
            }
            if (!serial.IsOpen)
            {    
                serial.Open();
            }
          
        }
        private void Close()
        {
            if (serial.IsOpen)
            {
                serial.Close();
            }
        }
        public string ComName { get; set; }
        public ItcCashier()
        {
            serial = new SerialPort();
            serial.BaudRate = 9600;
            serial.DataBits = 8;
            serial.StopBits = StopBits.One;
            serial.Handshake = Handshake.None;
            serial.Parity = Parity.Even;
            serial.DataReceived += Serial_DataReceived;
        }
        public ItcCashierInfo Transaction { get; private set; }
        private bool IsDoing = false;
        private void Opentransaction(ItcCashierInfo tracnsaction)
        {
            Transaction = tracnsaction;
            Transaction.Amount = 0;
            Transaction.Total = 0;
            Connect();
            Enable();
            Transaction.Doing = true;
            //Accept divice
            serial.Write(bufferSendAccept, 0, 1);
            //Write 0x3E to enable device
            serial.Write(bufferSendEnable, 0, 1);
            Thread.Sleep(500);
            var CompareTime = DateTime.Now;
            tmp = 0;
            Transaction.Doing = IsDoing = true;
            while (IsDoing && Transaction.Total < Transaction.Bill && (CompareTime - Transaction.From).TotalSeconds < Transaction.TimeOutSeconds)
            {
                CompareTime = DateTime.Now;   
                if (Transaction.Total >= Transaction.Bill)
                    break;
                else
                    Thread.Sleep(300);
            }
            Transaction.To = CompareTime;
            Transaction.Doing = IsDoing = false;
            Disable();
            Close();
        }
        public void TransactionProcess(ItcCashierInfo tracnsaction)
        {
            if (Free)
                //Opentransaction(tracnsaction);
                _task = Task.Factory.StartNew(() => Opentransaction(tracnsaction));
        }
        public ItcCashierInfo CloseTransaction()
        {
            if (Transaction != null)
            {
                Transaction.To = DateTime.Now;
                Transaction.Doing = IsDoing = false;
            }
            IsDoing = false;
            Disable();
            Close();
            return Transaction;
        }
        public void Reset()
        {
            Connect();
            //send Reset
            serial.Write(bufferSendReset, 0, 1);
            //After Reset, send Accept 0x02
            serial.Write(bufferSendAccept, 0, 1);
        }
        public void Enable()
        {
            Connect();
            serial.Write(bufferSendEnable, 0, 1);
        }
        public bool Free { get { return !IsDoing; } }
        public void Disable()
        {
            Connect();
            serial.Write(bufferSendDisable, 0, 1);
        }
    }
    public static class ItcCashierWrapper
    {
        private static List<ItcCashier> Cashiers;
        public static ItcCashier Add(string ComName)
        {
            if (Cashiers == null)
            {
                Cashiers = new List<ItcCashier>(); 
            }
            ItcCashier c = Cashiers.FirstOrDefault(cs => cs.ComName == ComName);
            if (c == null)
            {
                c = new ItcCashier() { ComName = ComName };
                Cashiers.Add(c);
            }
            return c;
        }
        public static void Remove(string ComName)
        {
            if(Cashiers!=null)
            {
                var c = Cashiers.FirstOrDefault(cs => cs.ComName == ComName);
                if(c!=null)
                {
                    c.CloseTransaction();
                    Cashiers.Remove(c);
                }
            }
        }
        public static ItcCashier GetItcCashier(string ComName)
        {
            return Add(ComName);
        }
    }
    public class ItcCashierInfo
    {
        public Action<CashierInfoField, object> PropertyChanged;
      
        private bool doing;
        private DateTime from;
        private DateTime to;
        private int timeoutSeconds;
        private int amount;
        private int total;
        private int bill;
      
        public bool Doing
        {
            get { return doing; }
            set
            {
                if (doing == value)
                    return;
                doing = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.Doing, doing);
            }
        }
    
        public DateTime From
        {
            get { return from; }
            set
            {
                if (from == value)
                    return;
                from = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.From, from);
            }
        }
        public DateTime To
        {
            get { return to; }
            set
            {
                if (to == value)
                    return;
                to = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.To, to);
            }
        }
        public int TimeOutSeconds
        {
            get { return timeoutSeconds; }
            set
            {
                if (timeoutSeconds == value)
                    return;
                timeoutSeconds = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.TimeOut, timeoutSeconds);
            }
        }
        public int Amount
        {
            get { return amount; }
            set
            {
                if (amount == value)
                    return;
                amount = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.Amount, amount);
            }
        }
        public int Total
        {
            get { return total; }
            set
            {
                if (total == value)
                    return;
                total = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.Total, total);
            }
        }
        public int Bill
        {
            get { return bill; }
            set
            {
                if (bill == value)
                    return;
                bill = value;
                if (PropertyChanged != null)
                    PropertyChanged(CashierInfoField.Bill, bill);
            }
        }
        public enum CashierInfoField
        {
            From,
            To,
            TimeOut,
            Amount,
            Total,
            Bill,    
            Doing
        }
    }
}
