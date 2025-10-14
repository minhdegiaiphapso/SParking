using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Green.Devices.Dal
{
    public delegate void CardReaderEventHandler(object sender, CardReaderEventArgs e);

    public class CardReaderEventArgs : EventArgs
    {
        /// <summary>
        /// The card reader that hold card id
        /// </summary>
        public ICardReader CardReader { get; set; }

        /// <summary>
        /// Get or set card id
        /// </summary>
        public string CardID { get; set; }
        
        public Exception ex { get; set; }
    }

    public interface ICardReaderInfo
    {
        /// <summary>
        /// Get card reader's serial number
        /// </summary>
        string SerialNumber { get; set; }

        /// <summary>
        /// Get card reader's name
        /// </summary>
        string DeviceName { get; set; }
    }

    public interface ICardReader
    {
        /// <summary>
        /// Card information
        /// </summary>
        ICardReaderInfo CardReaderInfo { get; }

        /// <summary>
        /// Callback for receiving card id
        /// </summary>
        event CardReaderEventHandler ReadingCompleted;

        event CardReaderEventHandler TakingOffCompleted;

        /// <summary>
        /// Start card reader
        /// </summary>
        void Run();

        /// <summary>
        /// Stop card reader
        /// </summary>
        void Stop();
    }

    public interface IRFIDCardReaderService
    {
        //event CardReaderEventHandler ReadingCompleted;

        /// <summary>
        /// Run scan device manager
        /// </summary>
        void Run();

        /// <summary>
        /// Stop scan device manager
        /// </summary>
        void Stop();

        /// <summary>
        /// Return all card reader's names that are installed
        /// </summary>
        /// <returns></returns>
        ICardReaderInfo[] GetDeviceInfos();

        /// <summary>
        /// Get card reader by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ICardReader GetCardReader(string id);

        List<ICardReader> GetCardReaders();

        /// <summary>
        /// Return all card readers
        /// </summary>
        Dictionary<string, ICardReader> Devices { get; }
    }
}
