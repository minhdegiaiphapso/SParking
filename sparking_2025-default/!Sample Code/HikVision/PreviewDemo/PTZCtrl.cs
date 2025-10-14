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
    public partial class PTZCtrl : Form
    {
        public PTZCtrl()
        {
            InitializeComponent();
        }

        private void btnUp_MouseDown(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.TILT_DOWN, 0);
        }

        private void btnUp_MouseUp(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.TILT_DOWN, 1);
        }

        private void btnLeft_MouseDown(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.PAN_LEFT, 0);
        }

        private void btnLeft_MouseUp(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.PAN_LEFT, 1);
        }

        private void btnRight_MouseDown(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.PAN_RIGHT, 0);
        }

        private void btnRight_MouseUp(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.PAN_RIGHT, 1);
        }

        private void btnDown_MouseDown(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.TILT_DOWN, 0);
        }

        private void btnDown_MouseUp(object sender, MouseEventArgs e)
        {
            PtzControl(CHCNetSDK.TILT_DOWN, 1);
        }

        private bool PtzControl(UInt32 dwPtzCommand, UInt32 dwStop)
        {
            if (CHCNetSDK.NET_DVR_PTZControl(m_lRealHandle, dwPtzCommand, dwStop))
            {

            }
            else
            {
                MessageBox.Show("Fail to ptz control");
                return false;
            }
            return true; 
        }
//         private BOOL NET_DVR_PTZControl(
//   LONG lRealHandle,
//   DWORD dwPTZCommand,
//   DWORD dwStop






    }
}
