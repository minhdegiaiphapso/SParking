using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    /// <summary>
    /// Check abstract model
    /// </summary>
    public abstract class Check : MvxNotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the identifier code of card
        /// </summary>
        [JsonProperty("card_id")]
        public string CardId { get; set; }

        /// <summary>
        /// Gets or sets the label printed on the card
        /// </summary>
        [JsonIgnore]
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

        //[JsonProperty("is_cancel")]
        //public string is_cancel { get; set; }

        [JsonIgnore]
        string _is_cancel;
        [JsonProperty("is_cancel")]
        public string is_cancel
        {
            get { return _is_cancel; }
            set
            {
                _is_cancel = value;
                RaisePropertyChanged(() => is_cancel);
            }
        }

        /// <summary>
        /// Gets or sets claim promotion create time
        /// </summary>
        [JsonIgnore]
        string _claim_promotion_create_time;
        [JsonProperty("claim_promotion_create_time")]
        public string ClaimPromotionCreateTime
        {
            get { return _claim_promotion_create_time; }
            set
            {
                _claim_promotion_create_time = value;
                RaisePropertyChanged(() => ClaimPromotionCreateTime);
            }
        }

        /// <summary>
        /// Gets or sets the path of front image
        /// </summary>
        [JsonProperty("front_image_path")]
        public string FrontImagePath { get; set; }

        /// <summary>
        /// Gets or sets the path of back image
        /// </summary>
        [JsonProperty("back_image_path")]
        public string BackImagePath { get; set; }

        /// <summary>
        /// Gets or sets the data in bytes of front image
        /// </summary>
        [JsonIgnore]
        private byte[] _frontImage;
        [JsonIgnore]
        public byte[] FrontImage
        {
            get { return _frontImage; }
            set
            {
                _frontImage = value;
                RaisePropertyChanged(() => FrontImage);
            }
        }

        /// <summary>
        /// Gets or sets the data in bytes of back image
        /// </summary>
        [JsonIgnore]
        private byte[] _backImage;
        [JsonIgnore]
        public byte[] BackImage
        {
            get { return _backImage; }
            set
            {
                _backImage = value;
                RaisePropertyChanged(() => BackImage);
            }
        }

        /// <summary>
        /// Gets or sets the license plate number of vehicle provided by ALPR service
        /// </summary>
        /// 
        private string _AlprVehicleNumber;
        [JsonProperty("alpr_vehicle_number")]
        public string AlprVehicleNumber
        {
            get { return _AlprVehicleNumber; }
            set
            {
                _AlprVehicleNumber = value;
                //_AlprVehicleNumber = "55-P1-36654";
                RaisePropertyChanged(() => AlprVehicleNumber);
            }
        }

        /// <summary>
        /// Gets or sets the verified license plate number of the vehicle
        /// </summary>
        [JsonIgnore]
        private string _vehicleNumber;
        [JsonProperty("vehicle_number")]
        public string VehicleNumber
        {
            get { return _vehicleNumber; }
            set
            {
                _vehicleNumber = value;
                //_vehicleNumber = "36654";
                RaisePropertyChanged(() => VehicleNumber);
            }
        }

        [JsonIgnore]
        string _prefixNumberVehicle;
        [JsonIgnore]
        public string PrefixNumberVehicle
        {
            get { return _prefixNumberVehicle; }
            set
            {
                _prefixNumberVehicle = value;
                //_prefixNumberVehicle = "51-N1";
                RaisePropertyChanged(() => PrefixNumberVehicle);
            }
        }

        /// <summary>
        /// Gets or sets the lane id processes this check
        /// </summary>
        [JsonProperty("lane_id")]
        public int LaneId { get; set; }

        /// <summary>
        /// Gets or sets the terminal id processes this check
        /// </summary>
        [JsonProperty("terminal_id")]
        public int TerminalId { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
        /// <summary>
        /// Gets or sets the operator id processes this check
        /// </summary>
        [JsonProperty("operator_id")]
        public int OperatorId { get; set; }

        [JsonProperty("card_type")]
        public int CardTypeId { get; set; }
        [JsonProperty("extra1_image_path")]
        public string Extra1ImagePath { get; set; }
        [JsonProperty("extra2_image_path")]
        public string Extra2ImagePath { get; set; }
        [JsonIgnore]
        private byte[] _extra1Image;
        [JsonIgnore]
        public byte[] Extra1Image
        {
            get { return _extra1Image; }
            set
            {
                _extra1Image = value;
                RaisePropertyChanged(() => Extra1Image);
            }
        }
        [JsonIgnore]
        private byte[] _extra2Image;
        [JsonIgnore]
        public byte[] Extra2Image
        {
            get { return _extra2Image; }
            set
            {
                _extra2Image = value;
                RaisePropertyChanged(() => Extra2Image);
            }
        }
       
    }
}