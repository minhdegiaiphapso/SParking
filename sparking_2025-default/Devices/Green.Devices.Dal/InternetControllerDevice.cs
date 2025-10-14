using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Green.Devices.Dal
{

    public delegate void InternetControllerDeviceHandle(ControllerDeviceInfo sender);
    //public event ControllerDeviceHandl 
    public class InternetControllerDevice
    {
        private NetworkCredential hearder;
        private WebClient client;
        
        private List<ControllerDeviceInfo> CommandsInfo;
        private Task _task;
        private static InternetControllerDevice _instance;

        private bool active = false;
        public bool Active{ get { return active; } }
        private InternetControllerDevice()
        {
            CommandsInfo = new List<ControllerDeviceInfo>();
            hearder = new NetworkCredential();
            client = new WebClient();
            client.Credentials = hearder;
            timeStamptG5 = timeStamptG6 = timeStamptG7 = timeStamptG8 = DateTime.Now;   
            _task = Task.Factory.StartNew(() => WatchCommand(), TaskCreationOptions.LongRunning);
        }
        private void WatchCommand()
        {
            while (true)
            {
                if (active && CommandsInfo != null && CommandsInfo.Count > 0)
                {
                    Switch();
                }
                else
                {
                    if(StatusInfo!=null && !string.IsNullOrEmpty(StatusInfo.IP))
                    {
                        try
                        {
                            WebClient sclient = new WebClient();
                            sclient.Credentials = new NetworkCredential(StatusInfo.UserName, StatusInfo.Password);
                            var sdata = sclient.OpenRead(string.Format("http://@{0}:{1}/status.xml", StatusInfo.IP, StatusInfo.Port));
                            var rd = new StreamReader(sdata);
                            var xml = rd.ReadToEnd();
                            rd.Close();
                            sdata.Close();
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(xml);    
                            if( doc.DocumentElement.SelectSingleNode("led4").InnerText=="1")
                            {
                                //this.AddCommandInfo(new ControllerDeviceInfo()
                                //{
                                //    UserName = StatusInfo.UserName,
                                //    Password = StatusInfo.Password,
                                //    IP = StatusInfo.IP,
                                //    Port = StatusInfo.Port,
                                //    PortNumber = "6"
                                //});
                                var timeStampt = DateTime.Now;
                                var ts = (timeStampt - timeStamptG5).Seconds;
                                if (RaiseStatusChangedG5 != null && ts >= 2)
                                {
                                    timeStamptG5 = timeStampt;
                                    RaiseStatusChangedG5(StatusInfo);
                                }
                            }
                            if (doc.DocumentElement.SelectSingleNode("led5").InnerText == "1")
                            {
                                //this.AddCommandInfo(new ControllerDeviceInfo()
                                //{
                                //    UserName = StatusInfo.UserName,
                                //    Password = StatusInfo.Password,
                                //    IP = StatusInfo.IP,
                                //    Port = StatusInfo.Port,
                                //    PortNumber = "6"
                                //});
                                var timeStampt = DateTime.Now;
                                var ts = (timeStampt - timeStamptG6).Seconds;
                                if (RaiseStatusChangedG6 != null && ts >= 2)
                                {
                                    timeStamptG6 = timeStampt;
                                    RaiseStatusChangedG6(StatusInfo);
                                }
                            }
                            if (doc.DocumentElement.SelectSingleNode("led6").InnerText == "1")
                            {
                                //this.AddCommandInfo(new ControllerDeviceInfo()
                                //{
                                //    UserName = StatusInfo.UserName,
                                //    Password = StatusInfo.Password,
                                //    IP = StatusInfo.IP,
                                //    Port = StatusInfo.Port,
                                //    PortNumber = "7"
                                //});
                                var timeStampt = DateTime.Now;
                                var ts = (timeStampt - timeStamptG7).Seconds;
                                if (RaiseStatusChangedG7 != null && ts >= 2)
                                {
                                    timeStamptG7 = timeStampt;
                                    RaiseStatusChangedG7(StatusInfo);
                                }
                            }
                            if (doc.DocumentElement.SelectSingleNode("led7").InnerText == "1")
                            {  
                                //this.AddCommandInfo(new ControllerDeviceInfo()
                                //{
                                //    UserName = StatusInfo.UserName,
                                //    Password = StatusInfo.Password,
                                //    IP = StatusInfo.IP,
                                //    Port = StatusInfo.Port,
                                //    PortNumber = "8"
                                //});
                                var timeStampt = DateTime.Now;
                                var ts = (timeStampt - timeStamptG8).Seconds;
                                if (RaiseStatusChangedG8 != null && ts >= 2)
                                {
                                    timeStamptG8 = timeStampt;
                                    RaiseStatusChangedG8(StatusInfo);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ;
                        }
                    }
                    Thread.Sleep(100);
                }
            }
        }
        //public 
        private string Switch()
        {
            string res = "";
            if (active && CommandsInfo != null && CommandsInfo.Count > 0)
            {
                ControllerDeviceInfo info = CommandsInfo[0];
                var lstNumPorts = info.PortNumber.Split(',');   
                try
                {
                    if (hearder.UserName != info.UserName)
                        hearder.UserName = info.UserName;
                    if (hearder.Password != info.Password)
                        hearder.Password = info.Password;
                    foreach (var portnum in lstNumPorts)
                    {
                        int pn;
                        if (int.TryParse(portnum, out pn))
                        {
                            pn--;
                            res += Read(info, pn);
                            Thread.Sleep(100);
                        }
                    }
                    res = "Success";
                    CommandsInfo.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    res = ex.Message;
                    ControlDeviceLog log = new ControlDeviceLog()
                    {
                        InternetControlInfo = info,
                        Message = res
                    };
                    Task.Factory.StartNew(() => SfactorsInternetControlDeviceLogServices.Log(log));
                    CommandsInfo.RemoveAt(0);
                }     
            }
            else
            {
                res = "No command!";
            }
            return res;
        }
     
        private string Read(ControllerDeviceInfo info, int portNumber)
        {
            string res = "";
            try
            {
                Stream data = client.OpenRead(string.Format("http://@{0}:{1}/leds.cgi?led={2}", info.IP, info.Port, portNumber));
                StreamReader reader = new StreamReader(data);
                res = reader.ReadToEnd();
                reader.Close();
                data.Close();
            }
            catch (Exception ex)
            {

                res = ex.Message;
                ControlDeviceLog log = new ControlDeviceLog()
                {
                    InternetControlInfo = info,
                    Message = res
                };
                Task.Factory.StartNew(() => SfactorsInternetControlDeviceLogServices.Log(log));
            }
            return res;
        }
        public Action<ControllerDeviceInfo> RaiseStatusChangedG5;
        public Action<ControllerDeviceInfo> RaiseStatusChangedG6;
        public Action<ControllerDeviceInfo> RaiseStatusChangedG7;
        public Action<ControllerDeviceInfo> RaiseStatusChangedG8;
        //public event InternetControllerDeviceHandle RaiseStatusChangedG5;

        //public event InternetControllerDeviceHandle RaiseStatusChangedG6;
        //public event InternetControllerDeviceHandle RaiseStatusChangedG7;
        //public event InternetControllerDeviceHandle RaiseStatusChangedG8;
        private DateTime timeStamptG5;
        private DateTime timeStamptG6;
        private DateTime timeStamptG7;
        private DateTime timeStamptG8;
        public ControllerDeviceInfo StatusInfo { get; set; }

        public static InternetControllerDevice GetInstance()
        {
            if (_instance == null)
                _instance = new InternetControllerDevice();
            return _instance;
        }
        public void AddCommandInfo(ControllerDeviceInfo info)
        {
            active = false;
            CommandsInfo.Add(info);
            active = true;
        }
    }
  
    public class ControllerDeviceInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string PortNumber { get; set; }
        public string ButtonNumber { get; set; }
    }
    public class ControlDeviceLog
    {
      
        public ControllerDeviceInfo InternetControlInfo { get; set; }
        public string Message { get; set; }
    }
    public class SfactorsInternetControlDeviceLogServices
    {
        private static string GetPreferenceDirectory()
        {
            var documents = @"C:\ProgramData";
            var folder = Path.Combine(documents, "SfactorsInternetControlDeviceLog");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }
        private static string GetFileName(DateTime date)
        {
            var folder = GetPreferenceDirectory();
            return Path.Combine(folder, date.ToString("yyyyMMdd") + ".conf");
        }
        private static object mylock = 0;
        public static void Log(ControlDeviceLog myLog)
        {
            
            lock (mylock)
            {
                mylock = 1;
                List<ControlDeviceLog> currentLog;
                var filePath = GetFileName(DateTime.Now);
                if (File.Exists(filePath))
                {
                    var data = File.ReadAllText(filePath);
                    currentLog = JsonConvert.DeserializeObject<List<ControlDeviceLog>>(data);
                }
                else
                {
                    currentLog = new List<ControlDeviceLog>();
                }
                currentLog.Add(myLog);
                var datasave = JsonConvert.SerializeObject(currentLog, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, datasave);
                mylock = 0;
            }
        }
    }
  
}
