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
        string PrayerDate { get; }
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
        bool _gregorianDate = true;
        bool _testFeastDates = false;
        bool _testingRange = false;
        PrayerSeason.PrayerSect _testPrayerSection = PrayerSeason.PrayerSect.Ignore;
        static public bool DisplayFilenames = false;
        static public bool DisplayFilenamesOnly = false;

        public string Message { get; set; }
        new public event PropertyChangedEventHandler PropertyChanged;

        DateTime _date;
        DateTime _1Jan2016 = new DateTime(2016, 1, 1, 0, 0, 1);
        DateTime _31Dec2050 = new DateTime(2050, 12, 31, 23, 59, 50);

        static SeasonDefn _seasons = null;
        DominicanCalender _dominicanCalender = null;

        public string PrayerHtml { get; set; }

        private string _prayerDate;
        public string PrayerDate
        {
            get { return _prayerDate; }
            set
            {
                _prayerDate = value;
                OnPropertyChanged("PrayerDate");
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
        string _filenames = "";
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
                IsDatePickerVisible = false;

                if (_date == arg)                       // no change, so don't do anyhting
                {
                    DisplayOnScreen();
                }
                else
                {
                    _date = arg;
                    _date.AddHours(6);                  // OT its set at 12:00AM
                    DisplayPrayer(_date);
                }
            });
            MessagingCenter.Unsubscribe<PrayerPage>(this, "DatePickerGoing");
            MessagingCenter.Subscribe<PrayerPage>(this, "DatePickerGoing", (sender) =>
            {
                Debug.WriteLine("PrayerPageModel.MessagingCenter () - DatePickerGoing");
                IsDatePickerVisible = false;
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
                PrayerVisible = -1;
                _seasons = PrayerModel.Instance.ParseJsonSeasonsDefn();
                _dominicanCalender = FreshIOC.Container.Resolve<IDominicanCalender>() as DominicanCalender;

                if (_testFeastDates || _testingRange)
                {
                    RunTests(_testPrayerSection);
                    return;
                }

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
            CreatePrayer(date);
            DisplayOnScreen();
        }

        public void CreatePrayer(DateTime date)
        {
            Debug.WriteLine("\nPrayerPageModel.CreatePrayer() - " + date.ToString());

            PrayerModel.NotFounds.Clear();

            Place place = _dominicanCalender.FindPlace(date);
            PrayerSeason prayerSeason = PrayerSeasonFactory.CreatePrayerSeason(place, false);

            string prayerDate = PrayerModel.Instance.MakeDate(date, prayerSeason);
            Debug.WriteLine($"PrayerPageModel.CreatePrayer() - {prayerDate.Length}, {prayerDate}");
            if (_gregorianDate)
                prayerDate += date.ToString("\r\nyyyy/MM/dd tt");
            if (App.SmallScreen && prayerDate.Length >= 65)
                prayerDate = prayerDate.Replace("\r\n", ", ");

            PrayerDate = prayerDate;

            _prayerHtml = PrayerModel.Instance.MakePrayer(date, prayerSeason, false);

            if (DisplayFilenames)
            {
                _filenames = $"<b>Date: {date.ToString("u")} </b><br/>";
                _filenames += prayerSeason.ToString() + "<p/>";
            }

#if DEBUG
            Debug.WriteLine("\nNot founds:\n" + prayerDate);
            foreach (string filename in PrayerModel.NotFounds)
            {
                Debug.WriteLine(filename);
            }
            Debug.WriteLine("\nPrayerPageModel.CreatePrayer() - FIN");
#endif
        }

        void DisplayOnScreen()
        {
            //Debug.WriteLine("PrayerPageModel.DisplayOnScreen()");
            PrayerHtml = _htmlStart;

            if (DisplayFilenames)
            {
                PrayerHtml += _filenames;
            }

            PrayerHtml += _prayerHtml;
            PrayerHtml += _htmlEnd;

            if (DisplayFilenamesOnly)
                Debug.WriteLine(PrayerHtml);

            WebViewSource = new HtmlWebViewSource { Html = PrayerHtml };
        }

        Place FindPlace(DateTime date)
        {
            // find the season, week, and day of the date in the Liturgical year
            bool foundIt = false;
            string stringDate = date.ToString("yyyyMMdd");
            DateTime startDate = new DateTime();
            Place place = new Place();
            for (int i = _seasons.seasons.Count - 1; i >= 0; i--)
            {
                LitYearSeason lys = _seasons.seasons[i];
                if (stringDate.CompareTo(lys.dateStart) >= 0)
                {
                    foundIt = true;
                    place.SeasonStr = lys.name;
                    startDate = DateTime.ParseExact(lys.dateStart, "yyyyMMdd", CultureInfo.CurrentCulture);
                    break;
                }
            }

            if (!foundIt)
            {
                //var notificator = DependencyService.Get<IToastNotificator>();
                //bool tapped = notificator.Notify(ToastNotificationType.Error,
                //    "Error", "Couldn't resolve date :(", TimeSpan.FromSeconds(2)).Result;
                return place;
            }

            place.DayNo = string.Format("{0}", (int)date.DayOfWeek + 1);                            // 0 == Sunday, but needs 2 b in range 1-7
            int seasonWeekNo = (startDate.DayOfYear - (int)startDate.DayOfWeek + 10) / 7;
            int dateWeekNo = (date.DayOfYear - (int)date.DayOfWeek + 10) / 7;
            place.WeekNo = string.Format("{0}", (dateWeekNo - seasonWeekNo) + 1);                   // first week = week1, not week0
            place.Morning = date.Hour <= 12.0;

            return place;
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

            DisplayOnScreen();

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
        public Command LargerTextCommand
        {
            get
            {
                return new Command(() =>
                {
                    //Debug.WriteLine("Command LargerTextCommand");
                    UpdateFont(_fontIncrement);
                });
            }
        }

        public Command SmallerTextCommand
        {
            get
            {
                return new Command(() =>
                {
                    //Debug.WriteLine("Command SmallerTextCommand");
                    UpdateFont(_fontIncrement * -1);
                });
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Old Code
        // not used
        public Command OnExportToEmailCommand
        {
            get
            {
                return new Command(async () =>
                {
                    System.Diagnostics.Debug.WriteLine("Command OnExportToEmailCommand");
                    Busy = true;
                    await CoreMethods.PushPageModel<DailyPrayer.ContactPageModel>();
                });
            }
        }

        private bool _isDatePickerVisible = true;
        public bool IsDatePickerVisible
        {
            get
            {
                return _isDatePickerVisible;
            }
            set
            {
                _isDatePickerVisible = value;
                RaisePropertyChanged("IsDatePickerVisible");
                System.Diagnostics.Debug.WriteLine($"IsDatePickerVisible setter - {IsDatePickerVisible.ToString()}");
            }
        }

        private int _datePickerVisible = -1;
        public int DatePickerVisible
        {
            get
            {
                return _datePickerVisible;
            }
            set
            {
                _datePickerVisible = value;
                RaisePropertyChanged("DatePickerVisible");
            }
        }


        private int _headingVisible = -1;
        public int HeadingVisible
        {
            get
            {
                return _headingVisible;
            }
            set
            {
                _headingVisible = value;
                RaisePropertyChanged("HeadingVisible");
            }
        }

        private int _lineVisible = 1;
        public int LineVisible
        {
            get
            {
                return _lineVisible;
            }
            set
            {
                _lineVisible = value;
                RaisePropertyChanged("LineVisible");
            }
        }

        //set HeightRequest = 0 instead of IsVisible = false, and HeightRequest = -1 instead of IsVisible = true.
        private int _prayerVisible = -1;
        public int PrayerVisible
        {
            get { return _prayerVisible; }
            set
            {
                _prayerVisible = value;
                OnPropertyChanged("PrayerVisible");
            }
        }

        public Command OnChangeDateCommand
        {
            get
            {
                return new Command(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Command PrayerPageModel.OnChangeDateCommand");

                    IsDatePickerVisible = true;
                    //PrayerDatePicker.Focus();

                    System.Diagnostics.Debug.WriteLine($"Command PrayerPageModel.OnChangeDateCommand {IsDatePickerVisible.ToString()}");

                    //HeadingVisible = 0;
                    //LineVisible = 0;
                    //PrayerVisible = 0;
                    //DatePickerVisible = -1;
                    //await CoreMethods.PushPageModel<NewDatePageModel>();
                    //MessagingCenter.Send("", "DateChange");
                });
            }
        }
        void OnDateSelected(object sender, DateChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Command OnDateSelected {args.NewDate.ToString()}");

            //IsDatePickerVisible = false;
            //HeadingVisible = -1;
            //LineVisible = 1;
            //PrayerVisible = -1;
            //DatePickerVisible = 0;
        }
        #endregion Old Code

        #region Test Methods
        void RunTests(PrayerSeason.PrayerSect testPrayerSect)
        {
            PrayerHtml = _htmlStartTestHtml;
            if (_testingRange)
            {
                DateTime startDate = new DateTime(2017, 12, 25, 6, 0, 0);
                DateTime endDate = new DateTime(2018, 1, 31, 18, 0, 0);

                PrayerHtml += PrayerModel.Instance.MakeTestPrayers(startDate, endDate, testPrayerSect, true);
            }
            //else
            //{   //_testFeastDates
            //    string[] feasts = { "Memorial", "Solemnity", "Fixed Solemnity", "optional Memorial", "MovingFeast", "Feast" };
            //    IList<DateTime> feastDates = new List<DateTime>();
            //    foreach (string solemnityType in feasts)
            //    {
            //        List<DateTime> dates = CreateFeastTestDates("2017", solemnityType);
            //        feastDates = feastDates.Concat<DateTime>(dates).ToList<DateTime>();
            //        Debug.WriteLine($"Number of dates for {solemnityType} == {dates.Count()}, {feastDates.Count()}");
            //    }
            //    PrayerHtml += PrayerModel.Instance.MakeTestPrayers(feastDates);
            //}

            //PrayerHtml += _htmlEnd;

            Debug.WriteLine(PrayerHtml);
        }

        public string TestRange(DateTime startDate, DateTime endDate)        //PrayerSeason.PrayerSect testPrayerSect)
        {
            PrayerHtml = _htmlStartTestHtml;
            PrayerHtml += PrayerModel.Instance.MakeTestPrayers(startDate, endDate, PrayerSeason.PrayerSect.Ignore, true);

            return PrayerHtml;
        }

        public string GeneratePrayers(DateTime startDate, DateTime endDate)       //, PrayerSeason.PrayerSect prayerSect)
        {
            PrayerHtml = _htmlStartTestHtml;
            PrayerHtml += PrayerModel.Instance.MakeTestPrayers(startDate, endDate, PrayerSeason.PrayerSect.Ignore, false);
            PrayerHtml += _htmlEnd;

            return PrayerHtml;
            //Debug.WriteLine(PrayerHtml);
            //WebViewSource = new HtmlWebViewSource { Html = PrayerHtml };
        }

        public string GeneratePrayers(DateTime startDate, DateTime endDate, PrayerSeason.PrayerSect prayerSect, bool filenames)
        {
            PrayerHtml = _htmlStartTestHtml;
            PrayerHtml += PrayerModel.Instance.MakeTestPrayers(startDate, endDate, prayerSect, filenames);
            PrayerHtml += _htmlEnd;

            return PrayerHtml;
        }

        public string GenerateFeastDates(string year)       //, PrayerSeason.PrayerSect prayerSect)
        {
            //IList<DateTime> feastDates = new List<DateTime>();
            string[] feastTypes = { "Solemnity", "Fixed Solemnity", "Feast", "MovingFeast", "Memorial", "optional Memorial" };

            PrayerHtml = _htmlStartTestHtml;
            foreach (string solemnityType in feastTypes)
            {
                //List<DateTime> dates = CreateFeastTestDates(year, solemnityType);
                //feastDates = feastDates.Concat<DateTime>(dates).ToList<DateTime>();
                //Debug.WriteLine($"Number of dates for {solemnityType} == {dates.Count()}, {feastDates.Count()}");
                List<FeastDate> feasts = CreateFeastTestDates(year, solemnityType);

                PrayerHtml += ToString(solemnityType, feasts);
            }
            //PrayerHtml += PrayerModel.Instance.MakeTestPrayers(feastDates);
            PrayerHtml += _htmlEnd;

            return PrayerHtml;
        }

        List<FeastDate> CreateFeastTestDates(string year, string feastType)
        {
            Debug.WriteLine($"{_Tag}.CreateFeastTestDates( {year}, {feastType})");

            //string dateFormat = @"yyyy/MM/dd HH:mm:ss";
            List<DateTime> feastDates = new List<DateTime>();
            FeastsModel feastsModel = FreshIOC.Container.Resolve<IFeastsModel>() as FeastsModel;
            /*IList<FeastDate> feastObjects =*/
            feastsModel.GetFeastsForYear(year);

            DatabaseModel databaseModel = FreshIOC.Container.Resolve<IDatabaseModel>() as DatabaseModel;
            IList<FeastDate> feastObjects = databaseModel.GetFeastsForYear(year).ToList();              // FeastObject
            Debug.WriteLine($"feastObjects count = {feastObjects.Count}");

            //foreach (FeastDate feastDate in feastObjects)
            //{
            //Debug.WriteLine($"feast {feastDate.date}, {feastDate.feast.ToString()} ");
            //}

            //IEnumerable
            List<FeastDate> feasts = feastObjects.Where<FeastDate>(d => d.feast.SolemnityType == feastType).ToList<FeastDate>();
            Debug.WriteLine($"feasts count = {feasts.Count<FeastDate>()}");

            return feasts;      // as List<FeastDate>;

            //Debug.WriteLine($"feasts count = {feasts.Count<FeastDate>()}");
            //if (feasts.Count<FeastDate>()== 0)
            //{
            //    Debug.WriteLine($"No {feastType} found\n");
            //    return feastDates;
            //}

            //Debug.WriteLine($"{feastType} Feasts:");
            //foreach (FeastDate feastObject in feasts)
            //{
            //    DateTime dateTime = feastObject.date.AddHours((feastObject.am) ? 6 : 18);                       // set up for morning or evening
            //    feastDates.Add(dateTime);
            //    Debug.WriteLine($"Feast: {feastObject.ToString()}");
            //}

            //Debug.WriteLine($"{feastType} Feast dates:");
            //foreach (DateTime dateTime in feastDates)
            //{
            //    Debug.WriteLine($"{dateTime.ToString(dateFormat)}");
            //}
            //Debug.WriteLine("");

            //return feastDates;
        }

        string ToString(string solemnityType, List<FeastDate> feastDates)
        {
            string htmlText = "<p/><b>" + solemnityType + "</b><br/>";
            bool movingFeast = (solemnityType.CompareTo("MovingFeast") == 0);

            if (feastDates == null)
            {
                htmlText += "None :( <br/>n";
            }
            else
            {
                foreach (FeastDate feast in feastDates)
                {
                    htmlText += feast.date.ToString("u").Replace(" 00:00:00Z", " ");
                    htmlText += ((feast.am) ? "am" : "pm") + " &nbsp; &nbsp; ";
                    htmlText += feast.feast.Title + ", ";
                    if (movingFeast)
                    {
                        htmlText += " &nbsp; &nbsp; ";
                        htmlText += feast.feast.Name.Replace('_', ' ') + " ";
                    }
                    htmlText += "<br/>";
                }
            }

            return htmlText;
        }


        void ListAllResources()
        {
            Debug.WriteLine($"{_Tag}.ListAllResources()");
            var assembly = typeof(PrayerPageModel).GetTypeInfo().Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                Debug.WriteLine("Found resource: " + res);
            }
            Debug.WriteLine($"{_Tag}.ListAllResources() Fin\n");
        }

        void SetDebug()
        {
            Debug.WriteLine("PrayerPageModel.SetDebug()");
        }

        void ShowAllFiles()
        {
            // use for debugging
            Debug.WriteLine("Resources: ");
            var assembly = typeof(PrayerPageModel).GetTypeInfo().Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
                Debug.WriteLine(">>> " + res);
        }
        #endregion
    }
}

