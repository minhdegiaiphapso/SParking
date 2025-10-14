using SP.Parking.Terminal.Core.Services;
using Cirrious.MvvmCross.Localization;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SP.Parking.Terminal.Core.Utilities;
using System.Windows.Input;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Models;

namespace SP.Parking.Terminal.Core.ViewModels
{
    /// <summary>
    /// Busy indicator.
    /// </summary>
    public class BusyIndicator
    {
        // Title string
        public string Message { get; set; }

        // Progression [0, 1]
        public float Progress { get; set; }

        // Block user interaction
        public bool BlockUserInteraction { get; set; }
    }


    /// <summary>
    /// Empty info that display on the ui when 
    /// the view is empty
    /// </summary>
    public class EmptyIndicator
    {
        // Image to show
        public string ImageName { get; set; }

        // Title string
        public string Title { get; set; }

        // Subtitle string
        public string Subtitle { get; set; }

        // Tap action
        public Action TapAction { get; set; }
    }

    public class ClosePresentationHint : MvxPresentationHint
    {
        public int PresentationObjectKey { get; set; }
    }

    public class CloseChildPresentationHint : ClosePresentationHint
    {
        //public int PresentationObjectKey { get; set; }
        public int ChildObjectKey { get; set; }
    }

    public class BaseViewModel : MvxViewModel
    {
        protected static int INFINITIVE = 999999;
        protected static int DEFAULT_NOTICE_TIMEOUT = 5000;

        IMvxMessenger _keyPressedMessenger;

        public bool IsStarted { get; private set; }

        // Block user interaction
        public bool BlockUserInteractionWhenBusy { get; set; }

        public IMvxCommandCollection Commands { get; set; }

        // Model services
        public IViewModelServiceLocator Services { get; private set; }

        private Action _lastRefreshableAction;

        // Presentation objecy
        public object PresentationObject { get; set; }

        public StringRes StringRes { get; private set; }

        // Onclose callback
        public Action<BaseViewModel> OnClose { get; set; }

        public BaseViewModel(IViewModelServiceLocator serviceLocator)
        {
            Commands = new MvxCommandCollectionBuilder().BuildCollectionFor(this);
            this.Services = serviceLocator;

            StringRes = new StringRes(this);

            _keyPressedMessenger = Mvx.Resolve<IMvxMessenger>();
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <returns>The text.</returns>
        /// <param name="key">Key.</param>
        public string GetText(string key)
        {
            return StringRes.GetText(key);
        }

        public string GetButtonText(string key)
        {
			return StringRes.GetButtonText(key);
        }

        public string GetCommonText(string key)
        {
			return StringRes.GetButtonText(key);
        }

        #region IStatusUpdate implementation

        public virtual void StatusChanged(ProgressStatus status, string message = null, float value = 0)
        {
            switch (status)
            {
                case ProgressStatus.Started:
                case ProgressStatus.Running:
                    if (status == ProgressStatus.Started)
                        EmptyIndicator = null;

                    BusyIndicator = new BusyIndicator()
                    {
                        Message = message,
                        Progress = value,
                        BlockUserInteraction = BlockUserInteractionWhenBusy,
                    };
                    break;

                case ProgressStatus.Ended:
                    BusyIndicator = null;
                    break;
            }
        }

        /// <summary>
        /// Indicate that exception has occur during the processing
        /// </summary>
        /// <param name="ex">Ex.</param>
        public virtual void HandleError(Exception ex)
        {
            //this.MessageToUser = new MessageToUser(
            //	Services.Localizer.GetText(LocaleNamespace.Error, "Common", "error-happen"),
            //	Services.Localizer.GetText(LocaleNamespace.Error, "Exception", ex.Message ), //ex.GetType().Name),
            //	Services.Localizer.GetText(LocaleNamespace.Button, "AlertView", "ok"), () => { return true; } );

            BusyIndicator = null;

            if (!PreferAlertWhenError)
            {
                var indicator = new EmptyIndicator();
                indicator.ImageName = "";

                if (ex is NoConnectionException)
                {
                    indicator.Title = GetCommonText("no-connection");
                    indicator.Subtitle = GetCommonText("recheck-connection");
                }
                else
                {
                    indicator.Title = GetCommonText("retrieve-data-error");
                    if (_lastRefreshableAction != null)
                    {
                        indicator.Subtitle = GetCommonText("tap-retry");
                        indicator.TapAction = () =>
                        {
                            this.EmptyIndicator = null;
                            _lastRefreshableAction();
                        };
                    }
                    else
                    {
                        indicator.Subtitle = GetCommonText("retry-nexttime");
                    }
                }

                EmptyIndicator = indicator;
            }
            else
            {
                var message = new MessageToUser();
                message.AddOption(GetButtonText("ok"), () => true);

                if (ex is NoConnectionException)
                {
                    message.Title = GetCommonText("no-connection");
                    message.Message = GetCommonText("recheck-connection");
                }
                else
                {
                    message.Title = GetCommonText("retrieve-data-error");
                    if (_lastRefreshableAction != null)
                    {
                        message.Message = GetCommonText("tap-retry");
                        message.AddOption(GetButtonText("retry"), () =>
                        {
                            _lastRefreshableAction();
                            return true;
                        });
                    }
                    else
                    {
                        message.Message = GetCommonText("retry-nexttime");
                    }
                }

                MessageToUser = message;
            }
            LastError = ex;
        }

        #endregion

        public bool PreferAlertWhenError { get; set; }

        /// <summary>
        /// Check if this view model is in busy mode, the view should bind this
        /// value to visibility atttribute of a sub or global activity indicator
        /// </summary>
        private BusyIndicator _busyIndicator = null;
        public BusyIndicator BusyIndicator
        {
            get { return _busyIndicator; }
            set { _busyIndicator = value; RaisePropertyChanged(() => BusyIndicator); }
        }

        /// <summary>
        /// Show the message to user
        /// </summary>
        private MessageToUser _messageToUser = null;
        public MessageToUser MessageToUser
        {
            get { return _messageToUser; }
            set
            {
                _messageToUser = value;
                //Console.Out.WriteLine(value.Message);
                RaisePropertyChanged(() => MessageToUser);
            }
        }

        private ISection _section;
        public virtual ISection Section
        {
            get { return _section; }
            set
            {
                if (_section == value)
                    return;

                _section = value;

                RaisePropertyChanged(() => Section);
            }
        }

        /// <summary>
        /// Show the notice to user
        /// </summary>
		private Notices _notices = new Notices();
        public Notices Notices
        {
            get { return _notices; }
            set
            {
                if (value == null || value.Count == 0)
                    _notices.Clear();
                else
                    _notices = value;
                RaisePropertyChanged(() => Notices);
            }
        }

        /// <summary>
        /// Check if this view model is empty, the view should bind this
        /// value to visibility atttribute of empty view
        /// </summary>
        private EmptyIndicator _emptyIndicator = null;
        public EmptyIndicator EmptyIndicator
        {
            get { return _emptyIndicator; }
            set { _emptyIndicator = value; RaisePropertyChanged(() => EmptyIndicator); }
        }

        private Exception _lastError = null;
        public Exception LastError
        {
            get { return _lastError; }
            set { _lastError = value; RaisePropertyChanged(() => LastError); }
        }
         

        /// <summary>
        /// Shows the <c>generic</c> view model.
        /// </summary>
        /// <param name="requestBy">Request by.</param>
        /// <param name="requestParameter">Request parameter.</param>
        public void ShowViewModelExt<TViewModel>(object parameter = null, object presentationParameter = null, Action<BaseViewModel> onClose = null)
            where TViewModel : IMvxViewModel
        {
            MvxBundle bundle = new MvxBundle();
            bundle.Write(new
            {
                Requester = Services.Parameter.Store(PresentationObject).Key,
                Parameter = Services.Parameter.Store(presentationParameter).Key,
                OnClose = Services.Parameter.Store(onClose).Key,
            });

            ShowViewModel<TViewModel>(parameter, bundle, null);
        }

        public void ShowViewModelExt<TViewModel>()
            where TViewModel : IMvxViewModel
        {
            ShowViewModelExt<TViewModel>(null, null, null);
        }

        /// <summary>
        /// Refreshables the execute.
        /// </summary>
        /// <param name="action">Action.</param>
        public void RefreshableExecute(Action action)
        {
            EmptyIndicator = null;
            _lastRefreshableAction = action;
            action();
        }

        /// <summary>
        /// Call after ui is ready
        /// </summary>
        public virtual void Start()
        {
            IsStarted = true;
        }


        public virtual void Close()
        {
            if (this.PresentationObject != null)
            {
                var hint = new ClosePresentationHint()
                {
                    PresentationObjectKey = Services.Parameter.Store(this.PresentationObject).Key,
                };

                ChangePresentation(hint);
            }
            if (OnClose != null)
                OnClose(this);
        }

        public virtual void Unloaded()
        {

        }

        public virtual void Loaded()
        {

        }

        /// <summary>
        /// Shows the confirm message.
        /// </summary>
        /// <param name="ok">Ok.</param>
        public void ShowConfirmMessage(string message, Action ok, Action cancel)
        {
            var confirmMessage = new MessageToUser(
                GetText("dialog.title-confirm"),
                message);
            confirmMessage.AddOption(GetButtonText("cancel"), () =>
            {
                if (cancel != null)
                    cancel();
                return true;
            }, MessageToUser.OptionStatus.Neutral);
            confirmMessage.AddOption(GetButtonText("ok"), () =>
            {
                if (ok != null)
                    ok();
                return true;
            }, MessageToUser.OptionStatus.Positive);
            this.MessageToUser = confirmMessage;
        }

        //public void KeyPressed(object sender, KeyEventArgs e)
        //{
        //    if(_keyPressedMessenger.HasSubscriptionsFor<KeyPressedMessage>())
        //    {
        //        //_keyPressedMessenger.Publish(new KeyPressedMessage(sender, e));
        //    }
        //}
    }
}
