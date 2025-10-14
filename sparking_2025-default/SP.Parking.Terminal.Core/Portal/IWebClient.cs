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
    public interface IWebClient
    {
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        string BaseUrl { get; set; }

        /// <summary>
        /// Executes the specified strong-typed request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        RestResponse<T> Execute<T>(RestRequest request) where T : new();

        /// <summary>
        /// Executes the specified strong-typed request and expect status code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns></returns>
        RestResponse<T> Execute<T>(RestRequest request, HttpStatusCode expectedStatusCode) where T : new();

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IAsyncWebClient
    {
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        string BaseUrl { get; set; }

        /// <summary>
        /// Executes the specified strong-typed request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        void Execute<T>(RestRequest request, Action<RestResponse<T>, Exception> complete) where T : class;

        /// <summary>
        /// Executes the specified strong-typed request and expect status code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns></returns>
        void Execute<T>(RestRequest request, HttpStatusCode expectedStatusCode, Action<RestResponse<T>, Exception> complete) where T : class;

        /// <summary>
        /// Download the specified request and complete.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="complete">Complete.</param>
        void Download(RestRequest request, Action<long, long> progress, Action<byte[], Exception> complete);

        void Download(string url, Action<long, long> progress, Action<byte[], Exception> complete);
    }
}
