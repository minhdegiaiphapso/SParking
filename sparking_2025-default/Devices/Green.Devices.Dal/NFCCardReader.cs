using System;
using System.Collections.Generic;
using System.Linq;
using PCSC.Monitoring;

namespace Green.Devices.Dal
{

    public class NFCCardReader : IGreenCardReader
    {
        public string Reader { get; set; }
        public bool IsConnected { get; private set; }
        private PCSC.ISCardContext _ctx;
        private PCSC.Monitoring.ISCardMonitor _monitor;

        public event GreenCardReaderEventHandler ReadingCompleted;
        public event GreenCardReaderEventHandler TakingOffCompleted;

        public NFCCardReader()
        {
        }

        private void _monitor_CardRemoved(object sender, CardStatusEventArgs e)
        {
            if(TakingOffCompleted != null)
            {
                TakingOffCompleted(this, new GreenCardReaderEventArgs { CardReader = this });
            }            
        }

        private void _monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            PCSC.ICardReader reader = _ctx.ConnectReader(e.ReaderName, PCSC.SCardShareMode.Shared, PCSC.SCardProtocol.Any);

            byte[] getCardId = new byte[]
                                           {
                                                0xff,       // CLA - the instruction class
                                                0xCA,       // INS - the instruction code
                                                0x00,       // P1 - 1st parameter to the instruction
                                                0x00,       // P2 - 2nd parameter to the instruction
                                                0x00        // Le - size of the transfer
                                           };

            var result = new GreenCardReaderEventArgs { CardReader = this };
            byte[] receiveBuffer = new byte[256];
            try
            {
                reader.Transmit(getCardId, receiveBuffer);
            }
            catch (Exception ex)
            {
                result.ex = ex;
            }

            var cardid = BitConverter.ToString(receiveBuffer.Take(4).ToArray()).Replace("-", "");
            result.CardID = cardid;
            if(ReadingCompleted != null)
            {
                ReadingCompleted(this, result);
            }            
        }

        public IGreenCardReaderInfo Info { get; set; }
        public CardState State { get; set; }

        public static List<string> GetReaders()
        {
            using (var ctx = PCSC.ContextFactory.Instance.Establish(PCSC.SCardScope.User))
            {
                return ctx.GetReaders().ToList();
            }
        }

        public bool Connect()
        {
            if(State != CardState.IsReady)
            {
                _ctx = PCSC.ContextFactory.Instance.Establish(PCSC.SCardScope.User);
                _monitor = MonitorFactory.Instance.Create(PCSC.SCardScope.System);
                _monitor.CardInserted += _monitor_CardInserted;
                _monitor.CardRemoved += _monitor_CardRemoved;

                _monitor.Start(Reader);
                this.State = CardState.IsReady;
            }

            return this.State == CardState.IsReady;
        }

        public void DisConnect()
        {
            this.State = CardState.IsStop;
            _monitor.CardInserted -= _monitor_CardInserted;
            _monitor.CardRemoved -= _monitor_CardRemoved;
            _monitor.Cancel();
            _monitor.Dispose();
            _ctx.Dispose();
        }

        public object GetController()
        {
            return null;
        }
    }
}
