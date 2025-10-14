using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicensePlateRecognition
{
    public interface IALPRWorker
    {
        string LicensePlateRecognize(Bitmap image);
        string LicensePlateRecognize(string filepath);
        string LicensePlateRecognize(byte[] imageData);
    }
}
