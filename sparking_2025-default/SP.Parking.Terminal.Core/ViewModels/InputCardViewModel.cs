using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class TapCardEventArgs : EventArgs
    {
        public List<CardHolder> Cards { get; set; }
    }

    public class SaveEventArgs: EventArgs
    {
        public bool IsCompleted { get; set; }
    }

    public class CardHolder: Card
    {
        bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaisePropertyChanged(() => IsChecked);
            }
        }

        public string CreatedTime { get; set; }
    }

    public class InputCardViewModel : BaseViewModel
    {
        IServer _server;
        ICardReaderService _cardReaderService;

        public event EventHandler CompletedReadingCard;
        public event EventHandler DetectedDuplicatedCards;
        public event EventHandler SaveCompleted;

        #region Properties
        //int _incrementalNumber = 0;
        //public int IncrementalNumber
        //{
        //    get { return _incrementalNumber; }
        //    set
        //    {
        //        _incrementalNumber = value;
        //        RaisePropertyChanged(() => IncrementalNumber);
        //    }
        //}

        string _incrementalString;
        public string IncrementalString
        {
            get { return _incrementalString; }
            set
            {
                _incrementalString = value;
                RaisePropertyChanged(() => IncrementalString);
            }
        }

        string _cardReaderIP;
        public string CardReaderIP
        {
            get { return _cardReaderIP; }
            set
            {
                _cardReaderIP = value;
                RaisePropertyChanged(() => CardReaderIP);
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
            }
        }

        ObservableCollection<CardHolder> _cardList;
        public ObservableCollection<CardHolder> CardList
        {
            get { return _cardList; }
            set
            {
                _cardList = value;
                RaisePropertyChanged(() => CardList);
            }
        }

        CardHolder _selectedCard;
        public CardHolder SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (_selectedCard == value) return;
                _selectedCard = value;
                RaisePropertyChanged(() => SelectedCard);
            }
        }

        bool _checkAll;
        public bool CheckAll
        {
            get { return _checkAll; }
            set
            {
                _checkAll = value;
                RaisePropertyChanged(() => CheckAll);
            }
        }

        private VehicleTypeEnum _selectedVehicleType;
        public VehicleTypeEnum SelectedVehicleType
        {
            get { return _selectedVehicleType; }
            set
            {
                _selectedVehicleType = value;
                RaisePropertyChanged(() => SelectedVehicleType);
            }
        }

        IEnumerable<VehicleTypeEnum> _vehicleTypes;
        public IEnumerable<VehicleTypeEnum> VehicleTypes
        {
            get { return _vehicleTypes; }
            set
            {
                _vehicleTypes = value;
                RaisePropertyChanged(() => VehicleTypes);
            }
        }

        CardType _selectedCardType;
        public CardType SelectedCardType
        {
            get { return _selectedCardType; }
            set
            {
                _selectedCardType = value;
                RaisePropertyChanged(() => SelectedCardType);
            }
        }

        List<CardType> _cardTypes;
        public List<CardType> CardTypes
        {
            get { return _cardTypes; }
            set
            {
                _cardTypes = value;
                RaisePropertyChanged(() => CardTypes);
            }
        }

        List<CardReaderWrapper> _cardReaders;
        public List<CardReaderWrapper> CardReaders
        {
            get { return _cardReaders; }
            set
            {
                if (_cardReaders == value) return;
                _cardReaders = value;
                RaisePropertyChanged(() => CardReaders);
            }
        } 
        #endregion

        void OnCompletedReadingCard(CardHolder card)
        {
            InvokeOnMainThread(() => {
                var handle = CompletedReadingCard;

                if (handle != null)
                {
                    if (card == null)
                    {
                        handle(null, new TapCardEventArgs { Cards = null });
                    }
                    else
                    {
                        handle(null, new TapCardEventArgs { Cards = new List<CardHolder> { card } });
                    }
                }
            });
        }

        void OnDetectDuplicatedCards(List<CardHolder> cards)
        {
            InvokeOnMainThread(() => {
                var handle = DetectedDuplicatedCards;

                if (handle != null)
                    handle(null, new TapCardEventArgs { Cards = cards });
            });
        }

        public InputCardViewModel(IViewModelServiceLocator service, 
            IServer server,
            ICardReaderService cardReaderService)
            : base(service)
        {
            _server = server;
            _cardReaderService = cardReaderService;
        }

        public override void Start()
        {
            base.Start();
            _cardList = new ObservableCollection<CardHolder>();

            this.CardReaders = _cardReaderService.GetCardReaders();
            //foreach (var item in this.CardReaders)
            //    item.RawCardReader.ReadingCompleted += RawCardReader_ReadingCompleted;

            VehicleTypes = Enum.GetValues(typeof(VehicleTypeEnum)).Cast<VehicleTypeEnum>();
            SelectedVehicleType = VehicleTypeEnum.All;
            //TypeHelper.GetVehicleTypes(result => {
            //    VehicleTypes = result;
            //    SelectedVehicleType = VehicleTypes.Where(t => t.Id == 0).FirstOrDefault();
            //});

            //CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>();
            TypeHelper.GetCardTypes(result => {
                if (result != null)
                {
                    CardTypes = result;
                    SelectedCardType = CardTypes.Where(t => t.Id == 0).FirstOrDefault();
                }
            });
        }
        void GreenCardReade_TakingCompleted(object sender, GreenCardReaderEventArgs e)
        {
            ;
        }
        void GreenCardReade_ReadingCompleted(object sender, GreenCardReaderEventArgs e)
        {
            string cardId = e.CardID;
            var existingCard = IsCardRegistered(cardId);
            if (existingCard != null)
            {
                OnCompletedReadingCard(existingCard);
                return;
            }

            InvokeOnMainThread(() => {
                var card = new CardHolder();
                card.Id = cardId;
                var now = TimeMapInfo.Current.LocalTime;
                //card.Label = (IncrementalNumber).ToString();
                card.Label = IncrementalString;
                card.VehicleTypeEnum = SelectedVehicleType;
                card.Status = CardStatus.Free;
                card.CardType = this.SelectedCardType;
                card.IsChecked = false;
                card.CreatedTime = now.ToString("dd/MM/yyyy  HH:mm:ss");
                //card.CreatedTime = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                //this.CardList.Add(card);
                this.CardList.Insert(0, card);

                //IncrementalNumber = IncrementalNumber + 1;
                IncrementalString = IncreaseLastNumber(IncrementalString);

                OnCompletedReadingCard(null);
            });
        }
        public override void Loaded()
        {
            base.Loaded();
            CurrentListCardReader.StartGreenCardReader(CurrentListCardReader.ListCardInfo, GreenCardReade_ReadingCompleted, GreenCardReade_TakingCompleted);
            //foreach (var item in this.CardReaders)
            //    item.RawCardReader.ReadingCompleted += RawCardReader_ReadingCompleted;
        }

        public override void Unloaded()
        {
            base.Unloaded();
            CurrentListCardReader.StoptGreenCardReader(CurrentListCardReader.ListCardInfo, GreenCardReade_ReadingCompleted, GreenCardReade_TakingCompleted);
            //foreach (var item in this.CardReaders)
            //    item.RawCardReader.ReadingCompleted -= RawCardReader_ReadingCompleted;
        }

        private CardHolder IsCardRegistered(string cardId)
        {
            return this.CardList.Where(c => c.Id.Equals(cardId)).FirstOrDefault();
        }

        void RawCardReader_ReadingCompleted(object sender, CardReaderEventArgs e)
        {
            string cardId = e.CardID;
            var existingCard = IsCardRegistered(cardId);
            if (existingCard != null)
            {
                OnCompletedReadingCard(existingCard);
                return;
            }

            InvokeOnMainThread(() => {
                var card = new CardHolder();
                var now = TimeMapInfo.Current.LocalTime;
                card.Id = cardId;
                //card.Label = (IncrementalNumber).ToString();
                card.Label = IncrementalString;
                card.VehicleTypeEnum = SelectedVehicleType;
                card.Status = CardStatus.Free;
                card.CardType = this.SelectedCardType;
                card.IsChecked = false;
                card.CreatedTime = now.ToString("dd/MM/yyyy  HH:mm:ss");
                //card.CreatedTime = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                //this.CardList.Add(card);
                this.CardList.Insert(0, card);

                //IncrementalNumber = IncrementalNumber + 1;
                IncrementalString = IncreaseLastNumber(IncrementalString);

                OnCompletedReadingCard(null);
            });
        }

        private string IncreaseLastNumber(string word)
        {
            string sNum = OtherUtilities.GetLastGroupNumber(word);

            if (string.IsNullOrEmpty(sNum))
                return word;

            int lengOfNum = sNum.Length;
            int num = Int32.Parse(sNum);
            num++;
            string newNum = num.ToString(string.Format("D{0}", lengOfNum));
            return word.Replace(sNum, newNum);

            //if (string.IsNullOrEmpty(word)) return "";

            //var regex = new Regex(@"(\d+)(?!.*\d)");
            //var match = regex.Match(word);
            //if (match.Success)
            //{
            //    string sNum = match.Groups[0].Value;
            //    int lengOfNum = sNum.Length;

            //    int num = Int32.Parse(match.Groups[0].Value);
            //    num++;

            //    string newNum = num.ToString(string.Format("D{0}", lengOfNum));
            //    word = word.Replace(sNum, newNum);
            //}
            //return word;
        }

        private void SaveCards(ObservableCollection<CardHolder> cards, Action complete)
        {
            _server.CreateCards(cards.ToArray(), (rs, ex) => {
                InvokeOnMainThread(() => {
                    if (ex != null)
                        ErrorMessage = ex.Message;
                    
                    if(rs.ErrorCards.Length > 0)
                    {
                        ErrorMessage = string.Format(GetText("createcard.error_cards"), cards.Count - rs.NumCreated);
                        var errCards = cards.Where(c => rs.ErrorCards.Contains(c.Id));
                        this.CardList = new ObservableCollection<CardHolder>(errCards);
                    }
                    else
                    {
                        cards.Clear();
                        ErrorMessage = string.Empty;
                    }

                    SaveCompleted(this, new SaveEventArgs { IsCompleted = true });

                    if (complete != null)
                        complete();
                });
            });
        }

        private void DeleteSelectedCards()
        {
            var selectedCards = this.CardList.Where(c => c.IsChecked);
            var temp = this.CardList.Except(selectedCards);
            this.CardList = new ObservableCollection<CardHolder>(temp);
        }

        private void HandleCheck(string s)
        {
            if (s.Equals("all"))
            {
                if (CheckAll)
                {
                    for (int i = 0; i < CardList.Count; i++)
                    {
                        CardList[i].IsChecked = true;
                    }
                }
                else
                {
                    for (int i = 0; i < CardList.Count; i++)
                    {
                        CardList[i].IsChecked = false;
                    }
                }
            }
            else if (s.Equals("item"))
            {
                var a = CardList.Where(c => c.IsChecked);
                if (a != null && a.Count() == CardList.Count)
                    CheckAll = true;
                else if (a == null || a.Count() < CardList.Count)
                    CheckAll = false;
            }
        }

        public void DownloadAllCards(Action<string, Exception> complete)
        {
            _server.GetCards((result, ex) => {

                if (complete != null)
                    complete(result, ex);
            });
        }

        private bool CheckDuplicateCardLabel()
        {
            var dupes = CardList.Where(a => CardList.Except(new List<CardHolder> { a }).Any(x => x.Label.Equals(a.Label))).ToList();
            if (dupes.Count > 0)
            {
                OnDetectDuplicatedCards(dupes);
                return true;
            }
            else
                return false;
        }

        MvxCommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                _saveCommand = _saveCommand ?? new MvxCommand(() => {

                    SaveCompleted(this, new SaveEventArgs { IsCompleted = false });

                    if (!CheckDuplicateCardLabel())
                    {
                        SaveCards(this.CardList, () => {
                            
                        });
                    }
                    else
                    {
                        ErrorMessage = GetText("createcard.duplicate_data");
                        SaveCompleted(this, new SaveEventArgs { IsCompleted = true });
                    }
                });
                return _saveCommand;
            }
        }

        MvxCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                _deleteCommand = _deleteCommand ?? new MvxCommand(() => {
                    DeleteSelectedCards();
                });

                return _deleteCommand;
            }
        }

        //CheckCommand
        MvxCommand<string> _checkCommand;
        public ICommand CheckCommand
        {
            get
            {
                _checkCommand = _checkCommand ?? new MvxCommand<string>((s) => {
                    HandleCheck(s);
                });

                return _checkCommand;
            }
        }

        MvxCommand _connectCardReaderCommand;
        public MvxCommand ConnectCardReaderCommand
        {
            get
            {
                _connectCardReaderCommand = _connectCardReaderCommand ?? new MvxCommand(() => {
                    string ip = string.Empty;
                    string port = string.Empty;
                    if (!string.IsNullOrEmpty(CardReaderIP))
                    {
                        var comps = CardReaderIP.Split(':');
                        ip = comps[0];
                        if (comps.Length > 1)
                            port = comps[1];

                        _cardReaderService.GetCardReader(ip, port);
                    }
                });

                return _connectCardReaderCommand;
            }
        }
    }
}
