using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System.Windows.Input;
using System.ServiceProcess;
using SP.Parking.Terminal.Core.Models;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class BootstrapViewModel : BaseViewModel
    {
        private IUserPreferenceService _userPreferenceService;
        IUIService _uiService;

        public const string msgDisconnect = "error.server_disconnect";
        public const string msgSyncError = "error.settings_sync";

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; RaisePropertyChanged(() => ErrorMessage); RaisePropertyChanged(() => HasError); }
        }

        private string _errorDetail;
        public string ErrorDetail
        {
            get { return _errorDetail; }
            set { _errorDetail = value; RaisePropertyChanged(() => ErrorDetail); }
        }

        public bool HasError
        {
            get { return _errorMessage != null; }
        }

        //private MvxCommand _retryCommand;
        //public ICommand RetryCommand
        //{
        //    get
        //    {
        //        _retryCommand = _retryCommand ?? new MvxCommand(() =>
        //        {
        //            Start();
        //        });

        //        return _retryCommand;
        //    }
        //}

        public BootstrapViewModel(IViewModelServiceLocator services, IUserPreferenceService userPreferenceService, IUIService uiService)
            : base(services)
        {
            _userPreferenceService = userPreferenceService;
            _uiService = uiService;
        }

        public void Init(ParameterKey key)
        {

        }

        public override void Start()
        {
            base.Start();
            ErrorMessage = "message.connecting";
            ErrorDetail = null;
            RunMode mode = Mvx.Resolve<IRunModeManager>().ArgumentParams.Mode;

            _uiService.ChangeColor(_userPreferenceService.OptionsSettings.ThemeColor);

            if (mode == RunMode.Production || mode == RunMode.Testing)
            {
                if (!_userPreferenceService.HasLocal)
                {
                    ShowViewModelExt<ConfigViewModel>();
                }
                else
                {
                    // If host settings changed, stop background server to apply new changes
                    string hostSettingsChecksum = _userPreferenceService.HostSettings.CalculateChecksum();
                    if(_userPreferenceService.HostSettings.Checksum != hostSettingsChecksum)
                    {
                        try
                        {
                            //ServiceController controller = new ServiceController("ApmsClientService");
                            //if (controller.Status == ServiceControllerStatus.Running)
                            //{
                            //    controller.Stop();
                            //    controller.WaitForStatus(ServiceControllerStatus.Stopped);
                            //}
                            //_userPreferenceService.HostSettings.Checksum = hostSettingsChecksum;
                            //_userPreferenceService.HostSettings.Save();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    // If background service not run, start it
                    try
                    {
                        //ServiceController controller = new ServiceController("ApmsClientService");
                        //if (controller.Status == ServiceControllerStatus.Paused || controller.Status == ServiceControllerStatus.Stopped)
                        //{
                        //    controller.Start();
                        //    controller.WaitForStatus(ServiceControllerStatus.Running);
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    _userPreferenceService.SyncToServer((exception) =>
                    {
                        if (exception == null)
                        {
                            ShowViewModelExt<LaneContainerViewModel>();
                        }
                        else
                        {
                            if (exception is ServerDisconnectException)
                            {
                                ErrorMessage = msgDisconnect;
                                ErrorDetail = exception.Message;
                            }
                            else if (exception is ServerErrorException)
                            {
                                ErrorMessage = msgSyncError;
                                ErrorDetail = exception.Message;
                            }
                        }
                    });
                }
            }

            //else if (mode == RunMode.Testing)
            //{
            //    ShowViewModelExt<TestViewModel>();
            //}
        }
    }
}
