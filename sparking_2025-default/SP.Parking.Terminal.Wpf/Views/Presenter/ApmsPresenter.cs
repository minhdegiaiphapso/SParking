using System;
using System.Linq;
using Cirrious.MvvmCross.Wpf.Views;
using System.Windows;
using SP.Parking.Terminal.Wpf.Views;
using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.ViewModels;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Views;
using System.Windows.Controls;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Wpf.Views.AppViews;

namespace SP.Parking.Terminal.Wpf.Views
{
    public class RegionAttribute : Attribute
    {
        public RegionAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class ApmsPresenter : MvxWpfViewPresenter, IApmsPresenter
    {
        private readonly MainWindow _contentControl;

        // Parameter service
        IParameterService _parameterService;
        IParameterService ParameterService
        {
            get
            {
                if (_parameterService == null)
                    _parameterService = Mvx.Resolve<IParameterService>();

                return _parameterService;
            }
        }

        public ApmsPresenter(Window mainWin)
        {
            //_contentControl = new MainWindow();
            _contentControl = mainWin as MainWindow;
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            if (hint.GetType() == typeof(ClosePresentationHint))
            {
                var _hint = hint as ClosePresentationHint;

                var presentationObject = ParameterService.Retrieve(new ParameterKey() { Key = _hint.PresentationObjectKey });
                if (presentationObject is BaseView)
                {
                    ((BaseView)presentationObject).Close();
                }
            }
            else if (hint.GetType() == typeof(CloseChildPresentationHint))
            {
                var _hint = hint as CloseChildPresentationHint;
                var presentationObject = ParameterService.Retrieve(new ParameterKey() { Key = _hint.PresentationObjectKey });
                var childObject = ParameterService.Retrieve(new ParameterKey() { Key = _hint.ChildObjectKey });
                if (presentationObject is BaseView)
                {
                    ((BaseView)presentationObject).InterceptCloseViewRequest(childObject as BaseViewModel);
                }
            }

            base.ChangePresentation(hint);
        }

        public override void Present(FrameworkElement frameworkElement)
        {
            
        }

        public BaseView CreateView<T>() where T : BaseViewModel
        {
            BaseView view = null;
            view = (BaseView)Mvx.Resolve<IMvxSimpleWpfViewLoader>().CreateView(MvxViewModelRequest<T>.GetDefaultRequest());
            
            if (view != null)
                view.Presenter = this;

            return view;
        }

        public override void Show(MvxViewModelRequest request)
        {
            BaseView frameworkElement;
            var viewModelType = request.ViewModelType;

            frameworkElement = (BaseView)Mvx.Resolve<IMvxSimpleWpfViewLoader>().CreateView(request);

            if (frameworkElement != null)
            {
                frameworkElement.Presenter = this;
                frameworkElement.RequestParameter = new RequestParameter();

                (frameworkElement.ViewModel as BaseViewModel).PresentationObject = frameworkElement;

                //  Get request parameters and init the view with these infomation
                if (request.PresentationValues != null)
                {
                    if (request.PresentationValues.ContainsKey("Requester"))
                    {
                        int key = int.Parse(request.PresentationValues["Requester"]);
                        frameworkElement.RequestParameter.Requester = ParameterService.Retrieve(new ParameterKey() { Key = key }) as BaseView;
                    }

                    if (request.PresentationValues.ContainsKey("Parameter"))
                    {
                        int key = int.Parse(request.PresentationValues["Parameter"]);
                        frameworkElement.RequestParameter.Parameter = ParameterService.Retrieve(new ParameterKey() { Key = key });
                    }

                    if (request.PresentationValues.ContainsKey("OnClose"))
                    {
                        int key = int.Parse(request.PresentationValues["OnClose"]);
                        (frameworkElement.ViewModel as BaseViewModel).OnClose = ParameterService.Retrieve(new ParameterKey() { Key = key }) as Action<BaseViewModel>;
                    }
                }
            }

            // Let the requester the chance to intercept the request
            if (frameworkElement != null)
            {
                if (frameworkElement.RequestParameter.Requester != null && frameworkElement.RequestParameter.Requester.InterceptViewRequest((BaseView)frameworkElement))
                {
                    //if (frameworkElement.View != null)
                        frameworkElement.Start(frameworkElement.DataContext as BaseViewModel);
                    return;
                }
            }

            var attribute = frameworkElement
                                .GetType()
                                .GetCustomAttributes(typeof(RegionAttribute), true)
                                .FirstOrDefault() as RegionAttribute;
            var regionName = attribute == null ? null : attribute.Name;


            frameworkElement.BindData();
            _contentControl.ShowInMainView(frameworkElement);
            frameworkElement.Start(null);
            //_contentControl.Show();
        }

        public void Show(FrameworkElement view)
        {
            
        }

        public void CloseCurrentView()
        {
            _contentControl.CloseCurrentView();
        }
    }
}
