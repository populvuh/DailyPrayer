using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPrayer.Models.PrayerSeason
{
    class OrdinaryTime : PrayerSeason
    {
        new private const string _Tag = "OrdinaryTime";

        public OrdinaryTime(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            int weekNo = Int32.Parse(_weekNo) % 4;
            if (weekNo == 0)
                weekNo = 4;

            _fileEnd = string.Format("ordinary{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
            string ordFileEnd = CreateOrdFilename();

            FileDetails fileDetails = new FileDetails();
            if (pSect == PrayerSect.AllSections)
            {
                fileDetails.Add(LoadIntro());
                fileDetails.Add(LoadPraise());
                fileDetails.Add(LoadWordOfGod());
                fileDetails.Add(LoadResponse());
                fileDetails.Add(LoadCanticles(ordFileEnd));
                fileDetails.Add(LoadPrayers());
                fileDetails.Add(LoadConclusion(ordFileEnd));
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
                    case PrayerSect.WordOfGod:
                        fileDetails.Add(LoadWordOfGod());
                        break;
                    case PrayerSect.Response:
                        fileDetails.Add(LoadResponse());
                        break;
                    case PrayerSect.Canticles:
                        fileDetails.Add(LoadCanticles(ordFileEnd));
                        break;
                    case PrayerSect.Prayers:
                        fileDetails.Add(LoadPrayers());
                        break;
                    case PrayerSect.Conclusion:
                        fileDetails.Add(LoadConclusion(ordFileEnd));
                        break;
                    default:
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
                string filenamePart2 = string.Format("{0}.ordinary{1}.refrain{2}.txt", _baseName[PrayerSect.Intro], _week, _dayNo);
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


        string CreateOrdFilename()
        {
            //2016 / 01 / 16(Sat) am - Season: OT1, Week: 1, Day: 7, am
            //DailyPrayer.Data.PrayerFiles._7.Conclusion.ord_weekend1.morning7.txt

            //2016 / 01 / 16(Sat) pm - Season: OT1, Week: 1, Day: 7, pm
            //DailyPrayer.Data.PrayerFiles._7.Conclusion.ord_weekday1.evening7.txt

            string filename = "";
            if ((_dayNo == "1") || (_dayNo == "7" && !_morning))
            {
                filename = string.Format("ord_weekend{0}.{1}{2}.txt", _weekNo, _sectionOfDay, _dayNo);
            }
            else
            {
                int weekNo = Int32.Parse(_weekNo) % 4;
                if (weekNo == 0) weekNo = 4;
                filename = string.Format("ord_weekday{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
            }

            return filename;
        }
    }
}
