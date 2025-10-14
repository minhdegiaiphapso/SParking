using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public class TestSettingsData : BaseSettingsData
    {
        public string TestHost { get; set; }
        public int TestingCountDown { get; set; }
        public int TestingDuration { get; set; }
        public int Delay { get; set; }
    }

    public interface ITestingSettings : IBaseSettings
    {
        string TestHost { get; }
        int TestingCountDown { get; }
        int TestingDuration { get; }
        int Delay { get; }
    }

    public class TestingSettings : BaseSettings<TestSettingsData>, ITestingSettings
    {
        public string TestHost
        {
            get { return _data.TestHost; }
            private set {
                _data.TestHost = value;
                //MarkChanged();
            }
        }
        public int TestingCountDown
        {
            get { return _data.TestingCountDown; }
            private set { _data.TestingCountDown = value; }
        }
        public int TestingDuration
        {
            get { return _data.TestingDuration; }
            private set { _data.TestingDuration = value; }
        }
        public int Delay
        {
            get { return _data.Delay; }
            private set { _data.Delay = value; }
        }

        public TestingSettings(ArgumentParameter argParams = null)
            : base(argParams)
        {
            if (!HasLocal)
            {
                this.TestingCountDown = 5;
                this.TestingDuration = 60;
                this.Delay = 3;
            }
            this.TestHost = argParams.TestHost;
            ForceSave();
        }
    
    }
}
