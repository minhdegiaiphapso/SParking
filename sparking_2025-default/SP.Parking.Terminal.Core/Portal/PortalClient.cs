using SP.Parking.Terminal.Core.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Portal
{
    public class PortalClient : IPortalClient
    {
        private readonly IWebClient _syncClient;

        public PortalClient(string url)
        {
            _syncClient = new WebClient(url);
        }

        /// <summary>
        /// Gets a resource from a resource url.
        /// </summary>
        /// <typeparam name="T">Type of the resource.</typeparam>
        /// <param name="resourceUrl">The resource URL.</param>
        /// <returns></returns>
        private T GetResource<T>(string resourceUrl) where T : new()
        {
            var request = new RestRequest(resourceUrl, RestSharp.Method.GET);
            return GetResource<T>(request);
        }

        private T GetResource<T>(RestRequest request) where T : new()
        {
            return _syncClient.Execute<T>(request).Data;
        }

        public Passenger GetPassenger(string url)
        {
            return GetResource<Passenger>(url);
        }
    }
}
