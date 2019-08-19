using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

using DailyPrayer.Models.PrayerSeason;

using FreshMvvm;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer.Models
{
    public interface IPrayerModel
    {
        SortedSet<string> NotFounds { get; set; }
        string MakePrayer(DateTime date, bool testMode, bool headingsOnly);
        string MakePrayerSection(DateTime date, PrayerSeason.PrayerSeason.PrayerSect prayerSect);
    }

    public class PrayerModel : IPrayerModel
    {
        string _Tag = "PrayerModel";
        DominicanCalender _dominicanCalender = null;

        private SortedSet<string> _NotFounds = new SortedSet<string>();
        public SortedSet<string> NotFounds
        {
            get {return _NotFounds; }
            set { _NotFounds = value; }
        }

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

        public string Filenames { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }

        public PrayerModel()
        {
            _dominicanCalender = FreshIOC.Container.Resolve<IDominicanCalender>() as DominicanCalender;
        }

        public string MakePrayer(DateTime date, bool testMode, bool headingOnly=false)
        {
            Debug.WriteLine("\nPrayerModel.MakePrayer() - " + date.ToString());

            NotFounds.Clear();
            Place place = _dominicanCalender.FindPlace(date);
            PrayerSeason.PrayerSeason prayerSeason = PrayerSeasonFactory.CreatePrayerSeason(place, testMode);

            PrayerHeading = MakeHeading(place, date, prayerSeason);

            string prayerHtml = (headingOnly) ? "" : MakePrayer(date, prayerSeason, testMode);

            if (testMode)
            {
                prayerHtml = "<p/><b><big>Prayer for " + date.ToString("yyyy/MM/dd tt") + "</big><br/>" +
                    PrayerHeading.Replace("\r\n", "<br/>") + "</b><p/>" + prayerHtml;

                Debug.WriteLine("\n\n"+prayerHtml+ "\n\n");
            }
            return prayerHtml;
        }

        private string MakePrayer(DateTime date, PrayerSeason.PrayerSeason prayerSeason, bool testMode)
        {

            string htmlText = "";
            try
            {
                FileDetails fileDetails = new FileDetails();
                fileDetails.Add(prayerSeason.LoadText(PrayerSeason.PrayerSeason.PrayerSect.AllSections));

                if (testMode)
                {
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

        public string MakePrayerSection(DateTime date, PrayerSeason.PrayerSeason.PrayerSect prayerSect)
        {
            //Debug.WriteLine($"{_Tag}.MakeTestPrayer( date.ToString(_dateFormat) )");
            Place place = _dominicanCalender.FindPlace(date);
            PrayerSeason.PrayerSeason prayerSeason = PrayerSeasonFactory.CreatePrayerSeason(place, true);

            string prayerDate = MakeHeading(place, date, prayerSeason);
            string dateString = date.ToString("u").Replace(":00Z", "");
            string htmlText = $"<b>Date: {dateString} </b><br/>";
            htmlText += prayerSeason.ToString() + "<br/>";
            htmlText += prayerDate + "<p/>";

            FileDetails fileDetails = new FileDetails();
            fileDetails.Add(prayerSeason.LoadText(prayerSect));
            htmlText += fileDetails.PrayerText;

            return htmlText;
        }

        private string MakeHeading(Place place, DateTime date, PrayerSeason.PrayerSeason prayerSeason)
        {
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

            string prayerHeading = prayerSeason.VietnameseName(place, satSunExtra);

            prayerHeading += date.ToString("\r\nyyyy/MM/dd tt");
            if (App.SmallScreen && prayerHeading.Length >= 65)
                prayerHeading = prayerHeading.Replace("\r\n", ", ");

            Debug.WriteLine($"{_Tag}.MakeHeading: PrayerSeason = {date.ToString()}, {prayerSeason.ToString()}");

            return prayerHeading;
        }
    }
}
