using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using DailyPrayer.Models.PrayerSections;

namespace DailyPrayer.Models.PrayerSeason
{
    class AshWednesdayWeek : PrayerSeason
    {
        new private const string _Tag = "AshWednesdayWeek";

        public AshWednesdayWeek(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            _fileEnd = string.Format("ash_wednesday_week.{0}{1}.txt", _sectionOfDay, _dayNo);

            FileDetails fileDetails = new FileDetails();
            if (pSect == PrayerSect.AllSections)
            {
                fileDetails.Add(LoadIntro());
                fileDetails.Add(LoadPraise());
                fileDetails.Add(base.LoadText(pSect));
            }
            else
            {
                switch (pSect)
                {
                    case PrayerSect.AllSections:
                        break;
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
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 1. Intro</b><p/>");
            if (_morning)
            {
                string filenamePart2 = string.Format("{0}.ash_wednesday_week.morning.txt", _baseName[PrayerSect.Intro]);
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