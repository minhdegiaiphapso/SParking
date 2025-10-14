using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Green.Devices.Vivotek
{
	public class ComPortBarrierDevice : IBarrierDevice
	{
		SerialPort _comPort = new SerialPort();
       
		public string DeviceName { get; private set; }
        public string DevicePort { get; set; }
		public string PortName { get; private set; }
        

		public ComPortBarrierDevice(string deviceName, string devicePort, string portName)
		{
			DeviceName = deviceName;
			PortName = portName;
            DevicePort = devicePort;

			_comPort.PortName = PortName;
			_comPort.BaudRate = 9600;
			_comPort.DataBits = 8;
			_comPort.StopBits = StopBits.One;
			_comPort.Handshake = Handshake.None;
			_comPort.Parity = Parity.None;
		}

		public bool Open()
		{
            try
            {
				_comPort.Open();
                _comPort.Write(string.Format("${0}#", DevicePort));
				_comPort.Close();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

        public bool Close()
        {
            try
            {
                _comPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
	}

	public class BarrierDeviceManager : IBarrierDeviceManager
	{
		Dictionary<string, ComPortBarrierDevice> _devices = new Dictionary<string, ComPortBarrierDevice>();

		Regex _friendlyNameToComPort;

		public BarrierDeviceManager()
		{
			_friendlyNameToComPort = new System.Text.RegularExpressions.Regex(@".? \((COM\d+)\)$");  // "..... (COMxxx)" -> COMxxxx
		}

		public string[] GetAllDeviceNames()
		{
			return COMPortInfo.GetCOMPortsInfo().Select(i => i.Description).ToArray();
		}

		public IBarrierDevice GetDevice(string deviceName, string devicePort)
		{
			if (string.IsNullOrEmpty(deviceName))
				return null;

			string portName = _friendlyNameToComPort.Match(deviceName).Groups[1].Value;

			var devices = SerialPort.GetPortNames();
			if (devices == null || !devices.Contains(portName))
				return null;

			ComPortBarrierDevice device;
			if( !_devices.TryGetValue(deviceName, out device) || device == null )
			{
                device = new ComPortBarrierDevice(deviceName, devicePort, portName);
				_devices[deviceName] = device;
                _devices[deviceName].DevicePort = devicePort;
			}
            else
            {
                _devices[deviceName].DevicePort = devicePort;
            }

			return device;
		}
	}

	//http://dariosantarelli.wordpress.com/2010/10/18/c-how-to-programmatically-find-a-com-port-by-friendly-name/
	//http://stackoverflow.com/questions/2937585/how-to-open-a-serial-port-by-friendly-name
	#region [COM helper]

	internal class ProcessConnection
	{

		public static ConnectionOptions ProcessConnectionOptions()
		{
			ConnectionOptions options = new ConnectionOptions();
			options.Impersonation = ImpersonationLevel.Impersonate;
			options.Authentication = AuthenticationLevel.Default;
			options.EnablePrivileges = true;
			return options;
		}

		public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)
		{
			ManagementScope connectScope = new ManagementScope();
			connectScope.Path = new ManagementPath(@"\\" + machineName + path);
			connectScope.Options = options;
			connectScope.Connect();
			return connectScope;
		}
	}

	public class COMPortInfo
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public COMPortInfo() { }

		public static List<COMPortInfo> GetCOMPortsInfo()
		{
			List<COMPortInfo> comPortInfoList = new List<COMPortInfo>();

			ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
			ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");

			ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
			ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

			using (comPortSearcher)
			{
				string caption = null;
				foreach (ManagementObject obj in comPortSearcher.Get())
				{
					if (obj != null)
					{
						object captionObj = obj["Caption"];
						if (captionObj != null)
						{
							caption = captionObj.ToString();
							if (caption.Contains("(COM"))
							{
								COMPortInfo comPortInfo = new COMPortInfo();
								comPortInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")",
																	 string.Empty);
								comPortInfo.Description = caption;
								comPortInfoList.Add(comPortInfo);
							}
						}
					}
				}
			}
			return comPortInfoList;
		}
	}
	
	#endregion
}
