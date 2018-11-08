using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;

using FreshMvvm;
using Newtonsoft.Json;
using PropertyChanged;
using Xamarin.Forms;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer.Models
{
    using DominicanSeason = Dictionary<DominicanSeasons, DateTime>;

    public interface IDominicanCalender
    {
        DominicanSeason GetDominicanSeasonForYear(string year);
    }


    public class DominicanCalender : IDominicanCalender
    {
        int _startYear = 2015;
        FeastsModel _feastsModel = null;
        string _Tag = "DominicanCalender";
        Dictionary<int, int> _OT2StartWeeks = new Dictionary<int, int>();
        List<DominicanSeason> _calender = new List<DominicanSeason>();

        //private static readonly DominicanCalender _instance = new DominicanCalender();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        //static DominicanCalender()
        //{
        //}

        public DominicanCalender()
        {
            DominicanDates dominicanDates = ParseJsonDominicanDates();
            CreateCalender(dominicanDates);

            _feastsModel = FreshIOC.Container.Resolve<IFeastsModel>() as FeastsModel;
        }

        //public static DominicanCalender Instance
        //{
        //    get
        //    {
        //        return _instance;
        //    }
        //}

        public DominicanSeason GetDominicanSeasonForYear(string year)
        {
            int yearIdx = Int32.Parse(year) - _startYear;               // 2015
            DominicanSeason ds = _calender[yearIdx];

            return ds;
        }

        DominicanDates ParseJsonDominicanDates()
        {
            Debug.WriteLine($"{_Tag}.ParseJsonDominicanDates()");

            DominicanDates dominicanDates = null;
            try
            {
                Debug.WriteLine($"{_Tag}.ParseJsonDominicanDates() in try");
                var assembly = typeof(PrayerPageModel).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("DailyPrayer.Data.Dates.json"))
                {
                    if (stream == null)
                        Debug.WriteLine("GetManifestResourceStream(DailyPrayer.Data.Dates.json) = null");
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            dominicanDates = serializer.Deserialize<DominicanDates>(reader);
                        }
                        //'reader' will be disposed by this point
                        Debug.WriteLine("Finished loading ParseJsonDominicanDates");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{_Tag}.ParseJsonWorkflow()( {0} ) - error\n{1}\n", ex.Message, ex.StackTrace);
            }

            return dominicanDates;
        }

        void CreateCalender(DominicanDates dominicanDates)
        {
            Debug.WriteLine($"{_Tag}.CreateCalender()");

            string dateFormat = "dd/MM/yyyy";
            CultureInfo provider = CultureInfo.InvariantCulture;
            foreach (DominicanDates.DominicanYear dominicanYear in dominicanDates.years)
            {
                //Debug.WriteLine("{0}", dominicanYear.ToString());

                DominicanSeason ds = new DominicanSeason();
                //int nextYear = dominicanYear.year + 1;
                //ds[DominicanSeasons.Advent] = DateTime.ParseExact(dominicanYear.dates.advent + "/" + lastYear, dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.Advent] = DateTime.ParseExact(dominicanYear.dates.advent + "/" + (dominicanYear.year-1), dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.XMas] = DateTime.ParseExact("25/12/" + (dominicanYear.year - 1), dateFormat, provider);
                //ds[DailyPrayer.DominicanSeasons.OctaveOfChristmas] = DateTime.ParseExact("25/12/" + (dominicanYear.year - 1), dateFormat, provider);
                //ds[DailyPrayer.DominicanSeasons.OctaveOfEpiphany] = DateTime.ParseExact("25/12/" + (dominicanYear.year - 1), dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.OT1] = DateTime.ParseExact(dominicanYear.dates.ot1+ "/" + dominicanYear.year, dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.Ash_Wednesday_Week] = DateTime.ParseExact(dominicanYear.dates.ashwed + "/" + dominicanYear.year, dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.Lent] = ds[DailyPrayer.DominicanSeasons.Ash_Wednesday_Week].AddDays(4);
                ds[DailyPrayer.DominicanSeasons.Easter] = DateTime.ParseExact(dominicanYear.dates.easter + "/" + dominicanYear.year, dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.Holy_Week] = ds[DailyPrayer.DominicanSeasons.Easter].AddDays(-7);
                ds[DailyPrayer.DominicanSeasons.OT2] = DateTime.ParseExact(dominicanYear.dates.pentecost + "/" + dominicanYear.year, dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.XMas_II] = DateTime.ParseExact("25/12/" + dominicanYear.year, dateFormat, provider);
                ds[DailyPrayer.DominicanSeasons.EndOfYear] = DateTime.ParseExact("01/01/" + (dominicanYear.year+1), dateFormat, provider);
                //int index = dominicanYear.year - _startYear;

                if (_calender.Count > 0)
                {
                    // advent for the Catholic year starts in the previous year, 
                    // but we also need the date in this year
                    DominicanSeason dsPrev = _calender[_calender.Count - 1];
                    dsPrev[DailyPrayer.DominicanSeasons.NextAdvent] = ds[DailyPrayer.DominicanSeasons.Advent];
                }

                _calender.Add(ds);
                //Debug.WriteLine($"{_Tag}.CreateCalender() - FIN");
            }
        }


        public Place FindPlace(DateTime date)
        {
            //Debug.WriteLine("DominicanCalender.FindPlace - " + date.ToString("yyyy/MM/dd HH:mm"));

            Place place = new Place();
            // find the season, week, and day of the date in the Liturgical year
            int yearIdx = date.Year - _startYear;
            DominicanSeason ds = _calender[yearIdx+1];      // get next years dates

            //Debug.WriteLine(string.Format("Year {0:yyyy/MM/dd HH:mm}", ds[DailyPrayer.DominicanSeasons.Advent]));

            int charYear = date.Year;                   // regarded as prev year till OT1
            if (date < ds[DailyPrayer.DominicanSeasons.Advent])         // check if date < advent start
            {
                ds = _calender[yearIdx];                    // if not, get prev year
                charYear = date.Year - 1;
            }

            place.Morning = date.Hour < 12.0;
            place.DayNo = string.Format("{0}", (int)date.DayOfWeek + 1);                            // 0 == Sunday, but needs 2 b in range 1-7
            place.DayInMonth = date.Day;
            place.Month = date.Month;
            place.YearChar = (char)((int)'A' + (charYear % 3));

            // first check if the date is a feast date
            // if so, we don't need to bother with anything else
            FeastDate feast = _feastsModel.GetFeastForDate(date);
            if (feast != null)
            {
                //Debug.WriteLine("DominicanCalender.FindPlace: Found Feast " + feast.feast.Name);
                place.DomSeason = DailyPrayer.DominicanSeasons.Feasts;
                place.Filename = feast.filename;
                place.Title = feast.feast.Title;
                place.WeekNo = "1";                         // just a dummy value, not used for feasts
                return place;
            }

            // not a feast
            int idx = -1;
            DateTime seasonDate = new DateTime();
            for (DailyPrayer.DominicanSeasons j = DailyPrayer.DominicanSeasons.Advent; j < DailyPrayer.DominicanSeasons.EndOfYear; j++)
            {
                //Debug.WriteLine(string.Format("between {0:yyyy/MM/dd HH:mm} - {1:yyyy/MM/dd HH:mm} ?", ds[j], ds[j+1]));
                if (date >= ds[j] && date < ds[j + 1])
                {
                    idx = (int)j;
                    seasonDate = ds[j];
                    // BotL and Pentecost r the Feasts which start on the first day of OT1 & 2
                    if (j == DailyPrayer.DominicanSeasons.OT1 && date.Date == ds[j])
                        place.DomSeason = DailyPrayer.DominicanSeasons.Baptism_of_the_Lord;
                    else if (j == DailyPrayer.DominicanSeasons.OT2 && date.Date == ds[j])
                        place.DomSeason = DailyPrayer.DominicanSeasons.Pentecost;
                    else
                        place.DomSeason = j;

                    place.SeasonStr = place.DomSeason.ToString();
                    //Debug.WriteLine("found !!!");
                    break;
                }
            }

            if (idx < 0)
            {
                return place;
            }

            int seasonWeekNo = (((idx==0)?1:seasonDate.DayOfYear) - (int)seasonDate.DayOfWeek + 10) / 7;
            int dateWeekNo = (date.DayOfYear - (int)date.DayOfWeek + 10) / 7;
            if (seasonWeekNo == 52 && dateWeekNo < 10)
                dateWeekNo = (seasonWeekNo + dateWeekNo) - 1;

            int weekNo = (dateWeekNo - seasonWeekNo) + 1;                                           // first week = week1, not week0

            if (place.DomSeason == DailyPrayer.DominicanSeasons.OT2)
            {
                if (!_OT2StartWeeks.ContainsKey(date.Year))
                {
                    int startWeek = CalcOT2StartWeek(ds);
                    _OT2StartWeeks[date.Year] = startWeek;
                }
                weekNo += _OT2StartWeeks[date.Year];
            }
            else if (place.DomSeason == DailyPrayer.DominicanSeasons.XMas ||
                     place.DomSeason == DailyPrayer.DominicanSeasons.XMas_II)
            {
                //if (place.DomSeason == DominicanSeasons.XMas)
                //charYear--;

                DateTime holyFamilyFeast;
                DateTime xmas = ds[place.DomSeason];
                if (xmas.DayOfWeek == 0)                                                // if Xmas is on a Sunday
                {
                    holyFamilyFeast = new DateTime(xmas.Year, xmas.Month, 30);          // HFF is on the Friday
                }
                else
                {
                    holyFamilyFeast = new DateTime(xmas.Year, xmas.Month, 25);
                    holyFamilyFeast = holyFamilyFeast.AddDays(7- (int)xmas.DayOfWeek);                    // HFF is following Sunday
                }

                if (date.Date == holyFamilyFeast.Date)
                    place.DomSeason = DailyPrayer.DominicanSeasons.Holy_Family;
                else if (place.DayNo == "1")
                {
                    // both these feasts r on Sundays after the respective dates
                    if (place.DayInMonth > 25)
                        place.DomSeason = DailyPrayer.DominicanSeasons.Holy_Family;
                    else if (place.DayInMonth > 1 && place.DayInMonth <= 8)
                        place.DomSeason = DailyPrayer.DominicanSeasons.Epiphany;
                }
                //if (place.DayInMonth == 1)                  // New Years Day
                //    place.DomSeason = DailyPrayer.DominicanSeasons.Mother_of_God;
            }


            place.WeekNo = string.Format("{0}", weekNo);                   // first week = week1, not week0

            if (place.DomSeason == DailyPrayer.DominicanSeasons.Advent)
            {
                /*int wkNo = weekNo % 4;
                if (wkNo == 0)
                    wkNo = 4;
                place.WeekNo = string.Format("{0}", wkNo);                   // first week = week1, not week0
                */
                place.WeekNo = string.Format($"{seasonWeekNo}");                   // first week = week1, not week0;
            }
            else
                place.WeekNo = string.Format($"{weekNo}");                   // first week = week1, not week0

            return place;
        }

        int CalcOT2StartWeek(DominicanSeason ds)
        {
            // To determine what week it is in we need to count from the first Sunday of Advent of church year e.g. 2016 - 2017, 27 / 11 / 2016.
            // The week before that first Sunday of Advent is always week 34 and count backward to 18/05/2016 and so it is in week 7 of Ordinary Time.
            int startWeek = -1;

            TimeSpan days = ds[DailyPrayer.DominicanSeasons.NextAdvent] - ds[DailyPrayer.DominicanSeasons.OT2];
            //Debug.WriteLine(string.Format("{0}.CalcOT2StartWeek: {1} - {2} = {3}",
            //    _Tag, ds[DailyPrayer.DominicanSeasons.NextAdvent].ToString(), ds[DailyPrayer.DominicanSeasons.OT2].ToString(), days.Days));
            int weeks = (days.Days) / 7;

            startWeek = 34 - weeks;

            DateTime dt = ds[DailyPrayer.DominicanSeasons.OT2];
            //Debug.WriteLine(string.Format("{0}.CalcOT2StartWeek: {1}, NumWeeks={2}, StartWeek={3}", _Tag, dt.Year, weeks, startWeek));

            return startWeek;
        }

        public override string ToString()
        {
            string toString = "";

            int year = 0;
            string dates = "";
            foreach (DominicanSeason ds in _calender)
            {
                dates = "";
                for (DominicanSeasons i = DominicanSeasons.Advent; i <= DominicanSeasons.EndOfYear; i++)
                {
                    dates += string.Format(", {1}={0:dd/MM/yyyy}", ds[i], i.ToString());
                }
                toString += string.Format($"{year+_startYear} {dates}\n");
                year++;
            }

            return toString;
        }
    }
}
