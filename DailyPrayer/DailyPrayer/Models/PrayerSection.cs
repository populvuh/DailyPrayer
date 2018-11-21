using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Xamarin.Forms;
using DailyPrayer;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer.Models.PrayerSeason
{
    class PrayerSection
    {
        protected string _Tag;
        protected string _filebase;
        protected string _name;
        protected string _title;
        protected string _weekNo;
        protected string _dayNo;
        protected string _sectionOfDay;
        protected bool _morning;
        const string _base = "App1.Data.PrayerFiles";
        const string _DC = "ÐC:";

        FormattedString _prayerText = new FormattedString();

        protected string Title
        {
            get { return string.Format("</i><B><Font size=+2>{0}</Font></B><br>", _title); }
        }


        public PrayerSection(string name, string title, Place place)
        {
            _Tag = "PrayerSection";
            _filebase = string.Format("{0}.{1}", _base, name);
            _name = name;
            _title = title;
            _weekNo = place.WeekNo;
            _dayNo = place.DayNo;
            _morning = place.Morning;
            _sectionOfDay = (_morning) ? "morning" : "evening";
        }

        public virtual string LoadText(bool testMode)
        {
            string filename = string.Format("{0}.week{1}.{3}{2}.txt", _filebase, _weekNo, _dayNo, _sectionOfDay);

            string text = Title;
            if (testMode)
            {
                text += filename + "<br/>";
                return text;
            }

            text += LoadFile(filename);

            return text;
        }

        protected string LoadFile(string filename)
        {
            string text = "";
            try
            {
                var assembly = typeof(PrayerSection).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream(filename))
                {
                    if (null != stream)
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            text = sr.ReadToEnd().Trim();
                            if (text.StartsWith(_DC))
                                text = string.Format("<i>{0}</i>", text);
                            text += "<p></p>";
                        }
                    }
                    else
                    {
                        Debug.WriteLine(_Tag + ".LoadText( " + filename + " ): Error - File not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(_Tag + ".LoadText(): Error - " + ex.Message);
            }

            return text;
        }
    }


    class IntroSection : PrayerSection
    {
        public IntroSection(string name, string title, Place place)
            : base(name, title, place)
        {
            _Tag = "IntroSection";
        }

        public override string LoadText(bool testMode)
        {
            string filename = string.Format("{0}.{1}Opening.txt", _filebase, _sectionOfDay);
            //Debug.WriteLine(_Tag + ".LoadText() - " + filename);

            string text = Title;
            text += (testMode) ? filename + "<br/>" : LoadFile(filename);

            if (_morning)
            {

                text += LoadPsalm(testMode);
                filename = string.Format("{0}.refrains{1}.refrain{2}.txt", _filebase, _weekNo, _dayNo);
                text += (testMode) ? filename + "<br/>" : LoadFile(filename);
            }

            return text;
        }

        private string LoadPsalm(bool testMode)
        {
            string psalm = "";
            if ("3" == _dayNo)
                psalm = "psalm23.txt";
            else if ("4" == _dayNo)
                psalm = "psalm94.txt";
            else if ("6" == _dayNo)
                psalm = "psalm66.txt";
            else
                psalm = "psalm99.txt";
            string filename = string.Format("{0}.{1}.psalms.{2}", _filebase, _name, psalm);

            string text = "</i><B><Font size=+2>Psalm</Font></B><br>";
            if (testMode)
            {
                text += filename + "<br/>";
                return text;
            }
            //Debug.WriteLine(_Tag + ".LoadText() - " + filename);

            text += LoadFile(filename);

            return text;
        }
    }


    class PraiseSection : PrayerSection
    {
        public PraiseSection(string name, string title, Place place)
            : base(name, title, place)
        {
            _Tag = "PraiseSection";
        }

        public override string LoadText(bool testMode)
        {
            string filename1 = string.Format("{0}.opening_hymn.ordinary{1}.{2}{3}.txt", _filebase, _weekNo, _sectionOfDay, _dayNo);
            string filename2 = string.Format("{0}.psalms.ordinary{1}.{2}{3}.txt", _filebase, _weekNo, _sectionOfDay, _dayNo);
            string filename3 = string.Format("{0}.refrains.ordinary{1}.{2}{3}.txt", _filebase, _weekNo, _sectionOfDay, _dayNo);

            string text = Title;
            if (testMode)
            {
                text += filename1 + "<br>" + filename2 + "<br>" + filename3 + "<br>";
                return text;
            }

            text += LoadFile(filename1);
            text += LoadFile(filename2);
            text += LoadFile(filename3);

            return text;
        }

        private string LoadPsalm(bool testMode)
        {
            string psalm = "";
            if ("3" == _dayNo)
                psalm = "psalm23.txt";
            else if ("4" == _dayNo)
                psalm = "psalm94.txt";
            else if ("6" == _dayNo)
                psalm = "psalm66.txt";
            else
                psalm = "psalm99.txt";
            string filename = string.Format("{0}.{1}.psalms.{2}", _filebase, _name, psalm);

            string text = "</i><B><Font size=+2>Psalm</Font></B><br>";
            if (testMode)
            {
                text = filename + "<br/>";
                return text;
            }
            //Debug.WriteLine(_Tag + ".LoadText() - " + filename);

            text += LoadFile(filename);

            return text;
        }
    }

    class ConclusionSection : PrayerSection
    {
        public ConclusionSection(string name, string title, Place place)
            : base(name, title, place)
        {
            _Tag = "CanticlesSection";
        }

        public override string LoadText(bool testMode)
        {
            string filename = "";
            if ((_dayNo == "1") || (_dayNo == "7" && _morning))
            {
                filename = string.Format("{0}.week{1}.{2}{3}.txt", _filebase, _weekNo, _sectionOfDay, _dayNo);
            }
            else
            {
                int weekNo = Int32.Parse(_weekNo) % 4;
                filename = string.Format("{0}.set{1}.{2}{3}.txt", _filebase, weekNo, _sectionOfDay, _dayNo);
            }

            string text = Title;
            if (testMode)
            {
                text += filename + "<br>";
                return text;
            }
            //Debug.WriteLine(_Tag + ".LoadText() - " + filename);

            text += LoadFile(filename);

            return text;
        }
    }


    class CanticlesSection : PrayerSection
    {
        public CanticlesSection(string name, string title, Place place)
            : base(name, title, place)
        {
            _Tag = "CanticlesSection";
        }

        public override string LoadText(bool testMode)
        {
            string text = base.LoadText(testMode);

            string filename = string.Format("{0}.{1}.txt", _filebase, (_morning) ? "benedictus" : "magnificat");
            text += (testMode) ? filename+"<br>" : LoadFile(filename);

            return text;
        }
    }
}