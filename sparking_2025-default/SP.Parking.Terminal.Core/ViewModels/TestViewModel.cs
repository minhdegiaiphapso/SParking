using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class TestViewModel : BaseTestViewModel
    {
        
        public TestViewModel(IViewModelServiceLocator services)
            : base(services)
        {
            
        }

        public void Init(ParameterKey key)
        {
                      
        }
        
        public override void Run()
        {
            ExecuteMethod(TestMethod1);
            ExecuteMethod(TestMethod2);

            ShowViewModelExt<CheckInLaneViewModel>();
        }

        public void TestMethod1()
        {
            System.Threading.Thread.Sleep(500);
            Console.WriteLine("Test method 1");
        }

        public void TestMethod2()
        {
            Console.WriteLine("Test method 2");
        }
    }
}
