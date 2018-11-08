using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using DailyPrayer.Models.PrayerSections;

namespace DailyPrayer.Models.PrayerSeason
{
    class Lent : PrayerSeason
    {

        public Lent(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            int weekNo = Int32.Parse(_weekNo) % 5;
            if (weekNo == 0)
                weekNo = 5;

            _fileEnd = string.Format("lent{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);

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
                string filenamePart2 = string.Format("{0}.lent.morning.txt", _baseName[PrayerSect.Intro]);
                fileDetails.Add(base.LoadIntro(filenamePart2));

                string filename = string.Format("{0}.{1}", _base, filenamePart2);
                fileDetails.Add(LoadFile(filename, PrayerSect.Intro));
            }
            else
            {
                fileDetails.Add(base.LoadIntro(""));
            }

            return fileDetails;
        }


    }
}
