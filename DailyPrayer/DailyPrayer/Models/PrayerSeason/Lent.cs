using System;

namespace DailyPrayer.Models.PrayerSeason
{
    class Lent : PrayerSeason
    {
        new private const string _Tag = "Lent";

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
