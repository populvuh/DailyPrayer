using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DailyPrayer;
using DailyPrayer.Models;
using DailyPrayer.Models.PrayerSeason;

using FreshMvvm;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Debug = System.Diagnostics.Debug;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        #region Declarations
        string _desktopFolder;
        const string _dateTimeFormat = "dd/MM/yyyy HH:mm ";
        const string _htmlStart = "<html>\n" +
    "<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">\n" +
    "<META HTTP-EQUIV=\"Content-language\" CONTENT=\"vi\">\n" +
    "<body style=\"background-color:#FFFFFF;color:#007AFF;font-size:16px\">\n";
        const string _htmlEnd = "\n</body>\n</html>\n";
        #endregion Declarations


        #region Initialisation
        [TestInitialize]
        public void SetIOC()
        {
            try
            {
                FreshIOC.Container.Register<IPrayerModel, PrayerModel>();
                FreshIOC.Container.Register<IPrayerPageModel, PrayerPageModel>();
                FreshIOC.Container.Register<IDatabaseModel, DatabaseModel>();
                FreshIOC.Container.Register<IFeastsModel, FeastsModel>();
                FreshIOC.Container.Register<IDominicanCalender, DominicanCalender>();

                PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;


                _desktopFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "DailyPrayers");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetIOC exception: {ex.Message}\n{ex.StackTrace}");
                WriteErrorFile("SetIOC", ex);
            }
        }
        #endregion Initialisation


        #region Places
        [TestMethod, Ignore]
        public void GeneratePlaces()
        {
            //GeneratePlacesForDates(new DateTime(2018, 6, 1, 6, 0, 0), new DateTime(2019, 6, 1, 6, 0, 0));
            //GeneratePlacesForDates(new DateTime(2019, 6, 1, 6, 0, 0), new DateTime(2020, 6, 1, 6, 0, 0));
            GeneratePlacesForDates(new DateTime(2022, 6, 1, 6, 0, 0), new DateTime(2023, 6, 1, 6, 0, 0));
        }

        private void GeneratePlacesForDates(DateTime startDate, DateTime endDate)
        {
            try
            {
                string htmlText = "";
                DominicanCalender dominicanCalender = FreshIOC.Container.Resolve<IDominicanCalender>() as DominicanCalender;
                DateTime date = startDate;
                while (date <= endDate)
                {
                    Place place = dominicanCalender.FindPlace(date);
                    htmlText += "<br/>" + date.ToString(_dateTimeFormat) + "&nbsp- &nbsp;" + place.ToString() + "&nbsp- &nbsp;" + date.ToString("ddd");

                    date = date.AddHours(12);
                }

                WriteFile("Places", startDate, endDate, htmlText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePlaces exception: {ex.StackTrace}");
                WriteErrorFile("GeneratePlaces", ex);
            }
        }
        #endregion Places


        //#region Filenames
        //[TestMethod, Ignore]
        //public void GenerateFilenames()
        //{
        //    try
        //    {
        //        DateTime startDate = new DateTime(2018, 12, 1, 6, 0, 0);
        //        DateTime endDate = new DateTime(2019, 1, 1, 18, 0, 0);
        //        string htmlText = PrayerModel.Instance.MakePrayers(startDate, endDate, PrayerSeason.PrayerSect.AllSections, true);

        //        WriteFile("Filenames", startDate, endDate, htmlText);
        //    }
        //    catch (Exception ex)
        //    {                                                                               
        //        Debug.WriteLine($"GenerateFilenames exception: {ex.StackTrace}");
        //        WriteErrorFile("GenerateFilenames", ex);
        //    }
        //}
        //#endregion Filenames


        #region Prayers
        [TestMethod, Ignore]
        public void GeneratePrayers()
        {
            try
            {
                int startYear = 2019;
                DateTime startDate = new DateTime(startYear, 1, 1, 6, 0, 0);
                PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;

                for (int i = 1; i <= 2; i++)                   // years
                {
                    for (int j = 1; j <= 12; j++)               // months
                    {
                        DateTime endDate = startDate.AddMonths(1);
                        string htmlText = MakePrayers(prayerModel, startDate, endDate, PrayerSeason.PrayerSect.AllSections);

                        WriteFile("Prayers", startDate, endDate, htmlText);
                        startDate = endDate;
                    }
                    startDate = new DateTime(startYear + i, 1, 1, 6, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
                WriteErrorFile("GeneratePrayers", ex);
            }
        }


        [TestMethod]
        public void GeneratePrayers2()
        {
            try
            {
                PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;

                DateTime startDate = new DateTime(2018, 12, 15, 6, 0, 0);
                DateTime endDate = new DateTime(2019, 1, 31, 18, 0, 0);
                string htmlText = MakePrayers(prayerModel, startDate, endDate, PrayerSeason.PrayerSect.AllSections);

                WriteFile("APrayers", startDate, endDate, htmlText);

                startDate = new DateTime(2019, 12, 15, 6, 0, 0);
                endDate = new DateTime(2020, 1, 31, 18, 0, 0);
                htmlText = MakePrayers(prayerModel, startDate, endDate, PrayerSeason.PrayerSect.AllSections);
                WriteFile("APrayers", startDate, endDate, htmlText);

                startDate = new DateTime(2020, 12, 15, 6, 0, 0);
                endDate = new DateTime(2021, 1, 31, 18, 0, 0);
                htmlText = MakePrayers(prayerModel, startDate, endDate, PrayerSeason.PrayerSect.AllSections);
                WriteFile("APrayers", startDate, endDate, htmlText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.Message}\n{ex.StackTrace}");
                WriteErrorFile("GeneratePrayers2", ex);
            }
        }

        public string MakePrayers(PrayerModel prayerModel, DateTime startDate, DateTime endDate, PrayerSeason.PrayerSect prayerSect)
        {
            Debug.WriteLine($"PrayerModel.MakeTestPrayer( {startDate.ToString()}, {endDate.ToString()}, {prayerSect.ToString()} )");

            string htmlText = "";
            DateTime date = startDate;
            SortedSet<string> NotFounds = new SortedSet<string>();
            try
            {
                while (date <= endDate)
                {
                    htmlText += (prayerSect == PrayerSeason.PrayerSect.AllSections) ?
                        prayerModel.MakePrayer(date, true) :
                        prayerModel.MakePrayerSection(date, prayerSect);
                    if (prayerModel.NotFounds.Count > 0)
                        NotFounds.UnionWith(prayerModel.NotFounds);

                    date = date.AddHours(12);
                }
            }
            catch (Exception ex)
            {
                htmlText += string.Format($"<p>Error : {ex.Message}<p>on {date.ToString()}");
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


        #endregion Prayers


        #region Feasts
        [TestMethod, Ignore]
        public void GenerateFeastDates()
        {
            try
            {
                string htmlText = GenerateFeastDates("2018");
                WriteFile(CreateFilename("FeastDates2018"), htmlText);
                htmlText = GenerateFeastDates("2019");
                WriteFile(CreateFilename("FeastDates2019"), htmlText);
                htmlText = GenerateFeastDates("2020");
                WriteFile(CreateFilename("FeastDates2020"), htmlText);

                htmlText = GenerateFeastDates("");
                WriteFile(CreateFilename("FeastDates2018-2020"), htmlText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GenerateFeastDates exception: {ex.StackTrace}");
                WriteErrorFile("GenerateFeastDates", ex);
            }
        }

        public string GenerateFeastDates(string year)       //, PrayerSeason.PrayerSect prayerSect)
        {
            //List<DateTime> feastDates = new List<DateTime>();
            FeastsModel feastsModel = FreshIOC.Container.Resolve<IFeastsModel>() as FeastsModel;
            feastsModel.GetFeastsForYear(year);

            DatabaseModel databaseModel = FreshIOC.Container.Resolve<IDatabaseModel>() as DatabaseModel;
            IList<FeastDate> feastObjects = databaseModel.GetFeastsForYear(year).ToList();              // FeastObject
            Debug.WriteLine($"feastObjects count = {feastObjects.Count}");

            // list feasts by type/date
            string prayerHtml = "";
            List<FeastDate> feasts = null;
            string[] feastTypes = { "Solemnity", "Fixed Solemnity", "Feast", "MovingFeast", "Memorial", "optional Memorial" };
            foreach (string solemnityType in feastTypes)
            {
                feasts = feastObjects.Where<FeastDate>(d => d.feast.SolemnityType == solemnityType).ToList<FeastDate>();

                prayerHtml += ToHtmlText(solemnityType, feasts);
            }


            // now list all feasts by date
            feasts = feastObjects.OrderBy(d => d.date).OrderBy(d => d.am).ToList<FeastDate>();
            prayerHtml += ToHtmlText("All", feasts);

            if (databaseModel.WriteFailures.Count > 0)
            {
                prayerHtml += "<p/><b>Write Failures</b><br/>";

                IList<FeastDateObject> realmFeastDates = databaseModel.FeastsForYear(year);
                foreach (WriteFailure wf in databaseModel.WriteFailures)
                {
                    // prolly already exists, so get the db one, and log both details
                    FeastDateObject dbFdo = realmFeastDates.Where(f => f.DayMonthYearAmPm == wf.FeastDateObj.DayMonthYearAmPm).FirstOrDefault<FeastDateObject>();

                    prayerHtml += wf.Reason + "<br/>Failed: " + wf.FeastDateObj.ToString() + "<br/>Realm:  " + dbFdo.ToString() + "<p/>";
                }
            }
            return prayerHtml;
        }

        string ToHtmlText(string heading, List<FeastDate> feastDates)
        {
            string htmlText = "<p/><b>" + heading + "</b><br/>";

            if (feastDates == null)
            {
                htmlText += "None :( <br/>n";
            }
            else
            {
                foreach (FeastDate feast in feastDates)
                {
                    htmlText += feast.ToHtmlString();
                }
            }

            return htmlText;
        }
        #endregion Feasts


        #region Sections
        [TestMethod, Ignore]
        public void GenerateSection()
        {
            PrayerSeason.PrayerSect prayerSection = PrayerSeason.PrayerSect.AllSections;

            try
            {
                PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;

                DateTime startDate = new DateTime(2018, 10, 1, 6, 0, 0);

                for (int i = 1; i <= 12; i++)
                {
                    DateTime endDate = startDate.AddMonths(1);
                    //string htmlText = _prayerPageModel.GeneratePrayers(startDate, endDate, prayerSect, filenames);
                    string htmlText = MakePrayers(prayerModel, startDate, endDate, prayerSection);
                    WriteFile(prayerSection.ToString(), startDate, endDate, htmlText);
                    startDate = endDate;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
                WriteErrorFile("GenerateSection", ex);
            }
        }

        [TestMethod, Ignore]
        public void GenerateSectionDay()
        {
            try
            {
                PrayerSeason.PrayerSect prayerSect = PrayerSeason.PrayerSect.Canticles;
                PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;
                DateTime startDate = new DateTime(2018, 12, 17, 6, 0, 0);

                //for (int i = 1; i <= 12; i++)
                {
                    DateTime endDate = startDate;
                    //string htmlText = _prayerPageModel.GeneratePrayers(startDate, endDate, prayerSect, filenames);
                    string htmlText = MakePrayers(prayerModel, startDate, endDate, prayerSect);
                    WriteFile(prayerSect.ToString(), startDate, endDate, htmlText);
                    //startDate = endDate;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
                WriteErrorFile("GenerateSectionDay", ex);
            }
        }

        [TestMethod, Ignore]
        public void GenerateDay()
        {
            try
            {
                PrayerForDate(new DateTime(2022,  1, 3, 6, 0, 0));
                //PrayerForDateSection(new DateTime(2019,  1, 11, 18, 0, 0), PrayerSeason.PrayerSect.AllSections);
                //PrayerForDateSection(new DateTime(2020, 12,  1,  6, 0, 0), PrayerSeason.PrayerSect.AllSections);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
                WriteErrorFile("GenerateDay", ex);
            }
        }

        private void PrayerForDateSection(DateTime date, PrayerSeason.PrayerSect prayerSect)
        {
            //string htmlText = _prayerModel.GeneratePrayers(date, date, prayerSect, true);
            PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;
            string htmlText = MakePrayers(prayerModel, date, date, prayerSect);
            WriteFile(prayerSect.ToString(), date, date, htmlText);
        }

        private void PrayerForDate(DateTime date)
        {
            PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;
            string htmlText = prayerModel.MakePrayer(date, true);
            WriteFile("Prayer", date, date, htmlText);
        }
        
        #endregion Sections


        #region Output File Creation
        string CreateFilename(string name)
        {
            string filename = name + ".html";
            var fullFilename = Path.Combine(_desktopFolder, filename);
            return fullFilename;
        }

        void WriteFile(string name, DateTime startDate, DateTime endDate, string text)
        {
            //Debug.WriteLine(htmlText);
            string filename = name + startDate.ToString("_yyyyMMdd");
            if (startDate.CompareTo(endDate) != 0) filename += endDate.ToString("_yyyyMMdd");
            filename += ".html";
            var fullFilename = Path.Combine(_desktopFolder, filename);

            WriteFile(fullFilename, text);
        }

        void WriteFile(string filename, string text)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(_htmlStart);
                sw.Write(text);
                sw.Write(_htmlEnd);
            }
        }

        void WriteErrorFile(string name, Exception ex)
        {
            string filename = "Error_" + name + ".html";
            var fullFilename = Path.Combine(_desktopFolder, filename);

            string text = "<pre>" + ex.Message + "\n\n" + ex.InnerException + "</pre>";
            WriteFile(fullFilename, text);
        }
        #endregion Output File Creation


        #region Various
        void ListAllResources()
        {
            Debug.WriteLine("ListAllResources()");
            var assembly = typeof(PrayerPageModel).GetTypeInfo().Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                Debug.WriteLine("Found resource: " + res);
            }
            Debug.WriteLine($"ListAllResources() Fin\n");
        }
        #endregion Various
    }
}
