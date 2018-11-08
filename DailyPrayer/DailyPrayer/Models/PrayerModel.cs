using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using DailyPrayer.Models.PrayerSeason;

using FreshMvvm;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer.Models
{
    class PrayerModel
    {
        string _Tag = "PrayerModel";
        bool _testMode = false;
        DominicanCalender _dominicanCalender = null;
        public static SortedSet<string> NotFounds = new SortedSet<string>();

        private static readonly PrayerModel _instance = new PrayerModel();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static PrayerModel()
        {
        }

        private PrayerModel()
        {
            _dominicanCalender = FreshIOC.Container.Resolve<IDominicanCalender>() as DominicanCalender;
        }

        public static PrayerModel Instance
        {
            get
            {
                return _instance;
            }
        }

        public SeasonDefn ParseJsonSeasonsDefn()
        {
            //Debug.WriteLine(_Tag+".ParseJsonSeasonsDefn()");
            SeasonDefn seasons = null;

            try
            {
                var assembly = typeof(PrayerPageModel).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("DailyPrayer.Data.SeasonsDefn.json"))
                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        seasons = serializer.Deserialize<SeasonDefn>(reader);
                    }
                    //Debug.WriteLine("Finished loading ParseJsonSeasonsDefn");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("{2}.ParseJsonWorkflow()( {0} ) - error\n{1}\n", ex.Message, ex.StackTrace, _Tag);
            }

            return seasons;
        }

        public string MakeDate(DateTime date, PrayerSeason.PrayerSeason prayerSeason)
        {
            Place place = _dominicanCalender.FindPlace(date);

            //PrayerSeason.PrayerSeason*/ prayerSeason = PrayerSeasonFactory.CreatePrayerSeason(place, _testMode);

            string satSunExtra = "";
            if (!place.Morning)
            {
                // feasts often start on the evening of the day before
                if (place.DomSeason == DominicanSeasons.Feasts)
                {
                    if (place.Filename.EndsWith("1.txt"))
                        satSunExtra = " 1";
                    else if (place.Filename.EndsWith("2.txt"))
                        satSunExtra = " 2";
                }
                else
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        DateTime date2 = date.AddDays(1.0);
                        place = _dominicanCalender.FindPlace(date2);
                        satSunExtra = " 1";
                    }
                    else if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        satSunExtra = " 2";
                    }
                }
            }

            string prayerDate = prayerSeason.VietnameseName(place, satSunExtra);
            //string prayerDate = prayerSeason.EnglishName(place, satSunExtra);
            Debug.WriteLine($"{_Tag}.MakeDate: PrayerSeason = {date.ToString()}, {prayerSeason.ToString()}");

            return prayerDate;
        }

        public string MakePrayer(DateTime date, PrayerSeason.PrayerSeason prayerSeason, bool filenamesOnly)
        {
            //Debug.WriteLine($"{_Tag}.MakePrayer( {date.ToString()} )");

            if (date.Year == 2016 && date.Month == 1 && date.Day == 1)
                _testMode = !_testMode;

            string htmlText = "";
            try
            {
                FileDetails fileDetails = new FileDetails();
                fileDetails.Add(prayerSeason.LoadText(PrayerSeason.PrayerSeason.PrayerSect.Ignore));

                if (filenamesOnly)
                {
                    //htmlText = prayerSeason.ToString() + "<p>";
                    htmlText += fileDetails.Filenames() + "<p>";
                }

                htmlText += fileDetails.PrayerText;
                htmlText = htmlText.Replace("<p/> <br/>", "<p/>");
                htmlText = htmlText.Replace("<p/><br/>", "<p/>");
                htmlText = htmlText.Replace("<p/><br>", "<p/>");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{_Tag}.MakePrayer: error - {ex.ToString()}");
            }

            return htmlText;
        }

        public string MakeTestPrayers(DateTime startDate, DateTime endDate, PrayerSeason.PrayerSeason.PrayerSect prayerSect, bool filenamesOnly)
        {
            Debug.WriteLine($"PrayerModel.MakeTestPrayer( {startDate.ToString()}, {endDate.ToString()}, {prayerSect.ToString()} )");

            NotFounds.Clear();
            string htmlText = "";
            DateTime date = startDate;
            try
            {
                while (date <= endDate)
                {
                    htmlText += MakeTestPrayer(date, prayerSect, filenamesOnly);

                    date = date.AddHours(12);
                }
            }
            catch (Exception ex)
            {
                htmlText += string.Format($"<p>Error : {ex.Message}<p>on {date.ToString()}");
            }
            //htmlText += "</body>/n</html>";

            if (NotFounds.Count > 0)
            {
                string notFounds = "<big>Not founds:</big><br/>";
                foreach (string filename in NotFounds)
                {
                    notFounds += filename + "<br/>";
                }
                htmlText = notFounds + "<p/>" + htmlText;
            }

            return htmlText;
        }

        public string MakeTestPrayers(IList<DateTime> dates)                //PrayerSeason.PrayerSeason.PrayerSect prayerSect)
        {
            Debug.WriteLine($"{_Tag}.MakeTestPrayers( {dates.Count} dates )");

            string htmlText = "";
            foreach (DateTime date in dates)
            {
                try
                {
                    htmlText += MakeTestPrayer(date, PrayerSeason.PrayerSeason.PrayerSect.Ignore, true);
                }
                catch (Exception ex)
                {
                    htmlText += string.Format($"<p>Error : {ex.Message}<p>on {date.ToString()}");
                }
            }

            if (NotFounds.Count > 0)
            {
                string notFounds = "<big>Not founds:</big><br/>";
                foreach (string filename in NotFounds)
                {
                    notFounds += filename + "<br/>";
                }
                htmlText = notFounds + "<p/>" + htmlText;
            }

            return htmlText;
        }

        private string MakeTestPrayer(DateTime date, PrayerSeason.PrayerSeason.PrayerSect prayerSect, bool filenamesOnly)
        {
            //Debug.WriteLine($"{_Tag}.MakeTestPrayer( date.ToString(_dateFormat) )");
            Place place = _dominicanCalender.FindPlace(date);
            PrayerSeason.PrayerSeason prayerSeason = PrayerSeasonFactory.CreatePrayerSeason(place, _testMode);

            string prayerDate = MakeDate(date, prayerSeason);
            string dateString = date.ToString("u").Replace(":00Z", "");
            string htmlText = $"<b>Date: {dateString} </b><br/>";
            htmlText += prayerSeason.ToString() + "<br/>";
            htmlText += prayerDate + "<p/>";

            if (prayerSect == PrayerSeason.PrayerSeason.PrayerSect.Ignore)
            {
                string prayerHtml = MakePrayer(date, prayerSeason, filenamesOnly);
                htmlText += prayerHtml;
            }
            else
            {
                FileDetails fileDetails = new FileDetails();
                fileDetails.Add(prayerSeason.LoadText(prayerSect));
                htmlText += fileDetails.PrayerText;
            }

            return htmlText;
        }
    }
}
