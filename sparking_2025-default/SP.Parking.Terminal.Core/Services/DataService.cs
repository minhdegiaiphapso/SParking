using SP.Parking.Terminal.Core.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public class DataService : IDataService
    {
        /// <summary>
        /// The portal client, use to consume web service endpoints
        /// </summary>
        IPortalClient _client;


        private static readonly string PASSENGER_ENDPOINT;

        static DataService()
        {

        }

        public DataService(IPortalClient client)
        {
            _client = client;
        }

        
    }
}
