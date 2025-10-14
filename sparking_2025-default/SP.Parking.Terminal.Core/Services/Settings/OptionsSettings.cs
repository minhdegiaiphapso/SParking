using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public enum CameraType
    {
        Vivotek,
        Hik,
        Bosch,
        Webcam,
		RTSP,
		VivotekTracker,
		HikTracker,
		RTSPTracker,
		RTSPHDTracker,
	}

    public class OptionsSettingsData : BaseSettingsData
    {
		public string MasterPasswordMd5 { get; set; }
        public bool PlateRecognitionEnale { get; set; }
        public bool PlateRecognitionBySfactors { get; set; }
        public bool NoMatchingPlateNoticeEnalbe { get; set; }
        public bool ForceUpdatePlateNumber { get; set; }

        public FarCardUsageRules FarCardUsageRules { get; set; }

		private bool _confirmCheckout = true;
        public bool ConfirmCheckout
        {
            get { return _confirmCheckout; }
            set { _confirmCheckout = value; }
        }
        private bool _barrierForcedWithPopup = false;
        public bool BarrierForcedWithPopup {
            get { return _barrierForcedWithPopup; }
            set { _barrierForcedWithPopup = value; }
        }
        private bool _barrierForcedWithCheckOutException = false;
        public bool BarrierForcedWithCheckOutException {
            get { return _barrierForcedWithCheckOutException; }
            set { _barrierForcedWithCheckOutException = value; }
        }
        private bool _confirmCheckin = true;
        public bool ConfirmCheckin
        {
            get { return _confirmCheckin; }
            set { _confirmCheckin = value; }
        }
        private bool _isVoucher = false;
        public bool IsVoucher
        {
            get { return _isVoucher; }
            set { _isVoucher = value; }
        }
        private bool _isPrintV2 = false;
        public bool IsPrintV2
        {
            get { return _isPrintV2; }
            set { _isPrintV2 = value; }
        }
        private bool _isVitualLaneLeft = false;
        public bool IsVitualLaneLeft
        {
            get { return _isVitualLaneLeft; }
            set { _isVitualLaneLeft = value; }
        }
        private bool _isVitualLaneRight = false;
        public bool IsVitualLaneRight
        {
            get { return _isVitualLaneRight; }
            set { _isVitualLaneRight = value; }
        }
        private bool _isSfactorsCom = true;
        public bool IsSfactorsCom
        {
            get { return _isSfactorsCom; }
            set { _isSfactorsCom = value; }
        }

        private int _waitingCheckDuration = 15;
        public int WaitingCheckDuration
        {
            get { return _waitingCheckDuration; }
            set { _waitingCheckDuration = value; }
        }
        private int _autoRefreshTime = 0;
        public int AutoRefreshTime
        {
            get { return _autoRefreshTime; }
            set { _autoRefreshTime = value; }
        }
        private CameraType _cameraType;
        public CameraType CameraType
        {
            get { return _cameraType; }
            set { _cameraType = value; }

        }
        
        private int _displayCheckOutDuration = 15;
        public int DisplayCheckOutDuration
        {
            get { return _displayCheckOutDuration; }
            set { _displayCheckOutDuration = value; }
        }
        private int _waitingSoyalCardReaderDuration = 3;

        public int WaitingSoyalCardReaderDuration
        {
            get { return _waitingSoyalCardReaderDuration; }
            set { _waitingSoyalCardReaderDuration = value; }
        }
        private int _waitingProlificCardReaderDuration = 10;

        public int WaitingProlificCardReaderDuration
        {
            get { return _waitingProlificCardReaderDuration; }
            set { _waitingProlificCardReaderDuration = value; }
        }

        public bool UseVehicleTypeFromCard { get; set; }

        //private Region _region = new Region();

        //public Region Region
        //{
        //    get { return _region; }
        //    set { _region = value; }
        //}

        public bool CanChangeVehicleType { get; set; }

        public string ThemeColor { get; set; }

        public bool Zoomable { get; set; }

        public bool EntryCheck { get; set; }

        //DisplayedPosition _displayedPosition;
        //public DisplayedPosition DisplayedPosition
        //{
        //    get { return _displayedPosition; }
        //    set { _displayedPosition = value; }
        //}
    }
    //request.AddParameter("is_cancel", data.is_cancel ==null?"NULL": data.is_cancel);
    public interface IOptionsSettings : IBaseSettings
    {
		string MasterPasswordMd5 { get; set; }
        bool PlateRecognitionEnable { get; set; }
        bool PlateRecognitionBySfactors { get; set; }
        bool NoMatchingPlateNoticeEnalbe { get; set; }
        bool ForceUpdatePlateNumber { get; set; }
        bool ConfirmCheckout { get; set; }
        bool ConfirmCheckin { get; set; }
		FarCardUsageRules FarCardUsageRules {  get; set; }  
		bool BarrierForcedWithPopup { get; set; }
        bool BarrierForcedWithCheckOutException { get; set; }
        int WaitingCheckDuration { get; set; }
        CameraType CameraType { get; set; }
        int DisplayCheckOutDuration { get; set; }
        //DisplayedPosition DisplayedPosition { get; set; }
        int WaitingProlificCardReaderDuration { get; set; }
        int WaitingSoyalCardReaderDuration { get; set; }
        int AutoRefreshTime { get; set; }
        bool CanChangeVehicleType { get; set; }
        string ThemeColor { get; set; }
        bool Zoomable { get; set; }
        bool EntryCheck { get; set; }
        bool IsVoucher { get; set; }
        bool IsPrintV2 { get; set; }
        bool IsVitualLaneLeft { get; set; }
        bool IsVitualLaneRight { get; set; }
        bool IsSfactorsCom { get; set; }

        bool UseVehicleTypeFromCard { get; set; }
        //Region Region { get; set; }
    }

    public class OptionsSettings : BaseSettings<OptionsSettingsData>, IOptionsSettings
    {
		public static string DEFAULT_MASTER_PASSWORD = "@Sp142536";

        public FarCardUsageRules FarCardUsageRules {
            get {  return _data.FarCardUsageRules; }

            set
            {
                _data.FarCardUsageRules = value;
            }
        }
		public string MasterPasswordMd5
		{
			get {
				if (string.IsNullOrEmpty(_data.MasterPasswordMd5))
					return EncryptionUtility.GetMd5Hash(DEFAULT_MASTER_PASSWORD);
				return _data.MasterPasswordMd5; 
			}
			set
			{
				_data.MasterPasswordMd5 = value;
				//MarkChanged();
			}
		}
        public bool IsVoucher
        {
            get { return _data.IsVoucher; }
            set
            {
                _data.IsVoucher = value;
                //MarkChanged();
            }
        }
        public bool IsPrintV2
        {
            get { return _data.IsPrintV2; }
            set
            {
                _data.IsPrintV2 = value;
                //MarkChanged();
            }
        }
        public bool IsVitualLaneLeft
        {
            get { return _data.IsVitualLaneLeft; }
            set
            {
                _data.IsVitualLaneLeft = value;
                //MarkChanged();
            }
        }
        public bool IsVitualLaneRight
        {
            get { return _data.IsVitualLaneRight; }
            set
            {
                _data.IsVitualLaneRight = value;
                //MarkChanged();
            }
        }
        public bool PlateRecognitionEnable
        {
            get { return _data.PlateRecognitionEnale; }
            set
            {
                _data.PlateRecognitionEnale = value;
                //MarkChanged();
            }
        }
        public bool PlateRecognitionBySfactors
        {
            get { return _data.PlateRecognitionBySfactors; }
            set
            {
                _data.PlateRecognitionBySfactors = value;
                //MarkChanged();
            }
        }

        public bool NoMatchingPlateNoticeEnalbe
        {
            get { return _data.NoMatchingPlateNoticeEnalbe; }
            set
            {
                _data.NoMatchingPlateNoticeEnalbe = value;
                //MarkChanged();
            }
        }

        public bool ForceUpdatePlateNumber
        {
            get { return _data.ForceUpdatePlateNumber; }
            set
            {
                _data.ForceUpdatePlateNumber = value;
                //MarkChanged();
            }
        }

        public bool ConfirmCheckout
        {
            get { return _data.ConfirmCheckout; }
            set
            {
                _data.ConfirmCheckout = value;
                //MarkChanged();
            }
        }
        public bool BarrierForcedWithPopup {
            get { return _data.BarrierForcedWithPopup; }
            set
            {
                _data.BarrierForcedWithPopup = value;
                //MarkChanged();
            }
        }
        public bool BarrierForcedWithCheckOutException {
            get { return _data.BarrierForcedWithCheckOutException; }
            set
            {
                _data.BarrierForcedWithCheckOutException = value;
                //MarkChanged();
            }
        }
        public bool ConfirmCheckin
        {
            get { return _data.ConfirmCheckin; }
            set
            {
                _data.ConfirmCheckin = value;
                //MarkChanged();
            }
        }
        public int WaitingCheckDuration
        {
            get { return _data.WaitingCheckDuration; }
            set
            {
                _data.WaitingCheckDuration = value;
                //MarkChanged();
            }
        }

        public CameraType CameraType
        {
            get { return _data.CameraType; }
            set
            {
                _data.CameraType = value;
                //MarkChanged();
            }
 		}
        public int AutoRefreshTime
        {
            get { return _data.AutoRefreshTime; }
            set
            {
                _data.AutoRefreshTime = value;
                //MarkChanged();
            }
        }
        public int DisplayCheckOutDuration
        {
            get { return _data.DisplayCheckOutDuration; }
            set
            {
                _data.DisplayCheckOutDuration = value;
               //MarkChanged();
            }
        }

        public int WaitingProlificCardReaderDuration
        {
            get { return _data.WaitingProlificCardReaderDuration; }
            set
            {
                _data.WaitingProlificCardReaderDuration = value;
                //MarkChanged();
            }
        }
        public int WaitingSoyalCardReaderDuration
        {
            get { return _data.WaitingSoyalCardReaderDuration; }
            set
            {
                _data.WaitingSoyalCardReaderDuration = value;
                //MarkChanged();
            }
        }
        public bool CanChangeVehicleType
        {
            get { return _data.CanChangeVehicleType; }
            set
            {
                _data.CanChangeVehicleType = value;
                //MarkChanged();
            }
        }

        public string ThemeColor
        {
            get { return _data.ThemeColor; }
            set
            {
                _data.ThemeColor = value;
                //MarkChanged();
            }
        }

        public bool Zoomable
        {
            get { return _data.Zoomable; }
            set
            {
                _data.Zoomable = value;
                //MarkChanged();
            }
        }

        public bool EntryCheck
        {
            get { return _data.EntryCheck; }
            set
            {
                _data.EntryCheck = value;
                //MarkChanged();
            }
        }

        public bool IsSfactorsCom
        {
            get { return _data.IsSfactorsCom; }
            set
            {
                _data.IsSfactorsCom = value;
                //MarkChanged();
            }
        }

        public bool UseVehicleTypeFromCard
        {
            get { return _data.UseVehicleTypeFromCard; }
            set
            {
                _data.UseVehicleTypeFromCard = value;
                //MarkChanged();
            }
        }

        //public Region Region { get { return _data.Region; } set { _data.Region = value; } }

        //public DisplayedPosition DisplayedPosition
        //{
        //    get { return _data.DisplayedPosition; }
        //    set
        //    {
        //        _data.DisplayedPosition = value;
        //        MarkChanged();
        //    }
        //}

        public OptionsSettings()
            : base()
        {

        }
    }
}
