using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RestSharp;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.CrossCore;
using System.Net;
using Newtonsoft.Json;

namespace SP.Parking.Terminal.Core.Services
{
    public class ChangeServerMessage : MvxMessage
    {
        public bool IsUseSecondary { get; set; }

        public ChangeServerMessage(object sender, bool isUseSecondary)
            : base(sender)
        {
            IsUseSecondary = isUseSecondary;
        }
    }

    public class WebClient : WebClientBasic
    {
        private IMvxMessenger _messenger;
        ILogService _logService;
        IHostSettings _hostSettings;

        public WebClient(IMvxMessenger messenger, IHostSettings hostSettings, int useSecondaryServerDuration = 1000 * 60 * 5)
            : base(hostSettings, useSecondaryServerDuration)
        {
            _messenger = messenger;
            _logService = Mvx.Resolve<ILogService>();
            _hostSettings = Mvx.Resolve<IHostSettings>();
        }

        protected override void RaiseServerChangedNotification() 
        {
            if (_messenger != null && _messenger.HasSubscriptionsFor<ChangeServerMessage>())
            {
                _logService.Log(new Exception("ChangeServerMessage"), _hostSettings.LogServerIP);
                _messenger.Publish(new ChangeServerMessage(this, IsUsingSecondaryServer));
            }
        }

        private string GetRequestInfo(IRestResponse response)
        {
            IRestRequest request = response.Request;
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["end_point"] = request.Resource;
            data["method"] = request.Method.ToString();
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            foreach(Parameter param in request.Parameters)
            {
                paramDict[param.Name] = param.Value;
            }
            data["params"] = paramDict;
            return JsonConvert.SerializeObject(data);
        }

        protected override void LogErrorIfOccur(IRestResponse response)
        {
            Exception exception = null;
            int status = 0;
            string requestInfo = null;
            switch (response.ResponseStatus)
            {
                case ResponseStatus.Error:
                case ResponseStatus.Aborted:
                case ResponseStatus.None:
                    exception = new Exception(response.ErrorMessage);
                    status = 500;
                    requestInfo = GetRequestInfo(response);
                    break;
                case ResponseStatus.TimedOut:
                    exception = new Exception("Request Timeout");
                    status = 408;
                    requestInfo = GetRequestInfo(response);
                    break;
                case ResponseStatus.Completed:
                    System.Net.HttpStatusCode code = response.StatusCode;
                    if (code == HttpStatusCode.BadGateway ||
                        code == HttpStatusCode.InternalServerError ||
                        code == HttpStatusCode.GatewayTimeout ||
                        code == HttpStatusCode.RequestTimeout ||
                        code == HttpStatusCode.ServiceUnavailable)
                    {
                        exception = new Exception(response.ErrorMessage);
                        status = (int)code;
                        requestInfo = GetRequestInfo(response);
                    }
                    break;
            }
            if (exception != null)
            {
                if (_hostSettings.LogServerIP != null)
                    _logService.Log(exception, _hostSettings.LogServerIP, UsingServerHost, status, requestInfo);
                else
                    _logService.Log(exception);
            }
        }
    }
}
