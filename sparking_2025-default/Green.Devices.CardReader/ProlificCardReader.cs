using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR6100Api;
using Green.Devices.Dal;

namespace Green.Devices.CardReader
{
    public interface IProlificCardReaderFactory
    {
        ProlificCardReader CreateNew(string ip, string port);
    }
    public class ProlificCardReaderFactory : IProlificCardReaderFactory
    {
        private Dictionary<string, ProlificCardReader> _dic = new Dictionary<string, ProlificCardReader>();

        public ProlificCardReader CreateNew(string ip, string port)
        {
            ProlificCardReader reader = null;
            if (!_dic.ContainsKey(ip))
            {
                reader = new ProlificCardReader(ip, port);

                if (!reader.IsConnected) 
                    return null;

                _dic.Add(ip, reader);
                reader.Run();
            }
            else
            {
                reader = _dic[ip];
            }
            return reader;
        }
    }

    public class ProlificCardReaderInfo : ICardReaderInfo
    {
        public string SerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
    }

    public class ProlificCardReader : ICardReader
    {
        MR6100Api.MR6100Api Api = new MR6100Api.MR6100Api();
        public bool IsConnected { get; private set; }
        public ProlificCardReader(string ip, string port)
        {
            if (string.IsNullOrEmpty(port))
                port = "100";

            CardReaderInfo = new ProlificCardReaderInfo { IP = ip, Port = port };
            ConnectToReader();
        }

        public void ConnectToReader()
        {
            var reConnect = 5;
            while (reConnect > 0 && !IsConnected)
            {
                ProlificCardReaderInfo info = (ProlificCardReaderInfo)CardReaderInfo;
                if (Api.isNetWorkConnect(info.IP))
                {
                    int status = Api.TcpConnectReader(info.IP, int.Parse(info.Port));
                    if (status == MR6100Api.MR6100Api.SUCCESS_RETURN)
                    {
                        //reConnect = 0;
                        IsConnected = true;
                        CardReaderInfo.SerialNumber = GetSerialNumber();
                        Run();
                    }
                    else
                        reConnect--;
                }
                else
                    reConnect--;
            }
            if (reConnect <= 0)
                IsConnected = false;
        }

        public string GetSerialNumber()
        {
            string serialNo=string.Empty;
            int status = Api.GetSerialNo(255, ref serialNo);
            return serialNo;
        }

        private int lastReadTime = 0;
        public async void ReadingThread()
        {
            while (true)
            {
                var getCardIdTask = Task.Factory.StartNew<string>(() => { return GetCardIdBlockFunc(); });

                string result = await getCardIdTask;
                var curr = Environment.TickCount;
                if (curr - lastReadTime > 3000)
                {
                    lastReadTime = curr;
                    ReadingCompleted(this, new CardReaderEventArgs { CardID = result, CardReader = this });
                }

                Console.WriteLine(result);
            }
        }

        public string GetCardIdBlockFunc()
        {
            string cardId = string.Empty;
            while (true)
            {
                cardId = GetCardInfo();
                if (!string.IsNullOrEmpty(cardId))
                    return cardId;
                else
                {
                    TakingOffCompleted(this, new CardReaderEventArgs { CardReader = this });
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        public string GetCardInfo()
        {
            byte tag_flag = 0;
            byte[,] tagData = new byte[500, 14];
            int tagCount = 0;
            tagCount = 0;
            int status = Api.EpcMultiTagIdentify(255, ref tagData, ref tagCount, ref tag_flag);
            if (status == MR6100Api.MR6100Api.SUCCESS_RETURN)
            {
                if (tagCount > 1) tagCount = 1;

                string strAnteNo = "", strID = "", strTemp = "";
                for (int i = 0; i < tagCount; i++)
                {
                    int j = 0;
                    strID = "";
                    strAnteNo = string.Format("{0:X2}", tagData[i, 1]);
                    for (j = 2; j < 14; j++) // update: 0->2, 12->14
                    {
                        strTemp = string.Format("{0:X2}", tagData[i, j]);
                        strID += strTemp;
                    }
                    if (strID == "000000000000000000000001")
                    {
                        //libInfo.Items.Add("000");
                        continue;
                    }

                    return strID;
                }
            }

            return null;
        }

        public ICardReaderInfo CardReaderInfo { get; private set; }

        public event CardReaderEventHandler ReadingCompleted;

        public event CardReaderEventHandler TakingOffCompleted;

        private Task _task;
        private bool _isReading;

        public void Run()
        {
            if (!_isReading)
            {
                _isReading = true;
                _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);
            } 
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
