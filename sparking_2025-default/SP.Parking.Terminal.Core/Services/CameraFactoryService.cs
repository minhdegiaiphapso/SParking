using System;
using DirectShowLib;
using Emgu.CV;

namespace SP.Parking.Terminal.Core.Services
{
    public class Video_Device
    {
        public string Device_Name { get; set; }
        public int Device_ID { get; set; }
        public Guid Identifier { get; set; }

        public Video_Device(int ID, string Name, Guid Identity = new Guid())
        {
            Device_ID = ID;
            Device_Name = Name;
            Identifier = Identity;
        }

        /// <summary>
        /// Represent the Device as a String
        /// </summary>
        /// <returns>The string representation of this color</returns>
        public override string ToString()
        {
            return String.Format("[{0}] {1}: {2}", Device_ID, Device_Name, Identifier);
        }
    }

    public interface IWebcamFactoryService
    {

    }

    public class WebcamFactoryService
    {
        public static Video_Device[] WebCams { get; set; }

        static WebcamFactoryService()
        {
            DsDevice[] webcams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            WebCams = new Video_Device[webcams.Length];

            for (int i = 0; i < webcams.Length; i++)
            {
                WebCams[i] = new Video_Device(i, webcams[i].Name, webcams[i].ClassID);
            }
        }

        public static Capture GetWebcam(int index)
        {
			Capture capture = new Capture(WebCams[index].Device_ID);
            return capture;
        }
    }
}