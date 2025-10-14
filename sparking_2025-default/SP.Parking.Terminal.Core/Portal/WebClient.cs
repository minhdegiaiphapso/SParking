using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Portal
{
    /// <summary>
    /// 
    /// </summary>
    public class WebClient : IWebClient
    {
        #region --- Private members ---
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiProxy"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        //        public WebApiClient(string baseUrl)
        public WebClient(string baseUrl)
        {
            this.BaseUrl = baseUrl;            
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Executes the specified strong-typed request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException"></exception>
        public virtual RestResponse<T> Execute<T>(RestRequest request) where T : new()
        {
#if WAITING_SIMULATION
			System.Threading.Thread.Sleep(500);
#endif

            var client = new RestClient();
            //client.SetUserAgent<WebClient>("OneManga-Client");
            if (this.BaseUrl != null)
                client.BaseUrl = this.BaseUrl;

            try
            {
                var response = client.Execute<T>(request);
                return response as RestResponse<T>;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                const string message = "Error retrieving response. Check inner details for more info.";
                throw new ApplicationException(message, ex);
            }
        }

        /// <summary>
        /// Executes the specified strong-typed request and expect status code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public virtual RestResponse<T> Execute<T>(RestRequest request, HttpStatusCode expectedStatusCode)
            where T : new()
        {
#if WAITING_SIMULATION
			System.Threading.Thread.Sleep(500);
#endif
            var response = Execute<T>(request);

            if (response.StatusCode != expectedStatusCode)
            {
                string message = string.Format("Error retrieving resource {0}. Expected status code is {1}, actual status code is {2}. Response message: {3}",
                    request.Resource, expectedStatusCode, response.StatusCode, string.Empty);
                throw new Exception(message);
            }

            return response;

        }
    }
}