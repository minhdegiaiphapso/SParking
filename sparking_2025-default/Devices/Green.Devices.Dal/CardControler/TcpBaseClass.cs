using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.Devices.Dal.CardControler
{
    public class TcpBaseClass
    {
        public Boolean IsconnectSuccess = false; //异步连接情况，由异步连接回调函数置位 
        public int StartTick = 0;

        protected byte[] BufferRX = new byte[1024];
        protected byte[] BufferTX = new byte[1024];

        protected byte isHeartTime = 0;
        protected String SockErrorStr = null;

        #region 委托事件声明
        public delegate void delSocketDisconnected();
        public event delSocketDisconnected OnDisconnected;

        public delegate void TOnRxTxDataHandler(byte rt, byte[] buffRX, int len);   //声明委托
        public event TOnRxTxDataHandler OnRxTxDataEvent;        //声明事件  
        #endregion

        virtual public void SetTcpTick()
        {
        }

        virtual public bool OpenIP(string ip, int port)
        {
            return false;
        }

        virtual public bool CloseTcpip()
        {
            return false;
        }

        protected void DoOnRxTxDataEvent(byte rt, byte[] buffRX, int len)
        {
            if (OnRxTxDataEvent != null)
            {
                OnRxTxDataEvent(rt, buffRX, len);
            }
        }

        virtual public byte DoSendData(byte[] buffTX, int WriteNum)
        {
            return 0;
        }
        /*
        protected void DoClearsocketDisconnected()
        {
            if (OnDisconnected != null)
            {
                OnDisconnected = null;
            }
        }*/

        protected void DosocketDisconnected()
        {
            if (OnDisconnected != null)
            {
                OnDisconnected();
            }
        }
    }
}
