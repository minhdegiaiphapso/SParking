using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public enum ParkingSessionEnum
    {
        [LocalizableDescription(@"ParkingSession.InParking")]
        InParking = 0,
        [LocalizableDescription(@"ParkingSession.OnlyOut")]
        OnlyOut,
        [LocalizableDescription(@"All")]
        All
    }

    public class SearchResult
    {
        [JsonProperty("count")]
        public int Total { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }

        [JsonProperty("results")]
        public List<ParkingSession> ParkingSessions { get; set; }
    }

    public class ParkingSession : MvxNotifyPropertyChanged
    {
        [JsonIgnore]
        private bool _IsDoSearch;
        [JsonIgnore]
        public bool IsDoSearch
        {
            get { return _IsDoSearch; }
            set
            {
                _IsDoSearch = value;
                RaisePropertyChanged(() => IsDoSearch);
            }
        }
        [JsonIgnore]
        System.Windows.Visibility _Searching;
        [JsonIgnore]
        public System.Windows.Visibility Searching
        {
            get { return _Searching; }
            set
            {
                if (_Searching == value) return;
                _Searching = value;
                RaisePropertyChanged(() => Searching);
            }
        }
        [JsonProperty("voucher")]
        public VoucherSearch VoucherSearch { get; set; }
        public VoucherSearch Voucher
        {
            get
            {
                if (VoucherSearch == null)
                    return new VoucherSearch() { VoucherAmount = 0, VoucherType = "", ActualFee = ParkingFee, ParkingFee = ParkingFee };
                return VoucherSearch;
            }
        }
        private string _checkinExtra1Image;
        [JsonProperty("check_in_extra1_image")]
        public string CheckInExtra1Image
        {
            get { return _checkinExtra1Image; }
            set
            {
                if (_checkinExtra1Image == value) return;
                _checkinExtra1Image = value;
            }
        }
        private string _checkinExtra2Image;
        [JsonProperty("check_in_extra2_image")]
        public string CheckInExtra2Image
        {
            get { return _checkinExtra2Image; }
            set
            {
                if (_checkinExtra2Image == value) return;
                _checkinExtra2Image = value;
            }
        }
        private string _checkoutExtra1Image;
        [JsonProperty("check_out_extra1_image")]
        public string CheckOutExtra1Image
        {
            get { return _checkoutExtra1Image; }
            set
            {
                if (_checkoutExtra1Image == value) return;
                _checkoutExtra1Image = value;
            }
        }
        private string _checkoutExtra2Image;
        [JsonProperty("check_out_extra2_image")]
        public string CheckOutExtra2Image
        {
            get { return _checkoutExtra2Image; }
            set
            {
                if (_checkoutExtra2Image == value) return;
                _checkoutExtra2Image = value;
            }
        }
        [JsonProperty("total")]
        public int Total { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("card_id")]
        public string CardId { get; set; }

        string _cardLabel;
        [JsonProperty("card_label")]
        public string CardLabel
        {
            get { return _cardLabel; }
            set
            {
                _cardLabel = value;
                RaisePropertyChanged(() => CardLabel);
            }
        }

        [JsonProperty("card_type")]
        public int CardTypeId { get; set; }

        private int _vehicleTypeId;
        [JsonProperty("vehicle_type")]
        public int VehicleTypeId
        {
            get { return _vehicleTypeId; }
            set
            {
                _vehicleTypeId = value;
                TypeHelper.GetVehicleType(_vehicleTypeId, type => VehicleType = type);
            }
        }
        public string StrCardType
        {
            get
            {
                if (this.CardTypeId == 0)
                    return "Thẻ vãng lai";
                else if (this.CardTypeId == 1)
                    return "Thẻ tháng";
                else
                    return "Thẻ Foc";
            }
        }

        private VehicleType _vehicleType;
        public VehicleType VehicleType
        {
            get { return _vehicleType; }
            set
            {
                _vehicleType = value;
                _vehicleTypeId = _vehicleType.Id;
                RaisePropertyChanged(() => VehicleType);
           
            }
        }

        [JsonProperty("vehicle_number")]
        public string VehicleNumber { get; set; }

        [JsonIgnore]
        public int? CurrentUserId { get; set; }

        [JsonIgnore]
        private bool _isCurrentUser;
        public bool IsCurrentUser
        {
            get { return _isCurrentUser; }
            set
            {
                _isCurrentUser = value;
                RaisePropertyChanged(() => IsCurrentUser);
            }
        }

        [JsonProperty("check_in_alpr_vehicle_number")]
        public string AlprVehicleNumber { get; set; }

        [JsonProperty("fee")]
        public float ParkingFee { get; set; }
        [JsonIgnore]
        public string StrParkingFee {
            get
            {
                if(ParkingFee>0)
                    return ParkingFee.ToString("#,###");
                return "0";
            }
        }
        public ParkingSessionEnum ParkingSessionType { get; set; }
        
        [JsonIgnore]
        public bool CanEditVehicleNumber
        {
            get
            {
                if (CheckOutTime == -1)
                    return true;
                else
                    return false;
            }
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private long _checkInTime;
        [JsonProperty("check_in_time")]
        public long CheckInTime
        {
            get { return _checkInTime; }
            set
            {
                _checkInTime = value;
            }
        }
        public string StrCheckInTime { get { return TimestampConverter.Timestamp2String(CheckInTime); } }

        private long _checkOutTime;
        [JsonProperty("check_out_time")]
        public long CheckOutTime
        {
            get { return _checkOutTime; }
            set
            {
                _checkOutTime = value;
            }
        }
        public string StrCheckOutTime { get { return TimestampConverter.Timestamp2String(CheckOutTime); } }

        private string _checkInFrontImage;
        [JsonProperty("check_in_front_image")]
        public string CheckInFrontImage
        {
            get { return _checkInFrontImage; }
            set
            {
                if (_checkInFrontImage == value) return;
                _checkInFrontImage = value;
            }
        }

        private string _checkInBackImage;
        [JsonProperty("check_in_back_image")]
        public string CheckInBackImage
        {
            get { return _checkInBackImage; }
            set
            {
                if (_checkInBackImage == value) return;
                _checkInBackImage = value;
            }
        }

        private string _checkOutFrontImage;
        [JsonProperty("check_out_front_image")]
        public string CheckOutFrontImage
        {
            get { return _checkOutFrontImage; }
            set
            {
                if (_checkOutFrontImage == value) return;
                _checkOutFrontImage = value;
            }
        }

        private string _checkOutBackImage;
        [JsonProperty("check_out_back_image")]
        public string CheckOutBackImage
        {
            get { return _checkOutBackImage; }
            set
            {
                if (_checkOutBackImage == value) return;
                _checkOutBackImage = value;
            }
        }

        private string _terminalName;
           [JsonProperty("name")]
        public string TerminalName
        {
            get { return _terminalName; }
            set
            {
                
                if (_terminalName == value) return;
                _terminalName = value;
                RaisePropertyChanged(() => TerminalName);
            }
        }

        private string _checkInLane;
        [JsonProperty("check_in_lane")]
        public string CheckInLane
        {
            get { return _checkInLane; }
            set
            {
                if (_checkInLane == value) return;
                _checkInLane = value;
            }
        }

        private string _checkOutLane;
        [JsonProperty("check_out_lane")]
        public string CheckOutLane
        {
            get { return _checkOutLane; }
            set
            {
                if (_checkOutLane == value) return;
                _checkOutLane = value;
            }
        }

        private TerminalGroup _terminalGroup;
        [JsonIgnore]
        public TerminalGroup TerminalGroup
        {
            get { return _terminalGroup; }
            set
            {
                if (_terminalGroup == value) return;
                _terminalGroup = value;
                RaisePropertyChanged(() => TerminalGroup);
            }
        }

        public ParkingSession()
        {
            var now = TimeMapInfo.Current.LocalTime;
            StartDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            EndDate = now;
            //StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            //EndDate = DateTime.Now;
            TypeHelper.GetVehicleType((int)VehicleTypeEnum.All, result => VehicleType = result);
        }
    }
}