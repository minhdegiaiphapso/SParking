using SP.Parking.Terminal.Core.Models;
using Green.Devices.CardReader;
using Green.Devices.Dal;
using Green.Devices.Vivotek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public interface ICardReaderService
    {
        /// <summary>
        /// Get card reader by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CardReaderWrapper GetCardReader(string id);

        CardReaderWrapper GetCardReader(string ip, string port);

        List<CardReaderWrapper> GetCardReaders();
        List<CardReaderWrapper> GetAllRFIDCardReaders();

        CardReaderWrapper AddRemoteCardReader(string ip, string port);
    }

    public class CardReaderService : ICardReaderService
    {
        IRFIDCardReaderService _cardReaderService;
        IProlificCardReaderFactory _prolificFactory;

        Dictionary<string, CardReaderWrapper> _cardReaders;

        public CardReaderService(IRFIDCardReaderService rfidService, IProlificCardReaderFactory prolificFactory)
        {
            _cardReaderService = rfidService;
            _cardReaderService.Run();

            InitCardReaders();

            _prolificFactory = prolificFactory;
        }

        private void InitCardReaders()
        {
            _cardReaders = new Dictionary<string, CardReaderWrapper>();
            Dictionary<string, ICardReader> dic = _cardReaderService.Devices;
            foreach(var item in dic)
            {
                _cardReaders.Add(item.Key, new CardReaderWrapper { RawCardReader = item.Value});
            }
        }

        public CardReaderWrapper GetCardReader(string serialNumber)
        {
			if (string.IsNullOrEmpty(serialNumber))
				return null;
            if (_cardReaders.ContainsKey(serialNumber))
                return _cardReaders[serialNumber];
            else
                return null;
            //ICardReader cardReader = _cardReaderService.GetCardReader(serialNumber);
            //return new CardReader() { RawCardReader = cardReader, SerialNumber = serialNumber };
        }

        public CardReaderWrapper GetCardReader(string ip, string port)
        {
            if (string.IsNullOrEmpty(ip))
                return null;
            else
            {
                var result = _cardReaders.Where(c => {
                    var info = c.Value.RawCardReader.CardReaderInfo;
                    if (info is ProlificCardReaderInfo)
                        return (info as ProlificCardReaderInfo).IP.Equals(ip);
                    return false;
                });
                if (result.Count() > 0)
                    return result.First().Value;
                else
                {
                    return AddRemoteCardReader(ip, port);
                }
            }
        }

        public List<CardReaderWrapper> GetCardReaders()
        {
            return _cardReaders.Values.ToList();
        }

        public List<CardReaderWrapper> GetAllRFIDCardReaders()
        {
            return _cardReaders.Values.Where(c => c.RawCardReader is IdentiveCardReader).ToList();
        }

        public CardReaderWrapper AddRemoteCardReader(string ip, string port)
        {
            var cardReader = new CardReaderWrapper();
            cardReader.RawCardReader = _prolificFactory.CreateNew(ip, port);
            if (cardReader.RawCardReader == null)
                return null;

            //cardReader.SerialNumber = cardReader.RawCardReader.CardReaderInfo.SerialNumber;
            if (!_cardReaders.ContainsKey(cardReader.SerialNumber))
                _cardReaders.Add(cardReader.SerialNumber, cardReader);
            return cardReader;
        }
    }
}
