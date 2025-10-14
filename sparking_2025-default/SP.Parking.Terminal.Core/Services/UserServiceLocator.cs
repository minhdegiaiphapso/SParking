using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public class UserServiceLocator : IUserServiceLocator
    {
        private Dictionary<SectionPosition, IUserService> _serviceMap = new Dictionary<SectionPosition, IUserService>();
        private Dictionary<DisplayedPosition, IUserService> _serviceMap1 = new Dictionary<DisplayedPosition, IUserService>();
        private IServer _server;

        public UserServiceLocator(IServer server)
        {
            _server = server;
        }

        public IUserService GetUserService(SectionPosition sectionPosition)
        {
            if (!_serviceMap.ContainsKey(sectionPosition))
            {
                _serviceMap[sectionPosition] = new UserService(_server);
            }
            return _serviceMap[sectionPosition];
        }

        public IUserService GetUserService(DisplayedPosition displayedPosition)
        {
            if (!_serviceMap1.ContainsKey(displayedPosition))
            {
                _serviceMap1[displayedPosition] = new UserService(_server);
            }

            return _serviceMap1[displayedPosition];
        }
    }
}
