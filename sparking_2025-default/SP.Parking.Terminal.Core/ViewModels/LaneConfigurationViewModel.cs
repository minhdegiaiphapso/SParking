using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
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
    public class LaneConfigurationViewModel : BaseViewModel
    {
        public List<SubLaneConfigurationViewModel> LaneConfigViewModels { get; private set; }

        IUserPreferenceService _userPreferenceService;

        IServer _server;
        
        private List<Section> _sections;
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

        private int _cameraPort;
        public int CameraPort
        {
            get { return _cameraPort; }
            set
            {
                if (_cameraPort == value) return;
                _cameraPort = value;
                RaisePropertyChanged(() => CameraPort);
            }
        }

        private string _saveResultMessage;
        public string SaveResultMessage
        {
            get { return _saveResultMessage; }
            set { _saveResultMessage = value; RaisePropertyChanged(() => SaveResultMessage); }
        }

        public LaneConfigurationViewModel(IViewModelServiceLocator service)
            : base(service)
        {
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _server = Mvx.Resolve<IServer>();

            LaneConfigViewModels = new List<SubLaneConfigurationViewModel>();
        }

        public void Init(ParameterKey key)
        {

        }

        public override void Start()
        {
            base.Start();

            var secs = _userPreferenceService.SystemSettings.GetAllSections();

            foreach (var item in secs)
            {
                if (!item.IsConfigured) continue;

                Section section = item;
                SubLaneConfigurationViewModel vm = Mvx.IocConstruct<SubLaneConfigurationViewModel>();
                vm.Section = section;
                LaneConfigViewModels.Add(vm);
            }

            Sections = secs;
        }

        public void SaveConfig()
        {
            ShowViewModelExt<BootstrapViewModel>();
        }

        MvxCommand _addLaneCommand;
        public ICommand AddLaneCommand
        {
            get
            {
                _addLaneCommand = _addLaneCommand ?? new MvxCommand(() => ShowViewModel<SubLaneConfigurationViewModel>(Services.Parameter.Store(null)));
                return _addLaneCommand;
            }
        }

        MvxCommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                _saveCommand = _saveCommand ?? new MvxCommand(() => {
                    _userPreferenceService.SystemSettings.CameraPort = CameraPort;
                    SaveResultMessage = GeneralConfigViewModel.msgSaved;
                });

                return _saveCommand;
            }
        }

    }
}