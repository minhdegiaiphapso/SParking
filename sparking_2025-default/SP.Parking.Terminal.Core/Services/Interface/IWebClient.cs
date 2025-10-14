using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Timers;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IWebClient
    {
        bool IsUsingSecondaryServer { get; }
        IRestResponse ExecuteSync(IRestRequest request);
        IRestResponse ExecuteSync(string host, IRestRequest request);
        void ExecuteAsync(IRestRequest request, Action<IRestResponse> callback);
        void ExecuteAsync(string host, IRestRequest request, Action<IRestResponse> callback);
        void DownloadData(string path, Action<byte[], Exception> callback);
        void DownloadData(string host, string path, Action<byte[], Exception> callback);
        byte[] DownloadData(string path);
        byte[] DownloadData(string host, string path);
        void StartUseSecondaryServer();
        void SwitchToPrimaryServer(object sender, ElapsedEventArgs e);
    }
}
