using SP.Parking.Terminal.Core.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Portal
{
    public interface IPortalClient
    {
        Passenger GetPassenger(string url);
    }
}
