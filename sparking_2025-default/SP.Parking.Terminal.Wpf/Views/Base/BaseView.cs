using Cirrious.CrossCore;
using Cirrious.MvvmCross.Wpf.Views;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.ComponentModel;
using System.Windows.Media;
using SP.Parking.Terminal.Wpf.UI;

namespace SP.Parking.Terminal.Wpf.Views
{
    /// <summary>
    /// Request parameter.
    /// </summary>
    public class RequestParameter
    {
        public BaseView Requester { get; set; }

        public object Parameter { get; set; }

        //public Action<BaseViewModel> OnClose { get; set; }
    }

    public class BasePage : NavigationWindow
    {
    }
    public class BaseView : MvxWpfView, IMvxBindingContextOwner, IDisposable
    {
        public MainWindow MainWindow { get; set; }
		// Gets the locale object.
		public StringRes StringRes { get; private set; }

		// Locale service
		public IViewModelServiceLocator Services { get; private set; }

		private IUIService _dialogService;

		public RequestParameter RequestParameter { get; set; }

		public IApmsPresenter Presenter { get; set; }

        private bool _isDisposing;

        // First change to show relate view
        public virtual bool InterceptViewRequest(BaseView requestedView)
        {
            return false;
        }

        public virtual bool InterceptCloseViewRequest(BaseViewModel childVM)
        {
            return false;
        }

        public void Dispose()
        {
            if (_isDisposing)
            {
                return;
            }

            BindingContext.ClearAllBindings();
        }

        public IMvxBindingContext BindingContext { get; set; }

        public BaseView()
        {
			if (DesignerProperties.GetIsInDesignMode(this))
				return;

            this.BindingContext = new MvxBindingContext();
            this.Loaded += ViewLoaded;
            this.Unloaded += ViewUnloaded;

			// Locale service
			Services = Mvx.Resolve<IViewModelServiceLocator>();
			_dialogService = Mvx.Resolve<IUIService>();

			// Build text source name
			StringRes = new StringRes(this);

            this.Background = Theme.Theme.Background;
        }

        public virtual void ViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ViewModel != null)
                (ViewModel as BaseViewModel).Unloaded();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }

        public virtual void ViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ViewModel != null)
                (ViewModel as BaseViewModel).Loaded();
        }

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

        public virtual void Close()
        {
            Presenter?.CloseCurrentView();
        }


		/// <summary>
		/// Gets or sets a value indicating whether this instance is busy.
		/// </summary>
		/// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
		BusyIndicator _busyIndicator;
		public BusyIndicator BusyIndicator
		{
			get { return _busyIndicator; }
			set
			{
				_busyIndicator = value;
				if (_busyIndicator != null)
				{
					
				}
				else
				{
					
				}
			}
		}

        List<CentralLaneWindow> _alerts = new List<CentralLaneWindow>();

		/// <summary>
		/// Display the message to user
		/// </summary>
		public MessageToUser MessageToUser
		{
			get { return null; }
			set
			{
				if (value != null)
				{
                    CentralLaneWindow alert = new ConfirmWindow(value.Title, value.Message, new List<string> { value.Options[0].Title, value.Options[1].Title }, value.Options[0].Handler, value.Options[1].Handler) { Container = this };
                    _alerts.Add(alert);
                    alert.ShowDialog();
				}
                else
                {
                    foreach (var alert in _alerts)
                        alert.Close();
                    _alerts.Clear();
                    if (this.MainWindow != null)
                        this.MainWindow.MainView.Focus();
                }
			}
		}

        Notices _noticesToUser;
		public virtual Notices NoticesToUser
        {
			get { return _noticesToUser; }
            set
            {
				_noticesToUser = value;
                DisplayNotices();
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is empty.
		/// </summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		EmptyIndicator _emptyIndicator;
		public EmptyIndicator EmptyIndicator
		{
			get
			{
				return _emptyIndicator;
			}
			set
			{
				_emptyIndicator = value;
				if (_emptyIndicator != null)
				{
					
				}
				else
				{

				}
			}
		}

		private Exception _lastError = null;
		public Exception LastError
		{
			get { return _lastError; }
			set
			{
				_lastError = value;

			}
		}

		public virtual void Start(BaseViewModel viewModel)
		{
			if (this.DataContext != viewModel && viewModel != null)
				this.DataContext = viewModel;

            if (this.ViewModel != viewModel && viewModel != null)
                this.ViewModel = ViewModel;

			viewModel = this.DataContext as BaseViewModel;
			if (!viewModel.IsStarted)
				viewModel.Start();
		}



        public virtual void BindData()
		{
            BindingContext.DataContext = this.ViewModel;
            
			var set = this.CreateBindingSet<BaseView, BaseViewModel>();
			set.Bind(this).For(c => c.BusyIndicator).To(vm => vm.BusyIndicator);
            set.Bind(this).For(c => c.MessageToUser).To(vm => vm.MessageToUser).OneWay();
            set.Bind(this).For(c => c.EmptyIndicator).To(vm => vm.EmptyIndicator);
            set.Bind(this).For(c => c.LastError).To(vm => vm.LastError);
            set.Bind(this).For(c => c.NoticesToUser).To(vm => vm.Notices);
			set.Apply();
		}

        public virtual void DisplayNotices()
        {

        }
    }
}
