using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.Models;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class OptionsConfigurationViewModel : BaseViewModel
    {
        private FarCardUsageRules _farCardUsageRules;
		public FarCardUsageRules FarCardUsageRules
		{
			get { return _farCardUsageRules; }
			set
			{
				if (_farCardUsageRules == value) return;
				_farCardUsageRules = value;
				RaisePropertyChanged(() => FarCardUsageRules);
			}
		}

		bool _plateRecognition = true;
        public bool PlateRecognition
        {
            get { return _plateRecognition; }
            set
            {
                if (_plateRecognition == value) return;
                _plateRecognition = value;
                RaisePropertyChanged(() => PlateRecognition);
            }
        }
        bool _plateRecognitionBySfactors = false;
        public bool PlateRecognitionBySfactors
        {
            get { return _plateRecognitionBySfactors; }
            set
            {
                if (_plateRecognitionBySfactors == value) return;
                _plateRecognitionBySfactors = value;
                RaisePropertyChanged(() => PlateRecognitionBySfactors);
            }
        }

        bool _noMatching = true;
        public bool NoMatching
        {
            get { return _noMatching; }
            set
            {
                if (_noMatching == value) return;
                _noMatching = value;
                RaisePropertyChanged(() => NoMatching);
            }
        }

        bool _forceUpdatePlateNumber = false;
        public bool ForceUpdatePlateNumber
        {
            get { return _forceUpdatePlateNumber; }
            set
            {
                if (_forceUpdatePlateNumber == value) return;
                _forceUpdatePlateNumber = value;
                RaisePropertyChanged(() => ForceUpdatePlateNumber);
            }
        }

        bool _confirmCheckout = true;
        public bool ConfirmCheckout
        {
            get { return _confirmCheckout; }
            set
            {
                if (_confirmCheckout == value) return;
                _confirmCheckout = value;
                _isVoucher = _isVoucher && value;       
            }
        }
        bool _isSfactorsCom = true;
        public bool IsSfactorsCom
        {
            get { return _isSfactorsCom; }
            set
            {
                if (_isSfactorsCom == value) return;
                _isSfactorsCom = value;
                RaisePropertyChanged(() => IsSfactorsCom);
            }
        }
        bool _isVoucher;
        public bool IsVoucher
        {
            get { return _isVoucher; }
            set
            {  
                if (_isVoucher == value)
                    return;
                _isVoucher = value;
                RaisePropertyChanged(() => IsVoucher);
            }
        }
        bool _isPrintV2;
        public bool IsPrintV2
        {
            get { return _isPrintV2; }
            set
            {
                if (_isPrintV2 == value)
                    return;
                _isPrintV2 = value;
                RaisePropertyChanged(() => IsPrintV2);
            }
        }
        bool _isVitualLaneLeft;
        public bool IsVitualLaneLeft
        {
            get { return _isVitualLaneLeft; }
            set
            {
                if (_isVitualLaneLeft == value)
                    return;
                _isVitualLaneLeft = value;
                RaisePropertyChanged(() => IsVitualLaneLeft);
            }
        }
        bool _isVitualLaneRight;
        public bool IsVitualLaneRight
        {
            get { return _isVitualLaneRight; }
            set
            {
                if (_isVitualLaneRight == value)
                    return;
                _isVitualLaneRight = value;
                RaisePropertyChanged(() => IsVitualLaneRight);
            }
        }

        bool _confirmCheckin = true;
        public bool ConfirmCheckin
        {
            get { return _confirmCheckin; }
            set
            {
                if (_confirmCheckin == value) return;
                _confirmCheckin = value;
                RaisePropertyChanged(() => ConfirmCheckin);
            }
        }
        private bool _barrierForcedWithPopup = false;
        public bool BarrierForcedWithPopup
        {
            get { return _barrierForcedWithPopup; }
            set
            {
                if (_barrierForcedWithPopup == value) return;
                _barrierForcedWithPopup = value;
                RaisePropertyChanged(() => BarrierForcedWithPopup);
            }
        }
        private bool _barrierForcedWithCheckOutException = false;
        public bool BarrierForcedWithCheckOutException
        {
            get { return _barrierForcedWithCheckOutException; }
            set
            {
                if (_barrierForcedWithCheckOutException == value) return;
                _barrierForcedWithCheckOutException = value;
                RaisePropertyChanged(() => BarrierForcedWithCheckOutException);
            }
        }

        bool _canChangeVehicleType = true;
        public bool CanChangeVehicleType
        {
            get { return _canChangeVehicleType; }
            set
            {
                if (_canChangeVehicleType == value) return;
                _canChangeVehicleType = value;
                RaisePropertyChanged(() => CanChangeVehicleType);
            }
        }
        IEnumerable<CameraType> _cameraTypes;
        public IEnumerable<CameraType> CameraTypes
        {
            get { return _cameraTypes; }
            set
            {
                _cameraTypes = value;
                RaisePropertyChanged(() => CameraType);
            }
        }

        bool _zoomable = false;
        public bool Zoomable
        {
            get { return _zoomable; }
            set
            {
                if (_zoomable == value) return;
                _zoomable = value;
                RaisePropertyChanged(() => Zoomable);
            }
        }

        bool _entryCheck = false;
        public bool EntryCheck
        {
            get { return _entryCheck; }
            set
            {
                if (_entryCheck == value) return;
                _entryCheck = value;
                RaisePropertyChanged(() => EntryCheck);
            }
        }

        IEnumerable<DisplayedPosition> _displayedPositions;
        public IEnumerable<DisplayedPosition> DisplayedPositions
        {
            get { return _displayedPositions; }
            set
            {
                _displayedPositions = value;
                RaisePropertyChanged(() => DisplayedPositions);
            }
        }

        IEnumerable<string> _themeColors;
        public IEnumerable<string> ThemeColors
        {
            get { return _themeColors; }
            set
            {
                _themeColors = value;
                RaisePropertyChanged(() => ThemeColors);
            }
        }

        string _currentThemeColor;
        public string CurrentThemeColor
        {
            get { return _currentThemeColor; }
            set
            {
                _currentThemeColor = value;
                RaisePropertyChanged(() => CurrentThemeColor);
                _uiService.ChangeColor(CurrentThemeColor);
            }
        }

        CameraType _cameraType = CameraType.Vivotek;
        public CameraType CameraType
        {
            get { return _cameraType; }
            set
            {
                if (_cameraType == value) return;
                _cameraType = value;
                RaisePropertyChanged(() => CameraType);
            }
        }

        DisplayedPosition _displayedPosition;
        public DisplayedPosition DisplayedPosition
        {
            get { return _displayedPosition; }
            set
            {
                if (_displayedPosition == value) return;
                _displayedPosition = value;
                RaisePropertyChanged(() => DisplayedPosition);
            }
        }
        public int _autoRefreshTime;
        public int AutoRefreshTime
        {
            get { return _autoRefreshTime; }
            set
            {
                if (_autoRefreshTime == value) return;
                _autoRefreshTime = value;
                RaisePropertyChanged(() => AutoRefreshTime);
            }
        }
        int _waitingCheckDuration;
        public int WaitingCheckDuration
        {
            get { return _waitingCheckDuration; }
            set
            {
                if (_waitingCheckDuration == value) return;
                _waitingCheckDuration = value;
                RaisePropertyChanged(() => WaitingCheckDuration);
            }
        }

        int _displayCheckOutDuration;
        public int DisplayCheckOutDuration
        {
            get { return _displayCheckOutDuration; }
            set
            {
                if (_displayCheckOutDuration == value) return;
                _displayCheckOutDuration = value;
                RaisePropertyChanged(() => DisplayCheckOutDuration);
            }
        }

        int _waitingProlificCardReaderDuration;
        public int WaitingProlificCardReaderDuration
        {
            get { return _waitingProlificCardReaderDuration; }
            set
            {
                if (_waitingProlificCardReaderDuration == value) return;
                _waitingProlificCardReaderDuration = value;
                RaisePropertyChanged(() => WaitingProlificCardReaderDuration);
            }
        }
        int _waitingSoyalCardReaderDuration;
        public int WaitingSoyalCardReaderDuration
        {
            get { return _waitingSoyalCardReaderDuration; }
            set
            {
                if (_waitingSoyalCardReaderDuration == value) return;
                _waitingSoyalCardReaderDuration = value;
                RaisePropertyChanged(() => WaitingSoyalCardReaderDuration);
            }
        }

        string _resultMessage;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set
            {
                if (_resultMessage == value) return;
                _resultMessage = value;
                RaisePropertyChanged(() => ResultMessage);
            }
        }

		string _resultFarcard;
		public string ResultFarcard
		{
			get { return _resultFarcard; }
			set
			{
				if (_resultFarcard == value) return;
				_resultFarcard = value;
				RaisePropertyChanged(() => ResultFarcard);
			}
		}

		string _otherResultMessage;
        public string OtherResultMessage
        {
            get { return _otherResultMessage; }
            set
            {
                if (_otherResultMessage == value) return;
                _otherResultMessage = value;
                RaisePropertyChanged(() => OtherResultMessage);
            }
        }

        string _currentPassword;
        public string CurrentPassword
        {
            get { return _currentPassword; }
            set
            {
                if (_currentPassword == value) return;
                _currentPassword = value;
                RaisePropertyChanged(() => CurrentPassword);
            }
        }

        string _newPassword;
        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                if (_newPassword == value) return;
                _newPassword = value;
                RaisePropertyChanged(() => NewPassword);
            }
        }

        string _newPasswordAgain;
        public string NewPasswordAgain
        {
            get { return _newPasswordAgain; }
            set
            {
                if (_newPasswordAgain == value) return;
                _newPasswordAgain = value;
                RaisePropertyChanged(() => NewPasswordAgain);
            }
        }

        bool _useVehicleTypeFromCard;
        public bool UseVehicleTypeFromCard
        {
            get { return _useVehicleTypeFromCard; }
            set
            {
                if (_useVehicleTypeFromCard == value) return;
                _useVehicleTypeFromCard = value;
                RaisePropertyChanged(() => UseVehicleTypeFromCard);
            }
        }

        //Region _region;
        //public Region Region
        //{
        //    get { return _region; }
        //    set
        //    {
        //        if (_region == value) return;
        //        _region = value;
        //        RaisePropertyChanged(() => Region);
        //    }
        //}
        IOptionsSettings _optionSettings;
        ISystemSettings _systemSettings;
        IUIService _uiService;
        IServer _server;
        //List<Region> _regions = new List<Region>();
        //public List<Region> Regions {
        //    get
        //    {
        //        return _regions;
        //    }
        //}
        //private void LoadRegion()
        //{
        //    if (_regions == null)
        //        _regions = new List<Region>();
        //    _server.GetRegions((fc, ex) =>
        //    {
        //        if (ex == null && fc != null)
        //        {
        //            _regions.Clear();
        //            _regions.AddRange(fc);
        //            if (Region != null)
        //            {
        //                var find = _regions.FirstOrDefault(r => r.RegionId == Region.RegionId);
        //                if (find != null)
        //                    Region = find;
        //            }
        //            //if (Region == null && _regions.Count > 0)
        //            //    Region = _regions[0];
        //            //else
        //            //{
        //            //    
        //            //    else if (Regions.Count > 0)
        //            //        Region = Regions[0];
        //            //}
        //        }
        //    });
        //}
        public OptionsConfigurationViewModel(IViewModelServiceLocator service)
            : base(service)
        {
            _uiService = Mvx.Resolve<IUIService>();
            _optionSettings = Mvx.Resolve<IOptionsSettings>();
            _systemSettings = Mvx.Resolve<ISystemSettings>();
            _server = Mvx.Resolve<IServer>();
           
            this.PlateRecognition = _optionSettings.PlateRecognitionEnable;
            this.PlateRecognitionBySfactors = _optionSettings.PlateRecognitionBySfactors;
            this._noMatching = _optionSettings.NoMatchingPlateNoticeEnalbe;
            this._forceUpdatePlateNumber = _optionSettings.ForceUpdatePlateNumber;
            this._confirmCheckout = _optionSettings.ConfirmCheckout;
            
            this._waitingCheckDuration = _optionSettings.WaitingCheckDuration;
            this._canChangeVehicleType = _optionSettings.CanChangeVehicleType;
            this._currentThemeColor = _optionSettings.ThemeColor;
            this._confirmCheckin = _optionSettings.ConfirmCheckin;
            this._barrierForcedWithCheckOutException = _optionSettings.BarrierForcedWithCheckOutException;
            this._barrierForcedWithPopup = _optionSettings.BarrierForcedWithPopup;
            this._isVoucher = _optionSettings.IsVoucher;
            this._isPrintV2 = _optionSettings.IsPrintV2;
            this._isVitualLaneLeft = _optionSettings.IsVitualLaneLeft;
            this._isVitualLaneRight = _optionSettings.IsVitualLaneRight;
            this._isSfactorsCom= _optionSettings.IsSfactorsCom;
            this._autoRefreshTime = _optionSettings.AutoRefreshTime;
            this._zoomable = _optionSettings.Zoomable;
            //this._cameraType = _optionSettings.CameraType;
            this._entryCheck = _optionSettings.EntryCheck;
            //this.Region = _optionSettings.Region;
            //LoadRegion();
            CameraTypes = Enum.GetValues(typeof(CameraType)).Cast<CameraType>();

            DisplayedPosition = _systemSettings.Sections[SectionPosition.Lane3].DisplayedPosition;
            DisplayedPositions = Enum.GetValues(typeof(DisplayedPosition)).Cast<DisplayedPosition>();

            var colors = new[] { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt",
                                       "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
            this.ThemeColors = colors;

            this._displayCheckOutDuration = _optionSettings.DisplayCheckOutDuration;
            this._waitingProlificCardReaderDuration = _optionSettings.WaitingProlificCardReaderDuration;
            this._waitingSoyalCardReaderDuration = _optionSettings.WaitingSoyalCardReaderDuration;
            this._useVehicleTypeFromCard = _optionSettings.UseVehicleTypeFromCard;
            if(_optionSettings.FarCardUsageRules != null)
            {
                this.FarCardUsageRules = _optionSettings.FarCardUsageRules;
            }
            else
            {
				this.FarCardUsageRules = new FarCardUsageRules();

			}
                
        }

        public override void Start()
        {
            base.Start();
        }

        protected bool UpdatePassword()
        {
            if (string.IsNullOrEmpty(CurrentPassword) ||
                string.IsNullOrEmpty(NewPassword) ||
                string.IsNullOrEmpty(NewPasswordAgain))
            {
                ResultMessage = "system.password_empty";
                return false;
            }

            var currPassMd5 = EncryptionUtility.GetMd5Hash(CurrentPassword);
            if (currPassMd5 != _optionSettings.MasterPasswordMd5)
            {
                ResultMessage = "system.invalid_master_password";
                return false;
            }

            if (NewPassword != NewPasswordAgain)
            {
                ResultMessage = "system.passwords_not_matched";
                return false;
            }

            _optionSettings.MasterPasswordMd5 = EncryptionUtility.GetMd5Hash(NewPassword);
            _optionSettings.Save();
            ResultMessage = "system.password_changed_success";
            return true;
        }

        protected void UpdateOtherOptions()
        {
            _optionSettings.PlateRecognitionEnable = this.PlateRecognition;
            _optionSettings.PlateRecognitionBySfactors = this.PlateRecognitionBySfactors;
            _optionSettings.NoMatchingPlateNoticeEnalbe = this.NoMatching;
            _optionSettings.ForceUpdatePlateNumber = this.ForceUpdatePlateNumber;
            _optionSettings.ConfirmCheckout = this.ConfirmCheckout;
            _optionSettings.WaitingCheckDuration = this.WaitingCheckDuration;
            //_optionSettings.CameraType = this.CameraType;
            _optionSettings.DisplayCheckOutDuration = this.DisplayCheckOutDuration;
            _optionSettings.WaitingProlificCardReaderDuration = this.WaitingProlificCardReaderDuration;
            _optionSettings.WaitingSoyalCardReaderDuration = this.WaitingSoyalCardReaderDuration;
            //_optionSettings.CanChangeVehicleType = this.CanChangeVehicleType;
            _optionSettings.ThemeColor = this.CurrentThemeColor;
            _optionSettings.ConfirmCheckin = this.ConfirmCheckin;
            _optionSettings.BarrierForcedWithPopup = this.BarrierForcedWithPopup;
            _optionSettings.BarrierForcedWithCheckOutException = this.BarrierForcedWithCheckOutException;
            _optionSettings.IsVoucher = this.IsVoucher;
            _optionSettings.IsPrintV2 = this.IsPrintV2;
            _optionSettings.Zoomable = this.Zoomable;
            _optionSettings.IsVitualLaneLeft= this.IsVitualLaneLeft;
            _optionSettings.IsVitualLaneRight = this.IsVitualLaneRight;
            _optionSettings.IsSfactorsCom = this.IsSfactorsCom;
            _optionSettings.AutoRefreshTime = this.AutoRefreshTime;
            _optionSettings.EntryCheck = this.EntryCheck;
            _optionSettings.UseVehicleTypeFromCard = this.UseVehicleTypeFromCard;
			_optionSettings.FarCardUsageRules = this.FarCardUsageRules;
			//_optionSettings.Region = this.Region;
			//Section section = _systemSettings.Sections[SectionPosition.Lane3];
			//section.DisplayedPosition = this.DisplayedPosition;
			// _systemSettings.ChangeDisplayedPositionLane(section.Id, DisplayedPosition);
			_optionSettings.MarkChanged();
            _optionSettings.Save();
            //_systemSettings.Save();
            OtherResultMessage = "system.other_options_changed_success";
            
        }

		protected void UpdateFarcardOptions()
		{
			_optionSettings.PlateRecognitionEnable = this.PlateRecognition;
			_optionSettings.PlateRecognitionBySfactors = this.PlateRecognitionBySfactors;
			_optionSettings.NoMatchingPlateNoticeEnalbe = this.NoMatching;
			_optionSettings.ForceUpdatePlateNumber = this.ForceUpdatePlateNumber;
			_optionSettings.ConfirmCheckout = this.ConfirmCheckout;
			_optionSettings.WaitingCheckDuration = this.WaitingCheckDuration;
			//_optionSettings.CameraType = this.CameraType;
			_optionSettings.DisplayCheckOutDuration = this.DisplayCheckOutDuration;
			_optionSettings.WaitingProlificCardReaderDuration = this.WaitingProlificCardReaderDuration;
			_optionSettings.WaitingSoyalCardReaderDuration = this.WaitingSoyalCardReaderDuration;
			//_optionSettings.CanChangeVehicleType = this.CanChangeVehicleType;
			_optionSettings.ThemeColor = this.CurrentThemeColor;
			_optionSettings.ConfirmCheckin = this.ConfirmCheckin;
			_optionSettings.BarrierForcedWithPopup = this.BarrierForcedWithPopup;
			_optionSettings.BarrierForcedWithCheckOutException = this.BarrierForcedWithCheckOutException;
			_optionSettings.IsVoucher = this.IsVoucher;
			_optionSettings.IsPrintV2 = this.IsPrintV2;
			_optionSettings.Zoomable = this.Zoomable;
			_optionSettings.IsVitualLaneLeft = this.IsVitualLaneLeft;
			_optionSettings.IsVitualLaneRight = this.IsVitualLaneRight;
			_optionSettings.IsSfactorsCom = this.IsSfactorsCom;
			_optionSettings.AutoRefreshTime = this.AutoRefreshTime;
			_optionSettings.EntryCheck = this.EntryCheck;
			_optionSettings.UseVehicleTypeFromCard = this.UseVehicleTypeFromCard;
			_optionSettings.FarCardUsageRules = this.FarCardUsageRules;
			//_optionSettings.Region = this.Region;
			//Section section = _systemSettings.Sections[SectionPosition.Lane3];
			//section.DisplayedPosition = this.DisplayedPosition;
			// _systemSettings.ChangeDisplayedPositionLane(section.Id, DisplayedPosition);
			_optionSettings.MarkChanged();
			_optionSettings.Save();
			//_systemSettings.Save();
			ResultFarcard = "Update farcard rules successfull!";

		}

		MvxCommand _passwordSaveCommand;
        public ICommand PasswordSaveCommand
        {
            get
            {
                _passwordSaveCommand = _passwordSaveCommand ?? new MvxCommand(() =>
                {

                    if (UpdatePassword())
                    {
                        CurrentPassword = "";
                        NewPassword = "";
                        NewPasswordAgain = "";
                    }
                });

                return _passwordSaveCommand;
            }
        }

        MvxCommand _otherOptSaveCommand;
        public ICommand OtherOptSaveCommand
        {
            get
            {
                _otherOptSaveCommand = _otherOptSaveCommand ?? new MvxCommand(() =>
                {
                    UpdateOtherOptions();
                });

                return _otherOptSaveCommand;
            }
        }

		MvxCommand _farcardOptSaveCommand;
		public ICommand FarCardOptSaveCommand
		{
			get
			{
				_farcardOptSaveCommand = _farcardOptSaveCommand ?? new MvxCommand(() =>
				{
					UpdateFarcardOptions();
				});

				return _farcardOptSaveCommand;
			}
		}
	}
}
