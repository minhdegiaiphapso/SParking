using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PreviewDemo
{
    public partial class AlarmConfig : Form
    {
        public AlarmConfig()
        {
            InitializeComponent();
        }

        private void AlarmConfig_Load(object sender, EventArgs e)
        {
            GetAlarmInConfig();
            GetAlarmOutConfig();

            InitWnd();

        }
        
        private void InitWnd()
        {
            string str;
            for (int i = 0; i < m_iAlarmInNum; i++ )
            {
                str = String.Format("AlarmIn{0}", i+1);
                comboBoxAlarmIn.Items.Add(str);
            }

            for (int i = 0; i < m_iAlarmOutNum; i++ )
            {
                str = String.Format("AlarmOut{0}", i+1);
                comboBoxAlarmOut.Items.Add(str);
            }
            comboBoxAlarmType.Items.Add("Normally open");
            comboBoxAlarmType.Items.Add("Normally close");

    //  0-5秒，1-10秒，2-30秒，3-1分钟，4-2分钟，5-5分钟，6-10分钟，7-手动，0xff-为无限
            int nIndex = 0;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "5s");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "10s");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "20s");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "1min");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "2min");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "5min");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "10min");
            nIndex++;
            comboBoxAlarmOutDelay.Items.Insert(nIndex, "Manual");
            nIndex++;

            comboBoxAlarmIn.SelectedIndex = 0;
            comboBoxAlarmOut.SelectedIndex = 0;
            comboBoxAlarmOutDelay.SelectedIndex = 0;
        }

        private bool GetAlarmInConfig()
        {
            Int32 nSize = Marshal.SizeOf(m_struAlarmInCfg);
            IntPtr ptrAlarmInCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struAlarmInCfg, ptrAlarmInCfg, false);
            UInt32 dwReturn = 0;
            bool bRet = false;
            Int32 lAlarmIn = comboBoxAlarmIn.SelectedIndex;
            bRet = CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_ALARMINCFG_V30, lAlarmIn, ptrAlarmInCfg, (UInt32)nSize, ref dwReturn);
            if (bRet)
            {
                Debug.Print("Succ to NET_DVR_GET_ALARAMINCFG_V30");

                m_struAlarmInCfg = (CHCNetSDK.NET_DVR_ALARMINCFG_V30)Marshal.PtrToStructure(ptrAlarmInCfg, typeof(CHCNetSDK.NET_DVR_ALARMINCFG_V30));

            }
            else
            {
                Debug.Print("Fail to NET_DVR_GET_ALARMINCFG_V30");
            }
            
            Marshal.FreeHGlobal(ptrAlarmInCfg);

            return bRet;
        }

        private bool SetAlarmInConfig()
        {
            uint dwSize = (uint)Marshal.SizeOf(m_struAlarmInCfg);
            IntPtr ptrAlarmInCfg = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struAlarmInCfg, ptrAlarmInCfg, false);
            Int32 lAlarmIn = comboBoxAlarmIn.SelectedIndex;
            if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_ALARMINCFG_V30, lAlarmIn, ptrAlarmInCfg, dwSize))
            {
                uint dwErrorNo = CHCNetSDK.NET_DVR_GetLastError();
                Debug.Print("NET_DVR_SET_ALARMINCFG_V30 fail");
                MessageBox.Show("NET_DVR_SET_ALARMINCFG_V30 Fail");
            }
            else
            {
                // succ
                Debug.Print("NET_DVR_SET_ALARMINCFG_V30 Succ");
                MessageBox.Show("NET_DVR_SET_ALARMINCFG_V30 Succ");
            }
            Marshal.FreeHGlobal(ptrAlarmInCfg);
            
            return true;
        }

        
        private bool GetAlarmOutConfig()
        {
            Int32 nSize = Marshal.SizeOf(m_struAlarmOutCfg);
            IntPtr ptrAlarmOutCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struAlarmOutCfg, ptrAlarmOutCfg, false);
            UInt32 dwReturn = 0;
            bool bRet = false;
            Int32 lAlarmOut = comboBoxAlarmOut.SelectedIndex;
            bRet = CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_ALARMOUTCFG_V30, lAlarmOut, ptrAlarmOutCfg, (UInt32)nSize, ref dwReturn);
            if (bRet)
            {
                Debug.Print("Succ to NET_DVR_GET_ALARAMOUTCFG_V30");

                m_struAlarmOutCfg = (CHCNetSDK.NET_DVR_ALARMOUTCFG_V30)Marshal.PtrToStructure(ptrAlarmOutCfg, typeof(CHCNetSDK.NET_DVR_ALARMOUTCFG_V30));

            }
            else
            {
                Debug.Print("Fail to NET_DVR_GET_ALARMOutCFG_V30");
            }

            Marshal.FreeHGlobal(ptrAlarmOutCfg);

            return bRet;
        }

        private bool SetAlarmOutConfig()
        {
            uint dwSize = (uint)Marshal.SizeOf(m_struAlarmOutCfg);
            IntPtr ptrAlarmOutCfg = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struAlarmOutCfg, ptrAlarmOutCfg, false);
            Int32 lAlarmOut = comboBoxAlarmOut.SelectedIndex;
            if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_ALARMOUTCFG_V30, lAlarmOut, ptrAlarmOutCfg, dwSize))
            {
                uint dwErrorNo = CHCNetSDK.NET_DVR_GetLastError();
                Debug.Print("NET_DVR_SET_ALARMOUTCFG_V30 fail");
                MessageBox.Show("NET_DVR_SET_ALARMOUTCFG_V30 Fail");
            }
            else
            {
                // succ
                Debug.Print("NET_DVR_SET_ALARMOUTCFG_V30 Succ");
                MessageBox.Show("NET_DVR_SET_ALARMOUTCFG_V30 Succ");
            }
            Marshal.FreeHGlobal(ptrAlarmOutCfg);

            return true;
        }

        private bool SetAlarmOut(Int32 lAlarmOutPort, Int32 lAlramOutStatic)
        {
            if (CHCNetSDK.NET_DVR_SetAlarmOut(m_lUserID, lAlarmOutPort, lAlramOutStatic))
            {
                Debug.Print("Succ to set alarm out");

                return true;
            }
            else
            {
                Debug.Print("Fail to set alarm out");
                MessageBox.Show("Fail to set alarm out");
                return false;
            }
        }
        private void btnAlarmInCfg_Click(object sender, EventArgs e)
        {
            m_struAlarmInCfg.sAlarmInName = CodeBytes(textBoxAlarmInName.Text, CHCNetSDK.NAME_LEN);
            m_struAlarmInCfg.byAlarmType = (byte)comboBoxAlarmType.SelectedIndex;
            SetAlarmInConfig();
        }

        private void btnAlarmOutCfg_Click(object sender, EventArgs e)
        {
            m_struAlarmOutCfg.sAlarmOutName = CodeBytes(textBoxAlarmOutName.Text, CHCNetSDK.NAME_LEN);
            m_struAlarmOutCfg.dwAlarmOutDelay = (UInt32)comboBoxAlarmOutDelay.SelectedIndex;
            SetAlarmOutConfig();
        }

        private void comboBoxAlarmIn_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetAlarmInConfig();

            textBoxAlarmInName.Text = System.Text.Encoding.Default.GetString(m_struAlarmInCfg.sAlarmInName);
            comboBoxAlarmType.SelectedIndex = m_struAlarmInCfg.byAlarmType;
            
        }



        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


        public byte[] CodeBytes(string str, int len)
        {

            byte[] result = new byte[len];
            byte[] strBytes = Encoding.Default.GetBytes(str);

            //copy the array converted into result, and fill the remaining bytes with 0
            for (int i = 0; i < len; i++)
                result[i] = ((i < strBytes.Length) ? strBytes[i] : (byte)0);

            return result;
        }

        private void comboBoxAlarmOut_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetAlarmOutConfig();
            textBoxAlarmOutName.Text = System.Text.Encoding.Default.GetString(m_struAlarmOutCfg.sAlarmOutName);
            comboBoxAlarmOutDelay.SelectedIndex = (Int32)m_struAlarmOutCfg.dwAlarmOutDelay;

        }

        private void btnSetAlarmOut_Click(object sender, EventArgs e)
        {
            int lAlarmOutStatic = comboBoxAlarmOutStatic.SelectedIndex;
            int lAlarmOutPort = comboBoxAlarmOut.SelectedIndex;
            SetAlarmOut(lAlarmOutPort, lAlarmOutStatic);
        } 
    }
}
