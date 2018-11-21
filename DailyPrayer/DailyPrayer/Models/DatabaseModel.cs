using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Realms;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer.Models
{
    // FeastInfoObject holds the details of the Feast, i.e. the filename to get for the feast
    // it does NOT contain the dates
    public class FeastInfoObject : RealmObject
    {
        [PrimaryKey]
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public string SolemnityType { get; set; }
        public bool PrevEvening { get; set; }

        public FeastInfoObject()
        { }

        public FeastInfoObject(DominicanFeasts.DominicanFeast dominicanFeast)
        {
            Filename = dominicanFeast.feast.name.Replace(@"/", "-");
            Name = dominicanFeast.feast.name;
            if (string.IsNullOrEmpty(Filename))
                Filename = Name;

            Title = dominicanFeast.feast.vTitle;
            EnglishTitle = dominicanFeast.feast.eTitle;
            SolemnityType = dominicanFeast.feast.solemnityType;
            PrevEvening = dominicanFeast.feast.prevEvening;
        }

        public string GetFilename()
        {
            if (string.IsNullOrEmpty(Filename))
                return Name.Replace(@"/", "-");

            return Filename;
        }

        public override string ToString()
        {
            string toString = string.Format($"{Filename}, {Name}, {EnglishTitle}, {Title}, {SolemnityType}, {PrevEvening}");
            return toString;
        }
    }

    public class FeastDateObject : RealmObject
    {
        // for feasts the filename will just b the feast date dd-mm
        // however for variable date solemnities, it will b the filename of the prayers in the feast directories
        [PrimaryKey]
        public string DayMonthYearAmPm { get; set; }
        public string Filename { get; set; }
        public string Year { get; set; }
        public string FeastKey { get; set; }
        //public DateTimeOffset Date { get; set; }
        //public string AmPm { get; set; }
        public string EveningId { get; set; }
        CultureInfo cultureInfo = new CultureInfo("en-AU");

        public FeastDateObject()
        { }

        //public FeastDateObject(string dmy, string filename, string feastKey, string year, string amPm, string eveningId)
        public FeastDateObject(string dmy, string filename, string feastKey, string year, string amPm, string eveningId)
        {
            DayMonthYearAmPm = dmy + amPm;
            Filename = filename;
            FeastKey = feastKey;
            Year = year;
            //AmPm = amPm;
            EveningId = eveningId;
        }

        public override string ToString()
        {
            return string.Format($"{DayMonthYearAmPm}, {Filename}");
        }
    }

    public interface IDatabaseModel
    {
        IList<FeastInfoObject> GetFeasts();
        IList<FeastInfoObject> SetFeasts(DominicanFeasts dominicanFeasts);
        IList<FeastDate> GetFeastsForYear(string year);
        bool SetFeastsForYear(string year, IList<FeastDateObject> listFeastDates);
        string FeastsToString();
        string FeastsForYearToString(string year);
        void DeleteAll();
    }

    public struct WriteFailure
    {
        public FeastDateObject FeastDateObj;
        public string Reason;
    }

    public class DatabaseModel : IDatabaseModel
    {
        // Workflow
        // Try to get feast dates for year
        // If exists, 
        //      join feast dates with feasts objects and return full list
        // If not exists
        //  get list of feast objects (not yet cvted to actual date)
        //  If not exists
        //      Read Data.Feasts.json, and load into db
        //  Use list of feast objects to calc actual dates for year, and then insert into db 

        static string _Tag = "DatabaseModel";
        private Realm _realm;
        //public static string _dateFormat = @"yyyy/MM/dd HH:mm:ss";
        private bool _bTest = false;
        private List<WriteFailure> _WriteFailures = new List<WriteFailure>();
        public List<WriteFailure> WriteFailures
        {
            get { return _WriteFailures; }
            private set { }
        }


        public DatabaseModel()
        {
            try
            {
                // need to do this if we change the schema
                RealmConfiguration config = new RealmConfiguration();
                Realm.DeleteRealm(config);
                _realm = Realm.GetInstance();
#if DEBUG
                DeleteAll();                                    // only for debugging
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{_Tag}.{_Tag} error: {ex.Message}\n{ex.InnerException}");

                RealmConfiguration config = new RealmConfiguration();
                Realm.DeleteRealm(config);
                _realm = Realm.GetInstance();
            }
        }

        public IList<FeastInfoObject> GetFeasts()
        {
            // these r the generic feast objects, not yet converted into actual dates in a year.
            //Debug.WriteLine("DatabaseModel.GetFeasts");

            IList<FeastInfoObject> feasts = _realm.All<FeastInfoObject>().ToList();
            //foreach (FeastObject fo in feasts)
            //{
            //    Debug.WriteLine("Feast = " + fo.ToString());
            //}

            return feasts;
        }

        public IList<FeastInfoObject> SetFeasts(DominicanFeasts dominicanFeasts)
        {
            //Debug.WriteLine($"{_Tag}.SetFeasts()");

            IList<FeastInfoObject> feasts = new List<FeastInfoObject>();
            if (dominicanFeasts.feasts.Count > 0)
            {
                try
                {
                    _realm.Write(() =>
                    {
                        IList<DominicanFeasts.FeastDetails> feastList = new List<DominicanFeasts.FeastDetails>();

                        foreach (DominicanFeasts.DominicanFeast df in dominicanFeasts.feasts)
                        {
                            //Debug.WriteLine("DominicanFeasts.DominicanFeast = " + df.ToString());
                            var entry = new FeastInfoObject(df);
                            feasts.Add(entry);
                            _realm.Add(entry);
                        }
                    });

                    /*IList<FeastDateObject> feastDates = _realm.All<FeastDateObject>().Where(d => d.Year == "2017").ToList<FeastDateObject>();
                    feasts = _realm.All<FeastObject>().ToList();
                    Debug.WriteLine("FeastObjects: ");
                    foreach (FeastObject fo in feasts)
                    {
                        Debug.WriteLine("FeastObject: " + fo.ToString());
                    }
                    Debug.WriteLine("\n");
                    */


                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DatabaseModel.SetFeasts: Error - " + ex.Message);
                }
            }

            return feasts;
        }

        public IList<FeastDate> GetFeastsForYear(string year)
        {
            //Debug.WriteLine($"DatabaseModel.GetFeastsForYear( {year} )");
            //DateTime yearDateTime = new DateTime(Int32.Parse(year), 1, 1);

            // get feast dates for the year
            IList<FeastDateObject> feastDates = (string.IsNullOrEmpty(year)) ?
                _realm.All<FeastDateObject>().ToList<FeastDateObject>() :
                FeastsForYear(year);
            // get the feast details i.e. the filenames for the feasts
            IList<FeastInfoObject> feasts = _realm.All<FeastInfoObject>().ToList();

            if (_bTest)
            {
                Debug.WriteLine("FeastDateObjects");
                foreach (FeastDateObject fdo2 in feastDates)
                    Debug.WriteLine($"{fdo2.ToString()}");
                Debug.WriteLine("\nFeastInfoObjects");
                foreach (FeastInfoObject f in feasts)
                    Debug.WriteLine($"{f.ToString()}");
                Debug.WriteLine("\nIterating over FeastDates, and getting the FeastInfoObject for it");
            }

            // iter over the feast dates, and FeastDate record which has the date, and the filename for that feast
            IList<FeastDate> listFeastDates = new List<FeastDate>();
            foreach (FeastDateObject fdo in feastDates)                         //.ToList<FeastDateObject>())
            {
                if (_bTest)
                    Debug.WriteLine($"{fdo.ToString()}");

                // we get the feast details for the filename from the feastdate
                var feastDetails = feasts.Where(d => d.Filename == fdo.FeastKey).FirstOrDefault<FeastInfoObject>();

                if (feastDetails == null)
                {
                    //Debug.WriteLine("No feast found for " + fdo.FeastKey);
                    continue;
                }
                FeastDate feastDate = new FeastDate(fdo.DayMonthYearAmPm, fdo.Filename, feastDetails);
                listFeastDates.Add(feastDate);
            }

            IEnumerable<FeastDate> sorted = listFeastDates.OrderBy(f => f.date);
            IList<FeastDate> sortedList = sorted.ToList();

            //Debug.WriteLine($"DatabaseModel.GetFeastsForYear( {year} ) Fin");
            return sortedList;
        }


        public bool SetFeastsForYear(string year, IList<FeastDateObject> listFeastDates)
        {
            bool result = false;

            Debug.WriteLine("DatabaseModel.SetFeastsForYear()");
            foreach (FeastDateObject fdo in listFeastDates)
                Debug.WriteLine($"{fdo.ToString()}");

#if DEBUG
            _WriteFailures.Clear();
#endif

            _realm.Write(() =>
            {
                foreach (FeastDateObject fdo in listFeastDates)
                {
                    try
                    {
                        _realm.Add(fdo);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        
                        _WriteFailures.Add(new WriteFailure() { FeastDateObj=fdo, Reason=ex.Message });
                        Debug.WriteLine("DatabaseModel.SetFeastsForYear: Error - " + ex.Message);
#endif
                    }
                }
            });
            result = true;

            return result;
        }

        public List<FeastDateObject> FeastsForYear(string year)
        {
            List<FeastDateObject> feastsForYear = (string.IsNullOrEmpty(year)) ?
                    _realm.All<FeastDateObject>().ToList<FeastDateObject>() :
                    _realm.All<FeastDateObject>().Where(d => d.Year == year).ToList<FeastDateObject>();

            return feastsForYear;
        }

        public string FeastsToString()
        {
            string toString = "\n";
            IList<FeastInfoObject> feasts = _realm.All<FeastInfoObject>().ToList();
            foreach (FeastInfoObject fo in feasts)
            {
                toString += fo.ToString() + "\n";
            }
            toString += "\n";
            return toString;
        }

        public string FeastsForYearToString(string year)
        {
            string toString = "\n";
            IList<FeastDate> feasts = GetFeastsForYear(year);
            foreach (FeastDate fd in feasts)
            {
                //String amPm = (fd.am) ? "am" : "pm";
                String date = fd.date.ToString("yyyy/MM/dd");
                String amPm = fd.filename.Contains("morning") ? "am" : "pm";
                String feastString = String.Format($"{date} {amPm} - {fd.feast.SolemnityType}, {fd.feast.Name}, {fd.feast.Title}\n");
                toString += feastString;
            }
            toString += "\n";
            return toString;
        }

        public void DeleteAll()
        {
            try
            {
                _realm.Write(() =>
                {
                    _realm.RemoveAll<FeastInfoObject>();
                    _realm.RemoveAll<FeastDateObject>();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DatabaseModel.DeleteAll: Error - " + ex.Message);
            }
        }
    }
}

