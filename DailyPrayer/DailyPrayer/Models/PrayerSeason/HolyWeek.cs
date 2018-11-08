using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using DailyPrayer.Models.PrayerSections;

namespace DailyPrayer.Models.PrayerSeason
{
    class HolyWeek : PrayerSeason
    {

        public HolyWeek(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            int weekNo = Int32.Parse(_weekNo) % 5;
            if (weekNo == 0)
                weekNo = 5;
            _fileEnd = string.Format("holy_week.{0}{1}.txt", _sectionOfDay, _dayNo);

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
                string filenamePart2 = string.Format("{0}.holy_week.morning.txt", _baseName[PrayerSect.Intro]);
                fileDetails.Add(base.LoadIntro(filenamePart2));                                                 // Load 1.Intro.morning/evening_opening.txt
                string filename = string.Format("{0}.{1}", _base, filenamePart2);
                fileDetails.Add(LoadFile(filename, PrayerSect.Intro));
            }
            else
            {
                fileDetails.Add(base.LoadIntro(""));
            }

            return fileDetails;
        }

        protected override FileDetails LoadPraise()
        {
            FileDetails fileDetails = new FileDetails();
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Praise]);

            string filename = string.Format("{0}.opening_hymn.{1}.txt", filebase, _sectionOfDay);       // just morning/evening here
            fileDetails.Add(LoadFile(filename, PrayerSect.Praise));
            string psalmsFilename = string.Format("{0}.psalms.{1}", filebase, _fileEnd);
            string refrainsFilename = string.Format("{0}.refrains.{1}", filebase, _fileEnd);

            fileDetails.Add(LoadRefrainAndPsalms(refrainsFilename, psalmsFilename));

            return fileDetails;
        }


    }
}
