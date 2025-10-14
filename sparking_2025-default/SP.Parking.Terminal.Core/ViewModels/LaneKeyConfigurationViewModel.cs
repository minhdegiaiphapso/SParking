using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class LaneKeyConfigurationViewModel : BaseViewModel
    {
        string _forcedBarierKey;
        public string ForcedBarierKey
        {
            get { return _forcedBarierKey; }
            set
            {
                _forcedBarierKey = value;
                RaisePropertyChanged(() => ForcedBarierKey);
            }
        }
        string _cashDrawerKey;
        public string CashDrawerKey
        {
            get { return _cashDrawerKey; }
            set
            {
                _cashDrawerKey = value;
                RaisePropertyChanged(() => CashDrawerKey);
            }
        }
        private string _laneId;
        public string LaneId
        {
            get { return _laneId; }
            set
            {
                if (_laneId == value) return;
                _laneId = value;
            }
        }

        private ISection _section;
        public override ISection Section
        {
            get { return _section; }
            set
            {
                if (_section == value) return;
                _section = value;
                LaneId = _section.Id.ToString();
            }
        }

        #region Properties
        string _logoutKey;
        public string LogoutKey
        {
            get { return _logoutKey; }
            set
            {
                _logoutKey = value;
                RaisePropertyChanged(() => LogoutKey);
            }
        }

        string _checkOutKey;
        public string CheckOutKey
        {
            get { return _checkOutKey; }
            set
            {
                _checkOutKey = value;
                RaisePropertyChanged(() => CheckOutKey);
            }
        }

        string _changeLaneKey;
        public string ChangeLaneKey
        {
            get { return _changeLaneKey; }
            set
            {
                _changeLaneKey = value;
                RaisePropertyChanged(() => ChangeLaneKey);
            }
        }
        string _changeLaneDirectionKey;
        public string ChangeLaneDirectionKey
        {
            get { return _changeLaneDirectionKey; }
            set
            {
                _changeLaneDirectionKey = value;
                RaisePropertyChanged(() => ChangeLaneDirectionKey);
            }
        }
        string _searchKey;
        public string SearchKey
        {
            get { return _searchKey; }
            set
            {
                _searchKey = value;
                RaisePropertyChanged(() => SearchKey);
            }
        }
        string _cashierKey;
        public string CashierKey
        {
            get { return _cashierKey; }
            set
            {
                _cashierKey = value;
                RaisePropertyChanged(() => CashierKey);
            }
        }

        string _showVehicleTypeKey;
        public string ShowVehicleTypeKey
        {
            get { return _showVehicleTypeKey; }
            set
            {
                _showVehicleTypeKey = value;
                RaisePropertyChanged(() => ShowVehicleTypeKey);
            }
        }

        string _cancelCheckOutKey;
        public string CancelCheckOutKey
        {
            get { return _cancelCheckOutKey; }
            set
            {
                _cancelCheckOutKey = value;
                RaisePropertyChanged(() => CancelCheckOutKey);
            }
        }

        string _backKey;
        public string BackKey
        {
            get { return _backKey; }
            set
            {
                _backKey = value;
                RaisePropertyChanged(() => BackKey);
            }
        }

        string _activateProlificCardReaderKey;
        public string ActivateProlificCardReaderKey
        {
            get { return _activateProlificCardReaderKey; }
            set
            {
                _activateProlificCardReaderKey = value;
                RaisePropertyChanged(() => ActivateProlificCardReaderKey);
            }
        }
        string _activateSoyalCardReaderKey;
        public string ActivateSoyalCardReaderKey
        {
            get { return _activateSoyalCardReaderKey; }
            set
            {
                _activateSoyalCardReaderKey = value;
                RaisePropertyChanged(() => ActivateSoyalCardReaderKey);
            }
        }
        string _printBillKey;
        public string PrintBillKey
        {
            get { return _printBillKey; }
            set
            {
                if (_printBillKey == value) return;
                _printBillKey = value;
                RaisePropertyChanged(() => PrintBillKey);
            }
        }

        string _addNewNumber;
        public string AddNewNumber
        {
            get { return _addNewNumber; }
            set
            {
                if (_addNewNumber == value) return;
                _addNewNumber = value;
                RaisePropertyChanged(() => AddNewNumber);
            }
        }

        string _exceptionalCheckout;
        public string ExceptionalCheckout
        {
            get { return _exceptionalCheckout; }
            set
            {
                _exceptionalCheckout = value;
                RaisePropertyChanged(() => ExceptionalCheckout);
            }
        }

        string _resultMessage;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set
            {
                _resultMessage = value;
                RaisePropertyChanged(() => ResultMessage);
            }
        }


        string _cancelCheckInKey;
        /// <summary>
        /// Hủy checkin
        /// </summary>
        public string CancelCheckInKey
        {
            get { return _cancelCheckInKey; }
            set
            {
                _cancelCheckInKey = value;
                RaisePropertyChanged(() => CancelCheckInKey);
            }
        }

        string _confirmCheckInKey;
        /// <summary>
        /// Xác nhận checkin
        /// </summary>
        public string ConfirmCheckInKey
        {
            get { return _confirmCheckInKey; }
            set
            {
                _confirmCheckInKey = value;
                RaisePropertyChanged(() => ConfirmCheckInKey);
            }
        }

        #endregion

        public LaneKeyConfigurationViewModel(IViewModelServiceLocator services)
            : base(services)
        {
        }

        public void Init(ParameterKey key)
        {
            this.Section = (Section)Services.Parameter.Retrieve(key);
        }

        public override void Start()
        {
            base.Start();
            LoadKey();
        }

        private void LoadKey()
        {
            LogoutKey = Section.KeyMap.GetKey(KeyAction.Logout);
            CheckOutKey = Section.KeyMap.GetKey(KeyAction.CheckOut);
            CancelCheckOutKey = Section.KeyMap.GetKey(KeyAction.CancelCheckOut);
            SearchKey = Section.KeyMap.GetKey(KeyAction.Search);
            CashierKey = Section.KeyMap.GetKey(KeyAction.Cashier);
            ChangeLaneKey = Section.KeyMap.GetKey(KeyAction.ChangeLane);
            ChangeLaneDirectionKey = Section.KeyMap.GetKey(KeyAction.ChangeLaneDirection);
            ShowVehicleTypeKey = Section.KeyMap.GetKey(KeyAction.ShowVehicleType);
            ActivateProlificCardReaderKey = Section.KeyMap.GetKey(KeyAction.ActivateProlificCardReader);
            ActivateSoyalCardReaderKey = Section.KeyMap.GetKey(KeyAction.ActivateSoyalCardReader);
            PrintBillKey = Section.KeyMap.GetKey(KeyAction.PrintBill);
            AddNewNumber = Section.KeyMap.GetKey(KeyAction.AddNewNumber);
            ForcedBarierKey = Section.KeyMap.GetKey(KeyAction.ForcedBarier);
            ConfirmCheckInKey = Section.KeyMap.GetKey(KeyAction.ConfirmCheckInKey);
            CancelCheckInKey = Section.KeyMap.GetKey(KeyAction.CancelCheckInKey);
            CashDrawerKey= Section.KeyMap.GetKey(KeyAction.CashDrawer);
        }

        public void SaveKey()
        {
            Section.KeyMap.AddItem(KeyAction.Logout, _logoutKey);
            Section.KeyMap.AddItem(KeyAction.ExceptionalCheckout, _exceptionalCheckout);
            //Section.KeyMap.AddItem(KeyAction.Back, _cancelCheckOutKey);
            Section.KeyMap.AddItem(KeyAction.CancelCheckOut, _cancelCheckOutKey);
            Section.KeyMap.AddItem(KeyAction.Search, _searchKey);
            Section.KeyMap.AddItem(KeyAction.Cashier, _cashierKey);
            Section.KeyMap.AddItem(KeyAction.ChangeLane, _changeLaneKey);
            Section.KeyMap.AddItem(KeyAction.ChangeLaneDirection, _changeLaneDirectionKey);
            Section.KeyMap.AddItem(KeyAction.CheckOut, _checkOutKey);
            Section.KeyMap.AddItem(KeyAction.ShowVehicleType, _showVehicleTypeKey);
            Section.KeyMap.AddItem(KeyAction.ActivateProlificCardReader, _activateProlificCardReaderKey);
            Section.KeyMap.AddItem(KeyAction.ActivateSoyalCardReader, _activateSoyalCardReaderKey);
            Section.KeyMap.AddItem(KeyAction.PrintBill, _printBillKey);
            Section.KeyMap.AddItem(KeyAction.AddNewNumber, _addNewNumber);
            Section.KeyMap.AddItem(KeyAction.ForcedBarier, _forcedBarierKey);
            Section.KeyMap.AddItem(KeyAction.CashDrawer, _cashDrawerKey);
            Section.KeyMap.AddItem(KeyAction.ConfirmCheckInKey, _confirmCheckInKey);
            Section.KeyMap.AddItem(KeyAction.CancelCheckInKey, _cancelCheckInKey);
            Section.savekeysmap();
            ResultMessage = GetText("keyconfigure.success");
        }

        MvxCommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                _saveCommand = _saveCommand ?? new MvxCommand(() => {
                    SaveKey();
                });

                return _saveCommand;
            }
        }
    }
}
