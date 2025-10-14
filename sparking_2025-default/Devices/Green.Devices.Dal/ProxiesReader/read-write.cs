using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.Devices.Dal.ProxiesReader
{
    public enum LightColor
    {

        CloseLED = 0x00,

        RedLED = 0x01,

        GreenLED = 0x02,

        AllLED = 0x03
    }


    class read_write
    {
        public byte[] Read_Em4001()
        {
            byte[] data = new byte[10];


            // Doc 5byte 
            int r = MasterRDimport.Read_Em4001(data);
            if (r == 0)
            {
                byte[] rData = new byte[10];
                for (int i = 0; i <= 4; i++)
                {
                    rData[i] = data[i];
                }
                return rData;
            }
            return null;
        }
        //public static[] byte GetDecimalFromByte(byte ByteCard)
        //{
        //    if (ByteCard == null)
        //        return null;
        //    byte[] DataCardIN = new byte[ByteCard];
        //    for (int i = 0; i < 4; i++)
        //    {
        //         Decimal DataCardOut = Convert.ToDecimal(BitConverter.ToDouble(DataCardIN, 0));

        //    }

        //    return DataCardOut;

        //}
        /// <summary>
        //chuyen doi tu hexa sang text
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetStringByData(byte[] data)
        {

            if (data == null) return null;
            StringBuilder sbText = new StringBuilder();
            for (int i = 0; i <= 4; i++)
            {
                sbText.Append(data[i].ToString("X2"));
            }
            return sbText.ToString();
        }
        public bool OpenLed(LightColor lc)
        {
            byte color = (byte)lc;
            int r = MasterRDimport.rf_light(0, color);
            return r == 0 ? true : false;
        }
        public bool Beep(int msec)
        {
            int r = MasterRDimport.rf_beep(0, msec);
            return r == 0 ? true : false;
        }
        public bool CloseCom()
        {
            try
            {
                int r = MasterRDimport.rf_ClosePort();
                return r == 0 ? true : false;
            }
            catch
            {
                throw new Exception("Hàm rf_ClosePort không thành công!");
            }
        }
        public bool OpenCom(ushort comNo, ushort baud)
        {
            try
            {
                int r = MasterRDimport.rf_init_com(comNo, baud);
                return r == 0 ? true : false;
            }
            catch
            {
                throw new Exception("Hàm rf_init_com Không Thành Công");
            }
        }

        internal byte[] GetBytesFromHexString(string cardHexString)
        {
            throw new NotImplementedException();
        }
    }

}
