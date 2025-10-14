using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Timers;
using Green.Devices.Dal.ProxiesReader;
using PCSC.Monitoring;

namespace Green.Devices.Dal
{

    public class ProxiesCardReader : IGreenCardReader
    {
        public string Port { get; set; }
        public bool IsConnected { get; private set; }

        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;

        private read_write _readerObj;
        private ushort baud = 19200;
        private string _currentCardId = "";

        private Timer _timer = new Timer();

        public ProxiesCardReader()
        {
            _readerObj = new read_write();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 200;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            byte[] Cardnumber = null;
            Cardnumber = _readerObj.Read_Em4001();
            var cardid = _readerObj.GetStringByData(Cardnumber);
            var result = new GreenCardReaderEventArgs { CardReader = this };
            if(!string.IsNullOrWhiteSpace(_currentCardId) &&
                string.IsNullOrWhiteSpace(cardid))
            {
                _currentCardId = cardid;
                if (TakingOffCompleted != null)
                {
                    TakingOffCompleted(this, result);
                }
            }

            if(_currentCardId != cardid && !string.IsNullOrWhiteSpace(cardid))
            {
                result.CardID = _currentCardId = cardid;

                if (ReadingCompleted != null)
                {
                    ReadingCompleted(this, result);
                }
            }
        }

        public IGreenCardReaderInfo Info { get; set; }
        public CardState State { get; set; }

        public static ICollection<string> GetReaders()
        {
            string[] ComList = SerialPort.GetPortNames();
            int[] ComNumberList = new int[ComList.Length];//tạo mảng giá trị chứa các số của cổng com
            for (int i = 0; i < ComList.Length; i++) //tạo
            {
                ComNumberList[i] = int.Parse(ComList[i].Substring(3));//lấy kí tự thứ 3 của cổng om trên máy
            }

            return ComList;
        }

        public bool Connect()
        {
            if(State != CardState.IsReady)
            {
                ushort comNo = Convert.ToUInt16(Port.Substring(3));
                try
                {
                    _readerObj.OpenCom(comNo, baud);
                    _timer.Start();
                }
                catch (Exception ex)
                {
                    throw;
                }
                
                this.State = CardState.IsReady;
            }

            return this.State == CardState.IsReady;
        }

        public void DisConnect()
        {
            this.State = CardState.IsStop;
            try
            {
                _readerObj.CloseCom();
                _timer.Stop();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public object GetController()
        {
            return null;
        }
    }
}
