using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    /// <summary>
    /// Check out model
    /// </summary>
    public class CheckOut : Check
    {
        public DateTime CheckOutTime { get; set; }
        [JsonIgnore]
        private VehicleType _vehicleType;
        [JsonIgnore]
        public VehicleType VehicleType
        {
            get { return _vehicleType; }
            set
            {
                _vehicleType = value;
                RaisePropertyChanged(() => VehicleType);
            }
        }
        [JsonIgnore]
        public string StrCheckOutTime { get { return this.CheckOutTime.ToString("dd/MM/yyyy  HH:mm:ss"); } }

        [JsonIgnore]
        public string StrReferenceCheckInTime { get { return ReferenceCheckInTime.ToString("dd/MM/yyyy  HH:mm:ss"); } }

        DateTime _referenceCheckInTime;
        public DateTime ReferenceCheckInTime
        {
            get { return _referenceCheckInTime; }
            set
            {
                _referenceCheckInTime = value;
                RaisePropertyChanged(() => ReferenceCheckInTime);
            }
        }

        private byte[] _referenceFrontImage;
        public byte[] ReferenceFrontImage
        {
            get { return _referenceFrontImage; }
            set
            {
                _referenceFrontImage = value;
                RaisePropertyChanged(() => ReferenceFrontImage);
            }
        }

        private byte[] _referenceBackImage;
        public byte[] ReferenceBackImage
        {
            get { return _referenceBackImage; }
            set
            {
                _referenceBackImage = value;
                RaisePropertyChanged(() => ReferenceBackImage);
            }
        }

        string _referenceVehicleNumber;
        public string ReferenceVehicleNumber
        {
            get { return _referenceVehicleNumber; }
            set
            {
                _referenceVehicleNumber = value;
                RaisePropertyChanged(() => ReferenceVehicleNumber);
            }
        }

        //[JsonIgnore]
        //string _ReferencePrefixNumber;
        //[JsonIgnore]
        //public string ReferencePrefixNumber
        //{
        //    get { return ReferencePrefixNumber; }
        //    set
        //    {
        //        ReferencePrefixNumber = value;
        //        RaisePropertyChanged(() => ReferencePrefixNumber);
        //    }
        //}
    }
}
