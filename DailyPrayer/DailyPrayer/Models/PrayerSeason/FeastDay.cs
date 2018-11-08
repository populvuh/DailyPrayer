using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Debug = System.Diagnostics.Debug;




namespace DailyPrayer.Models.PrayerSeason
{
    class FeastDay : PrayerSeason
    {
        new const string _Tag = "FeastDay";

        public FeastDay(Place place, bool testMode) : base(place, testMode)
        {
            //_filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.]);
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            //_fileEnd = string.Format("feasts.{0}_{1}.txt", GetSeasonString(), _sectionOfDay);
            _fileEnd = string.Format("feasts.{0}", GetSeasonString(_sectionOfDay));
            //if (!_fileEnd.EndsWith(".txt"))
            //    Debug.WriteLine($"{_Tag}.LoadText: _fileEnd = {_fileEnd}");

            FileDetails fileDetails = new FileDetails();
            if (pSect == PrayerSect.Ignore)
            {
                fileDetails.Add(LoadIntro());
                fileDetails.Add(LoadPraise());
                fileDetails.Add(base.LoadText(pSect));
            }
            else
            {
                switch (pSect)
                {
                    case PrayerSect.Intro:
                        fileDetails.Add(LoadIntro());
                        break;
                    case PrayerSect.Praise:
                        fileDetails.Add(LoadPraise());
                        break;
                    default:
                        fileDetails.Add(base.LoadText(pSect));
                        break;
                }
            }

            return fileDetails;
        }

        FileDetails LoadIntro()
        {
            FileDetails fileDetails = new FileDetails();
            if (_morning)
            {
                //string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Intro]);
                //string filename = string.Format("{0}.{1}", filebase, _fileEnd);

                string filenamePart2 = string.Format("{0}.{1}", _baseName[PrayerSect.Intro], _fileEnd);
                fileDetails.Add(base.LoadIntro(filenamePart2));                                                 // Load 1.Intro.morning/evening_opening.txt
                string filename = string.Format("{0}.{1}", _base, filenamePart2);
                fileDetails.Add(LoadFile(filename, PrayerSect.Intro));
            }
            else
            {
                fileDetails.Add(base.LoadIntro(""));                                                 // Load 1.Intro.morning/evening_opening.txt
            }

            return fileDetails;
        }
    }
}
