using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class LaneContainerViewModel : BaseViewModel
    {
        IRunModeManager _modeManager;

        IUserPreferenceService _userPreferenceService;
        IUserServiceLocator _userServiceLocator;

        //MvxSubscriptionToken _changeLaneToken;
        MvxSubscriptionToken _showChildToken;
        MvxSubscriptionToken _closeChildToken;
        MvxSubscriptionToken _loginSuccessToken;
        MvxSubscriptionToken _logoutSuccessToken;

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

        public LaneContainerViewModel(IViewModelServiceLocator service)
            : base(service)
        {
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _modeManager = service.ModeManager;
            _userServiceLocator = Mvx.Resolve<IUserServiceLocator>();
            _loginSuccessToken = service.Messenger.Subscribe<LoginSuccessMessage>(OnLoginSuccess);
            //_logoutSuccessToken = service.Messenger.Subscribe<LogoutSuccessMessage>(OnLogoutSuccess);
            //_changeLaneToken = service.Messenger.Subscribe<ChangeLaneMessage>(OnLaneDirectionChanged);
            _showChildToken = service.Messenger.Subscribe<ShowChildMessage>(OnShowChildView);
            _closeChildToken = service.Messenger.Subscribe<CloseChildMessage>(OnCloseChild);
        }

        public override void Start()
        {
            base.Start();
            Setup();
        }

        public void OnCloseChild(CloseChildMessage msg)
        {
            BaseViewModel requestedViewModel = msg.Sender as BaseViewModel;
            Section section = _userPreferenceService.SystemSettings.Sections[msg.SectionId];
            // Close view
            if (this.PresentationObject != null)
            {
                var hint = new CloseChildPresentationHint()
                {
                    PresentationObjectKey = Services.Parameter.Store(this.PresentationObject).Key,
                    ChildObjectKey = Services.Parameter.Store(requestedViewModel).Key
                };

                ChangePresentation(hint);
            }
        }

        public void OnShowChildView(ShowChildMessage msg)
        {
            BaseViewModel requestedViewModel = msg.Sender as BaseViewModel;
            Section section = _userPreferenceService.SystemSettings.Sections[msg.SectionId];

            ShowChildView(section, msg.ChildTypeViewModel, msg.Params);
        }

        //private void OnLogoutSuccess(LogoutSuccessMessage msg)
        //{
        //    BaseViewModel vm = msg.Sender as BaseViewModel;
        //    if (this.PresentationObject != null)
        //    {
        //        var hint = new CloseChildPresentationHint()
        //        {
        //            PresentationObjectKey = Services.Parameter.Store(this.PresentationObject).Key,
        //            ChildObjectKey = Services.Parameter.Store(vm).Key
        //        };

        //        ChangePresentation(hint);
        //        vm.PresentationObject = null;
        //    }
        //    ShowChildView(vm.Section, typeof(LoginViewModel));
        //}

        private void OnLoginSuccess(LoginSuccessMessage msg)
        {
            LoginViewModel vm = msg.Sender as LoginViewModel;
            if (this.PresentationObject != null)
            {
                var hint = new CloseChildPresentationHint()
                {
                    PresentationObjectKey = Services.Parameter.Store(this.PresentationObject).Key,
                    ChildObjectKey = Services.Parameter.Store(vm).Key
                };

                ChangePresentation(hint);
                vm.PresentationObject = null;
            }
        }

        public void Setup()
        {
            // Get sections from config service
            Sections = _userPreferenceService.SystemSettings.GetAllSections();
            
            foreach (var item in Sections)
            {
				//item.ShouldBeDisplayed

				if (!item.IsConfigured || !item.ShouldBeDisplayed) continue;
                ShowLaneView(item);
            }
        }

        private void ShowLaneView(ISection section)
        {
            IUserService laneUserService = _userServiceLocator.GetUserService(section.Id);
            if (laneUserService.IsLogin)
            {
                ShowChildView(section, typeof(BaseLaneViewModel));
            }
            else
            {
                ShowChildView(section, typeof(LoginViewModel));
                //ShowViewModelExt<LoginViewModel>(Services.Parameter.Store(section), null, vm => {
                //    ShowLaneView(section);
                //});
            }
        }

        private void ShowChildView(ISection section, Type childType, object param = null)
        {
            if (childType == typeof(SearchViewModel))
                ShowViewModelExt<SearchViewModel>(Services.Parameter.Store(section), null, null);
            else if (childType == typeof(BaseLaneViewModel))
            {
                LaneDirection direction = section.TemporaryDirection;
                if (direction == LaneDirection.In)
                    ShowViewModelExt<CheckInLaneViewModel>(Services.Parameter.Store(section), null, null);
                else if (direction == LaneDirection.Out)
                    ShowViewModelExt<CheckOutLaneViewModel>(Services.Parameter.Store(section), null, null);
            }
            else if (childType == typeof(LoginViewModel))
                ShowViewModelExt<LoginViewModel>(Services.Parameter.Store(section), null, vm => {
                    ShowLaneView(section);
                });
            else if (childType == typeof(EndingShiftInformationViewModel))
                ShowViewModelExt<EndingShiftInformationViewModel>(Services.Parameter.Store(section), null, null);
            else if (childType == typeof(ExceptionalCheckOutViewModel))
                ShowViewModelExt<ExceptionalCheckOutViewModel>(Services.Parameter.Store(new object[] { section, param }));
        }

        public override void Close()
        {
            base.Close();
            Unsubscribe();
        }

        public override void Unloaded()
        {
            base.Unloaded();
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            IMvxMessenger messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<LoginSuccessMessage>(_loginSuccessToken);
            //messenger.Unsubscribe<LogoutSuccessMessage>(_logoutSuccessToken);
            messenger.Unsubscribe<ShowChildMessage>(_showChildToken);
            messenger.Unsubscribe<CloseChildMessage>(_closeChildToken);
        }
    }
}