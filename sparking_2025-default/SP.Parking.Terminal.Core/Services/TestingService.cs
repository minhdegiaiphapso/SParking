using Cirrious.CrossCore;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace SP.Parking.Terminal.Core.Services
{
    //public class ActionHandler
    //{
    //    public Timer Timer { get; set; }
    //    public Action Callback { get; set; }

    //    public ActionHandler()
    //    {
    //        Timer = new Timer();
    //    }
    //}

    public class TestCard
    {
        [JsonProperty("delay")]
        public int Delay { get; set; }
        [JsonProperty("card_id")]
        public string CardId { get; set; }
    }

    public class TestLane
    {
        [JsonProperty("direction")]
        public LaneDirection Direction { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }

    public interface IWebApiTestingServer
    {
        void GetCardCheckin(ISection section, Action<TestCard, Exception> complete);
        void GetCardCheckout(ISection section, Action<TestCard, Exception> complete);
        void MarkCheckinCardUsed(ISection section, string cardId, long duration, Exception ex, Action<TestCard> complete);
        void MarkCheckoutCardUsed(ISection section, string cardId, long duration, Exception ex, Action<TestCard> complete);
        void RegisterLane(ISection section, Action<TestLane> complete);
    }

    public class WebApiTestingServer : IWebApiTestingServer
    {
        private RestClient _restClient;

        public WebApiTestingServer()
        {
            _restClient = new RestClient();
            _restClient.Timeout = 10000;
            _restClient.BaseUrl = "http://" + Mvx.Resolve<IRunModeManager>().ArgumentParams.TestHost; ;
        }

        public void GetCardCheckin(ISection section, Action<TestCard, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/check-in-card/?lane_id={0}", section.Lane.Id), Method.GET);
            _restClient.ExecuteAsync(request, (response) => OnGetCardCompleted(response, complete));
        }

        public void MarkCheckinCardUsed(ISection section, string cardId, long duration, Exception ex, Action<TestCard> complete)
        {
            var request = new RestRequest(string.Format("/api/check-in-card/{0}/used/", cardId), Method.PUT);
            request.AddParameter("lane_id", section.Lane.Id);
            request.AddParameter("duration", duration);
            request.AddParameter("error", ex != null ? ex.ToString() : string.Empty);
            _restClient.ExecuteAsync(request, null);
        }

        public void MarkCheckoutCardUsed(ISection section, string cardId, long duration, Exception ex, Action<TestCard> complete)
        {
            var request = new RestRequest(string.Format("/api/check-out-card/{0}/used/", cardId), Method.PUT);
            request.AddParameter("lane_id", section.Lane.Id);
            request.AddParameter("duration", duration);
            request.AddParameter("error", ex != null ? ex.ToString() : string.Empty);
            _restClient.ExecuteAsync(request, null);
        }

        public void GetCardCheckout(ISection section, Action<TestCard, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/check-out-card/?lane_id={0}", section.Lane.Id), Method.GET);
            _restClient.ExecuteAsync(request, (response) => OnGetCardCompleted(response, complete));
        }

        public void RegisterLane(ISection section, Action<TestLane> complete)
        {
            var request = new RestRequest("/api/gate-register/", Method.POST);
            request.AddParameter("lane_id", section.Lane.Id);
            _restClient.ExecuteAsync(request, (response) => OnRegisterLaneCompleted(response, complete));
        }

        private void OnGetCardCompleted(IRestResponse response, Action<TestCard, Exception> complete)
        {
            Exception exception = GetException(response);

            TestCard rs = null;
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<TestCard>(response.Content);
            }

            if (complete != null)
                complete(rs, exception);
        }

        private void OnRegisterLaneCompleted(IRestResponse response, Action<TestLane> complete)
        {
            Exception exception = GetException(response);

            TestLane rs = null;
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<TestLane>(response.Content);
            }

            if (complete != null)
                complete(rs);
        }

        private Exception GetException(IRestResponse response)
        {
            if (response != null)
            {
                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                        case System.Net.HttpStatusCode.Created:
                            return null;
                        case System.Net.HttpStatusCode.NotFound:
                            return new NotFoundException(response.Content);
                        default:
                            if (response.ErrorException != null)
                                return new ServerErrorException(response.ErrorException.Message);
                            else
                                return new ServerErrorException(response.Content);
                    }
                }
                else
                {
                    return new ServerDisconnectException(response.ErrorMessage);
                }
            }
            else
            {
                return new ServerDisconnectException("Disconnect");
            }
        }

    }

    public interface ITestingService
    {
        int TestingDuration { get; }
        int TestingCountDown { get; }
        int Delay { get; }

        void Start(Action complete);
        //System.Timers.Timer CreateScheduler(string fileName, Action<Action<TestCard>> callback);
        void CreateSchedule(string fileName, Action<Action<ISection, TestCard, Exception>> callback);
        //void RegisterLane(ISection section, Action<TestLane> complete);
    }

    public class TestingService : ITestingService
    {
        IUserPreferenceService _userPreferenService;

        public int TestingDuration { get { return _userPreferenService.TestSettings.TestingDuration * 1000; } }
        public int TestingCountDown { get { return _userPreferenService.TestSettings.TestingCountDown * 1000; } }
        public int Delay { get { return _userPreferenService.TestSettings.Delay * 1000; } }

        //System.Timers.Timer _superTimer;
        //List<System.Timers.Timer> _timers;
        //Dictionary<string, string> _contents;
        
        public TestingService()
        {
            _userPreferenService = Mvx.Resolve<IUserPreferenceService>();

            //_timers = new List<System.Timers.Timer>();
            //_contents = new Dictionary<string, string>();

            // Super timer init
            //_superTimer = new System.Timers.Timer();
            //_superTimer.Interval = TestingDuration;
            //_superTimer.Elapsed += _superTimer_Elapsed;
        }

        //void _superTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    _superTimer.Stop();
        //    StopActionTimers();
        //}

        public async void Start(Action complete)
        {
            await Task.Delay(TestingCountDown);

            if (complete != null)
                complete();
        }

        public LaneDirection GetLaneDirection(ISection section)
        {

            return LaneDirection.In;
        }

        public void CreateSchedule(string fileName, Action<Action<ISection, TestCard, Exception>> callback)
        {
            try
            {
                Stopwatch watch = new Stopwatch();

                watch.Restart();

                callback(async (section, card, ex) => {
                    watch.Stop();

                    if (card != null)
                    {
                        if (!string.IsNullOrEmpty(card.CardId))
                        {
                            if (section.Direction == LaneDirection.In)
                            {
                                Console.WriteLine("tap in: " + card.CardId);
                                Mvx.Resolve<IWebApiTestingServer>().MarkCheckinCardUsed(section, card.CardId, watch.ElapsedMilliseconds, ex, null);
                            }
                            else
                            {
                                Console.WriteLine("tap out: " + card.CardId);
                                Mvx.Resolve<IWebApiTestingServer>().MarkCheckoutCardUsed(section, card.CardId, watch.ElapsedMilliseconds, ex, null);
                            }
                        }

                        if (card.Delay != -1)
                            await Task.Delay(card.Delay);
                        else
                            return;
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }

                    CreateSchedule(fileName, callback);
                });
            }catch(Exception exception)
            {
            }
        }

        //private void WriteFile(string fileName, string data)
        //{
        //    var documents = @"C:\ProgramData\APMS";
        //    var folder = Path.Combine(documents, "Performance");
        //    if (!Directory.Exists(folder))
        //    {
        //        Directory.CreateDirectory(folder);
        //    }

        //    string savedFilePath = Path.Combine(folder, fileName + ".log");
        //    File.WriteAllText(savedFilePath, data);
        //}
        
        //private void StopActionTimers()
        //{
        //    for (int i = 0; i < _timers.Count; i++)
        //    {
        //        _timers[i].Stop();
        //        _timers[i] = null;
        //    }
        //    _timers.Clear();

        //    List<string> keys = new List<string>(_contents.Keys);

        //    foreach (var key in keys)
        //    {
        //        double avg;
        //        double sd;
        //        CalculateStandardDeviation(_contents[key], out avg, out sd);
        //        _contents[key] += avg + " - " + sd + Environment.NewLine;
        //        WriteFile(key, _contents[key]);
        //    }
        //}

        //private void CalculateStandardDeviation(string data, out double avg, out double sd)
        //{
        //    // calculate avg
        //    List<string> comps = data.Split('\n').ToList();
        //    comps.RemoveAll(x => string.IsNullOrEmpty(x));

        //    int[] ints = comps.Select(x => int.Parse(x)).ToArray();
        //    avg = ints.Sum() / comps.Count;

        //    // calculate standard deviation
        //    double sqrSum = 0;
        //    foreach(var item in ints)
        //    {
        //        sqrSum += Math.Pow(avg - item, 2);
        //    }

        //    sd = Math.Sqrt(sqrSum / ints.Length);

        //}
    }
}