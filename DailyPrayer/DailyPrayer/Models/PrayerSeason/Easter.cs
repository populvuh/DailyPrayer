using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using DailyPrayer.Models.PrayerSections;

namespace DailyPrayer.Models.PrayerSeason
{
    class Easter : PrayerSeason
    {

        public Easter(Place place, bool testMode) : base(place, testMode)
        {
        }


        public override FileDetails LoadText(PrayerSect pSect)
        {
            _fileEnd = string.Format("easter{0}.{1}{2}.txt", _weekNo, _sectionOfDay, _dayNo);

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
                string filenamePart2 = string.Format("{0}.easter.morning.txt", _baseName[PrayerSect.Intro]);
                fileDetails.Add(base.LoadIntro(filenamePart2));                                                 // Load 1.Intro.morning/evening_opening.txt
                string filename = string.Format("{0}.{1}", _base, filenamePart2);
                fileDetails.Add(LoadFile(filename, PrayerSect.Intro));
            } else
            {
                fileDetails.Add(base.LoadIntro(""));                                                 // Load 1.Intro.morning/evening_opening.txt
            }

            return fileDetails;
        }

        protected override FileDetails LoadPraise()
        {
            FileDetails fileDetails = new FileDetails();
            if (_weekNo == "7" && _dayNo == "1" && _morning)
            {
                // This Ascending morning opening is actually a special refrain for the Sunday when we celebrate Jesus ascending to Heaven.
                // This feast takes place on the 7th Sunday of Easter.
                fileDetails.Add(SundayOfJesusAscending());
            }
            else
                fileDetails.Add(base.LoadPraise());

            return fileDetails;
        }


        FileDetails SundayOfJesusAscending()
        {
            FileDetails fileDetails = new FileDetails();
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Praise]);
            string filename = string.Format("{0}.opening_hymn.pentecost.txt", filebase);
            fileDetails.Add(LoadFile(filename, PrayerSect.Praise));

            filename = string.Format("{0}.psalms.pentecost.txt", filebase);
            fileDetails.Add(LoadFile(filename, PrayerSect.Praise));

            filename = string.Format("{0}.refrains.pentecost.txt", filebase);
            fileDetails.Add(LoadFile(filename, PrayerSect.Praise));

            return fileDetails;
        }
    }
}
