using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Services;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
	public class ConfigViewModel : BaseViewModel
    {
        public LaneConfigurationViewModel LaneConfigurationViewModel { get; private set; }

        public GeneralConfigViewModel GeneralConfigurationViewModel { get; private set; }

        public OptionsConfigurationViewModel OptionsConfigurationViewModel { get; private set; }

        public InputCardViewModel InputCardViewModel { get; private set; }

        public KeyConfigurationViewModel KeyConfigurationViewModel { get; private set; }

        //public GetCheckedInformationViewModel GetCheckedInformationViewModel { get; private set; }

        public FindImagesViewModel FindImagesViewModel { get; private set; }

		public ConfigViewModel(IViewModelServiceLocator serviceLocator)
			: base(serviceLocator)
		{
            LaneConfigurationViewModel = Mvx.IocConstruct<LaneConfigurationViewModel>();
            GeneralConfigurationViewModel = Mvx.IocConstruct<GeneralConfigViewModel>();
            OptionsConfigurationViewModel = Mvx.IocConstruct<OptionsConfigurationViewModel>();
            InputCardViewModel = Mvx.IocConstruct<InputCardViewModel>();
            KeyConfigurationViewModel = Mvx.IocConstruct<KeyConfigurationViewModel>();
            FindImagesViewModel = Mvx.IocConstruct<FindImagesViewModel>();
		}

        MvxCommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                _startCommand = _startCommand ?? new MvxCommand(() => {
                    LaneConfigurationViewModel.SaveConfig();
                });
                return _startCommand;
            }
        }
	}
}
