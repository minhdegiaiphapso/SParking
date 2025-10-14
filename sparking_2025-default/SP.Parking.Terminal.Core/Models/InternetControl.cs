using Cirrious.MvvmCross.ViewModels;
using Green.Devices.Dal;
using Green.Devices.Dal.Siemens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.Models
{
    public class InternetControl: MvxNotifyPropertyChanged
    {
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName == value)
                    return;
                _userName = value;
                RaisePropertyChanged(() => UserName);
            }
        }
        private string _password;
        public string Password {
            get { return _password; }
            set
            {
                if (_password == value)
                    return;
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }
        private string _ip;
        public string IP {
            get { return _ip; }
            set
            {
                if (_ip == value)
                    return;
                _ip = value;
                RaisePropertyChanged(() => IP);
            }
        }
        private string _port;
        public string Port {
            get { return _port; }
            set
            {
                if (_port == value)
                    return;
                _port = value;
                RaisePropertyChanged(() => Port);
            }
        }
        private string _portNumber;
        public string PortNumber {
            get { return _portNumber; }
            set
            {
                if (_portNumber == value)
                    return;
                _portNumber = value;
                RaisePropertyChanged(() => PortNumber);
            }
        }
        private string _buttonNumber;
        public string ButtonNumber
        {
            get { return _buttonNumber; }
            set
            {
                if (_buttonNumber == value)
                    return;
                _buttonNumber = value;
                RaisePropertyChanged(() => ButtonNumber);
            }
        }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;
                _active = value;
                RaisePropertyChanged(() => Active);
            }
        }
        private MvxCommand _detectControl;
        public ICommand DetectControl
        {
            get
            {
                _detectControl = _detectControl ?? new MvxCommand(() =>
                {
                    if (Active)
                    {
                        InternetControllerDevice mydevice = InternetControllerDevice.GetInstance();
                        mydevice.AddCommandInfo(new ControllerDeviceInfo
                        {
                            IP = this.IP,
                            Port = this.Port,
                            UserName = this.UserName,
                            Password = this.Password,
                            PortNumber = this.PortNumber
                        });
                    }
                });
                return _detectControl;
            }
        }
    }
    public class SiemensControl : MvxNotifyPropertyChanged
    {  
        private string _ip;
        public string IP
        {
            get { return _ip; }
            set
            {  
                _ip = value;
                RaisePropertyChanged(() => IP);
            }
        }
        private LogoTypeOut4 typeOut;
        public LogoTypeOut4 TypeOut
        {
            get { return typeOut; }
            set
            {  
                typeOut = value;
                RaisePropertyChanged(() => TypeOut);
            }
        }
        private LogoTypeIn4 typeIn;
        public LogoTypeIn4 TypeIn
        {
            get { return typeIn; }
            set
            {   
                typeIn = value;
                RaisePropertyChanged(() => TypeIn);
            }
        }
        public List<LogoTypeIn4> TypeIns
        {
            get { return Enum.GetValues(typeof(LogoTypeIn4)).Cast<LogoTypeIn4>().ToList<LogoTypeIn4>(); }
        }
        public List<LogoTypeOut4> TypeOuts
        {
            get { return Enum.GetValues(typeof(LogoTypeOut4)).Cast<LogoTypeOut4>().ToList<LogoTypeOut4>(); }
        }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;
                _active = value;
                RaisePropertyChanged(() => Active);
            }
        }
        private MvxCommand _detectControl;
        public ICommand DetectControl
        {
            get
            {
                _detectControl = _detectControl ?? new MvxCommand(() =>
                {
                    if (Active)
                    {
                        Port4 mydevice = Port4.GetInstance();
                        mydevice.AddCommandOut(new SiemenInfo
                        {
                            TcpIp = this.IP,
                            TypeOut = this.TypeOut  
                        });
                    }
                });
                return _detectControl;
            }
        }
    }
}
