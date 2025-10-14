using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SP.Parking.Terminal.Core.Models
{
    public class ForcedInfo : MvxNotifyPropertyChanged
    {
        private string _ForcedId;
        [JsonProperty("forced_id")]
        public string ForcedId
        {
            get { return _ForcedId; }
            set
            {
                _ForcedId = value;
                RaisePropertyChanged(() => ForcedId);
            }
        }
        [JsonProperty("forced_time")]
        public long ForcedTimeStamp { get; set; }
        [JsonIgnore]
        public DateTime ForcedTime { get { return TimestampConverter.Timestamp2DateTime(ForcedTimeStamp); } }

        [JsonIgnore]
        public string StrForcedTime { get { return this.ForcedTime.ToString("dd/MM/yyyy  HH:mm:ss"); } }
        [JsonProperty("front_image_path")]
        public string FrontImagePath { get; set; }

        /// <summary>
        /// Gets or sets the path of back image
        /// </summary>
        [JsonProperty("back_image_path")]
        public string BackImagePath { get; set; }
        private byte[] _referenceFrontImage;
        [JsonIgnore]
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
        [JsonIgnore]
        public byte[] ReferenceBackImage
        {
            get { return _referenceBackImage; }
            set
            {
                _referenceBackImage = value;
                RaisePropertyChanged(() => ReferenceBackImage);
            }
        }
        private string _PCAddress;
        [JsonProperty("pc_address")]
        public string PCAddress
        {
            get { return _PCAddress; }
            set
            {
                _PCAddress = value;
                RaisePropertyChanged(() => PCAddress);
            }
        }
        private string _Lane;
        [JsonProperty("lane")]
        public string Lane
        {
            get { return _Lane; }
            set
            {
                _Lane = value;
                RaisePropertyChanged(() => Lane);
            }
        }
        private string _User;
        [JsonProperty("user")]
        public string User
        {
            get { return _User; }
            set
            {
                _User = value;
                RaisePropertyChanged(() => User);
            }
        }
        private string _note;
        [JsonProperty("note")]
        public string Note
        {
            get { return _note; }
            set
            {
                _note = value;
                RaisePropertyChanged(() => Note);
            }
        }
    }
}
