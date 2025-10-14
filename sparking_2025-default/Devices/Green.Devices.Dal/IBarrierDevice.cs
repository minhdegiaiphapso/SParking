using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Green.Devices.Dal
{
    public interface IBarrierDevice
    {
		string DeviceName { get; }
        string DevicePort { get; set; }
        string PortName { get; }
        bool Open();
        bool Close();
        
        
    }

	public interface IBarrierDeviceManager
	{
		string[] GetAllDeviceNames();

		IBarrierDevice GetDevice(string deviceName, string devicePort);
	}
}
