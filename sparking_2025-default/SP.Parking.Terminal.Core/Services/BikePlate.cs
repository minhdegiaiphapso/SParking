using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Utilities;
using IPSS;

namespace SP.Parking.Terminal.Core.Services
{
    public class BikePlate : IALPRService
    {
        private const string DATALENGTHFORMAT = "00000000";
        private const string DEFAULT_VEHICLE_NUMBER = "";
        private ConcurrentDictionary<string, Action<string, Exception>> _dict = new ConcurrentDictionary<string, Action<string, Exception>>();
        IUserPreferenceService _userpreferenceService;
        IPSSbike detectorbike;// = new IPSSbike();
        IPSScar detectorcar;
        public BikePlate()
        {
            _userpreferenceService = Mvx.Resolve<IUserPreferenceService>();
            //detectorbike = new IPSSbike();
            //detectorcar = new IPSScar();

        }
        private Bitmap ConvertToBitmap(byte[] imag)
        {
            try
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imag))
                {
                    return new Bitmap(ms);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public void RecognizeLicensePlate(byte[] image, Action<string, Exception> complete)
        {
            if (_userpreferenceService.OptionsSettings.PlateRecognitionEnable)
            {
                var bmp = ConvertToBitmap(image);
                if (bmp != null && detectorbike.IsLicense)
                {
                    var key = Guid.NewGuid();
                    string dir = System.AppDomain.CurrentDomain.BaseDirectory + "Temp";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    string imagePath = string.Format(dir + @"\{0}.jpg", key);
                    _dict.TryAdd(key.ToString(), complete);
                    Task.Factory.StartNew(() => GetPlate(key.ToString(), bmp, 1));
                    //var plate = detectorbike.ReadPlate(bmp);
                    //complete(plate.text, null);
                }
                else
                {
                    complete(string.Empty, new Exception("No image to received!"));
                }
            }
        }

        private void GetPlate(string key, Bitmap bmp, int v)
        {
            var vehiclenumber = "";
            Exception exception = null;
            try
            {
                if(v==1)
                {
                    var plate = detectorbike.ReadPlate(bmp);
                    vehiclenumber = plate.text;
                }
                if(v==2)
                {
                    var plate = detectorcar.ReadPlate(bmp);
                    vehiclenumber = plate.text;
                }
            }
            catch(Exception ex)
            {
                exception = (Exception)Activator.CreateInstance(ex.GetType(), ex.Message , ex);
                Mvx.Resolve<ILogService>().Log(exception, _userpreferenceService.HostSettings.LogServerIP);
            }
            finally
            {
                if (_dict.ContainsKey(key))
                {
                    var tValue = _dict[key];
                    if (tValue != null)
                        tValue(vehiclenumber, exception);

                    Action<string, Exception> action = null;
                    _dict.TryRemove(key, out action);
                }
            }
        }

        public void RecognizeLicensePlate(byte[] image, int VehicleType, Action<string, Exception> complete)
        {
            if (_userpreferenceService.OptionsSettings.PlateRecognitionEnable)
            {
                var bmp = ConvertToBitmap(image);
                if (VehicleType == 1000001)
                {
                    if (detectorbike == null)
                    {
                        detectorbike = new IPSSbike();
                        detectorbike.EnableLog = false;
                    }
                    if (bmp != null && detectorbike.IsLicense)
                    {
                        var key = Guid.NewGuid();
                       
                        _dict.TryAdd(key.ToString(), complete);
                        Task.Factory.StartNew(() => GetPlate(key.ToString(),bmp, 1));
                        //var plate = detectorbike.ReadPlate(bmp);
                        //complete(plate.text, null);
                        //if (plate.hasPlate && plate.isValid)

                        //else
                        //    complete(string.Empty, new Exception(plate.error));
                    }
                    else
                    {
                        complete(string.Empty, new Exception("No image to received!"));
                    }
                }
                else
                {
                    if (detectorcar == null)
                        detectorcar = new IPSScar();
                    if (bmp != null && detectorcar.IsLicense)
                    {
                        var key = Guid.NewGuid();
                        _dict.TryAdd(key.ToString(), complete);
                        Task.Factory.StartNew(() => GetPlate(key.ToString(), bmp, 2));
                        //var plate = detectorcar.ReadPlate(bmp);
                        ////if (plate.hasPlate && plate.isValid)
                        //complete(plate.text, null);
                        //else
                        //complete(string.Empty, new Exception(plate.error));
                    }
                    else
                    {
                        complete(string.Empty, new Exception("No image to received!"));
                    }
                }
            }
        }
        public static string ExtractVehicleNumber(string rawNumber)
        {
            if (string.IsNullOrEmpty(rawNumber)) return DEFAULT_VEHICLE_NUMBER;
            string result = OtherUtilities.GetLastGroupNumber(rawNumber);
            if (string.IsNullOrEmpty(result))
                return DEFAULT_VEHICLE_NUMBER;
            return result;
        }
        public static string ExtractPrefixVehicleNumber(string rawNumber, string vehicleNumber)
        {
            string prefix = DEFAULT_VEHICLE_NUMBER;

            if (!string.IsNullOrEmpty(rawNumber) && !string.IsNullOrEmpty(vehicleNumber))
                prefix = rawNumber.Replace(vehicleNumber, "");

            prefix = OtherUtilities.RemoveLastNonDigitWordChar(prefix);
            if (!string.IsNullOrEmpty(rawNumber) && !string.IsNullOrEmpty(prefix))
                prefix = prefix.Replace(vehicleNumber, "");

            return prefix;
        }
    }
}
