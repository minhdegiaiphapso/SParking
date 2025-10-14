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
    /// Card status
    /// </summary>
    public enum CardStatus
    {
        Block = 0,
        Free,
        Registered,
    }

    /// <summary>
    /// Card model
    /// </summary>
    public class Card : MvxNotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the card's identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty("card_id")]
        public string Id { get; set; }
        [JsonIgnore]
        public string TimeRide { get; set; }
        /// <summary>
        /// Gets or sets the label (friendly id) on card.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        [JsonIgnore]
        string _label;
        [JsonProperty("card_label")]
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                RaisePropertyChanged(() => Label);
            }
        }

        [JsonIgnore]
        CardStatus _status;
        [JsonProperty("status")]
        public CardStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged(() => Status);
            }
        }

        int _cardTypeId;
        [JsonProperty("card_type")]
        public int CardTypeId
        {
            get { return _cardTypeId; }
            set
            {
                _cardTypeId = value;
                TypeHelper.GetCardType(_cardTypeId, result => CardType = result);
            }
        }

        CardType _cardType;
        public CardType CardType
        {
            get { return _cardType; }
            set
            {
                _cardType = value;
                RaisePropertyChanged(() => CardType);
            }
        }
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
            set
            {
                _vehicleType = value;
                if (_vehicleType != null)
                    _vehicleTypeId = _vehicleType.Id;
            }
        }

        private VehicleTypeEnum _vehicleTypeEnum;
        [JsonIgnore]
        public VehicleTypeEnum VehicleTypeEnum
        {
            get { return _vehicleTypeEnum; }
            set
            {
                _vehicleTypeEnum = value;
                TypeHelper.GetVehicleType((int)_vehicleTypeEnum, result => VehicleType = result);
            }
        }

        public Card() { }

        public Card(string cardId)
        {
            this.Id = cardId;
        }
        

        //public void Set(VehicleTypeEnum vehicleEnum)
        //{
        //    this.VehicleTypeId = (int)vehicleEnum;
        //    TypeHelper.GetVehicleType((int)vehicleEnum, result => {
        //        this.VehicleType = result;
        //    });
        //}
    }
}
