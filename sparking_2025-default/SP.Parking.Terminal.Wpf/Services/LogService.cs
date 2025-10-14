using NLog;
using RestSharp;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Threading;
using log4net;
using NLog.Config;
using NLog.Targets;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.Models;

namespace SP.Parking.Terminal.Wpf.Services
{
    public class ExceptionLog
    {
        public Exception Exception { get; set; }
        public string LogServer { get; set; }
        public string Target { get; set; }
        public int StatusCode { get; set; }
        public string RequestInfo { get; set; }
    }

    public class LogService : ILogService
    {
        const int LOG_INTERVAL = 30 * 1000;
        System.Timers.Timer _logTimer;
        Logger _logger;
        //log4net.ILog _4NetLogger;
        BlockingCollection<ExceptionLog> _queue;
        int fileId = 0;
        IHostSettings _hostSettings;
        string _version = string.Empty;

        public LogService(IHostSettings hostSettings)
        {
            _hostSettings = new HostSettings();
            _version = OtherUtilities.GetVersion();
            ReConfigureNLog();
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _queue = new BlockingCollection<ExceptionLog>();
            PushLog();
        }

        private void ReConfigureNLog()
        {
            string dir = Path.Combine(_hostSettings.StoragePath, "logs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var target1 = (FileTarget)NLog.LogManager.Configuration.FindTargetByName("logfile");
            target1.FileName = Path.Combine(dir, @"${date:format=yyyy-MM-dd}.log");
            target1.ArchiveFileName = Path.Combine(dir, @"logs\${shortdate}-{####}.log");

            var target2 = (FileTarget)NLog.LogManager.Configuration.FindTargetByName("requestException");
            target2.FileName = Path.Combine(dir, @"exception\${date:format=yyyy-MM-dd}\${date:format=yyyy-MM-dd HH00}.log");
            target2.ArchiveFileName = Path.Combine(dir, @"exception\${shortdate}-{####}.log");

            var target3 = (FileTarget)NLog.LogManager.Configuration.FindTargetByName("replicatefile");
            target3.FileName = Path.Combine(dir, @"replicate\${date:format=yyyy-MM-dd}\${date:format=yyyy-MM-dd HH00}.log");
            target3.ArchiveFileName = Path.Combine(dir, @"replicate\${shortdate}-{####}.log");

            var target4 = (FileTarget)NLog.LogManager.Configuration.FindTargetByName("loadimagedurationfile");
            target4.FileName = Path.Combine(dir, @"loadimageduration\${date:format=yyyy-MM-dd}\${date:format=yyyy-MM-dd HH00}.log");
            target4.ArchiveFileName = Path.Combine(dir, @"loadimageduration\${shortdate}-{####}.log");

            NLog.LogManager.ReconfigExistingLoggers();
        }

        private void PushLog()
        {
            Task.Factory.StartNew(() => {
                try
                {
                    while (true)
                    {
                        int count = 0;
                        foreach (ExceptionLog item in _queue.GetConsumingEnumerable())
                        {
                            if (count > 5)
                                break;

                            RestClient restClient = new RestClient("http://" + item.LogServer);
                            var request = new RestRequest("/api/log-client-exception/", Method.POST);
                            request.AddParameter("detail", _version + " - " + item.Exception.ToString());
                            if (item.StatusCode > 0)
                                request.AddParameter("status_code", item.StatusCode);
                            if (item.Target != null)
                                request.AddParameter("target", item.Target);
                            if (item.RequestInfo != null)
                                request.AddParameter("request_info", item.RequestInfo);
                            //byte[] snapshot = MemCapture();
                            //if (snapshot != null)
                            //    request.AddFile("snapshot", snapshot, "snapshot");
                            //restClient.ExecuteAsync(request, response => { });

                            restClient.Execute(request);
                            count++;
                        }
                        System.Threading.Thread.Sleep(LOG_INTERVAL);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private Bitmap Capture()
        {
            try
            {
                Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(bitmap as Image);
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Capture exception");
                return null;
            }
        }

        private void DiskCapture()
        {
            using (Bitmap bitmap = Capture())
            {
                if (bitmap != null)
                {
                    var now = TimeMapInfo.Current.LocalTime;
                    Interlocked.Increment(ref fileId);
                    string path = Path.Combine(GetPath(), fileId + "-" + now.ToString("HHmmss") + ".jpg");
                    //string path = Path.Combine(GetPath(), fileId + "-" + DateTime.Now.ToString("HHmmss") + ".jpg");
                    bitmap.Save(path, ImageFormat.Jpeg);
                }
            }
        }

        //private byte[] MemCapture()
        //{
        //    Bitmap bitmap = Capture();
        //    if (bitmap != null)
        //    {
        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            bitmap.Save(stream, ImageFormat.Jpeg);
        //            stream.Close();
        //            return stream.ToArray();
        //        }
        //    }
        //    return null;
        //}

        private string GetLogPath()
        {
            string path = Path.Combine(_hostSettings.StoragePath, "logs");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        private string GetPath()
        {
            var now = TimeMapInfo.Current.LocalTime;
            string path = Path.Combine(GetLogPath(), "exception", now.ToString("yyyy-MM-dd"));
            //string path = Path.Combine(GetLogPath(), "exception", DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public void Log(Exception exception, string logServer = null, string target = null, int statusCode = 0, string requestInfo = null, bool captureScreen = false)
        {
            lock (this)
            {
                try
                {
                    if (exception != null)
                    {

                        _logger.Error(_version + " - " + exception.ToString());
                        //_4NetLogger.Error(exception.ToString());

                        if (string.IsNullOrEmpty(logServer) && captureScreen)
                        {
                            DiskCapture();
                        }
                        else if (!string.IsNullOrEmpty(logServer))
                        {
                            ExceptionLog item = new ExceptionLog { Exception = exception, LogServer = logServer, Target = target, StatusCode = statusCode, RequestInfo = requestInfo };
                            if (_queue.Count < 50)
                            {
                                _queue.Add(item);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}