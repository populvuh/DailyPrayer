using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPrayer.Models.PrayerSeason
{
    class Advent : PrayerSeason
    {
        new private const string _Tag = "Advent";

        public Advent(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            int weekNo = Int32.Parse(_weekNo) % 4;
            if (weekNo == 0)
                weekNo = 4;

            //if (_dayInMonth >= 17 && _month == 12 && _dayNo != "1")
            //{
            // if >= 17 Dec, and NOT a Sunday, take file from 17_31Dec folder
            // _fileEnd = string.Format("_17_31Dec.{0}_Dec_{1}.txt", _dayInMonth, _sectionOfDay);
            //_fileEnd = string.Format("_17_31Dec.week{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
            //}

            if (_dayInMonth >= 17 && _month == 12)
            { 
                //fileEnd = string.Format("_17_31Dec.{0}{1}.txt", _sectionOfDay, _dayNo);
                if (_dayNo == "1")
                    _fileEnd = string.Format("_17_31Dec.{0}_Dec_{1}.txt", _dayInMonth, _sectionOfDay);
                else
                    _fileEnd = string.Format("_17_31Dec.week{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
            }
            else
            {
                _fileEnd = string.Format("advent{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
            }

            FileDetails fileDetails = new FileDetails();
            if (pSect == PrayerSect.AllSections)
            {
                fileDetails.Add(LoadIntro());
                fileDetails.Add(LoadPraise());
                fileDetails.Add(LoadWordOfGod());
                fileDetails.Add(LoadResponse());
                fileDetails.Add(LoadCanticles(_fileEnd));
                fileDetails.Add(LoadPrayers());
                fileDetails.Add(LoadConclusion(_fileEnd));
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
                        fileDetails.Add(LoadCanticles(_fileEnd));
                        break;
                    case PrayerSect.Prayers:
                        fileDetails.Add(LoadPrayers());
                        break;
                    case PrayerSect.Conclusion:
                        fileDetails.Add(LoadConclusion(_fileEnd));
                        break;
                    case PrayerSect.AllSections:
                    default:
                        break;
                }
            }

            return fileDetails;
        }

        FileDetails LoadIntro()
        {
            //am/pm opening, am psalm, am refrain.
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 1. Intro</b><p/>");
            if (_morning)
            {
                String filenamePart2 = string.Format("{0}.advent.morning.txt", _baseName[PrayerSect.Intro]);
                fileDetails.Add(base.LoadIntro(filenamePart2));                                                 // Load 1.Intro.morning/evening_opening.txt
                string filename = string.Format("{0}.{1}", _base, filenamePart2);
                fileDetails.Add(LoadFile(filename, PrayerSect.Intro));
            }

            return fileDetails;
        }

        override protected FileDetails LoadPraise()
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Praise]);
            //string fileEnd = string.Format("ordinary{0}.{1}{2}.txt", _week,_sectionOfDay, _dayNo);

            string filename = "";
            if (_dayInMonth >= 17 && _month == 12 && _dayNo != "1")
                filename = string.Format("{0}.opening_hymn._17_31Dec.{1}.txt", filebase, _sectionOfDay);
            else
                filename = string.Format("{0}.opening_hymn.{1}", filebase, _fileEnd);
            //string text = LoadFile(filename);
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 2. Praise</b><p/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.Praise));

            int weekNo = Int32.Parse(_weekNo) % 4;
            if (weekNo == 0)
                weekNo = 4;
            string fileEnd = string.Format("advent{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
            string psalmsFilename = string.Format("{0}.psalms.{1}", filebase, fileEnd);

            //fileEnd = string.Format("_17_31Dec.{1}{2}.txt", filebase, _sectionOfDay, _dayNo);
            if (_dayInMonth >= 17 && _month == 12)
                //fileEnd = string.Format("_17_31Dec.{0}{1}.txt", _sectionOfDay, _dayNo);
                if (_dayNo != "1")
                    fileEnd = string.Format("_17_31Dec.week{0}.{1}{2}.txt", weekNo, _sectionOfDay, _dayNo);
                else
                    fileEnd = string.Format("_17_31Dec.{0}_Dec_{1}.txt", _dayInMonth, _sectionOfDay);

            else
                fileEnd = _fileEnd;

            string refrainsFilename = string.Format("{0}.refrains.{1}", filebase, fileEnd);

            fileDetails.Add(LoadRefrainAndPsalms(refrainsFilename, psalmsFilename));

            return fileDetails;
        }



        override protected FileDetails LoadResponse()
        {
            string filename = "";
            if (_dayInMonth >= 17 && _month == 12 && _dayNo != "1")
                filename = string.Format("{0}.{1}._17_31Dec.{2}.txt", _base, _baseName[PrayerSect.Response], _sectionOfDay);
            else
                filename = string.Format("{0}.{1}.{2}", _base, _baseName[PrayerSect.Response], _fileEnd);

            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 3. Response</b><p/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.Response));

            return fileDetails;
        }
    }
}
