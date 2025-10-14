using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RestSharp;
using System.Diagnostics;
using NLog;
using System.Reflection;
//using NLog;

namespace SP.Parking.Terminal.Core.Services
{
    public class WebClientBasic : IWebClient
    {
        private IHostSettings _hostSettings;
        private Timer _timer;
        private RestClient _restClient;
        private bool _isUsingSecondaryServer = false;
        static Logger _logger = LogManager.GetCurrentClassLogger();
        //static log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Is using secondary server
        /// </summary>
        public bool IsUsingSecondaryServer
        {
            get { return _isUsingSecondaryServer; }
            private set
            {
                if (_isUsingSecondaryServer == value) return;
                _isUsingSecondaryServer = value;
            }
        }

        protected virtual void RaiseServerChangedNotification() { }

        /// <summary>
        /// Gets using server host
        /// </summary>
        public string UsingServerHost
        {
            get { return IsUsingSecondaryServer ? _hostSettings.SecondaryServerIP : _hostSettings.PrimaryServerIP; }
        }

        /// <summary>
        /// Contructs WebClientBasic
        /// </summary>
        /// <param name="hostSettings">Host settings contains primary and secondary servers information.</param>
        /// <param name="useSecondaryServerDuration">Duration of temporary using secondary server when primary one is down. Default is 5 minutes.</param>
        public WebClientBasic(IHostSettings hostSettings, int useSecondaryServerDuration = 1000 * 30)
        {
            // Init internal attributes
            _isUsingSecondaryServer = false;
            _restClient = new RestClient();
            _restClient.Timeout = 10000;
            _hostSettings = hostSettings;
            // Init timer to switch back primary server after change to secondary server
            _timer = new Timer();
            _timer.Interval = useSecondaryServerDuration;
            _timer.AutoReset = false;
            _timer.Elapsed += SwitchToPrimaryServer;
        }

        /// <summary>
        /// Start using secondary server and active timer to switch back primary server
        /// </summary>
        public void StartUseSecondaryServer()
        {
            IsUsingSecondaryServer = true;
            RaiseServerChangedNotification();
            _timer.Start();
        }

        /// <summary>
        /// Switch back to primary server
        /// </summary>
        public void SwitchToPrimaryServer(object sender, ElapsedEventArgs e)
        {
            IsUsingSecondaryServer = false;
            RaiseServerChangedNotification();
        }

        /// <summary>
        /// Execute a request synchronously
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Response of request</returns>
        public IRestResponse ExecuteSync(IRestRequest request)
        {
            if (!IsUsingSecondaryServer)
            {
                return ExecuteSync(_hostSettings.PrimaryServerIP, request);
                //var response = ExecuteSync(_hostSettings.PrimaryServerIP, request);
                //if (response.ResponseStatus != ResponseStatus.Completed)
                //{
                //    StartUseSecondaryServer();
                //    return ExecuteSync(request);
                //}
                //else
                //{
                //    return response;
                //}
            }
            else
            {
                return ExecuteSync(_hostSettings.SecondaryServerIP, request);
            }
        }

        /// <summary>
        /// Execute a request synchronously
        /// </summary>
        /// <param name="host">Host to request to</param>
        /// <param name="request">Request to execute</param>
        /// <returns>Response of request</returns>
        public IRestResponse ExecuteSync(string host, IRestRequest request)
        {
            _restClient.BaseUrl = "http://" + host;
            return _restClient.Execute(request);
        }

        /// <summary>
        /// Execute a request asynchronously
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <param name="callback">Result callback</param>
        public void ExecuteAsync(RestSharp.IRestRequest request, Action<RestSharp.IRestResponse> callback)
        {
            try
            {
                // If using primary server and fail to connect, switch to secondary server and request again
                if (!IsUsingSecondaryServer)
                {
                    ExecuteAsync(_hostSettings.PrimaryServerIP, request, (response) => {
                        //LogErrorIfOccur(response);
                        //if (response.ResponseStatus == ResponseStatus.TimedOut)
                        //{
                        //    StartUseSecondaryServer();
                        //    ExecuteAsync(request, callback);
                        //}
                        //else
                        //{
                        //    callback(response);
                        //}
                        LogErrorIfOccur(response);
                        callback(response);
                    });
                }
                else
                {
                    ExecuteAsync(_hostSettings.SecondaryServerIP, request, (response) => {
                        LogErrorIfOccur(response);
                        callback(response);
                    });
                }
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Process logging error if occurs
        /// </summary>
        /// <param name="response">Response to check</param>
        protected virtual void LogErrorIfOccur(IRestResponse response) { }

        /// <summary>
        /// Execute a request asynchronous to a specified host
        /// </summary>
        /// <param name="host">Host to request to</param>
        /// <param name="request">Request to execute</param>
        /// <param name="callback">Result callback</param>
        public void ExecuteAsync(string host, IRestRequest request, Action<IRestResponse> callback)
        {
            try
            {
                _restClient.BaseUrl = "http://" + host;
                _restClient.ExecuteAsync(request, callback);
            }
            catch(Exception ex)
            {
                IRestResponse response = new RestResponse();
                response.ErrorException = ex;
                response.ErrorMessage = ex.Message;
                response.Request = request;
                response.ResponseStatus = ResponseStatus.Aborted;
                callback(response);
            }
        }

        /// <summary>
        /// Download data at specified path from server asynchronously
        /// </summary>
        /// <param name="path">Path of file to download</param>
        /// <param name="callback">Result callback</param>
        public void DownloadData(string path, Action<byte[], Exception> callback)
        {
            DoDownloadData(UsingServerHost, path, callback);
        }

        /// <summary>
        /// Download data at specified path from specified host asynchronously
        /// </summary>
        /// <param name="host">Host to download from</param>
        /// <param name="path">Path of file to download</param>
        /// <param name="callback">Result callback</param>
        public void DownloadData(string host, string path, Action<byte[], Exception> callback)
        {
            DoDownloadData(host, path, callback);
        }

        private class MyWebClient : System.Net.WebClient
        {
            protected override System.Net.WebRequest GetWebRequest(Uri address)
            {
                System.Net.WebRequest w = base.GetWebRequest(address);
                w.Timeout = 1000;
                return w;
            }
        }

        private void DoDownloadData(string host, string path, Action<byte[], Exception> callback)
        {
            try
            {
                MyWebClient webClient = new MyWebClient();
                Stopwatch timer = new Stopwatch();
                timer.Start();
                webClient.DownloadDataCompleted += OnDownloadDataCompleted;
                //webClient.DownloadDataCompleted += (sender, e) => {
                //    OnDownloadDataCompleted(sender, e, timer, host, path);
                //};

                // Create additional request information
                Dictionary<string, object> requestInfo = new Dictionary<string, object>();
                requestInfo["host"] = host;
                requestInfo["path"] = path;
                requestInfo["callback"] = callback;
                requestInfo["timer"] = timer;
                // Execute download with request information as an argument

                webClient.DownloadDataAsync(new Uri(string.Format("http://{0}/{1}", host, path)), requestInfo);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }
        }

        /// <summary>
        /// Download data at specified path from server synchronously
        /// </summary>
        /// <param name="path">Path of file to download</param>
        /// <returns>Downloaded bytes</returns>
        public byte[] DownloadData(string path)
        {
            return DownloadData(UsingServerHost, path);
        }

        /// <summary>
        /// Download data at specified path from specified host synchronously
        /// </summary>
        /// <param name="host">Host to download from</param>
        /// <param name="path">Path of file to download</param>
        /// <returns>Downloaded bytes</returns>
        public byte[] DownloadData(string host, string path)
        {
            try
            {
                System.Net.WebClient webClient = new System.Net.WebClient();
                return webClient.DownloadData(new Uri(string.Format("http://{0}/{1}", host, path)));
            }
            catch
            {
                if (host == UsingServerHost)
                    return null;
                else
                    return DownloadData(UsingServerHost, path);
            }
        }

        private void OnDownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            if (e.UserState != null)
            {
                Dictionary<string, object> requestInfo = (Dictionary<string, object>)e.UserState;
                Action<byte[], Exception> callback = (Action<byte[], Exception>)requestInfo["callback"];
                string host = (string)requestInfo["host"];
                string path = (string)requestInfo["path"];
                Stopwatch watch = (Stopwatch)requestInfo["timer"];
                watch.Stop();

                // If file is downloaded successfully, callback with successful result
                if (e.Error == null)
                {
                    _logger.Info(string.Format("{0} - Success load from: {1}/{2} ({3})", GetVersion(), host, path, watch.ElapsedMilliseconds));
                    callback(e.Result, null);
                }
                // If file is fail to download, retry with server
                else
                {
                    _logger.Info(string.Format("{0} - Fail load from: {1}/{2} ({3})", GetVersion(), host, path, watch.ElapsedMilliseconds));
                    //if (host == UsingServerHost)
                    //    callback(null, e.Error);
                    //else
                    //    DoDownloadData(UsingServerHost, path, callback);
                }
            }
        }

        public static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            return version;
        }
    }
}
