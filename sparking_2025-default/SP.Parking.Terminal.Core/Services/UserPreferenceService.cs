using Cirrious.CrossCore;
using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Models;
using Green.Devices.Dal;
using Green.Devices.Vivotek;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Reflection;
using System.Diagnostics;
using SP.Parking.Terminal.Core.Utilities;

namespace SP.Parking.Terminal.Core.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private Action<Exception> _syncComplete;
        private IServer _server;
        public ISystemSettings SystemSettings { get; private set; }
        public IHostSettings HostSettings { get; private set; }
        public IOptionsSettings OptionsSettings { get; private set; }
        public ITestingSettings TestSettings { get; private set; }

        public bool HasLocal { get { return SystemSettings.HasLocal && HostSettings.HasLocal; } }

        public UserPreferenceService(IServer server)
        {
            _server = server;
            SystemSettings = Mvx.Resolve<ISystemSettings>();
            HostSettings = Mvx.Resolve<IHostSettings>();
            OptionsSettings = Mvx.Resolve<IOptionsSettings>();
            TestSettings = Mvx.Resolve<ITestingSettings>();
        }
        public ServerTimeInfo GetServerTime()
        {
            ServerTimeInfo svt = new ServerTimeInfo();
            Task t = Task.Factory.StartNew(()=> 
                _server.GetServerTime((st,ex)=> {
                    if(ex!=null)
                    {
                        svt.UtcTime = DateTime.UtcNow;
                        svt.LocalTime = DateTime.Now;
                    }
                    else
                    {
                        svt.UtcTime = st.UtcTime;
                        svt.LocalTime = st.LocalTime;
                    }
                })
            );
            t.Wait();
            return svt;
        }
        public void SyncToServer(Action<Exception> complete)
        {
            _syncComplete = complete;
            if(!HasLocal)
            {
                _syncComplete(new Exception());
                return;
            }
            SyncGlobalConfig();    
        }

        private void SyncGlobalConfig()
        {
            _server.GetGlobalConfig(HostSettings.Terminal.Id, OtherUtilities.GetVersion(), SyncGlobalConfigReceived);
        }

        private void SyncGlobalConfigReceived(GlobalConfig resObj, Exception exception)
        {
            if(exception == null)
            {
                HostSettings.ParkingName = resObj.ParkingName;
                HostSettings.LogServerIP = resObj.LogServer;
                HostSettings.Save();
                SyncHostSettings();
            }
            else
            {
                _syncComplete(exception);
            }
        }

        private void SyncHostSettings()
        {
            //if (CalcCheckSum(HostSettings) == HostSettings.SyncDataChecksum && false)
            //{
            //    SyncSystemSettings();
            //}
            //else
            //{
                //Models.Terminal terminal = HostSettings.Terminal;
                //if (terminal.Id > 0)
                //{
                //    _server.UpdateTerminal(terminal, SyncHostSettingsReceived);
                //}
                //else
                //{
                //_server.CreateTerminal(terminal, SyncHostSettingsReceived);
                //}
            //}

            Models.Terminal terminal = HostSettings.Terminal;
            _server.CreateTerminal(terminal, SyncHostSettingsReceived);
        }

        private void SyncHostSettingsReceived(Models.Terminal terminal, Exception exception)
        {
            if (exception == null)
            {
                if (!HostSettings.Terminal.Equals(terminal))
                {
                    HostSettings.Terminal = terminal;
                    HostSettings.SyncDataChecksum = CalcCheckSum(HostSettings);
                    HostSettings.Save();
                }
                SyncSystemSettings();
            }
            else
            {
                _syncComplete(exception);
            }
        }

        private void SyncSystemSettings()
        {
            if (CalcCheckSum(SystemSettings) == SystemSettings.SyncDataChecksum && false)
            {
                _syncComplete(null);
            }
            else
            {
                _server.CreateLane(GetActiveLanes(SystemSettings), SyncSystemSettingsReceived);
            }
        }

        private void SyncSystemSettingsReceived(Lane[] lanes, Exception exception)
        {
            if (exception == null)
            {
                foreach (Lane lane in lanes)
                {
                    foreach (Section section in SystemSettings.Sections.Values)
                    {
                        if (section.Lane != null && section.Lane.Name == lane.Name)
                        {
                            section.Lane = lane;
                            SystemSettings.UpdateSection(section);
                            break;
                        }
                    }
                }
                SystemSettings.SyncDataChecksum = CalcCheckSum(SystemSettings);
                SystemSettings.Save();
            }
            _syncComplete(exception);
        }

        private Lane[] GetActiveLanes(ISystemSettings settings)
        {
            List<Lane> lanes = new List<Lane>();
            foreach (Section section in SystemSettings.Sections.Values)
            {
                if (section.Lane != null && section.Lane.Name != null)
                {
                    section.Lane.Direction = section.Direction;
                    section.Lane.TerminalId = HostSettings.Terminal.Id;
                    lanes.Add(section.Lane);
                }
            }
            return lanes.ToArray();
        }

        private string CalcCheckSum(object obj)
        {
            string dumpString = null;
            if(obj is HostSettings)
            {
                IHostSettings settings = (IHostSettings)obj;
                dumpString = JsonConvert.SerializeObject(settings.Terminal);
            }
            else if(obj is SystemSettings)
            {
                ISystemSettings settings = (ISystemSettings)obj;
                dumpString = JsonConvert.SerializeObject(GetActiveLanes(settings));
            }
            
            if(dumpString == null)
                return null;
            byte[] result = null;
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                result = hash.ComputeHash(enc.GetBytes(dumpString));
            }
            return Convert.ToBase64String(result);
        }
    }
}
