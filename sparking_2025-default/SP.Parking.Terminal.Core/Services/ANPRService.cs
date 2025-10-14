using Cirrious.CrossCore;
using SilverSea.Sockets;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ND.ANPR.Service.Client;

namespace SP.Parking.Terminal.Core.Services
{
    public enum ConnectStatus
    {
        Init = 0,
        Connecting = 1,
        Connected = 2,
        Fail = 3,
    }

    public class ANPRService : IALPRService
    {
        private const string DATALENGTHFORMAT = "00000000";
        private const string DEFAULT_VEHICLE_NUMBER = "";

        IUserPreferenceService _userpreferenceService;
        private SocketClient _socketClient;

        private ConcurrentDictionary<string, Action<string, Exception>> _dict = new ConcurrentDictionary<string, Action<string, Exception>>();
        public string ServerIP { get; set; }

        public int ServerPort { get; set; }

        public static ConnectStatus Status { get; set; }

        private Task _worker = null;

        // start socket client
        private void StartSocketClient()
        {
            _socketClient = new SocketClient();
            if (_socketClient == null)
                return;

            _socketClient.ServerIP = ServerIP;
            _socketClient.ServerPort = ServerPort;
            _socketClient.DataReceivedInStringEvent += new DataReceivedInStringEventHandler(socketClient_DataReceivedEvent);
            _socketClient.SocketErrorEvent += new SocketErrorEventHandler(socketClient_SocketErrorEvent);
            _socketClient.ServerConnected += _socketClient_ServerConnected;
            Status = ConnectStatus.Connecting;
            _socketClient.Connect();
        }

        // send message to server
        private void SendMessageToServer(string message)
        {
            if (_socketClient == null || !_socketClient.IsRunning)
                return;
            _socketClient.SendMessage(message);
        }

        private void SendMessageToServer(byte[] buffer)
        {
            if (_socketClient == null || !_socketClient.IsRunning)
                return;
            _socketClient.SendMessage(buffer);
        }

        public ANPRService()
        {
            _userpreferenceService = Mvx.Resolve<IUserPreferenceService>();
            ServerIP = "127.0.0.1";
            ServerPort = 50000;
            if (_worker == null)
            {
                _worker = Task.Factory.StartNew(() => {
                    WorkerDoWork();
                });
            }
        }

        private void WorkerDoWork()
        {
            StartSocketClient();
        }

        private void _socketClient_ServerConnected(string serverIP, int serverPort)
        {
            Status = ConnectStatus.Connected;
        }

        private void socketClient_SocketErrorEvent(string errorString)
        {
            Status = ConnectStatus.Fail;
        }

        // stop socket client
        private void StopSocketClient()
        {
            if (_socketClient == null)
                return;
            _socketClient.DataReceivedInStringEvent -= new DataReceivedInStringEventHandler(socketClient_DataReceivedEvent);
            _socketClient.SocketErrorEvent -= new SocketErrorEventHandler(socketClient_SocketErrorEvent);

            _socketClient.Disconnect();
        }

        public void RecognizeLicensePlate(byte[] image, int VehicleType, Action<string, Exception> complete)
        {
            if (_userpreferenceService.OptionsSettings.PlateRecognitionEnable)
            {
                switch (Status)
                {
                    case ConnectStatus.Init:
                        complete(DEFAULT_VEHICLE_NUMBER, new Exception("anpr.init_fail"));
                        return;
                    case ConnectStatus.Connecting:
                        complete(DEFAULT_VEHICLE_NUMBER, new Exception("anpr.connecting"));
                        return;
                    case ConnectStatus.Connected:
                        break;
                    case ConnectStatus.Fail:
                        complete(DEFAULT_VEHICLE_NUMBER, new Exception("anpr.cannot_connect"));
                        return;
                    default:
                        break;
                }
                var key = Guid.NewGuid();
                string dir = System.AppDomain.CurrentDomain.BaseDirectory + "Temp";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string imagePath = string.Format(dir + @"\{0}.jpg", key);
                _dict.TryAdd(key.ToString(), complete);
                File.WriteAllBytes(imagePath, image);
                if (VehicleType == 1000001 || VehicleType == 4000022)
                {
                    string sendMessage = string.Format("%CM30{0}{1}$", imagePath.Length.ToString(DATALENGTHFORMAT), imagePath);
                    SendMessageToServer(sendMessage);
                    string sendMessageap = string.Format("%CM40{0}{1}$", imagePath.Length.ToString(DATALENGTHFORMAT), imagePath);
                    SendMessageToServer(sendMessageap);
                }
                else
                {
                    string sendMessagealp = string.Format("%CM20{0}{1}$", imagePath.Length.ToString(DATALENGTHFORMAT), imagePath);
                    SendMessageToServer(sendMessagealp);
                }

            }
            else
            {
                complete(DEFAULT_VEHICLE_NUMBER, null);
            }
        }

        public void RecognizeLicensePlate(byte[] image, Action<string, Exception> complete)
        {
            if (_userpreferenceService.OptionsSettings.PlateRecognitionEnable)
            {
                switch (Status)
                {
                    case ConnectStatus.Init:
                        complete(DEFAULT_VEHICLE_NUMBER, new Exception("anpr.init_fail"));
                        return;
                    case ConnectStatus.Connecting:
                        complete(DEFAULT_VEHICLE_NUMBER, new Exception("anpr.connecting"));
                        return;
                    case ConnectStatus.Connected:
                        break;
                    case ConnectStatus.Fail:
                        complete(DEFAULT_VEHICLE_NUMBER, new Exception("anpr.cannot_connect"));
                        return;
                    default:
                        break;
                }
                var key = Guid.NewGuid();
                string dir = System.AppDomain.CurrentDomain.BaseDirectory + "Temp";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string imagePath = string.Format(dir + @"\{0}.jpg", key);
                _dict.TryAdd(key.ToString(), complete);
                File.WriteAllBytes(imagePath, image);
                string sendMessage = string.Format("%CM30{0}{1}$", imagePath.Length.ToString(DATALENGTHFORMAT), imagePath);
                SendMessageToServer(sendMessage);
                string sendMessageap = string.Format("%CM40{0}{1}$", imagePath.Length.ToString(DATALENGTHFORMAT), imagePath);
                SendMessageToServer(sendMessageap);
                //string sendMessagealp = string.Format("%CM02{0}{1}$", imagePath.Length.ToString(DATALENGTHFORMAT), imagePath);
                //SendMessageToServer(sendMessagealp);

               
            }
            else
            {
                complete(DEFAULT_VEHICLE_NUMBER, null);
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

        // socket data event
        private void socketClient_DataReceivedEvent(string receivedString)
        {
            Exception exception = null;
            string vehicleNumber = DEFAULT_VEHICLE_NUMBER;
            string key = string.Empty;
            string receivedMessage = receivedString;

            try
            {
                int index = receivedMessage.IndexOf("RP30");
                if (index > -1)
                    receivedMessage = receivedMessage.Remove(receivedMessage.Length - 1)
                                                     .Remove(0, 1)
                                                     .Replace("RP30", "");
                else
                    receivedMessage = receivedMessage.Remove(receivedMessage.Length - 1)
                                                 .Remove(0, 1)
                                                 .Replace("RP20", "");
                int length = int.Parse(receivedMessage.Substring(0, DATALENGTHFORMAT.Length));
                string content = receivedMessage.Substring(DATALENGTHFORMAT.Length, length);
                vehicleNumber = content.Substring(0, content.IndexOf(";"));
                //vehicleNumber = "30A-12894";
                string path = receivedMessage.Substring(receivedMessage.IndexOf(';') + 1);
                string oldPath = path.Substring(0, path.LastIndexOf('.'));
                key = Path.GetFileNameWithoutExtension(oldPath);

                if (string.IsNullOrEmpty(vehicleNumber))
                    vehicleNumber = DEFAULT_VEHICLE_NUMBER;//"51A-7979";//

                File.Delete(path);
                File.Delete(oldPath);
            }
            catch (Exception ex)
            {
                exception = (Exception)Activator.CreateInstance(ex.GetType(), ex.Message + "\n Data: " + receivedString, ex);
                Mvx.Resolve<ILogService>().Log(exception, _userpreferenceService.HostSettings.LogServerIP);
            }
            finally
            {
                if (_dict.ContainsKey(key))
                {
                    var tValue = _dict[key];
                    if (tValue != null)
                        tValue(vehicleNumber, exception);

                    Action<string, Exception> action = null;
                    _dict.TryRemove(key, out action);
                }
            }
        }

        // socket error event
        
    }

	public class ANPRV8 : IALPRService
	{
		Caller caller;
		public ANPRV8()
		{

			caller = new Caller();
		}
		~ANPRV8()
		{

		}
		private const string DEFAULT_VEHICLE_NUMBER = "";
		public void RecognizeLicensePlate(byte[] image, Action<string, Exception> complete)
		{
			if (image != null)
			{
				caller.Anpr(image, (res, ex) =>
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						if (ex == null && res != null)
						{
							if (!string.IsNullOrEmpty(res.CarVehicleNumberPaddingSelected))
								complete(res.CarVehicleNumberPaddingSelected, ex);
							else if (!string.IsNullOrEmpty(res.BikeVehicleNumberPaddingSelected))
								complete(res.BikeVehicleNumberPaddingSelected, ex);
							else
								complete(DEFAULT_VEHICLE_NUMBER, ex);
						}
						else
							complete(DEFAULT_VEHICLE_NUMBER, ex);
					});
					
				});
			}
			else
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					complete(DEFAULT_VEHICLE_NUMBER, new Exception("Hình không hợp lệ"));
				});
				
			}
		}

		public void RecognizeLicensePlate(byte[] image, int VehicleType, Action<string, Exception> complete)
		{
			RecognizeLicensePlate(image, complete);
		}
	}

}