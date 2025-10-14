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
    public partial class MotionDetect : Form
    {

        public MotionDetect()
        {
            InitializeComponent();
        }

        private void MotionDetect_Load(object sender, EventArgs e)
        {
            string str = "";
            byte byCount;
            for (byCount = 1; byCount <= m_struDeviceInfo.byChanNum; byCount++)
            {
                str = String.Format("Camera {0}", byCount);
                treeViewRecordChan.Nodes.Add(str);
                
            }

            for (byCount = 1; byCount <= m_struDeviceInfo.byIPChanNum; byCount++)
            {
                str = String.Format("IPCamera {0}", byCount);
                treeViewRecordChan.Nodes.Add(str);
            }

            for (byCount = 1; byCount <= m_struDeviceInfo.byAlarmOutPortNum; byCount++)
            {
                str = String.Format("Alarm Out {0}", byCount);
                treeViewAlarmOut.Nodes.Add(str);
            }

            CHCNetSDK.NET_DVR_CLIENTINFO struClientInfo;
            struClientInfo.lChannel = 1; // channel can  be change
            struClientInfo.lLinkMode = 0;
            struClientInfo.hPlayWnd = MotionRealPlay.Handle;
            struClientInfo.sMultiCastIP = "";

            m_lMotionPlayHandle = CHCNetSDK.NET_DVR_RealPlay_V30(m_lUserID, ref struClientInfo, null, IntPtr.Zero, 1);

            if (m_lMotionPlayHandle < 0)
            {
                Debug.Print("NET_DVR_RealPlay_V30 faile");
            }
            else
            {
                m_fMotionDraw = new CHCNetSDK.DRAWFUN(MotionDrawCallBack);

                CHCNetSDK.NET_DVR_RigisterDrawFun(m_lMotionPlayHandle, m_fMotionDraw, 0);
            }
            uint dwSize = (uint)Marshal.SizeOf(m_struPicCfgV30);
            IntPtr ptrPicCfgV30 = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struPicCfgV30, ptrPicCfgV30, false);
            uint dwReturn = 0;
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_PICCFG_V30, m_lChannel, ptrPicCfgV30, dwSize, ref dwReturn))
            {
                // fail 
                // Preview.DebugInfo("NET_DVR_GET_PICCFG_V30 Succ");
                Debug.Print("NET_DVR_GET_PICCFG_V30 fail");
                MessageBox.Show("NET_DVR_GET_PICCFG_V30 Fail");
            }
            else
            {
                // succ
                Debug.Print("NET_DVR_GET_PICCFG_V30 Succ");
                m_struPicCfgV30 = (CHCNetSDK.NET_DVR_PICCFG_V30)Marshal.PtrToStructure(ptrPicCfgV30, typeof(CHCNetSDK.NET_DVR_PICCFG_V30));
                m_struMotionV30 = m_struPicCfgV30.struMotion;

            }
            Marshal.FreeHGlobal(ptrPicCfgV30);

            SetMotionSensitivityCurSel(m_struMotionV30.byMotionSensitive);
            SetAlarmHandleWnd();
            comboBoxWeekday.SelectedIndex = 0;
            comboBoxWeekdayCopy.SelectedIndex = 0;
            SetAlarmTimeWnd();
            SetRecordChanWnd();
            if (m_struMotionV30.byEnableHandleMotion == 0)
            {
                checkBoxEnableMotion.Checked = false;
            }
            else
            {
                checkBoxEnableMotion.Checked = true;
            }
            
        }

        private void SetRecordChanWnd()
        {
            byte byCount = 0;
            for (byCount = 0; byCount < m_struDeviceInfo.byChanNum; byCount++ )
            {
                if (m_struMotionV30.byRelRecordChan[byCount] ==1)
                {
                    treeViewRecordChan.Nodes[byCount].Checked = true;
                }
                else
                {
                    treeViewRecordChan.Nodes[byCount].Checked = false;
                }

            }
            for (int j = 0; j < m_struDeviceInfo.byIPChanNum; j++, byCount++ )
            {
                if (m_struMotionV30.byRelRecordChan[j + CHCNetSDK.MAX_ANALOG_CHANNUM] == 1)
                {
                    treeViewRecordChan.Nodes[byCount].Checked = true;
                }
                else
                {
                    treeViewRecordChan.Nodes[byCount].Checked = false;
                }
            }
        }

        private void GetRecordChanWnd()
        {
            byte byCount =0;
            for (byCount = 0; byCount < m_struDeviceInfo.byChanNum; byCount++ )
            {
                if (treeViewRecordChan.Nodes[byCount].Checked)
                {
                    m_struMotionV30.byRelRecordChan[byCount] = 1;
                }
                else
                {
                    m_struMotionV30.byRelRecordChan[byCount] = 0;
                }
            }
            for (int j = 0; j < m_struDeviceInfo.byIPChanNum; j++ ,byCount++)
            {
                if (treeViewRecordChan.Nodes[byCount].Checked)
                {
                    m_struMotionV30.byRelRecordChan[CHCNetSDK.MAX_ANALOG_CHANNUM + j] = 1;
                }
                else
                {
                    m_struMotionV30.byRelRecordChan[CHCNetSDK.MAX_ANALOG_CHANNUM + j] = 0;
                }
            }
        }

        public void MotionDrawCallBack(int lRealHandle, IntPtr hDc, uint dwUser)
        {
            
        }

        private void SetAlarmTimeWnd()
        {
            int nDayIndex = comboBoxWeekday.SelectedIndex;
            textBoxHour11.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartHour.ToString();
            textBoxMin11.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartMin.ToString();
            textBoxHour12.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopHour.ToString();
            textBoxMin12.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopMin.ToString();

            textBoxHour21.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartHour.ToString();
            textBoxMin21.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartMin.ToString();
            textBoxHour22.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopHour.ToString();
            textBoxMin22.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopMin.ToString();

            textBoxHour31.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartHour.ToString();
            textBoxMin31.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartMin.ToString();
            textBoxHour32.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopHour.ToString();
            textBoxMin32.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopMin.ToString();

            textBoxHour41.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartHour.ToString();
            textBoxMin41.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartMin.ToString();
            textBoxHour42.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopHour.ToString();
            textBoxMin42.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopMin.ToString();

            textBoxHour51.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartHour.ToString();
            textBoxMin51.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartMin.ToString();
            textBoxHour52.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopHour.ToString();
            textBoxMin52.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopMin.ToString();

            textBoxHour61.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartHour.ToString();
            textBoxMin61.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartMin.ToString();
            textBoxHour62.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopHour.ToString();
            textBoxMin62.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopMin.ToString();

            textBoxHour71.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartHour.ToString();
            textBoxMin71.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartMin.ToString();
            textBoxHour72.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopHour.ToString();
            textBoxMin72.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopMin.ToString();

            textBoxHour81.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartHour.ToString();
            textBoxMin81.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartMin.ToString();
            textBoxHour82.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopHour.ToString();
            textBoxMin82.Text = m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopMin.ToString();
        }

        private void GetAlarmTimeWnd()
        {
            int nDayIndex = comboBoxWeekday.SelectedIndex;
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartHour = byte.Parse(textBoxHour11.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStartMin = byte.Parse(textBoxMin11.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopHour = byte.Parse(textBoxHour12.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 0].byStopMin = byte.Parse(textBoxMin12.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartHour = byte.Parse(textBoxHour21.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStartMin = byte.Parse(textBoxMin21.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopHour = byte.Parse(textBoxHour22.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 1].byStopMin = byte.Parse(textBoxMin22.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartHour = byte.Parse(textBoxHour31.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStartMin = byte.Parse(textBoxMin31.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopHour = byte.Parse(textBoxHour32.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 2].byStopMin = byte.Parse(textBoxMin32.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartHour = byte.Parse(textBoxHour41.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStartMin = byte.Parse(textBoxMin41.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopHour = byte.Parse(textBoxHour42.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 3].byStopMin = byte.Parse(textBoxMin42.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartHour = byte.Parse(textBoxHour51.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStartMin = byte.Parse(textBoxMin51.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopHour = byte.Parse(textBoxHour52.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 4].byStopMin = byte.Parse(textBoxMin52.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartHour = byte.Parse(textBoxHour61.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStartMin = byte.Parse(textBoxMin61.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopHour = byte.Parse(textBoxHour62.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 5].byStopMin = byte.Parse(textBoxMin62.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartHour = byte.Parse(textBoxHour71.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStartMin = byte.Parse(textBoxMin71.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopHour = byte.Parse(textBoxHour72.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 6].byStopMin = byte.Parse(textBoxMin72.Text);

            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartHour = byte.Parse(textBoxHour81.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStartMin = byte.Parse(textBoxMin81.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopHour = byte.Parse(textBoxHour82.Text);
            m_struMotionV30.struAlarmTime[nDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + 7].byStopMin = byte.Parse(textBoxMin82.Text);

        }
        private void SetAlarmHandleWnd()
        {
            if (1 == ((m_struMotionV30.struMotionHandleType.dwHandleType >> 0) & 0x01))
            {
                checkBoxShowAlarm.Checked = true;
            }

            if (1 == ((m_struMotionV30.struMotionHandleType.dwHandleType >> 1) & 0x01))
            {
                checkBoxAudioAlarm.Checked = true;
            }

            if (1 == ((m_struMotionV30.struMotionHandleType.dwHandleType >> 2) & 0x01))
            {
                checkBoxReportAlarm.Checked = true;
            }

            if (1 == ((m_struMotionV30.struMotionHandleType.dwHandleType >> 3) & 0x01))
            {
                checkBoxTriggerAlarm.Checked = true;
            }

            if (1 == ((m_struMotionV30.struMotionHandleType.dwHandleType >> 4) & 0x01))
            {
                checkBoxEmailAlarm.Checked = true;
            }

            treeViewAlarmOut.Enabled = checkBoxTriggerAlarm.Checked;
            for (int i = 0; i < treeViewAlarmOut.Nodes.Count; i++)
            {
                if (m_struMotionV30.struMotionHandleType.byRelAlarmOut[i] == 0)
                {
                    treeViewAlarmOut.Nodes[i].Checked = false;
                }
                else
                {
                    treeViewAlarmOut.Nodes[i].Checked = true;
                }
            }
        }
        private void GetAlarmHandleWnd()
        {
            m_struMotionV30.struMotionHandleType.dwHandleType = 0;
            if (checkBoxShowAlarm.Checked)
            {
                m_struMotionV30.struMotionHandleType.dwHandleType |= (0x01 << 0);
            }

            if (checkBoxAudioAlarm.Checked)
            {
                m_struMotionV30.struMotionHandleType.dwHandleType |= (0x01 << 1);
            }

            if (checkBoxReportAlarm.Checked)
            {
                m_struMotionV30.struMotionHandleType.dwHandleType |= (0x01 << 2);
            }

            if (checkBoxTriggerAlarm.Checked)
            {
                m_struMotionV30.struMotionHandleType.dwHandleType |= (0x01 << 3);
            }

            if (checkBoxEmailAlarm.Checked)
            {
                m_struMotionV30.struMotionHandleType.dwHandleType |= (0x01 << 4);
            }

            for (int i = 0; i < treeViewAlarmOut.Nodes.Count; i++)
            {
                if (treeViewAlarmOut.Nodes[i].Checked)
                {
                    m_struMotionV30.struMotionHandleType.byRelAlarmOut[i] = 1;
                }
                else
                {
                    m_struMotionV30.struMotionHandleType.byRelAlarmOut[i] = 0;
                }
            }

        }
        private void SetMotionSensitivityCurSel(byte bySensitity)
        {
            if (bySensitity == 0xff)
            {
                comboBoxMotionSensitive.SelectedIndex = 0;
            }
            else
            {
                comboBoxMotionSensitive.SelectedIndex = bySensitity + 1;
            }
        }
        private byte GetMotionSensitivityCurSel()
        {
            byte byReturn;
            if (0 == comboBoxMotionSensitive.SelectedIndex)
            {
                byReturn = 0xff;
            }
            else
            {
                byReturn = (byte)(comboBoxMotionSensitive.SelectedIndex - 1);
            }
            return byReturn;
        }
        private void comboBoxMotionSensitive_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAlarmTimeWnd();
        }

        private void btnMotionSetup_Click(object sender, EventArgs e)
        {

            m_struMotionV30.byMotionSensitive = GetMotionSensitivityCurSel();
            GetAlarmHandleWnd();
            GetAlarmTimeWnd();
            GetRecordChanWnd();
            if (checkBoxEnableMotion.Checked)
            {
                m_struMotionV30.byEnableHandleMotion = 1;
            }
            else
            {
                m_struMotionV30.byEnableHandleMotion = 0;
            }

            // set motion region 
            if (checkBoxMotionScope.Checked)
            {
                for (int i = 0; i < 18; i++)
                {
                    for (int j = 0; j < 22; j++ )
                    {
                        m_struMotionV30.byMotionScope[i*96 + j] =1;
                    }
                }
            }

            m_struPicCfgV30.struMotion = m_struMotionV30;

            uint dwSize = (uint)Marshal.SizeOf(m_struPicCfgV30);
            IntPtr ptrPicCfgV30 = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struPicCfgV30, ptrPicCfgV30, false);
            if (!CHCNetSDK.NET_DVR_SetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_SET_PICCFG_V30, m_lChannel, ptrPicCfgV30, dwSize))
            {
                // fail 
                // Preview.DebugInfo("NET_DVR_GET_PICCFG_V30 Succ");
                uint dwErrorNo = CHCNetSDK.NET_DVR_GetLastError();
                Debug.Print("NET_DVR_SET_PICCFG_V30 fail");
                MessageBox.Show("NET_DVR_SET_PICCFG_V30 Fail");
            }
            else
            {
                // succ
                Debug.Print("NET_DVR_SET_PICCFG_V30 Succ");
                MessageBox.Show("NET_DVR_SET_PICCFG_V30 Succ");
            }
            Marshal.FreeHGlobal(ptrPicCfgV30);

        }

        private void btnMotionDayCopy_Click(object sender, EventArgs e)
        {
            int iDayIndex = comboBoxWeekday.SelectedIndex;
            int iCopyDayIndex = comboBoxWeekdayCopy.SelectedIndex - 1;
            if (iCopyDayIndex == -1)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (iDayIndex != j)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            m_struMotionV30.struAlarmTime[j * CHCNetSDK.MAX_TIMESEGMENT_V30 + i] = m_struMotionV30.struAlarmTime[iDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + i];
                        }
                    }
                }
            }
            else
            {
                if (iDayIndex != iCopyDayIndex)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        m_struMotionV30.struAlarmTime[iCopyDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + i] = m_struMotionV30.struAlarmTime[iDayIndex * CHCNetSDK.MAX_TIMESEGMENT_V30 + i];
                    }
                }

            }
        }

        private void MotionDetect_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_lMotionPlayHandle >= 0)
            {
                if (CHCNetSDK.NET_DVR_StopRealPlay(m_lMotionPlayHandle))
                {
                    Debug.Print("NET_DVR_StopRealPlay Succ\n");
                }
                else
                {
                    Debug.Print("NET_DVR_StopRealPlay Fail\n");
                }
            }
        }

        private void btnMotionDayOk_Click(object sender, EventArgs e)
        {
            GetAlarmHandleWnd();
        }

        private void checkBoxTriggerAlarm_CheckedChanged(object sender, EventArgs e)
        {
            treeViewAlarmOut.Enabled =  checkBoxTriggerAlarm.Checked;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }



    }
}
