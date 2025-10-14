using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Green.Devices.Vivotek
{
    public class IdentiveCardInfo : ICardReaderInfo
    {
        public string SerialNumber { get; set; }

        public string DeviceName { get; set; }
    }

    public class IdentiveCardReader : ICardReader
    {
        public event CardReaderEventHandler ReadingCompleted;

        public event CardReaderEventHandler TakingOffCompleted;

        private int _context;

        private int _cardHandle;

        private int _activeProtocol;

        public ICardReaderInfo CardReaderInfo { get; private set; }
        
        private bool isReading { get; set; }

        SCARD_IO_REQUEST _sendRequest = new SCARD_IO_REQUEST();
        SCARD_IO_REQUEST _recvRequest = new SCARD_IO_REQUEST();
        byte[] _recvBuff;
        byte[] _sendBuff;

        private Task _task;
        

        public IdentiveCardReader(string deviceName)
        {
            this.CardReaderInfo = new IdentiveCardInfo();
            this.CardReaderInfo.DeviceName = deviceName;
            this.CardReaderInfo.SerialNumber = string.Empty;
            isReading = false;
            _cardHandle = -1;
            _activeProtocol = -1;
            _recvBuff = new byte[128];
            _sendBuff = new byte[128];

            InitCardReader();
        }

        private void InitCardReader()
        {
            int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                return;

            //this.CardReaderInfo.SerialNumber = GetSerialNumberWMI();

            GetSerialNumber((serialNo, success) => {
                if (success)
                    this.CardReaderInfo.SerialNumber = serialNo;
                else
                {
                    Thread.Sleep(100);
                    GetSerialNumber((number1, success1) => {
                        if (success1)
                            this.CardReaderInfo.SerialNumber = number1;
                    });
                }
            });
        }

        private string GetSerialNumberWMI()
        {
            string serialNo = string.Empty;

            ManagementObjectSearcher mos = new ManagementObjectSearcher(@"\root\cimv2", @"Select * From Win32_PnPEntity Where Service='S11GEN64' or Service='S11GEN32'");

            ManagementObjectCollection mob = mos.Get();

            foreach (ManagementObject mo in mob)
            {
                string[] strs = mo["DeviceID"].ToString().Split('\\');
                serialNo = strs[strs.Length - 1];
                foreach (PropertyData prop in mo.Properties)
                {
                    Console.WriteLine("{0}: {1}", prop.Name, prop.Value);
                }
            }

            return serialNo;
        }

        private void GetSerialNumber(Action<string, bool> complete)
        {
            // http://stackoverflow.com/questions/6940824/getting-pcsc-reader-serial-number-with-winscard
            int readerHandle = 0;

            int protocol = 0;
            int ret = ModWinsCard.SCardConnect(_context, this.CardReaderInfo.DeviceName, ModWinsCard.SCARD_SHARE_DIRECT, ModWinsCard.SCARD_PROTOCOL_UNDEFINED, ref readerHandle, ref protocol);

            byte[] data = new byte[128];
            int leng = 128;
            ret = ModWinsCard.SCardGetAttrib(readerHandle, ModWinsCard.SCARD_ATTR_VENDOR_IFD_SERIAL_NO, data, ref leng);

            string serialNo = System.Text.ASCIIEncoding.ASCII.GetString(data, 0, leng);

            //int b = ModWinsCard.SCardFreeMemory(_context, data);

            ModWinsCard.SCardDisconnect(readerHandle, ModWinsCard.SCARD_LEAVE_CARD);

            if (complete != null)
                if (ret != ModWinsCard.SCARD_S_SUCCESS)
                    complete(string.Empty, false);
                else
                    complete(serialNo, true);
        }

        public void Run()
        {
            if (!isReading)
            {
                isReading = true;
                _task = Task.Factory.StartNew(() => ReadingThread(), TaskCreationOptions.LongRunning);            
            }            
        }

        public void Stop()
        {
            isReading = false;
            int retCode = ModWinsCard.SCardCancel(_context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                Console.WriteLine(string.Format("{0} cancel failed", this.CardReaderInfo.SerialNumber));

            retCode = ModWinsCard.SCardReleaseContext(_context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                Console.WriteLine(string.Format("{0} release failed", this.CardReaderInfo.SerialNumber));
        }

        private void ReadingThread()
        {
            try
            {
                SCARD_READERSTATE readerState;
                readerState.RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
                readerState.RdrEventState = ModWinsCard.SCARD_STATE_UNKNOWN;
                readerState.UserData = new IntPtr(0);
                readerState.ATRLength = 0;
                readerState.ATRValue = new byte[36];
                readerState.RdrName = this.CardReaderInfo.DeviceName;

                while (isReading)
                {
                    int retCode = ModWinsCard.SCardGetStatusChange(_context, ModWinsCard.INFINITE, ref readerState, 1);

                    if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    {
                        readerState.RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
                        readerState.RdrEventState = ModWinsCard.SCARD_STATE_UNKNOWN;
                        readerState.UserData = new IntPtr(0);
                        readerState.ATRLength = 0;
                        readerState.ATRValue = new byte[36];
                        readerState.RdrName = this.CardReaderInfo.DeviceName;

                        ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
                        Thread.Sleep(1000);
                        continue;
                        //ReadingCompleted(this, new CardReaderEventArgs() { CardID = string.Empty, CardReader = this, ex = new Exception("Reading failed") });
                    }

                    if ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_CHANGED) == ModWinsCard.SCARD_STATE_CHANGED)
                    {
                        if ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_EMPTY) == ModWinsCard.SCARD_STATE_EMPTY)
                        {
                            if (TakingOffCompleted != null)
                                TakingOffCompleted(this, new CardReaderEventArgs() { CardID = string.Empty, CardReader = this });
                        }
                        else if (((readerState.RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) == ModWinsCard.SCARD_STATE_PRESENT)
                            && ((readerState.RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) != (readerState.RdrCurrState & ModWinsCard.SCARD_STATE_PRESENT)))
                        {
                            GetCardId();
                        }
                    }

                    readerState.RdrCurrState = readerState.RdrEventState;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("BUGGG: {0}", exception.Message);
                Stop();
                InitCardReader();
                Run();
            }
        }

        private int ConnectCard()
        {
            return ModWinsCard.SCardConnect(_context, this.CardReaderInfo.DeviceName, ModWinsCard.SCARD_SHARE_SHARED, ModWinsCard.SCARD_PROTOCOL_T0 | ModWinsCard.SCARD_PROTOCOL_T1, ref _cardHandle, ref _activeProtocol);            
        }

        /// <summary>
        /// Get card id
        /// </summary>
        /// <returns></returns>
        private string GetCardId()
        {
            if (ConnectCard() != ModWinsCard.SCARD_S_SUCCESS)
            {
                return string.Empty;
            }            

            int RecvBuffLen = 0x6;

            Array.Clear(_sendBuff, 0, _sendBuff.Length);

            _sendBuff[0] = 0xFF;      //CLA
            _sendBuff[1] = 0xCA;      //P1 : Same for all source type
            _sendBuff[2] = 0x0;       //INS : for stored key input
            _sendBuff[3] = 0x0;       //P2  : for stored key input
            _sendBuff[4] = 0x0;          //P3  : for stored key input
            int sendBuffLen = 0x5;            

            _sendRequest.dwProtocol = _activeProtocol;
            _sendRequest.cbPciLength = Marshal.SizeOf(_sendRequest);

            _recvRequest.dwProtocol = _activeProtocol;
            _recvRequest.cbPciLength = Marshal.SizeOf(_recvRequest);

            int retCode = ModWinsCard.SCardTransmit(_cardHandle, ref _sendRequest, ref _sendBuff[0], sendBuffLen, ref _recvRequest, ref _recvBuff[0], ref RecvBuffLen);

            string sCardID = string.Empty;
            for (int i = 0; i < RecvBuffLen - 2; i++)
            {
                sCardID = sCardID + String.Format("{0:X2}", _recvBuff[i]);
            }
            if (ReadingCompleted != null)
                ReadingCompleted(this, new CardReaderEventArgs() { CardID = sCardID, CardReader = this });
            
            // Disconnect card after reading completed
            retCode = ModWinsCard.SCardDisconnect(_cardHandle, ModWinsCard.SCARD_LEAVE_CARD);

            return sCardID;
        }
       
    }
    public class RFIDCardReaderService : IRFIDCardReaderService
    {
        //private bool IsRunning { get; set; }

        private int _context = 1;

        public Dictionary<string, ICardReader> Devices { get; set; }

        public RFIDCardReaderService()
        {
            Devices = new Dictionary<string, ICardReader>();
            Init();
        }

        /// <summary>
        /// Get all card readers' name that are installed in this pc
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<string> GetCardReaderNames(int context)
        {
            if (_context == -1) return null;

            List<string> readerList = null;
            
            int readerCount = 255;
            
            Byte[] bytes = new Byte[readerCount];

            int retCode = ModWinsCard.SCardListReaders(context, null, bytes, ref readerCount);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                return null;

            try
            {
                string[] readerArr = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 0, readerCount).Split('\0');
                foreach (string readerName in readerArr)
                {
                    if (!string.IsNullOrEmpty(readerName) && readerName.Length > 1)
                    {
                        if (readerList == null) readerList = new List<string>();

                        readerList.Add(readerName);
                    }
                }
            }
            catch { return null; }

            return readerList;
        }

        private void ConnectCardReaders(List<string> names)
        {
            if (names != null)
            {
                foreach (var item in names)
                {
                    ICardReader device = new IdentiveCardReader(item);
                    if(!string.IsNullOrWhiteSpace(device.CardReaderInfo.SerialNumber))
                    {
                        Devices.Add(device.CardReaderInfo.SerialNumber, device);
                    }
                }
            }
        }

        private List<string> GetAllCardReaders()
        {
            List<string> names = GetCardReaderNames(_context);
            return names;
        }

        public ICardReader GetCardReader(string id)
        {
            return this.Devices.Where(d => d.Key.Equals(id)).Select(d => d.Value).FirstOrDefault();
        }

        public List<ICardReader> GetCardReaders()
        {
            return this.Devices.Values.ToList();
        }

        /// <summary>
        /// Create context and load card readers
        /// </summary>
        private void Init()
        {
            int retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref _context);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                return;
            List<string> names = GetAllCardReaders();
            ReleaseContext();

            ConnectCardReaders(names);
        }

        /// <summary>
        /// Make card readers to run
        /// </summary>
        public void Run()
        {    
            foreach (var item in Devices)
            {
                item.Value.Run();
            }
        }

        /// <summary>
        /// Stop scan device manager
        /// </summary>
        public void Stop()
        {
            foreach (var item in Devices)
                item.Value.Stop();
        }

        public ICardReaderInfo[] GetDeviceInfos()
        {
            return Devices.Values.Select(d => d.CardReaderInfo).ToArray();
        }

        /// <summary>
        /// Release all resources
        /// </summary>
        private void ReleaseContext()
        {         
            int retCode = ModWinsCard.SCardCancel(_context);
            //if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            //    Console.WriteLine("Cancel failed");

            retCode = ModWinsCard.SCardReleaseContext(_context);
            //if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            //    Console.WriteLine("Release failed");
        }
    }
}
