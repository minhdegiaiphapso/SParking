using Cirrious.MvvmCross.Wpf.Views;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Wpf.Views
{
    public interface IApmsPresenter
    {
        void CloseCurrentView();

        BaseView CreateView<T>() where T : BaseViewModel;
    }
}
