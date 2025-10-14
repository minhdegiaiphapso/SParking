using Squarebit.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squarebit.Apms.Terminal.Core.Services
{
    public interface IConnectScheduler
    {
        void Run();
    }

    public class CardConnectScheduler : IConnectScheduler
    {
        IRFIDCardReaderService _cardReaderService;

        public CardConnectScheduler()
        {

        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
