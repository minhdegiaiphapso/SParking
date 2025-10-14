using Cirrious.MvvmCross.Plugins.Messenger;

namespace SP.Parking.Terminal.Core.Services
{
    public class ViewModelServiceLocator : IViewModelServiceLocator
    {
        public IMvxMessenger Messenger { get; private set; }

        public IParameterService Parameter { get; private set; }

        public IRunModeManager ModeManager { get; private set; }

        public ViewModelServiceLocator(
            IMvxMessenger messenger,
            IParameterService parameter,
            IRunModeManager modeManager)
        {
            this.Messenger = messenger;
            this.Parameter = parameter;
            this.ModeManager = modeManager;
        }
    }
}
