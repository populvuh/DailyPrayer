using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

using DailyPrayer.Models;
using DailyPrayer.Models.PrayerSeason;

using FreshMvvm;

using Xamarin.Forms;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer
{
    interface IPrayer
    {
        string PrayerHtml { get; }
        string PrayerHeading { get; }
    }

    public class Place
    {
        public string SeasonStr;
        public DominicanSeasons DomSeason;
        public string WeekNo;
        public string DayNo;
        public bool Morning;
        public int DayInMonth;
        public int Month;
        public char YearChar;
        public string Filename;
        public string Title;

        public override string ToString()
        {
            string outText = (string.IsNullOrEmpty(Filename)) ?
                String.Format("Season: {0}, Week:{1}, Day:{2}, {3}", SeasonStr, WeekNo, DayNo, (Morning) ? "am" : "pm") :
                String.Format("Feast Filename: {0}", Filename);
            return outText;
        }
    };

    public enum DominicanSeasons
    {
        Advent = 0,
        XMas = 1,
        OT1,
        Ash_Wednesday_Week,
        Lent,
        Holy_Week,
        Easter,
        OT2,
        XMas_II,
        EndOfYear,
        NextAdvent,
        Baptism_of_the_Lord,
        Pentecost,
        Holy_Family,
        Mother_of_God,
        Epiphany,
        Feasts
    };

    public interface IPrayerPageModel
    {
        void DisplayPrayer(DateTime date);
    }


    //[ImplementPropertyChanged]                                                              // uses fody for property changed
    public class PrayerPageModel : FreshBasePageModel, IPrayerPageModel, IPrayer, INotifyPropertyChanged
    {
        #region Constructor
        string _Tag = "PrayerPageModel";
        private bool _testMode = false;
        static public bool DisplayFilenames = false;
        static public bool DisplayFilenamesOnly = false;

        public string Message { get; set; }
        new public event PropertyChangedEventHandler PropertyChanged;

        DateTime _date;
        DateTime _1Jan2016 = new DateTime(2016, 1, 1, 0, 0, 1);
        DateTime _31Dec2050 = new DateTime(2050, 12, 31, 23, 59, 50);

        PrayerModel _prayerModel = null;
        DominicanCalender _dominicanCalender = null;

        public string PrayerHtml { get; set; }

        private string _prayerHeading;
        public string PrayerHeading
        {
            get { return _prayerHeading; }
            set
            {
                _prayerHeading = value;
                OnPropertyChanged("PrayerHeading");
            }
        }

        public HtmlWebViewSource _webViewSource;
        public HtmlWebViewSource WebViewSource
        {
            get { return _webViewSource; }
            set
            {
                _webViewSource = value;
                OnPropertyChanged("WebViewSource");
            }
        }

        public bool Busy { get; set; }


        int _fontSize = 16;
        int _fontIncrement = 4;

        string _htmlStart = "";
        string _prayerHtml = "";
        string _htmlEnd = "";
        //string _filenames = "";
        string _htmlEndTemplate = "";
        const string _htmlStartTestHtml = "<html>\n" +
            "<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">\n" +
            "<META HTTP-EQUIV=\"Content-language\" CONTENT=\"vi\">\n" +
            "<body style=\"background-color:#FFFFFF;color:#007AFF;font-size:16px\">\n";
        const string _htmlStartTemplate = "<html>\n<body style = \"background-color:#FFFFFF;color:#007AFF;font-size:{0}px\">";


        public PrayerPageModel()
        {
            Debug.WriteLine("PrayerPageModel.PrayerPageModel()");
            FreshMvvm.FreshIOC.Container.Register<IPrayer>(this);

            MessagingCenter.Unsubscribe<PrayerPage, int>(this, "NextPrayer");
            MessagingCenter.Subscribe<PrayerPage, int>(this, "NextPrayer", (sender, arg) =>
            {
                Debug.WriteLine("PrayerPageModel.MessagingCenter () - Next Prayer( " + ((int)arg).ToString() + " )");
                NextPrayer((int)arg);
            });
            MessagingCenter.Unsubscribe<PrayerPage, int>(this, "CurrentPrayer");
            MessagingCenter.Subscribe<PrayerPage>(this, "CurrentPrayer", (sender) =>
            {
                Debug.WriteLine("PrayerPageModel.MessagingCenter () - Current Prayer");
                CurrentPrayer();
            });
            MessagingCenter.Unsubscribe<PrayerPage, int>(this, "PinchGesture");
            MessagingCenter.Subscribe<PrayerPage>(this, "PinchGesture", (sender) =>
            {
                Debug.WriteLine("PrayerPageModel.MessagingCenter () - PinchGesture");
                PinchGesture();
            });

            MessagingCenter.Unsubscribe<PrayerPage, DateTime>(this, "NewDate");
            MessagingCenter.Subscribe<PrayerPage, DateTime>(this, "NewDate", (sender, arg) =>
            {
                Debug.WriteLine("PrayerPageModel.MessagingCenter() - NewDate");
                //IsDatePickerVisible = false;

                if (_date != arg)
                {
                    _date = arg.AddHours(6);         // OT its set at 12:00AM
                }
                DisplayPrayer(_date);
            });

            MessagingCenter.Unsubscribe<PrayerPage, int>(this, "ContactsPage");
            MessagingCenter.Subscribe<PrayerPage>(this, "ContactsPage", async (sender) =>
            {
                Debug.WriteLine("PrayerPageModel.MessagingCenter () - ContactsPage");
                await CoreMethods.PushPageModel<DailyPrayer.ContactPageModel>();
            });


        }
#endregion

#region Overrides

        public override void Init(object initData)
        {
            Debug.WriteLine("PrayerPageModel.Init()");
            base.Init(initData);

            try
            {
                _prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;
                _dominicanCalender = FreshIOC.Container.Resolve<IDominicanCalender>() as DominicanCalender;

                if (initData is null)
                {
                    return;
                }

                // get date time passed in
                string[] data = (initData as string).Split(' ');
                if (data.Length != 2)
                    return;                                                         // WTF !!!

                string stringDate = data[0] + " " + data[1];
                _date = DateTime.ParseExact(stringDate, "yyyyMMdd HH:mm", CultureInfo.CurrentCulture);

                _fontIncrement = (Device.Idiom == TargetIdiom.Phone) ? 4 : 8;
                _htmlEndTemplate = PrayerSeason.LoadEndHtml();

                if (Application.Current.Properties.ContainsKey("FontSize"))
                {
                    _fontSize = (int)Application.Current.Properties["FontSize"];
                    if (_fontSize < 1)
                        _fontSize = 12;
                }
                else
                {   // first time thru or they haven't updated the font size yet
                    _fontSize = (Device.Idiom == TargetIdiom.Phone) ? 16 : 20;
                }
                _htmlStart = string.Format(_htmlStartTemplate, _fontSize);
                _htmlEnd = string.Format(_htmlEndTemplate, _fontSize);

                DisplayPrayer(_date);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PrayerPageModel.Init() - Error: {ex.Message}\n{ex.InnerException}");
            }
            Debug.WriteLine("PrayerPageModel.Init() - FIN");
        }

        public override void ReverseInit(object returnedData)
        {
            Debug.WriteLine("PrayerPageModel.ReverseInit()");
            bool ok = DateTime.TryParseExact(returnedData as string,
                "yyyyMMdd HH:mm",
                new CultureInfo("fr-FR"),
                DateTimeStyles.None,
                out _date);

            if (ok)
                DisplayPrayer(_date);
            else
                Debug.WriteLine("PrayerPageModel.ReverseInit( " + returnedData + " ) - failed");
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PrayerPageModel.ViewIsDisappearing()");
            Busy = true;
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PrayerPageModel.ViewIsAppearing()");
            Busy = false;
        }
#endregion

#region Methods
        void CurrentPrayer()
        {
            _date = DateTime.Now;
            Debug.WriteLine("PrayerPageModel.CurrentPrayer() - " + _date.ToString());

            DisplayPrayer(_date);
        }

        public void DisplayPrayer(DateTime date)
        {
#if DEBUG
            //_testMode = true;
#endif
            string prayerHtml = _prayerModel.MakePrayer(date, _testMode);
            DisplayOnScreen(prayerHtml);
        }

        void DisplayOnScreen(string prayerHtml)
        {
            PrayerHeading = _prayerModel.PrayerHeading;

            PrayerHtml = _htmlStart;
            PrayerHtml += prayerHtml;
            PrayerHtml += _htmlEnd;

            // we save this for when they change font size so we don't have to recalc the prayer for each font change
            _prayerHtml = prayerHtml;                

            WebViewSource = new HtmlWebViewSource { Html = PrayerHtml };
        }

        void NextPrayer(int add)
        {
            // add 12 hours to the current date/time
            DateTime date = _date.AddHours(add * 12.0);
            if (date < _1Jan2016 || date > _31Dec2050)
            {
                Debug.WriteLine("PrayerPageModel.CreatePrayer() - Invalid date " + date.ToString());
                return;
            }
            Debug.WriteLine("PrayerPageModel.NextPrayer( "+add.ToString()+" ) - " + _date.ToString());
            _date = date;

            DisplayPrayer(_date);
        }

        void UpdateFont(int amount)
        {
            _fontSize += amount;
            if (_fontSize < 4)
                _fontSize = 4;

            _htmlStart = string.Format(_htmlStartTemplate, _fontSize);
            _htmlEnd = string.Format(_htmlEndTemplate, _fontSize);

            DisplayOnScreen(_prayerHtml);

            Application.Current.Properties["FontSize"] = _fontSize;
            Application.Current.SavePropertiesAsync();
        }

        void PinchGesture()
        {
            System.Diagnostics.Debug.WriteLine("Command PinchGesture");
            UpdateFont(_fontIncrement);
        }
#endregion

#region Commands
        public Command<string> ChangeTextSizeCommand
        {
            get
            {
                return new Command<string>((sizeDirection) =>
                {
                    //Debug.WriteLine($"Command ChangeTextSizeCommand( {sizeDirection} )");
                    UpdateFont(_fontIncrement * Convert.ToInt32(sizeDirection));
                });
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
#endregion
    }
}

