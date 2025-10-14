using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class EntryCheckViewModel : MvxViewModel
    {
        #region Properties

        private CheckIn _checkInData;

        public CheckIn CheckInData
        {
            get { return _checkInData; }
            set
            {
                _checkInData = value;
                RaisePropertyChanged(() => CheckInData);
            }
        }

        #endregion
    }
}