using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SP.Parking.Terminal.Core.Models
{

    public enum VehicleTypeEnum
    {

        [LocalizableDescription(@"VehicleType.All")]
        All = 100000000,

        [LocalizableDescription(@"VehicleType.Bike")]
        Bike = 1000001,

        [LocalizableDescription(@"VehicleType.Van")]
        Van = 1010101,

        [LocalizableDescription(@"VehicleType.Car")]
        Car = 2000101,

        [LocalizableDescription(@"VehicleType.CarConten")]
        CarConten = 5000401,

        [LocalizableDescription(@"VehicleType.DeliverryMobi")]
        DeliverryMobi = 4000301,
    }

    public class CardType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class VehicleType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ActualId { get { return Id / 10000; } }

        //public void Set(VehicleTypeEnum vehicleEnum)
        //{
        //    TypeHelper.GetVehicleType((int)vehicleEnum, null);
        //}
    }

    public static class TypeHelper
    {
        public static List<CardType> CardTypes { get; set; }
        public static List<VehicleType> VehicleTypes { get; set; }

        public static List<Lane> Lanes { get; set; }
        public static List<Terminal> Terminals { get; set; }
        public static List<TerminalGroup> TerminalGroups { get; set; }

        static IServer _server = Mvx.Resolve<IServer>();

        public static void GetCardTypes(Action<List<CardType>> complete)
        {
            if (CardTypes == null)
            {
                _server.GetCardTypes((result, ex) =>
                {
                    if (ex == null)
                        CardTypes = result;

                    if (complete != null)
                        complete(CardTypes);
                });
            }
            else
            {
                if (complete != null)
                    complete(CardTypes);
            }
        }

        public static void GetVehicleTypes(Action<List<VehicleType>> complete)
        {
            if (VehicleTypes == null)
            {
                _server.GetVehicleTypes((result, ex) =>
                {
                    if (ex == null)
                        VehicleTypes = result;

                    if (complete != null)
                        complete(VehicleTypes);
                });
            }
            else
            {
                if (complete != null)
                    complete(VehicleTypes);
            }
        }

        public static void GetVehicleType(int id, Action<VehicleType> complete)
        {
            GetVehicleTypes(result =>
            {
                VehicleTypes = result;
                if (complete != null)
                    if (VehicleTypes != null)
                    {
                        if (id <= 0)
                            id = (int)VehicleTypeEnum.All;

                        complete(VehicleTypes.Where(t => t.ActualId == id || t.Id == id).FirstOrDefault());
                    }
            });
        }

        public static void GetCardType(int id, Action<CardType> complete)
        {
            GetCardTypes(result =>
            {
                CardTypes = result;
                if (complete != null)
                    if (CardTypes != null)
                        complete(CardTypes.Where(t => t.Id == id).FirstOrDefault());
            });
        }

        public static void GetLanes(Action<List<Lane>> complete)
        {
            if (Lanes == null)
            {
                _server.GetLanes((result, ex) =>
                {
                    if (ex == null)
                        Lanes = result;

                    if (complete != null)
                        complete(Lanes);
                });
            }
            else
            {
                if (complete != null)
                    complete(Lanes);
            }
        }

        public static void GetLane(string name, Action<Lane> complete)
        {
            GetLanes(result =>
            {
                if (complete != null)
                    if (result != null)
                        complete(result.Where(l => l.Name.Equals(name)).FirstOrDefault());
            });
        }

        public static void GetTerminals(Action<List<Terminal>> complete)
        {
            if (Terminals == null)
            {
                _server.GetTerminals((result, ex) =>
                {
                    if (ex == null)
                        Terminals = result.ToList();
                    if (complete != null)
                        complete(Terminals);
                });
            }
            else
            {
                if (complete != null)
                    complete(Terminals);
            }
        }

        public static void GetTerminal(int id, Action<Terminal> complete)
        {
            GetTerminals(result =>
            {
                if (complete != null)
                    if (result != null)
                        complete(result.Where(t => t.Id == id).FirstOrDefault());
            });
        }

        public static void GetTerminalGroups(Action<List<TerminalGroup>> complete)
        {
            if (TerminalGroups == null)
            {
                _server.GetTerminalGroups((result, ex) =>
                {
                    if (ex == null)
                        TerminalGroups = result.ToList();
                    if (complete != null)
                        complete(TerminalGroups);
                });
            }
            else
            {
                if (complete != null)
                    complete(TerminalGroups);
            }
        }

        public static void GetTerminalGroup(int id, Action<TerminalGroup> complete)
        {
            GetTerminalGroups(result =>
            {
                if (complete != null)
                    if (result != null)
                        complete(result.Where(t => t.Id == id).FirstOrDefault());
            });
        }
    }

    //public enum CardType
    //{
    //    [LocalizableDescription(@"CardType.Guest")]
    //    Guest = 0,

    //    [LocalizableDescription(@"CardType.Staff")]
    //    Staff,

    //    [LocalizableDescription(@"CardType.AEON_Staff")]
    //    AEONStaff,
    //}

    ///// <summary>
    ///// Vehicle Type
    ///// </summary>
    //public enum VehicleType
    //{
    //    [LocalizableDescription(@"VehicleType.None")]
    //    None = 0,

    //    [LocalizableDescription(@"VehicleType.Car")]
    //    Car = 1,

    //    [LocalizableDescription(@"VehicleType.Bike")]
    //    Bike = 2,

    //    [LocalizableDescription(@"VehicleType.ElectricBicycle")]
    //    ElectricBicycle = 3
    //}

    /// <summary>
    /// Vehicle sub type.
    /// Note: Type = SubType / 1000
    /// </summary>
    public enum VehicleSubType
    {
        None = 0,
        Car_Sedan = 1001,
        Car_Minivan = 1002,
        Bike_Manual = 2001,
        Bike_Auto = 2002
    }

    public enum VehicleRegistrationStatus
    {
        Break = 0,
        InUse = 1,
        Suspend = 2,
        OutOfDate = 3,
    }

    public class VehicleRegistrationInfo
    {
        [JsonProperty("vehicle_brand")]
        public string VehicleBrand { get; set; }

        [JsonProperty("vehicle_driver_name")]
        public string vehicle_driver_name { get; set; }

        [JsonProperty("total_remain_duration")]
        public int RemainDays { get; set; }

        [JsonProperty("vehicle_paint")]
        public string VehicleColor { get; set; }

        [JsonProperty("vehicle_number")]
        public string VehicleNumber { get; set; }

        [JsonProperty("check_in_alpr_vehicle_number")]
        public string AlprVehicleNumber { get; set; }

        [JsonProperty("status")]
        public VehicleRegistrationStatus Status { get; set; }

        public int MessageLevel
        {
            get
            {
                if (RemainDays < 10) return 3;
                else
                {
                    switch (Status)
                    {
                        case VehicleRegistrationStatus.Break:
                        case VehicleRegistrationStatus.Suspend:
                        case VehicleRegistrationStatus.OutOfDate:
                            return 3;
                        default:
                            return 0;
                    }
                }
            }
        }
        public string StrRemainDay
        {
            get
            {
                switch (Status)
                {
                    case VehicleRegistrationStatus.InUse:
                        return string.Format("{0} day(s)", (RemainDays + 1).ToString());
                    case VehicleRegistrationStatus.Break:
                        return string.Format(" ");
                    case VehicleRegistrationStatus.Suspend:
                        return string.Format("Suspended");
                    case VehicleRegistrationStatus.OutOfDate:
                        return string.Format("Expired");
                    default:
                        return string.Format("{0} day(s)", RemainDays.ToString());
                }
            }
        }
    }

    public class CustomerInfo : MvxNotifyPropertyChanged
    {
        private string _strClaimTime;
        public string StrClaimTime
        {
            get { return _strClaimTime; }
            set
            {
                _strClaimTime = value;
                RaisePropertyChanged(() => StrClaimTime);
            }
        }

        private float _parkingFee;
        [JsonProperty("parking_fee")]
        public float ParkingFee
        {
            get { return _parkingFee; }
            set
            {
                _parkingFee = value;
                StrParkingFee = string.Format("{0:0,0 VND}", _parkingFee);//{0:0,0 VND}
                RaisePropertyChanged(() => ParkingFee);
            }
        }

        private string _strParkingFee;


        public string StrParkingFee
        {
            get { return _strParkingFee; }
            set
            {
                _strParkingFee = value;
                RaisePropertyChanged(() => StrParkingFee);
            }
        }

        [JsonProperty("parking_fee_detail")]
        public string ParkingFeeDetail { get; set; }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty("vehicle_type_from_card")]
        public int VehicleTypeFromCard { get; set; }

        [JsonProperty("vehicle_registration_info")]
        public VehicleRegistrationInfo VehicleRegistrationInfo { get; set; }

        public CustomerInfo()
        {
            VehicleRegistrationInfo = new VehicleRegistrationInfo();
        }
    }

    /// <summary>
    /// Check in model
    /// </summary>
    public class CheckIn : Check
    {

        /// <summary>
        /// Gets or sets the type of the vehicle
        /// </summary>
        [JsonIgnore]
        int _vehicleTypeId;
        [JsonProperty("vehicle_type")]
        public int VehicleTypeId
        {
            get { return _vehicleTypeId; }
            set
            {
                _vehicleTypeId = value;
                TypeHelper.GetVehicleType(_vehicleTypeId, result => VehicleType = result);
                RaisePropertyChanged(() => VehicleTypeId);
            }
        }
        private VehicleType _vehicleType;
        [JsonIgnore]
        public VehicleType VehicleType
        {
            get { return _vehicleType; }
            set { _vehicleType = value; _vehicleTypeId = _vehicleType.Id; }
        }

        /// <summary>
        /// Gets or sets the sub type of the vehicle
        /// </summary>
        //[JsonProperty("vehicle_sub_type")]
        //public VehicleSubType VehicleSubType { get; set; }

        /// <summary>
        /// Gets or sets the check in timestamp. Number of seconds from 01/07/2014
        /// </summary>
        [JsonProperty("check_in_time")]
        public long CheckInTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the image host terminals
        /// </summary>
        [JsonProperty("image_hosts")]
        public Terminal[] ImageHostTerminals { get; set; }

        /// <summary>
        /// Gets or sets the hosts of the check in images of this vehicle
        /// </summary>
        [JsonIgnore]
        public string[] ImageHosts
        {
            get
            {
                if (ImageHostTerminals == null) return null;

                string[] rs = new string[ImageHostTerminals.Length];
                for (int i = 0; i < rs.Length; i++)
                {
                    rs[i] = ImageHostTerminals[i].Ip;
                }
                return rs;
            }
        }

        /// <summary>
        /// Gets or sets the check in time of the vehicle
        /// </summary>
        [JsonIgnore]
        public DateTime CheckInTime { get { return TimestampConverter.Timestamp2DateTime(CheckInTimestamp); } }

        [JsonIgnore]
        public string StrCheckInTime { get { return this.CheckInTime.ToString("dd/MM/yyyy  HH:mm:ss"); } }



        [JsonIgnore]
        public string StrClaimTime
        {
            get
            {
                if (ClaimPromotionCreate == default(DateTime))
                {
                    return string.Empty;
                }

                var timeLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(ClaimPromotionCreate, DateTimeKind.Utc), TimeZoneInfo.Local);
                return timeLocal.ToString("dd/MM/yyyy  HH:mm:ss");
            }
        }

        /// <summary>
        /// Gets or sets the flag indicates if vehicle number exist
        /// </summary>
        [JsonProperty("vehicle_number_exist")]
        public bool VehicleNumberExists { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of parking slots
        /// </summary>
        [JsonProperty("limit_num_slots")]
        public int LimitNumSlots { get; set; }

        /// <summary>
        /// Gets or sets the number of parking slots is occupied now
        /// </summary>
        [JsonProperty("current_num_slots")]
        public int CurrentNumSlots { get; set; }

        [JsonProperty("customer_info")]
        public CustomerInfo CustomerInfo { get; set; }

        [JsonProperty("parking_session_id")]
        public int ParkingSessionId { get; set; }

        [JsonProperty("parking_fee")]
        public float ParkingFee { get; set; }

        [JsonProperty("claim_promotion_id")]
        public string ClaimPromotionId { get; set; }

        [JsonProperty("claim_promotion_create_time")]
        public DateTime ClaimPromotionCreate { get; set; }

        [JsonProperty("claim_promotion_hold_time")]
        public DateTime ClaimPromotionDeadline { get; set; }

        [JsonProperty("bill_amount")]
        public float BillAmount { get; set; }

        [JsonProperty("entryCount")]
        public int EntryCount { get; set; }

        [JsonProperty("entries")]
        public EntryInfo[] Entries { get; set; }

        [JsonIgnore]
        public bool EntryCheck { get; set; }


        private DateTime _checkOutTimeServer;
        [JsonProperty("check_out_time_server")]
        public DateTime CheckOutTimeServer
        {
            get
            {
                return _checkOutTimeServer;
            }
            set
            {
                _checkOutTimeServer = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeZoneInfo.Local);
            }
        }

        private DateTime _checkInTimeServer;
        [JsonProperty("check_in_time_server")]
        public DateTime CheckInTimeServer
        {
            get
            {
                return _checkInTimeServer;
            }
            set
            {
                _checkInTimeServer = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeZoneInfo.Local);
            }
        }

        [JsonIgnore]
        public string StrCheckOutTimeServer { get { return this.CheckOutTimeServer.ToString("dd/MM/yyyy  HH:mm:ss"); } }

        [JsonIgnore]
        public string StrCheckInTimeServer { get { return CheckInTimeServer.ToString("dd/MM/yyyy  HH:mm:ss"); } }

        //public CheckIn Clone()
        //{
        //    if (this == null)
        //        return null;
        //    var strData = JsonConvert.SerializeObject(this);
        //    return JsonConvert.DeserializeObject<CheckIn>(strData);
        //}

        public CheckIn GetClone { get { return this.Clone() as CheckIn; } }
        public object Clone()
        {
            if (this == null)
                return null;
            return new CheckIn()
            {
                AlprVehicleNumber = this.AlprVehicleNumber,
                BackImage = this.BackImage,
                BackImagePath = this.BackImagePath,
                BillAmount = this.BillAmount,
                CardId = this.CardId,
                CardLabel = this.CardLabel,
                CardTypeId = this.CardTypeId,
                CheckInTimeServer = this.CheckInTimeServer,
                CheckInTimestamp = this.CheckInTimestamp,
                CheckOutTimeServer = this.CheckOutTimeServer,
                ClaimPromotionCreate = this.ClaimPromotionCreate,
                ClaimPromotionCreateTime = this.ClaimPromotionCreateTime,
                ClaimPromotionDeadline = this.ClaimPromotionDeadline,
                ClaimPromotionId = this.ClaimPromotionId,
                CurrentNumSlots = this.CurrentNumSlots,
                CustomerInfo = this.CustomerInfo,
                Entries = this.Entries,
                EntryCheck = this.EntryCheck,
                EntryCount = this.EntryCount,
                Extra1Image = this.Extra1Image,
                Extra1ImagePath = this.Extra1ImagePath,
                Extra2Image = this.Extra2Image,
                Extra2ImagePath = this.Extra2ImagePath,
                FrontImage = this.FrontImage,
                FrontImagePath = this.FrontImagePath,
                ImageHostTerminals = this.ImageHostTerminals,
                is_cancel = this.is_cancel,
                LaneId = this.LaneId,
                LimitNumSlots = this.LimitNumSlots,
                name = this.name,
                OperatorId = this.OperatorId,
                ParkingFee = this.ParkingFee,
                ParkingSessionId = this.ParkingSessionId,
                PrefixNumberVehicle = this.PrefixNumberVehicle,
                TerminalId = this.TerminalId,
                VehicleNumber = this.VehicleNumber,
                VehicleNumberExists = this.VehicleNumberExists,
                VehicleType = this.VehicleType,
                VehicleTypeId = this.VehicleTypeId
            };
        }
    }

    public class EntryInfo
    {
        [JsonProperty("check_in_time")]
        public DateTime? CheckInTime { get; set; }

        public string StrCheckInTime
        {
            get { return CheckInTime.HasValue ? CheckInTime.Value.ToLocalTime().ToString("dd/MM/yyyy  HH:mm:ss") : ""; }
        }

        [JsonProperty("check_out_time")]
        public DateTime? CheckOutTime { get; set; }

        public string StrCheckOutTime { get { return CheckOutTime.HasValue ? CheckOutTime.Value.ToLocalTime().ToString("dd/MM/yyyy  HH:mm:ss") : ""; } }
    }
}
