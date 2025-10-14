using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BoschControl
{
   
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Failed
    };
    public partial class CamCtrol : UserControl
    {
        
        public static List<CamData> CurrentStreams { get; set; }
        public string IPAddress { get; set; }
        public string ProgId { get; set; }  
        private Bosch.VideoSDK.Device.DeviceConnector deviceConnector = null;
        private Bosch.VideoSDK.Device.DeviceProxy proxy;
        public ConnectionState State { get; private set; }
        public CamCtrol()
        {
            InitializeComponent();
            State = ConnectionState.Disconnected;
            deviceConnector = new Bosch.VideoSDK.Device.DeviceConnector();
            deviceConnector.ConnectResult += DeviceConnector_ConnectResult;     
        }
        private void DeviceConnector_ConnectResult(Bosch.VideoSDK.Device.ConnectResultEnum ConnectResult, string URL, Bosch.VideoSDK.Device.DeviceProxy pDeviceProxy)
        {
            proxy = pDeviceProxy;
            switch (ConnectResult)
            {
                case Bosch.VideoSDK.Device.ConnectResultEnum.creConnected:
                    pnlMain.Controls.Clear();
                    break;
                case Bosch.VideoSDK.Device.ConnectResultEnum.creInitialized:
                    State = ConnectionState.Connecting;
                    if (proxy.VideoInputs == null || proxy.VideoInputs.Count == 0)
                    {
                        pnlMain.Controls.Clear();
                        return;
                    }
                    bool done = false;
                    foreach (Bosch.VideoSDK.Live.VideoInput vi in proxy.VideoInputs)
                    {
                        
                        if (done)
                            break;
                        done = true;
                        if (CamCtrol.CurrentStreams == null)
                            CamCtrol.CurrentStreams = new List<CamData>();
                        var curStream = CamCtrol.CurrentStreams.FirstOrDefault(s => s.Ip == IPAddress && s.Prog == ProgId);
                        if (curStream == null)
                            CamCtrol.CurrentStreams.Add(new CamData(IPAddress, ProgId, vi.Stream));
                       
                        SetVideo();          
                    }
                    break;
                default:
                    pnlMain.Controls.Clear();
                    proxy.Disconnect();
                    State = ConnectionState.Failed;
                    break;
            }
        }
        public void Connect()
        {
            try
            {
                if (CamCtrol.CurrentStreams != null && CamCtrol.CurrentStreams.Exists(s=>s.Ip==IPAddress && s.Prog==ProgId))
                {
                    SetVideo();    
                }
                else
                {     
                    if (State == ConnectionState.Disconnected || State == ConnectionState.Failed)
                    {
                        State = ConnectionState.Connecting;  
                        deviceConnector.ConnectAsync(IPAddress, ProgId);
                    }
                    else
                    {
                        SetVideo();
                    }
                }
            }
            catch
            {
                State = ConnectionState.Failed;
            }
        }
        private void SetVideo()
        {      
            if (CurrentStreams == null)
            {
                pnlMain.Controls.Clear();
                return;
            }
            var curStream = CamCtrol.CurrentStreams.FirstOrDefault(s => s.Ip == IPAddress && s.Prog == ProgId);
            if(curStream!=null)
            {
                pnlMain.Controls.Clear();
                var axcameo = new Bosch.VideoSDK.AxCameoLib.AxCameo();
                axcameo.BackColor = Color.BlueViolet;
                axcameo.Dock = DockStyle.Fill;
                pnlMain.Controls.Clear();
                pnlMain.Controls.Add(axcameo);
                var cameo = (Bosch.VideoSDK.CameoLib.Cameo)axcameo.GetOcx();
                if (cameo != null)
                {
                    cameo.EnableInWinPTZ = false;
                    cameo.SetVideoStream(curStream.Stream);
                }
                State = ConnectionState.Connected;
            }
            else
            {
                pnlMain.Controls.Clear();
            }
        }
        private void SetBackgroundVideo()
        {

        }
        public void DisConnect()
        {
            pnlMain.Controls.Clear();
            //if (CurrentStreams != null)
            //{
            //    var curStream = CamCtrol.CurrentStreams.FirstOrDefault(s => s.Ip == IPAddress && s.Prog == ProgId);
            //    if(curStream!=null)
            //        CamCtrol.CurrentStreams.Remove(curStream);
            //}
            //try
            //{
            //    if (State == ConnectionState.Connected)
            //    {
            //        State = ConnectionState.Disconnecting;
            //        proxy.Disconnect();
            //        State = ConnectionState.Disconnected;
            //    }
            //}
            //catch
            //{
            //    State = ConnectionState.Failed;
            //}
        }
    }
}
