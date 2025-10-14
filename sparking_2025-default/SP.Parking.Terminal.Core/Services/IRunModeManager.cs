using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IRunModeManager
    {
        ArgumentParameter ArgumentParams { get; set; }
    }

    public class RunModeManager : IRunModeManager
    {
        public ArgumentParameter ArgumentParams { get; set; }
    }
}
