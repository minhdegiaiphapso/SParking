using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class EndingShiftInformationViewModel : BaseViewModel
    {
        IMvxMessenger _messenger;

        int _numberOfVehicleOut;
        public int NumberOfVehicleOut
        {
            get { return _numberOfVehicleOut; }
            set
            {
                if (_numberOfVehicleOut == value) return;
                _numberOfVehicleOut = value;
                RaisePropertyChanged(() => NumberOfVehicleOut);
            }
        }

        int _numberOfVehicleIn;
        public int NumberOfVehicleIn
        {
            get { return _numberOfVehicleIn; }
            set
            {
                if (_numberOfVehicleIn == value) return;
                _numberOfVehicleIn = value;
                RaisePropertyChanged(() => NumberOfVehicleIn);
            }
        }

        int _usedCards;
        public int UsedCards
        {
            get { return _usedCards; }
            set
            {
                if (_usedCards == value) return;
                _usedCards = value;
                RaisePropertyChanged(() => UsedCards);
            }
        }

        string _strTotalAmount = "0 VND";
        public string StrTotalAmount
        {
            get { return _strTotalAmount; }
            set
            {

                _strTotalAmount = value;
                RaisePropertyChanged(() => StrTotalAmount);
            }
        }

        int _revenue;
        public int Revenue
        {
            get { return _revenue; }
            set
            {
                if (_revenue == value) return;
                _revenue = value;
                RaisePropertyChanged(() => Revenue);
            }
        }

        string _beginTime;
        public string BeginTime
        {
            get { return _beginTime; }
            set
            {
                if (_beginTime == value) return;
                _beginTime = value;
                RaisePropertyChanged(() => BeginTime);
            }
        }

        string _endTime;
        public string EndTime
        {
            get { return _endTime; }
            set
            {
                if (_endTime == value) return;
                _endTime = value;
                RaisePropertyChanged(() => EndTime);
            }
        }

        string _staffID;
        public string StaffID
        {
            get { return _staffID; }
            set
            {
                if (_staffID == value) return;
                _staffID = value;
                RaisePropertyChanged(() => StaffID);
            }
        }

        string _staffName;
        public string StaffName
        {
            get { return _staffName; }
            set
            {
                if (_staffName == value) return;
                _staffName = value;
                RaisePropertyChanged(() => StaffName);
            }
        }

        IUserPreferenceService _userPreferenceService;
        IUserServiceLocator _userServiceLocator;

        public EndingShiftInformationViewModel(IViewModelServiceLocator service, IMvxMessenger messenger)
            : base(service)
        {
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _userServiceLocator = Mvx.Resolve<IUserServiceLocator>();
            _messenger = messenger;
        }

        public void PublishCloseChildEvent(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<CloseChildMessage>())
            {
                _messenger.Publish(new CloseChildMessage(this, position));
            }
        }

        public void PublishShowLoginView(SectionPosition position)
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                _messenger.Publish(new ShowChildMessage(this, position, typeof(LoginViewModel)));
            }
        }

        public void PublishShowCheckingLaneEvent()
        {
            if (_messenger.HasSubscriptionsFor<ShowChildMessage>())
            {
                _messenger.Publish(new ShowChildMessage(this, Section.Id, typeof(BaseLaneViewModel)));
            }
        }

        private void LogoutSuccess()
        {
            PublishCloseChildEvent(Section.Id);
            PublishShowLoginView(Section.Id);
        }

        UserShift _shift;
        List<Section> _sections;

        private void LogoutAll()
        {
            int vehicleIn = 0;
            int vehicleOut = 0;
            int count = _sections.Count;
            float _totalAmount = 0;
            foreach (var item in _sections)
            {
                item.UserService.Logout(item.Lane.Id, (sft, ex) => {
                    if (ex == null)
                    {
                        var now = TimeMapInfo.Current.LocalTime;
                        _totalAmount += item.TotalAmount;
                        StrTotalAmount = string.Format("{0:0,0 vnđ}", _totalAmount);
                        StaffID = sft.User.StaffID;
                        StaffName = sft.User.DisplayName;
                        BeginTime = sft.BeginTime.ToString("dd/MM/yyyy - HH:mm");
                        EndTime = now.ToString("dd/MM/yyyy - HH:mm");
                        //EndTime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
                        Interlocked.Add(ref vehicleIn, sft.NumberOfCheckIn);
                        Interlocked.Add(ref vehicleOut, sft.NumberOfCheckOut);
                        Interlocked.Decrement(ref count);
                        if (count == 0)
                        {
                            NumberOfVehicleIn = vehicleIn;
                            NumberOfVehicleOut = vehicleOut;
                            UsedCards = NumberOfVehicleIn - NumberOfVehicleOut;
                        }
                    }
                });
            }
           
        }


        private void ShowAll()
        {
            int vehicleIn = 0;
            int vehicleOut = 0;
            int count = _sections.Count;
            float _totalAmount = 0;
            foreach (var item in _sections)
            {
                _totalAmount += item.TotalAmount;
                StrTotalAmount = string.Format("{0:0,0 vnđ}", _totalAmount);
                StaffID = item.UserService.CurrentUser.StaffID;
                //StaffName = item.UserService.CurrentUser.DisplayName;
                //BeginTime = sft.BeginTime.ToString("dd/MM/yyyy - HH:mm");
                //EndTime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
                //Interlocked.Add(ref vehicleIn, sft.NumberOfCheckIn);
                //Interlocked.Add(ref vehicleOut, sft.NumberOfCheckOut);
                //Interlocked.Decrement(ref count);
                //if (count == 0)
                //{
                //    NumberOfVehicleIn = vehicleIn;
                //    NumberOfVehicleOut = vehicleOut;
                //    UsedCards = NumberOfVehicleIn - NumberOfVehicleOut;
                //}
            }

        }

        public virtual void Init(ParameterKey key)
        {
            this.Section = (Section)Services.Parameter.Retrieve(key);
            _sections = _userPreferenceService.SystemSettings.GetAllSections(Section.DisplayedPosition);

            LogoutAll();
        }

        private MvxCommand _logoutCommand;
        public ICommand LogoutCommand
        {
            get
            {
                _logoutCommand = _logoutCommand ?? new MvxCommand(() => {
                    if (_shift != null && _shift.Revenue != Revenue)
                    {
                        _shift.Revenue = Revenue;
                        this.Section.UserService.UpdateLogout(_shift, (shift, exception) => {
                            if (exception != null)
                                Mvx.Resolve<ILogService>().Log(exception, _userPreferenceService.HostSettings.LogServerIP);

                            LogoutSuccess();
                        });
                    }
                    else
                    {
                        if (_shift == null)
                            Mvx.Resolve<ILogService>().Log(new Exception("_shift is null"), _userPreferenceService.HostSettings.LogServerIP);

                        LogoutSuccess();
                    }
                });
                return _logoutCommand;
            }
        }

        private MvxCommand _backCommand;
        public ICommand BackCommand
        {
            get
            {
                _backCommand = _backCommand ?? new MvxCommand(() => {
                   // Section.UserService.CurrentUser = _shift.User;
                    PublishCloseChildEvent(this.Section.Id);
                    PublishShowCheckingLaneEvent();
                });
                return _backCommand;
            }
        }
        MvxCommand _goBackOUT;
        public ICommand goBackOUT
        {
            get
            {
                _goBackOUT = _goBackOUT ?? new MvxCommand(() =>
                {
                    //Section.UserService.CurrentUser = _shift.User;
                    PublishCloseChildEvent(this.Section.Id);
                    PublishShowCheckingLaneEvent();
                });
                return _goBackOUT;
            }
        }
    }
}
