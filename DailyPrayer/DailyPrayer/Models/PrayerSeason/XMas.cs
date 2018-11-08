
namespace DailyPrayer.Models.PrayerSeason
{
    class XMas : PrayerSeason
    {
        new private string _Tag = "Xmas";

        public XMas(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            if (_dayInMonth==25 || _dayInMonth==26)
                _fileEnd = string.Format("_17_31Dec.{0}_Dec_{1}.txt", _dayInMonth, _sectionOfDay);
            else
                _fileEnd = string.Format("xmas{0}.{1}{2}.txt", _week, _sectionOfDay, _dayNo);

            System.Diagnostics.Debug.WriteLine($"{_Tag}.LoadText() - FileEnd = {_fileEnd}");

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
                string filenamePart2 = string.Format("{0}.{1}", _baseName[PrayerSect.Intro], _fileEnd);
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

        /*string LoadPraise(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Praise]);
            //string fileEnd = string.Format("ordinary{0}.{1}{2}.txt", _week,_sectionOfDay, _dayNo);

            string filename = string.Format("{0}.opening_hymn.{1}", filebase, fileEnd);
            string text = LoadFile(filename);
            string psalmsFilename = string.Format("{0}.psalms.{1}", filebase, fileEnd);
            string refrainsFilename = string.Format("{0}.refrains.{1}", filebase, fileEnd);

            return text;
        }

        string LoadWordOfGod(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.WordOfGod]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);

            string text = LoadFile(filename);

            return text;

        }

        string LoadResponse(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Response]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);

            string text = LoadFile(filename);

            return text;

        }

        string LoadCanticles(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Canticles]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);
            string text = LoadFile(filename);

            filename = string.Format("{0}.{1}.txt", filebase, (_morning) ? "benedictus" : "magnificat");
            text += LoadFile(filename);


            return text;

        }

        string LoadPrayers(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Prayers]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);

            string text = LoadFile(filename);

            return text;

        }

        string LoadConclusion(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Conclusion]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);

            string text = LoadFile(filename);

            return text;

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
                filename = string.Format("{0}.ord_weekend{1}.{2}{3}.txt", _filebase, _weekNo, _sectionOfDay, _dayNo);
            }
            else
            {
                int weekNo = Int32.Parse(_weekNo) % 4;
                if (weekNo == 0) weekNo = 4;
                filename = string.Format("{0}.ord_weekday{1}.{2}{3}.txt", _filebase, weekNo, _sectionOfDay, _dayNo);
            }

            return filename;
        }*/
    }
}
