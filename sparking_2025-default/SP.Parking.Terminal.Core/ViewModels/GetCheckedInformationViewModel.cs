using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using HtmlAgilityPack;
using Cirrious.CrossCore;
using System.Threading;
using System.Net;
using Cirrious.MvvmCross.ViewModels;
using System.Windows.Input;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using SP.Parking.Terminal.Core.Utilities;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class GetCheckedInformationViewModel : BaseViewModel
    {
        IServer _server;
        ICardReaderService _cardReaderService;
        IUserPreferenceService _userPreferenceService;
        IStorageService _storageService;
        List<CardReaderWrapper> _cardReaders;
        Models.Terminal[] _terminals;

        public GetCheckedInformationViewModel(IViewModelServiceLocator services, ICardReaderService cardReaderService)
            : base(services)
        {
            _cardReaderService = cardReaderService;
            _server = Mvx.Resolve<IServer>();
            _storageService = Mvx.Resolve<IStorageService>();
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
        }

        public override void Start()
        {
            base.Start();
            var now = TimeMapInfo.Current.LocalTime;
            _cardReaders = _cardReaderService.GetCardReaders();
            IpAddresses = _userPreferenceService.SystemSettings.OtherTerminalIPs;
            SearchingDate = now;
            //SearchingDate = DateTime.Now;

            foreach (var item in _cardReaders)
                item.RawCardReader.ReadingCompleted += RawCardReader_ReadingCompleted;
        }

        void RawCardReader_ReadingCompleted(object sender, CardReaderEventArgs e)
        {
            CardId = e.CardID;
            FindCommand.Execute(null);
        }

        private void GetTerminal()
        {
            _server.GetTerminals((terminals, exception) => {
                _terminals = terminals;
            });
        }

        #region Properties
        byte[] _frontImage;
        public byte[] FrontImage
        {
            get { return _frontImage; }
            set
            {
                _frontImage = value;
                RaisePropertyChanged(() => FrontImage);
            }
        }

        byte[] _backImage;
        public byte[] BackImage
        {
            get { return _backImage; }
            set
            {
                _backImage = value;
                RaisePropertyChanged(() => BackImage);
            }
        }

        string _resultMessage;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set
            {
                _resultMessage = value;
                RaisePropertyChanged(() => ResultMessage);
            }
        }

        string _imageDateTime;
        public string ImageDateTime
        {
            get { return _imageDateTime; }
            set
            {
                _imageDateTime = value;
                RaisePropertyChanged(() => ImageDateTime);
            }
        }

        bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                _isSearching = value;
                RaisePropertyChanged(() => IsSearching);
            }
        }

        string _cardLabel;
        public string CardLabel
        {
            get { return _cardLabel; }
            set
            {
                _cardLabel = value;
                RaisePropertyChanged(() => CardLabel);
            }
        }


        string _cardId;
        public string CardId
        {
            get { return _cardId; }
            set
            {
                _cardId = value;
                RaisePropertyChanged(() => CardId);
            }
        }

        string _ipAddresses;
        public string IpAddresses
        {
            get { return _ipAddresses; }
            set
            {
                _ipAddresses = value;
                _userPreferenceService.SystemSettings.OtherTerminalIPs = _ipAddresses;
                RaisePropertyChanged(() => IpAddresses);
            }
        }

        DateTime _searchingDate;
        public DateTime SearchingDate
        {
            get { return _searchingDate; }
            set
            {
                _searchingDate = value;
                RaisePropertyChanged(() => SearchingDate);
            }
        }
        #endregion

        private string[] GetIPs(string ips)
        {
            string[] ipArr = ips.Split(';');
            return ipArr.Select(ip => ip.Replace(" ", "")).ToArray();
        }

        public void GetNewestImages()
        {
            if (string.IsNullOrEmpty(CardLabel) && string.IsNullOrEmpty(CardId))
                return;

            IsSearching = true;
            FindImage(CardLabel, CardId, (items) =>
            {
                //var items = value.Value;
                if (items == null || items.Count < 1)
                {
                    ResultMessage = GetText("search.not_found");
                    FrontImage = null;
                    BackImage = null;
                    IsSearching = false;
                    ImageDateTime = string.Empty;
                    return;
                }
                var dt = DateTime.ParseExact(items.First().CreatedDateTime, "yyyyMMdd", CultureInfo.InvariantCulture);
                var datetimeofImages = ParseDateTimeFromFileName(Path.GetFileName(items.First().Path), dt);

                if (datetimeofImages == null)
                    ImageDateTime = "Unknown";
                else
                    ImageDateTime = datetimeofImages.Value.ToString("dd/MM/yyyy HH:mm");

                ResultMessage = string.Empty;
                ImageItemPath frontItem = items.Where(p => p.Path.Contains("_f")).FirstOrDefault();
                ImageItemPath backItem = items.Where(p => p.Path.Contains("_b")).FirstOrDefault();
                _storageService.LoadImage(frontItem.Path, frontItem.Host, (result, ex) =>
                {
                    FrontImage = result;
                    IsSearching = false;
                });
                _storageService.LoadImage(backItem.Path, backItem.Host, (result, ex) =>
                {
                    BackImage = result;
                    IsSearching = false;
                });
            });
        }

        private DateTime? ParseDateTimeFromFileName(string fileName, DateTime date)
        {
            string[] comps = fileName.Split('_');
            if(comps.Length > 0)
            {
                string time = comps[0];
                
                var t = DateTime.ParseExact(time, "HHmm", CultureInfo.InvariantCulture);
                DateTime newDateTime = date + TimeSpan.Parse(t.ToString("HH:mm"));
                return newDateTime.ToLocalTime();
            }

            return null;
        }

        public class ImageItemPath
        {
            public string Host { get; set; }
            public string Path { get; set; }
            public string CreatedDateTime { get; set; }
        }

        CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public void FindImage(string cardLabel, string cardId = null, Action<List<ImageItemPath>> complete = null)
        {
            if (IpAddresses == null)
                return;

            IpAddresses = IpAddresses.Replace(" ", "");
            var terminals = GetIPs(IpAddresses);
            int numberOfTerminal = terminals.Length;
            string time = SearchingDate.ToString("yyyyMMdd");

            Task.Factory.StartNew(async () =>
            {

                Task[] tasks = new Task[numberOfTerminal];
                ConcurrentDictionary<string, List<ImageItemPath>> results = new ConcurrentDictionary<string, List<ImageItemPath>>();
                for (int i = 0; i < terminals.Length; i++)
                {
                    string ter = terminals[i];
                    GetImagesInformation(ter, "9191", time, cardLabel, cardId, ohyeah =>
                    {
                        if (ohyeah != null && ohyeah.Count > 0)
                        {
                            results.AddOrUpdate(time, ohyeah, (key, val) =>
                            {
                                return val;
                            });
                        }
                    });
                }

                await Task.Delay(3000);

                var paths = GetApproriateImages(results);

                if (complete != null) complete(paths);
            });
        }


        private List<ImageItemPath> GetApproriateImages(ConcurrentDictionary<string, List<ImageItemPath>> data)
        {
            string key = data.Keys.Max<string>();
            
            if (key == null) return null;

            return data[key];
        }

        private void GetImagesInformation(string host, string port, string time, string cardLabel, string cardId = null, Action<List<ImageItemPath>> complete = null)
        {
            string path = null;
            string url = null;
            if (!string.IsNullOrEmpty(cardLabel))
            {
                path = CreateSearchImagePath(cardLabel, time);
                url = string.Format("/images{0}", path);
            }

            string symPath = null;
            string symUrl = null;
            if (!string.IsNullOrEmpty(cardId))
            {
                symPath = CreateSearchImagePath(cardId, time);
                symUrl = string.Format("/images{0}", symPath);
            }

            if (string.IsNullOrEmpty(CardId))
                GetInformation(host + ":" + port, url, images =>
                {
                    List<ImageItemPath> result = new List<ImageItemPath>();
                    foreach (var item in images)
                    {
                        result.Add(new ImageItemPath { Path = path + item, Host = host, CreatedDateTime = time });
                    }

                    if (complete != null)
                        complete(result);
                });
            else
                GetInformation(host + ":" + port, symUrl, images =>
                {
                    List<ImageItemPath> result = new List<ImageItemPath>();
                    foreach (var item in images)
                    {
                        result.Add(new ImageItemPath { Path = symPath + item, Host = host, CreatedDateTime = time });
                    }

                    if (complete != null)
                        complete(result);
                });
        }

        private string CreateSearchImagePath(string cardSomething, string time)
        {
            string suffix = cardSomething.Substring(Math.Max(0, cardSomething.Length - 2));
            suffix = suffix.Length < 2 ? suffix.Insert(0, "0") : suffix;
            string path = string.Format("/{0}/in/{1}/{2}/", time, suffix, cardSomething);

            return path;
        }
        

        /// <summary>
        /// Gets the information synchoronously.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        public void GetInformation(string host, string endpoint, Action<List<string>> complete)
        {
            _server.CrawlPage(host, endpoint, (response, exception) => {
                List<string> result = new List<string>();
                if (exception == null)
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(response.Content);
                    var nodes = doc.DocumentNode.Descendants("a");

                    foreach (var node in nodes)
                    {
                        string val = node.InnerText;
                        if (!val.Contains(".."))
                        {
                            if (val.Contains("/"))
                                val = val.Replace("/", "");

                            result.Add(val);
                        }
                    }
                    result.Reverse();
                }

                if (complete != null)
                    complete(result);
            });
        }

        MvxCommand _findCommand;
        public ICommand FindCommand
        {
            get
            {
                _findCommand = _findCommand ?? new MvxCommand(() => {
                    _tokenSource = new CancellationTokenSource();
                    GetNewestImages();
                });
                return _findCommand;
            }
        }

        public void KeyUpEvent(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                FindCommand.Execute(null);
            }
        }

        public override void Unloaded()
        {
            base.Unloaded();

            //_userPreferenceService.SystemSettings.Save();
            foreach (var item in _cardReaders)
                item.RawCardReader.ReadingCompleted -= RawCardReader_ReadingCompleted;
        }
    }
}
