using Newtonsoft.Json;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class CardReaderWrapper
    {
        private string _serialNumber = string.Empty;
        [JsonIgnore]
        public string SerialNumber
        {
            get
            {
                if (string.IsNullOrEmpty(_serialNumber))
                    return CardReaderInfo != null ? CardReaderInfo.SerialNumber : _serialNumber;
                else
                    return _serialNumber;
            }
            set { _serialNumber = value; }
        }

        [JsonIgnore]
        ICardReader _cardReader;
        [JsonIgnore]
        public ICardReader RawCardReader
        {
            get { return _cardReader; }
            set
            {
                if (_cardReader == value) return;
                _cardReader = value;
                _cardReaderInfo = _cardReader.CardReaderInfo;
                _cardReader.ReadingCompleted += ReceivedCard;
                _cardReader.TakingOffCompleted += TakeoffCard;
            }
        }

        /// <summary>
        /// Card information
        /// </summary>
        private ICardReaderInfo _cardReaderInfo;
        [JsonProperty("CardReaderInfo")]
        public ICardReaderInfo CardReaderInfo
        {
            get { return _cardReaderInfo; }
            set { _cardReaderInfo = value; }
        }

        /// <summary>
        /// Callback for receiving card id
        /// </summary>
        public virtual event CardReaderEventHandler ReadingCompleted;
        public void ReceivedCard(object sender, CardReaderEventArgs e)
        {
            CardReaderEventHandler handler = ReadingCompleted;

            if (handler != null)
                handler(this, e);
        }

        public virtual event CardReaderEventHandler TakingOffCompleted;
        public void TakeoffCard(object sender, CardReaderEventArgs e)
        {
            CardReaderEventHandler handler = TakingOffCompleted;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Start card reader
        /// </summary>
        public void Run()
        {

        }

        /// <summary>
        /// Stop card reader
        /// </summary>
        public void Stop()
        {

        }

        public void Setup(string serialNumber)
        {

        }
    }
}