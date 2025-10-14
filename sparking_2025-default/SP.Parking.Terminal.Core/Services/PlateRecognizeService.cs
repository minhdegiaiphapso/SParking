using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Utilities;

namespace SP.Parking.Terminal.Core.Services
{
    public class PlateRecognizeService : IALPRService
    {
        private const string DATALENGTHFORMAT = "00000000";
        private const string DEFAULT_VEHICLE_NUMBER = "";
        private ConcurrentDictionary<string, Action<string, Exception>> _dict = new ConcurrentDictionary<string, Action<string, Exception>>();
        IUserPreferenceService _userpreferenceService;
        
        public PlateRecognizeService()
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
            if (_userpreferenceService.OptionsSettings.PlateRecognitionBySfactors)
            {
                var bmp = ConvertToBitmap(image);
                if (bmp != null)
                {
                    var key = Guid.NewGuid();
                    _dict.TryAdd(key.ToString(), complete);
                    Task.Factory.StartNew(() => GetPlate(key.ToString(), image));
                    //var plate = detectorbike.ReadPlate(bmp);
                    //complete(plate.text, null);
                }
                else
                {
                    complete(string.Empty, new Exception("No image to received!"));
                }
            }
        }

        private void GetPlate(string key, byte[] imgByte)
        {
            var vehiclenumber = "";
            Exception exception = null;
            try
            {
                var mat = OpenCvSharp.Mat.FromImageData(imgByte);
                
                
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
            if (_userpreferenceService.OptionsSettings.PlateRecognitionBySfactors)
            {
                RecognizeLicensePlate(image, complete);
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
