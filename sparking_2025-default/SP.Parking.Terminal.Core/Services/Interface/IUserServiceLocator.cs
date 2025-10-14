using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IUserServiceLocator
    {
        IUserService GetUserService(SectionPosition sectionPosition);
        //IUserService GetUserService(DisplayedPosition displayedPosition);
    }
}
