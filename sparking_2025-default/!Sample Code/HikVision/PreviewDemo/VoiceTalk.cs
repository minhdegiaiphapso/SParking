using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
namespace PreviewDemo
{
    public partial class VoiceTalk : Form
    {
        public VoiceTalk()
        {
            InitializeComponent();
        }

        private void VoiceTalk_Load(object sender, EventArgs e)
        {
            string str;
            for (int i = 0; i < m_dwAudioChanNum; i++ )
            {
                str = String.Format("voice talk {0}", i);
                
                comboBoxVoiceChan.Items.Add(str);
            }
            comboBoxVoiceChan.SelectedIndex = 0;
            comboBoxCallBack.SelectedIndex = 0;
            
                        
        }

        private void btnStartVoiceTale_Click(object sender, EventArgs e)
        {
            UInt32 dwVoiceChan = 0;
            dwVoiceChan = (UInt32)(comboBoxVoiceChan.SelectedIndex + 1);
            bool bNeedCBNoEncData ;
            if (0 == comboBoxCallBack.SelectedIndex)
            {
                bNeedCBNoEncData = false;
            }
            else
            {
                bNeedCBNoEncData = true;
            }
            m_fVoiceData = new CHCNetSDK.VOICEDATACALLBACKV30(VoiceTalkCallBack);

            //todo!!!  this should callback  
            m_lVoiceTalkHandle = CHCNetSDK.NET_DVR_StartVoiceCom_V30(m_lUserID, dwVoiceChan, bNeedCBNoEncData, m_fVoiceData,IntPtr.Zero );
            
            if (m_lVoiceTalkHandle < 0)
            {
                MessageBox.Show("Fail to start voice talk");
            }
        }

        private void btnStopVoiceTale_Click(object sender, EventArgs e)
        {
            if (CHCNetSDK.NET_DVR_StopVoiceCom(m_lVoiceTalkHandle))
            {
                Debug.Print("succ to NET_DVR_StopVoiceCom ");
            }
            else
            {
                Debug.Print("Fail to NET_DVR_StopVoiceCom ");
            }

                        
        }

        public void VoiceTalkCallBack(int lVoiceComHandle, string pRecvDataBuffer, uint dwBufSize, byte byAudioFlag, System.IntPtr pUser)
        {
            Debug.Print("RealDataCallBack");
        }
    }
}
