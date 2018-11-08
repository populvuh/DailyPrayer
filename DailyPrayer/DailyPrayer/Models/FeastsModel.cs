using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;
using System.Globalization;
using DailyPrayer;
using FreshMvvm;

namespace DailyPrayer.Models
{
    using DominicanSeason = Dictionary<DominicanSeasons, DateTime>;

    public class FeastDate
    {
        public DateTime date { get; set; }
        public bool am { get; set; }
        public string filename { get; set; }
        public FeastInfoObject feast { get; set; }
        static CultureInfo cultureInfo = new CultureInfo("en-AU");

        public FeastDate(string dmy, bool morning, string fname, FeastInfoObject fo)
        {
            date = DateTime.Parse(dmy, cultureInfo);
            am = morning;
            filename = fname;
            feast = fo;
        }

        public override string ToString()
        {
            string amPm = (am) ? "am" : "pm";
            string toString = string.Format($"{date.ToString(@"yyyy/MM/dd")}, {amPm}, {filename}, {feast.ToString()}"); 
            return toString;
        }
    }

    public interface IFeastsModel
    {
        FeastDate GetFeastForDate(DateTime date);
    }

    public class FeastsModel : IFeastsModel
    {
        string _Tag = "FeastsModel";
        bool _test = true;
        IDatabaseModel _databaseModel = null;
        //DominicanFeasts _dominicanFeasts = null;
        //IList<DominicanFeasts.FeastDetails> _feasts = null;
        IList<FeastInfoObject> _feastObjects = null;
        TimeSpan _oneDay = new TimeSpan(1, 0, 0, 0);
        string _dateFormat = "dd/MM/yyyy";

        //private static readonly FeastsModel _instance = new FeastsModel();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        //static FeastsModel()
        //{
        //}

        public FeastsModel()
        {
            _databaseModel = FreshIOC.Container.Resolve<IDatabaseModel>() as DatabaseModel;
        }

        //public static FeastsModel Instance
        //{
        //    get
        //    {
        //        return _instance;
        //    }
        //}

        // FeastsForYear = databaseModel.GetFeastsForYear(year);
        // if (FeastsForYear.Count == 0)
        //      Feasts = databaseModel.GetFeasts()
        //      if (Feasts.Count == 0)
        //          FeastDetails = GetDominicanFeasts
        //          databaseModel.GetFeasts(FeastDetails)


        public FeastDate GetFeastForDate(DateTime prayerDate)
        {
            // NOTE: this needs making WAY more efficient
            Debug.WriteLine($"{_Tag}.GetFeastForDate( {prayerDate.ToString()} )");
            IList<FeastDate> feastsForYear = GetFeastsForYear(prayerDate.Year.ToString());

            var feasts4Date = feastsForYear.Where<FeastDate>(d => d.date.Date == prayerDate.Date);      //.FirstOrDefault<FeastDate>();
            foreach (FeastDate feast in feasts4Date)
            {
                if (prayerDate.Hour < 12)
                {
                    if (feast.filename.Contains("morning"))
                        return feast;
                } else
                {
                    if (feast.filename.Contains("evening"))
                        return feast;
                }
            }

            return null;
        }


        public IList<FeastDate> GetFeastsForYear(string year)
        {
            IList<FeastDate> feastsForYear = _databaseModel.GetFeastsForYear(year);
            if (feastsForYear.Count == 0)
            {
                IList<FeastInfoObject> feasts = _databaseModel.GetFeasts();
                if (feasts.Count == 0)
                {
                    feasts = _databaseModel.SetFeasts(GetDominicanFeasts());
                }

                DominicanCalender dominicanCalender = FreshIOC.Container.Resolve<IDominicanCalender>() as DominicanCalender;
                DominicanSeason dominicanSeason = dominicanCalender.GetDominicanSeasonForYear(year);

                IList<FeastDate> feastDates = CreateFeastsForYear(year, dominicanSeason);

                feastsForYear = _databaseModel.GetFeastsForYear(year);

                if (_test)
                {
                    Debug.WriteLine($"{_Tag}.GetFeastForYear( {year} )");
                    foreach (FeastDate feastDate in feastsForYear)
                    {
                        Debug.WriteLine(feastDate.ToString());
                    }
                    Debug.WriteLine($"{_Tag}.GetFeastForYear( Fin )");
                }
            }

            return feastsForYear;
        }


        /*There are three ranks of feast in Catholic Church.They are in order Solemnity, Feast and Memorial.

            * Solemnity has three types: solemnity of the Lord, solemnity of Our Lady and solemnity of other saints.
            * Solemnities of the Lord have precedence over those of Our Lady and those of Our Lady have precedence over those of other saints.
            * The solemnities of the Lord will replace the celebration of a Sunday in Ordinary Time. 
            * The solemnities of Our Lady and the ones of other saints will be transferred to the next free day if it falls on a Sunday, 
                except the following solemnities: 
	        - Blessed Virgin Mary Mother of God (01/01)
	        - Assumption of Our Lady(15/08)
	        - Nativity of St John the Baptism(24/06).
	        - Sts Peter and Paul(29/06)
	        * All feasts or memorial is transferred to the next free day if it falls on a Sunday or a same day of a higher ranking feast.
	        * There are two types of memorial: normal and optional. An optional memorial is celebrated by the faithful's choice.
	
        - The Sunday between 02/01 and 08/01 is the Solemnity of Epiphany of the Lord.
        - The Sunday after 06/01 is the Feast of Baptism of the Lord.
        - The Sunday between 25/12 and 31/12 is the Feast of Holy Family.If there is no Sunday in that period then Holy Family is on 31/12.

        There are 4 solemnities for Lord in Ordinary Time period.
	        -- Trinity Sunday: Sunday after Pentecost (17 May to 20 June).
	        -- Most Holy Body and Blood of Christ or Corpus Christi: Sunday after Trinity Sunday
	        -- Most Sacred Heart of Jesus: Friday after Corpus Christi.
	        -- Our Lord Jesus Christ, King of the Universe: 34th Sunday of Ordinary Time.
        */
        public IList<FeastDate> CreateFeastsForYear(string year, DominicanSeason dominicanSeason)
        {
            //GetDominicanFeasts();

            IList<FeastDate> feasts = new List<FeastDate>();
            _feastObjects = _databaseModel.GetFeasts();

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            feastDateObjects.AddRange(CreateFixedFeasts(year, feasts));
            feastDateObjects.AddRange(CreateMovingFeasts(year, feasts, dominicanSeason));

            _databaseModel.SetFeastsForYear(feastDateObjects);

            // debug check that its all worked
            //Debug.WriteLine($"CreateFeastsForYear({year}) - {_databaseModel.FeastsForYearToString(year)}\nFin");

            return feasts;
        }

        List<FeastDateObject> CreateFixedFeasts(string year, IList<FeastDate> feasts)
        {
            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();

            feastDateObjects.AddRange(CreateFixedSolemnities(year));
            feastDateObjects.AddRange(CreateSolemnities(year));
            feastDateObjects.AddRange(CreateMemorials(year));
            feastDateObjects.AddRange(CreateOptionalMemorials(year));
            feastDateObjects.AddRange(CreateFeasts(year));

            return feastDateObjects;
        }

        List<FeastDateObject> CreateFixedSolemnities(string year)
        {
            Debug.WriteLine(_Tag + "CreateFixedSolemnities( " + year + " )");
            // Blessed Virgin Mary Mother of God(01 / 01)
            // Assumption of Our Lady(15 / 08)
            // Nativity of St John the Baptism(24 / 06).
            // Sts Peter and Paul(29 / 06)
            // solemnityType == "Fixed Solemnity"

            IEnumerable<FeastInfoObject> fixedFeasts = _feastObjects.Where<FeastInfoObject>(d => d.SolemnityType == "Fixed Solemnity");

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            foreach (FeastInfoObject fo in fixedFeasts)
            {
                //Debug.WriteLine($"fixedFeast: {fo.ToString()}");
                string filename = fo.GetFilename();
                DateTime date = DateTime.ParseExact(filename.Replace("-", @"/") + @"/" + year, _dateFormat, CultureInfo.InvariantCulture);
                feastDateObjects.AddRange(SetFeastDates(date, year, filename, fo.PrevEvening));
            }

            //Debug.WriteLine("FeastDateObjects:");
            //foreach (FeastDateObject fdo in feastDateObjects)
            //{
            //    Debug.WriteLine(fdo.ToString());
            //}

            return feastDateObjects;
        }

        List<FeastDateObject> CreateSolemnities(string year)
        {
            // solemnityType == "Solemnity"

            IEnumerable<FeastInfoObject> feasts = _feastObjects.Where<FeastInfoObject>(d => d.SolemnityType == "Solemnity");

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            foreach (FeastInfoObject feastObject in feasts)
            {
                string filename = feastObject.GetFilename();
                DateTime date = DateTime.ParseExact(filename.Replace("-", @"/") + @"/" + year, _dateFormat, CultureInfo.InvariantCulture);
                feastDateObjects.AddRange(SetFeastDates(date, year, filename, true));
            }

            return feastDateObjects;
        }

        List<FeastDateObject> CreateMemorials(string year)
        {
            // solemnityType == "Memorial"

            IEnumerable<FeastInfoObject> feasts = _feastObjects.Where<FeastInfoObject>(d => d.SolemnityType == "Memorial");

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            foreach (FeastInfoObject feastObject in feasts)
            {
                string filename = feastObject.Filename;
                DateTime date = DateTime.ParseExact(feastObject.Filename.Replace("-", @"/") + @"/" + year, _dateFormat, CultureInfo.InvariantCulture);
                feastDateObjects.AddRange(SetFeastDates(date, year, filename, true));
            }

            return feastDateObjects;
        }

        List<FeastDateObject> CreateOptionalMemorials(string year)
        {
            // solemnityType == "optional Memorial"

            IEnumerable<FeastInfoObject> feasts = _feastObjects.Where<FeastInfoObject>(d => d.SolemnityType == "optional Memorial");

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            foreach (FeastInfoObject feastObject in feasts)
            {
                string filename = feastObject.Filename;
                DateTime date = DateTime.ParseExact(feastObject.Filename.Replace("-", @"/") + @"/" + year, _dateFormat, CultureInfo.InvariantCulture);
                feastDateObjects.AddRange(SetFeastDates(date, year, filename, true));
            }

            return feastDateObjects;
        }

        List<FeastDateObject> CreateFeasts(string year)
        {
            // solemnityType == "Feast"

            IEnumerable<FeastInfoObject> feasts = _feastObjects.Where<FeastInfoObject>(d => d.SolemnityType == "Feast");

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            foreach (FeastInfoObject feastObject in feasts)
            {
                string filename = feastObject.GetFilename();
                DateTime date = DateTime.ParseExact(filename.Replace("-", @"/") + @"/" + year, _dateFormat, CultureInfo.InvariantCulture);
                feastDateObjects.AddRange(SetFeastDates(date, year, filename, feastObject.PrevEvening));
            }

            return feastDateObjects;
        }

        List<FeastDateObject> CreateMovingFeasts(string year, IList<FeastDate> feasts, DominicanSeason dominicanSeason)
        {
            /*
            epiphany                - Sunday between 02 / 01 and 08 / 01
            baptism _of_the_lord    - Sunday after 06 / 01. 
                If Epiphany already occupy that Sunday then it will be celebrated on the first Monday after that Sunday.
            ascension               - 7th Sunday of Easter
            pentecost               - Nine days after Ascension on a Sunday.
            trinity                 - First Sunday after Pentecost
            Corpus Christi          - Thursday after Trinity. However it is usually celebrated on the second Sunday after Pentecost.
            sacred_heart_jesus      - Friday following second Sunday after Pentecost
            Jesus_the_king          - last Sunday in Ordinary Time.
            holy_family             - Sunday between Christmas Day and New Year Day if there is. Otherwise 30 / 12.
            sacred_heart_mary       - 9 days after Corpus Christi
            */

            DateTime pentecostDate;
            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();

            int yr = Int32.Parse(year);

            // get epiphany dates, and add to the list
            IList<FeastDateObject> epiphanyDates = GetEpiphanyDates(year, yr);
            feastDateObjects.AddRange(epiphanyDates);

            feastDateObjects.AddRange(GetBotLDates(year, yr, epiphanyDates));                 // baptism _of_the_lord
            IList<FeastDateObject> ascensionDates = GetAscensionDates(year, yr, dominicanSeason);
            feastDateObjects.AddRange(ascensionDates);
            feastDateObjects.AddRange(GetPentecostDates(year, yr, ascensionDates, out pentecostDate));
            feastDateObjects.AddRange(GetTrinityCorpusChristiDates(year, yr, pentecostDate));
            feastDateObjects.AddRange(GetSacredHeartJesusDates(year, yr, pentecostDate));
            feastDateObjects.AddRange(GetJesusTheKingDates(year, yr, dominicanSeason[DominicanSeasons.NextAdvent]));
            feastDateObjects.AddRange(GetHolyFamilyDates(year, yr));

            return feastDateObjects;
        }

        List<FeastDateObject> GetEpiphanyDates(string year, int yr)
        {
            // Sunday between 02 / 01 and 08 / 01
            DateTime startDate = new DateTime(yr, 1, 2);
            DateTime endDate = new DateTime(yr, 1, 8);
            DateTime epiphanyDate = GetDateOfDayAfterDate(startDate, endDate, DayOfWeek.Sunday);
            List<FeastDateObject> feastDates = SetFeastDates(epiphanyDate, year, "epiphany", true);

            return feastDates;
        }

        List<FeastDateObject> GetBotLDates(string year, int yr, IList<FeastDateObject> epiphanyDates)
        {
            // Sunday after 06 / 01
            DateTime startDate = new DateTime(yr, 1, 6);
            DateTime endDate = new DateTime(yr, 1, 14);
            DateTime botlDate = GetDateOfDayAfterDate(startDate, endDate, DayOfWeek.Sunday);

            // If Epiphany already occupy that Sunday then it will be celebrated on the first Monday after that Sunday.
            if (botlDate.Day <= 8)
            {
                // if botlDate within epiphany range, then move it forward 1 day yo the MOnday
                botlDate = botlDate.Add(_oneDay);
            }

            // if BotL falls on a Monday, then we can't have its evening1 falling on the prev Sunday, as that will b Epiphany
            List<FeastDateObject> feastDates = SetFeastDates(botlDate, year, "baptism_of_the_lord", (botlDate.DayOfWeek!=DayOfWeek.Monday));

            return feastDates;
        }

        List<FeastDateObject> GetAscensionDates(string year, int yr, DominicanSeason dominicanSeason)
        {
            // 7th Sunday of Easter
            DateTime easterDate = dominicanSeason[DominicanSeasons.Easter];
            DateTime ascensionDate = easterDate.AddDays(42.0D);         // add 7 weeks to easter  ????
            if (ascensionDate.DayOfWeek != DayOfWeek.Sunday)
            {
                Debug.WriteLine($"{_Tag}.GetAscensionDates: calculated date NOT a Sunday!");
            }

            List<FeastDateObject> feastDates = SetFeastDates(ascensionDate, year, "ascension", true);

            return feastDates;
        }

        List<FeastDateObject> GetPentecostDates(string year, int yr, IList<FeastDateObject> ascensionDates, out DateTime pentecostDate)
        {
            // Nine days after Ascension on a Sunday.
            if (ascensionDates.Count != 2)
            {
                Debug.WriteLine($"{_Tag}.GetPentecostDates: ascension date NOT a Sunday!");
            }

            FeastDateObject ascensionObject = ascensionDates.ElementAt<FeastDateObject>(ascensionDates.Count-1);
            DateTime ascensionDate = DateTime.ParseExact(ascensionObject.DayMonthYear, _dateFormat, CultureInfo.InvariantCulture);
            pentecostDate = ascensionDate.AddDays(9);
            List<FeastDateObject> feastDates = SetFeastDates(pentecostDate, year, "pentecost", true);

            return feastDates;
        }

        List<FeastDateObject> GetTrinityCorpusChristiDates(string year, int yr, DateTime pentecostDate)
        {
            // First Sunday after Pentecost
            DateTime trinityDate = GetDateOfDayAfterDate(pentecostDate, pentecostDate, DayOfWeek.Sunday);
            List<FeastDateObject> feastDates = SetFeastDates(trinityDate, year, "trinity", true);

            // ChristiDate - Thursday after Trinity.
            // However it is usually celebrated on the second Sunday after Pentecost.
            DateTime corpusChristiDate = GetDateOfDayAfterDate(trinityDate.AddDays(1), pentecostDate, DayOfWeek.Sunday);
            List < FeastDateObject > feastDates2 = SetFeastDates(corpusChristiDate, year, "corpus_christi", true);
            feastDates = feastDates.Concat<FeastDateObject>(feastDates2).ToList<FeastDateObject>();

            // sacred_heart_mary - 9 days after Corpus Christi
            //feastDates2 = SetFeastDates(corpusChristiDate.AddDays(9), year, "sacred_heart_mary", false);
            string dayMth = corpusChristiDate.AddDays(9).ToString(@"dd/MM").Replace('-', '/'); 

            // sacred_heart_mary is ONLY in the morning, the evening prayer is 
            feastDates.Add(new FeastDateObject(dayMth + @"/" + year, "sacred_heart_mary_morning.txt", "sacred_heart_mary", year, "am", ""));
            //public FeastDateObject(string dmy, string filename, string feastKey, string year, string amPm, string eveningId)


            return feastDates;
        }

        List<FeastDateObject> GetSacredHeartJesusDates(string year, int yr, DateTime pentecost)
        {
            // Friday following second Sunday after Pentecost
            DateTime date1 = GetDateOfDayAfterDate(pentecost.AddDays(1), pentecost, DayOfWeek.Sunday);  // get first sunday
            DateTime sacredHeartJesusDate = date1.AddDays(12);                                          // get next sunday+friday

            List<FeastDateObject> feastDates = SetFeastDates(sacredHeartJesusDate, year, "sacred_heart_jesus", true);

            return feastDates;
        }

        List<FeastDateObject> GetJesusTheKingDates(string year, int yr, DateTime advent)
        {
            // last Sunday in Ordinary Time (ashWed is end of ord time 1)
            DateTime date = new DateTime();
            for (int i = 1; i < 10; i++)
            {
                date = advent.AddDays(i * -1);
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    break;
            }
            List<FeastDateObject> feastDates = SetFeastDates(date, year, "jesus_the_king", true);

            return feastDates;
        }

        List<FeastDateObject> GetHolyFamilyDates(string year, int yr)
        {
            // Sunday between Christmas Day and New Year Day if there is. Otherwise 30 / 12.
            DateTime boxingDay = DateTime.ParseExact(@"26/12/" + year, _dateFormat, CultureInfo.InvariantCulture);
            DateTime newYear = DateTime.ParseExact($"01/01/{yr+1}", _dateFormat, CultureInfo.InvariantCulture);
            DateTime holyFamilyDate = GetDateOfDayAfterDate(boxingDay, newYear, DayOfWeek.Sunday);      // get sunday between
            if (holyFamilyDate < boxingDay)
            {   // no Sunday between, so just use 31/12
                holyFamilyDate = DateTime.ParseExact(@"30/12/" + year, _dateFormat, CultureInfo.InvariantCulture);
            }

            List<FeastDateObject> feastDates = SetFeastDates(holyFamilyDate, year, "holy_family", true);

            return feastDates;
        }

        DateTime GetDateOfDayAfterDate(DateTime startDate, DateTime endDate, DayOfWeek dayOfWeek)
        {
            // find date after startDate where date.day == dayOfWeek
            int numDays = (endDate > startDate) ? (int)endDate.Subtract(startDate).TotalDays+1 : 7;
            for (int i = 0; i < numDays; i++)
            {
                DateTime date = startDate.AddDays(i);
                if (date.DayOfWeek == dayOfWeek)
                    return date;
            }
            return new DateTime(1,1,1);
        }

        List<FeastDateObject> SetFeastDates(DateTime feastDate, string year, string filename, bool prevEvening)
        {
            //Debug.WriteLine($"{_Tag}.SetFeastDates({feastDate.ToString()}, {year}, {filename}, {prevEvening.ToString()})");

            List<FeastDateObject> feastDateObjects = new List<FeastDateObject>();
            string dayMth = feastDate.ToString(@"dd/MM").Replace('-','/');

            // do the morning first
            feastDateObjects.Add(new FeastDateObject(dayMth + @"/" + year, filename+"_morning.txt", filename, year, "am", ""));

            if (prevEvening)
            {
                // need an evening1 (prev day), and an evening2 (curr day)
                feastDateObjects.Add(new FeastDateObject(dayMth + @"/" + year, filename + "_evening2.txt", filename, year, "pm", "2"));
                DateTime prevDay = feastDate.AddDays(-1);
                string dayMth2 = prevDay.ToString(@"dd/MM").Replace('-', '/');
                feastDateObjects.Add(new FeastDateObject(dayMth2 + @"/" + year, filename + "_evening1.txt", filename, year, "pm", "1"));
            }
            else
            {
                feastDateObjects.Add(new FeastDateObject(dayMth + @"/" + year, filename + "_evening.txt", filename, year, "pm", ""));
            }

            //foreach (FeastDateObject fdo in feastDateObjects)
            //{
            //    Debug.WriteLine($"adding {fdo.ToString()})");
            //}

            return feastDateObjects;
        }

        DominicanFeasts GetDominicanFeasts()
        {
            //Debug.WriteLine(_Tag + ".ParseJsonDominicanFeasts()");

            DominicanFeasts dominicanFeasts = null;
            try
            {
                var assembly = typeof(PrayerPageModel).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("DailyPrayer.Data.Feasts.json"))
                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        dominicanFeasts = serializer.Deserialize<DominicanFeasts>(reader);
                    }
                    //Debug.WriteLine("Finished loading ParseJsonDominicanFeasts");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(_Tag + ".ParseJsonDominicanFeasts()( {0} ) - error\n{1}\n", ex.Message, ex.StackTrace);
            }
            //Debug.WriteLine(_Tag + ".ParseJsonDominicanFeasts() - {0}\n", dominicanFeasts.ToString());

            return dominicanFeasts;
        }
    }
}
