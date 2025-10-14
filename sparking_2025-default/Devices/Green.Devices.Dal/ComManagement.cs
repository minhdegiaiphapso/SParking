using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Green.Devices.Dal
{
    public class ComManagement
    {
        private SerialPort _serialPort;
        private static ComManagement myInstance;
        private List<ComParameter> CommandsWaiting;
        //private ComParameter CurrentWorking;
        private Task _task; 
        private List<ComLog> Logs;
        private DateTime ApplyTime;
        private bool active = false;
        private ComManagement()
        {
            _serialPort = new SerialPort();
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Parity = Parity.None;
            _serialPort.WriteTimeout = 1000;
            _serialPort.ReadTimeout = 1000;
            _serialPort.DataReceived += _serialPort_DataReceived;
            CommandsWaiting = new List<ComParameter>();
            Logs = new List<ComLog>();
            _task = Task.Factory.StartNew(() => WatchCommand(), TaskCreationOptions.LongRunning);
        }
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var receivetime = DateTime.Now;
            var log = new ComLog()
            {
                Commands = new List<ComCommand>()
                    {
                        new ComCommand()
                        {
                            Command="Read",
                            CommandMessage=""
                        }
                    },
                TimeApply = receivetime,
                TimeReceive = receivetime,
            };
            try
            {
                log.State = "Thành công";
                byte[] buffer = new byte[(sender as SerialPort).ReadBufferSize];
                int bytesRead = (sender as SerialPort).Read(buffer, 0, buffer.Length);
                log.Message = (sender as SerialPort).PortName + ": ReceiveData: " + bytesRead.ToString() + " bytes";
                Task.Factory.StartNew(() => SfactorsComLogServices.Log(receivetime, log));
            }
            catch (Exception exp)
            {
                log.State = "Lỗi";
                log.Message = (sender as SerialPort).PortName + ": ReceiveData: " + exp.Message;
                Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
            }
        }
        public static ComManagement GetInstance()
        {
            if (myInstance == null)
            {
                myInstance = new ComManagement();  
            }
            return myInstance;
        }
        private void WatchCommand()
        {
            while(true)
            {
                if (active && CommandsWaiting != null && CommandsWaiting.Count > 0)
                {
                    WriteCom();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
        private void WriteCom()
        {
            if (CommandsWaiting != null && CommandsWaiting.Count > 0)
            {
                var CurrentWorking = CommandsWaiting[0];
                ComLog log = new ComLog
                {
                    TimeApply = CurrentWorking.TimeApply,
                    Description = CurrentWorking.Description
                };
                var _portExists = System.IO.Ports.SerialPort.GetPortNames().Any(p => p == CurrentWorking.ComName.ToUpper());
                if(!_portExists)
                {
                    log.State = "Cảnh báo:" + CurrentWorking.ComName;
                    log.Message = string.Format("{0} không có trong hệ thống.", CurrentWorking.ComName.ToUpper());
                    log.TimeReceive = DateTime.Now;
                    log.Commands = CurrentWorking.Commands;
                    CommandsWaiting.RemoveAt(0);
                    //Logs.Add(log);
                    Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                }
                else
                {
                    if (_serialPort.IsOpen)
                    {
                        ApplyTime = DateTime.Now;
                        var CurrentTime = DateTime.Now;
                        bool chk = true;
                        while(chk && _serialPort.IsOpen)
                        {
                            Thread.Sleep(100);
                            CurrentTime = DateTime.Now;
                            chk = (CurrentTime - ApplyTime).Seconds < 15;
                        }
                        if(_serialPort.IsOpen)
                        {
                            try
                            {
                                _serialPort.Close();
                                if (_serialPort.PortName != CurrentWorking.ComName.ToUpper())
                                {
                                    _serialPort.PortName = CurrentWorking.ComName.ToUpper();
                                }
                                _serialPort.Open();
                                foreach (var cm in CurrentWorking.Commands)
                                {
                                    if (cm.Command == "Write")
                                        _serialPort.Write(cm.CommandMessage);
                                    else
                                        _serialPort.WriteLine(cm.CommandMessage);
                                    //Thread.Sleep(100);
                                }
                                Thread.Sleep(100);
                                log.State = "Thành công:" + CurrentWorking.ComName;
                                log.TimeReceive = DateTime.Now;
                                log.Commands = CurrentWorking.Commands;
                                CommandsWaiting.RemoveAt(0);
                                Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                                //Logs.Add(log);
                                _serialPort.Close();
                            }
                            catch(Exception ex)
                            { 
                                log.State = "Lỗi:" + CurrentWorking.ComName;
                                log.Message = string.Format("{0} Chạy quá 15 giây không dừng. {1}", _serialPort.PortName,ex.Message);
                                log.TimeReceive = DateTime.Now;
                                log.Commands = CurrentWorking.Commands;
                                CommandsWaiting.RemoveAt(0);  
                                //Logs.Add(log);
                                Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                            }
                        }
                        else
                        {
                            try
                            {
                                if (_serialPort.PortName != CurrentWorking.ComName.ToUpper())
                                {
                                    _serialPort.PortName = CurrentWorking.ComName.ToUpper();
                                }
                                _serialPort.Open();
                                foreach (var cm in CurrentWorking.Commands)
                                {
                                    if (cm.Command == "Write")
                                        _serialPort.Write(cm.CommandMessage);
                                    else
                                        _serialPort.WriteLine(cm.CommandMessage);
                                    //Thread.Sleep(100);
                                }
                                Thread.Sleep(100);
                                log.State = "Thành công:" +CurrentWorking.ComName;
                                log.TimeReceive = DateTime.Now;
                                log.Commands = CurrentWorking.Commands;
                                CommandsWaiting.RemoveAt(0);
                                Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                                //Logs.Add(log);
                                _serialPort.Close();
                            }
                            catch (Exception ex)
                            {
                                log.State = "Lỗi:" + CurrentWorking.ComName;
                                log.Message = string.Format("{0}:{1}", _serialPort.PortName, ex.Message);
                                log.TimeReceive = DateTime.Now;
                                log.Commands = CurrentWorking.Commands;
                                CommandsWaiting.RemoveAt(0);
                                //Logs.Add(log);
                                Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                                _serialPort.Close();
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (_serialPort.PortName != CurrentWorking.ComName.ToUpper())
                            {
                                _serialPort.PortName = CurrentWorking.ComName.ToUpper();
                            }
                            _serialPort.Open();
                            foreach (var cm in CurrentWorking.Commands)
                            {
                                if (cm.Command == "Write")
                                    _serialPort.Write(cm.CommandMessage);
                                else
                                    _serialPort.WriteLine(cm.CommandMessage);
                                //
                            }
                            Thread.Sleep(100);
                            log.State = "Thành công:" + CurrentWorking.ComName;
                            log.TimeReceive = DateTime.Now;
                            log.Commands = CurrentWorking.Commands;
                            CommandsWaiting.RemoveAt(0);
                            Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                            //Logs.Add(log);
                            _serialPort.Close();
                        }
                        catch (Exception ex)
                        {
                            log.State = "Lỗi:" + CurrentWorking.ComName;
                            log.Message = string.Format("{0}:{1}", _serialPort.PortName, ex.Message);
                            log.TimeReceive = DateTime.Now;
                            log.Commands = CurrentWorking.Commands;
                            CommandsWaiting.RemoveAt(0);
                            //Logs.Add(log);
                            Task.Factory.StartNew(() => SfactorsComLogServices.Log(log.TimeReceive, log));
                            _serialPort.Close();
                        }
                    }
                }
            }
        }
        public void AddCommand(ComParameter Com)
        {
            active = false;
            CommandsWaiting.Add(Com);
            active = true;
        }
    }
    public class ComParameter
    {
        public string ComName { get; set; }
        public List<ComCommand> Commands { get; set; }
        public string Description { get; set; }
        public DateTime TimeApply { get; set; }
        public DateTime TimeReceive { get; set; } 

    }
    public class ComLog
    {
        public string State { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public DateTime TimeApply { get; set; }
        public DateTime TimeReceive { get; set; }
        public List<ComCommand> Commands { get; set; }
    }
    public class ComCommand
    {
        public string Command { get; set; }
        public string CommandMessage { get; set; }
    }
    public class SfactorsComLogServices
    {
        private static string GetPreferenceDirectory()
        {
            var documents = @"C:\ProgramData";
            var folder = Path.Combine(documents, "SfactorsComLog");
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
        public static void Log(DateTime date, ComLog myLog)
        {
            lock (mylock)
            {
                mylock = 1;
                List<ComLog> currentLog;
                var filePath = GetFileName(date);
                if (File.Exists(filePath))
                {
                    var data = File.ReadAllText(filePath);
                    currentLog = JsonConvert.DeserializeObject<List<ComLog>>(data);
                }
                else
                {
                    currentLog = new List<ComLog>();
                }
                currentLog.Add(myLog);
                var datasave = JsonConvert.SerializeObject(currentLog.OrderBy(S=>S.TimeReceive).Reverse().ToList(),Formatting.Indented);
                File.WriteAllText(filePath, datasave);    
                mylock = 0;
            }
        }
    }
}
