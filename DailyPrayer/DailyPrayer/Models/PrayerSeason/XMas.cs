
namespace DailyPrayer.Models.PrayerSeason
{
    class XMas : PrayerSeason
    {
        new private string _Tag = "Xmas";
        private string _psalmEnd = "";
        private string _refrainEnd = "";

        public XMas(Place place, bool testMode) : base(place, testMode)
        {
        }

        public override FileDetails LoadText(PrayerSect pSect)
        {
            if (_dayInMonth==25 || _dayInMonth==26)
                _fileEnd = string.Format("_17_31Dec.{0}_Dec_{1}.txt", _dayInMonth, _sectionOfDay);
            else
                _fileEnd = string.Format("xmas{0}.{1}{2}.txt", _week, _sectionOfDay, _dayNo);

            _psalmEnd = string.Format("advent{0}.{1}{2}.txt", _week, _sectionOfDay, _dayNo);
            _refrainEnd = string.Format("_17_31Dec.week{0}.{1}{2}.txt", _week, _sectionOfDay, _dayNo);


            if (!string.IsNullOrEmpty(_filename))
                System.Diagnostics.Debug.WriteLine($"{_Tag}.LoadText({pSect.ToString()}) - _filename = {_filename}");
            System.Diagnostics.Debug.WriteLine($"{_Tag}.LoadText({pSect.ToString()}) - FileEnd = {_fileEnd}");

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

        override protected FileDetails LoadPraise()
        {
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 2. Praise</b><p/>");
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Praise]);

            string openHymnFilename = string.Format("{0}.opening_hymn.{1}", filebase, _fileEnd);
            string psalmsFilename = string.Format("{0}.psalms.{1}", filebase, _psalmEnd);
            string refrainsFilename = string.Format("{0}.refrains.{1}", filebase, _refrainEnd);
            System.Diagnostics.Debug.WriteLine($"{_Tag}.LoadPraise({openHymnFilename}, \n\t\t{psalmsFilename}, \n\t\t{refrainsFilename})");

            fileDetails.Add(LoadFile(openHymnFilename, PrayerSect.Praise));
            fileDetails.Add(LoadRefrainAndPsalms(refrainsFilename, psalmsFilename));

            return fileDetails;
        }
    }
}
