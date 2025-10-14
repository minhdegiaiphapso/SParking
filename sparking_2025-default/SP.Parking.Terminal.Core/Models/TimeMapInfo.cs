using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class TimeMapInfo
    {
        public static ServerTimeInfo Current = new ServerTimeInfo() { UtcTime = DateTime.UtcNow, LocalTime = DateTime.Now };
    }
}
