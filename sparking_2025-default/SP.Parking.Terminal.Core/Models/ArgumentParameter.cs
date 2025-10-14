using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public enum RunMode
    {
        Production = 0,
        Testing,
    }

    public class ArgumentParameter
    {
        public RunMode Mode { get; set; }
        public string[] Host { get; set; }
        public string TestHost { get; set; }
    }
}
