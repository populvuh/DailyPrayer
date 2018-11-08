using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPrayer.Models
{
    public class FileDetails
    {
        List<string> _filenames = new List<string>();
        string _prayerText = "";
        public string PrayerText
        {
            get { return _prayerText; }
            set {}
        }
        static string prevName = "";
        public const string Error = "Error: ";

        public void Add(FileDetails fileDetails)
        {
            _filenames.AddRange(fileDetails._filenames);
            if (PrayerPageModel.DisplayFilenames)
                _prayerText += fileDetails.Filenames();
            _prayerText += fileDetails.PrayerText;
        }

        public void Add(string filename, string text)
        {
            _filenames.Add(filename);
            if (PrayerPageModel.DisplayFilenames || PrayerPageModel.DisplayFilenamesOnly)
            {
                string fname = (text.StartsWith(Error)) ? text : filename.Substring(30); ;
                if (fname.CompareTo(prevName) != 0)
                {
                    _prayerText += fname + "<p/>";
                    prevName = fname;
                }
            }
            _prayerText += text;
        }

        public void AddFilename(string filename)
        {
            _filenames.Add(filename);
        }

        public void AddText(string text)
        {
            _prayerText += text;
        }

        public string Filenames()
        {
            string fname = "";
            string filenames = "";
            foreach (string filename in _filenames)
            {
                fname = filename.Substring(30);
                if (fname.CompareTo(prevName) == 0)
                    continue;
                prevName = fname;
                filenames += fname + "<br/>";
            }
            filenames += "&nbsp; <br/>";

            return filenames;
        }
    }
}

