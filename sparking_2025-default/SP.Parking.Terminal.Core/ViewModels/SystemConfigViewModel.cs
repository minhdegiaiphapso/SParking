using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class SystemConfigViewModel : BaseViewModel
    {
        private string _serverIPAddress;
        public string ServerIPAddress
        {
            get { return _serverIPAddress; }
            set
            {
                if (_serverIPAddress == value)
                    return;

                _serverIPAddress = value;
                RaisePropertyChanged(() => ServerIPAddress);
            }
        }

        public SystemConfigViewModel(IViewModelServiceLocator service)
            : base(service)
        {
        }

        public void AutoScanCommand()
        {

        }

        MvxCommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand;
            }
        }
    }
}
