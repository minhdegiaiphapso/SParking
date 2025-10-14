using Cirrious.MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IViewModelServiceLocator
    {
        // Messenger service
        IMvxMessenger Messenger { get; }

        // Parameter service
        IParameterService Parameter { get; }

        IRunModeManager ModeManager { get; }

    }
}
