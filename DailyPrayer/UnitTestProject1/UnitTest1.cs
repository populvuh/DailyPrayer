using System;
using FreshMvvm;
using DailyPrayer;
using DailyPrayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Debug = System.Diagnostics.Debug;
using System.IO;
using DailyPrayer.Models.PrayerSeason;


namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        string _desktopFolder;
        PrayerPageModel _prayerPageModel;

        [TestInitialize]
        public void SetIOC()
        {
            FreshIOC.Container.Register<IPrayerPageModel, PrayerPageModel>();
            FreshIOC.Container.Register<IDatabaseModel, DatabaseModel>();
            FreshIOC.Container.Register<IFeastsModel, FeastsModel>();
            FreshIOC.Container.Register<IDominicanCalender, DominicanCalender>();

            _desktopFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "DailyPrayers");
            _prayerPageModel = FreshIOC.Container.Resolve<IPrayerPageModel>() as PrayerPageModel;
        }

        [TestMethod, Ignore]
        public void GenerateFilenames()
        {
            try
            {
                DateTime startDate = new DateTime(2018, 12, 1, 6, 0, 0);
                DateTime endDate = new DateTime(2019, 1, 1, 18, 0, 0);
                string htmlText = _prayerPageModel.TestRange(startDate, endDate);
                WriteFile("Filenames", startDate, endDate, htmlText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GenerateFilenames exception: {ex.StackTrace}");
            }
        }

        [TestMethod, Ignore]
        public void GeneratePrayers()
        {
            try
            {
                DateTime startDate = new DateTime(2019, 1, 1, 6, 0, 0);

                for (int i = 1; i <= 10; i++)                // years
                {
                    for (int j = 1; j <= 12; j++)           // months
                    {
                        DateTime endDate = startDate.AddMonths(1);
                        string htmlText = _prayerPageModel.GeneratePrayers(startDate, endDate);
                        WriteFile("Prayers", startDate, endDate, htmlText);
                        startDate = endDate;
                    }
                    startDate = new DateTime(2019 + i, 1, 1, 6, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
            }
        }


        [TestMethod, Ignore]
        public void GeneratePrayers2()
        {
            try
            {
                DateTime startDate = new DateTime(2018, 12, 1, 6, 0, 0);
                DateTime endDate = new DateTime(2018, 12, 31, 18, 0, 0);
                string htmlText = _prayerPageModel.TestRange(startDate, endDate);
                WriteFile("Prayers", startDate, endDate, htmlText);

                startDate = new DateTime(2019, 12, 1, 6, 0, 0);
                endDate = new DateTime(2019, 12, 31, 18, 0, 0);
                htmlText = _prayerPageModel.TestRange(startDate, endDate);
                WriteFile("Prayers", startDate, endDate, htmlText);

                startDate = new DateTime(2020, 12, 1, 6, 0, 0);
                endDate = new DateTime(2020, 12, 31, 18, 0, 0);
                htmlText = _prayerPageModel.TestRange(startDate, endDate);
                WriteFile("Prayers", startDate, endDate, htmlText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
            }
        }


        [TestMethod, Ignore]
        public void GenerateFeastDates()
        {
            try
            {
                string htmlText = _prayerPageModel.GenerateFeastDates("2018");
                string filename = CreateFilename("FeastDates");
                WriteFile(filename, htmlText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GenerateFeastDates exception: {ex.StackTrace}");
            }
        }


        [TestMethod, Ignore]
        public void GenerateSection()
        {
            PrayerSeason.PrayerSect prayerSect = PrayerSeason.PrayerSect.Canticles;
            try
            {
                bool filenames = false;
                DateTime startDate = new DateTime(2018, 10, 1, 6, 0, 0);

                for (int i = 1; i <= 12; i++)
                {
                    DateTime endDate = startDate.AddMonths(1);
                    string htmlText = _prayerPageModel.GeneratePrayers(startDate, endDate, prayerSect, filenames);
                    WriteFile(prayerSect.ToString(), startDate, endDate, htmlText);
                    startDate = endDate;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
            }
        }

        [TestMethod, Ignore]
        public void GenerateSectionDay()
        {
            try
            {
                bool filenames = true;
                PrayerSeason.PrayerSect prayerSect = PrayerSeason.PrayerSect.Canticles;
                DateTime startDate = new DateTime(2018, 12, 17, 6, 0, 0);

                //for (int i = 1; i <= 12; i++)
                {
                    DateTime endDate = startDate;
                    string htmlText = _prayerPageModel.GeneratePrayers(startDate, endDate, prayerSect, filenames);
                    WriteFile(prayerSect.ToString(), startDate, endDate, htmlText);
                    //startDate = endDate;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
            }
        }

        [TestMethod]
        public void GenerateDay()
        {
            try
            {
                PrayerForDateSection(new DateTime(2019, 4, 17, 18, 0, 0), PrayerSeason.PrayerSect.Ignore);
                PrayerForDateSection(new DateTime(2020, 4, 8, 18, 0, 0), PrayerSeason.PrayerSect.Ignore);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeneratePrayers exception: {ex.StackTrace}");
            }
        }

        private void PrayerForDateSection(DateTime date, PrayerSeason.PrayerSect prayerSect)
        {
            string htmlText = _prayerPageModel.GeneratePrayers(date, date, prayerSect, true);
            WriteFile(prayerSect.ToString(), date, date, htmlText);
        }

        string CreateFilename(string name)
        {
            string filename = name + ".html";
            var fullFilename = Path.Combine(_desktopFolder, filename);
            return fullFilename;
        }

        void WriteFile(string name, DateTime startDate, DateTime endDate, string text)
        {
            //Debug.WriteLine(htmlText);
            string filename = name + startDate.ToString("_yyyyMMdd") + endDate.ToString("_yyyyMMdd") + ".html";
            var fullFilename = Path.Combine(_desktopFolder, filename);

            WriteFile(fullFilename, text);
        }

        void WriteFile(string filename, string text)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(text);
            }
        }

    }
}
