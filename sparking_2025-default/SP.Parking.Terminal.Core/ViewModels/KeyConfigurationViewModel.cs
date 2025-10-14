using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class KeyConfigurationViewModel : BaseViewModel
    {
        IUserPreferenceService _userPreferenceService;

        List<Section> _sections;
        public List<Section> Sections
        {
            get { return _sections; }
            set
            {
                if (_sections == value) return;

                _sections = value;
                RaisePropertyChanged(() => Sections);
            }
        }

        public KeyConfigurationViewModel(IViewModelServiceLocator service)
            : base(service)
        {
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
        }

        public override void Start()
        {
            base.Start();
            Setup();
        }

        public void Setup()
        {
            Sections = _userPreferenceService.SystemSettings.GetAllSections();
            foreach (var item in Sections)
            {
                if (!item.IsConfigured) continue;
                ShowViewModelExt<LaneKeyConfigurationViewModel>(Services.Parameter.Store(item), null, null);
            }
        }
    }
}