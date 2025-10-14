using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public interface ILogService
    {
        void Log(Exception exception, string logServer = null, string target = null, int statusCode = 0, string requestUrl = null, bool captureScreen = false);
    }
}
