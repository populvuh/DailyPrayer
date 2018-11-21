using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FreshMvvm;
using Debug = System.Diagnostics.Debug;

namespace DailyPrayer.Models.PrayerSeason
{
    public class PrayerSeason
    {
        int _baseLen = 0;
        protected bool _testMode = false;
        protected bool _testMode2 = false;
        protected string _fileEnd;
        //protected string _title;
        protected string _weekNo;
        protected string _dayNo;
        protected string _sectionOfDay;
        protected string _filename;
        protected bool _morning;
        protected int _dayInMonth;
        protected int _month;
        protected int _week;
        protected Place _place;
        protected DominicanSeasons _domSeason;
        protected static string _Tag = "PrayerSeason";
        protected const string _base = "DailyPrayer.Data.PrayerFiles";
        const string _afterAshWedWeek = " sau lễ Tro";

        const string _DC = "ÐC:";
        const string _Dc = "Đc:";
        public const string _singleCR = "\n";
        public static string _doubleR = "\r \r";
        public static string _doubleCR = "\n\n";
        public static string _tripleCR = "\n\n\n";
        public static string _singleLE = "\n\r";
        public static string _singleRN = "\r\n";
        public static string _doubleLE = _singleLE + _singleLE;
        public static string _doubleRN = _singleRN + _singleRN;
        public static string _doubleSpacedRN = _singleRN + " " + _singleRN;
        public static string _doubleSpacedLE = _singleLE + " " + _singleLE;


        public enum PrayerSect
        {
            AllSections = 0,
            Intro = 1,
            Praise,
            WordOfGod,
            Response,
            Canticles,
            Prayers,
            Conclusion
        };

        protected static Dictionary<PrayerSect, string> baseNames = new Dictionary<PrayerSect, string>()
        {
            [PrayerSect.Intro] = "_1.Intro",
            [PrayerSect.Praise] = "_2.Praise",
            [PrayerSect.WordOfGod] = "_3.TheWordOfGod",
            [PrayerSect.Response] = "_4.Response",
            [PrayerSect.Canticles] = "_5.Canticles",
            [PrayerSect.Prayers] = "_6.Prayers",
            [PrayerSect.Conclusion] = "_7.Conclusion"
        };
        protected static Dictionary<PrayerSect, string> _baseName =
            new Dictionary<PrayerSect, string>();


        Dictionary<DominicanSeasons, string> _vietnameseSeason = new Dictionary<DominicanSeasons, string>()
        {
            [DominicanSeasons.Advent] = "Mùa Vọng",
            [DominicanSeasons.Easter] = "Mùa Phục Sinh",
            [DominicanSeasons.Lent] = "Mùa Chay",
            [DominicanSeasons.OT1] = "Thường Niên",
            [DominicanSeasons.OT2] = "Thường Niên",
            [DominicanSeasons.XMas] = "Mùa Giáng Sinh",
            [DominicanSeasons.XMas_II] = "Mùa Giáng Sinh",
            [DominicanSeasons.Ash_Wednesday_Week] = "Mùa Chay",
            [DominicanSeasons.Holy_Week] = "Holy Week",
            //[DominicanSeasons.Ash_Wednesday_Week] = "Ash Wednesday Week",
            //[DominicanSeasons.OctaveOfChristmas] = "Tuần Bát Nhật Giáng Sinh",
            //[DominicanSeasons.OctaveOfEpiphany] = "Tuần Bát Nhật Lễ Hiển Linh"
        };

        Dictionary<string, string> _vietnameseDays = new Dictionary<string, string>()
        {
            ["1"] = "Chủ Nhật",                                   // Sunday
            ["2"] = "Thứ Hai",                                    // Monday
            ["3"] = "Thứ Ba",                                     // Tuesday
            ["4"] = "Thứ Tư",                                     // Wednesday
            ["5"] = "Thứ Năm",                                    // Thursday            
            ["6"] = "Thứ Sáu",                                    // Friday              
            ["7"] = "Thứ Bảy",                                     // Saturday 
        };
        Dictionary<string, string> _englishDays = new Dictionary<string, string>()
        {
            ["1"] = "Sunday",
            ["2"] = "Monday",
            ["3"] = "Tuesday",
            ["4"] = "Wednesday",
            ["5"] = "Thursday",
            ["6"] = "Friday",
            ["7"] = "Saturday",
        };

        //string _vietOpening = "Giáo đầu";
        //string _vietDay = "Ngày";
        string _vietWeek = "Tuần";
        string _vietYear = "Năm";
        string _vietAmPrayer = "Kinh Sáng";
        string _vietPmPrayer = "Kinh Chiều";

        public PrayerSeason(Place place, bool testMode)
        {
            _place = place;
            _testMode = testMode;
            _baseLen = _base.Length;
            _weekNo = place.WeekNo;
            _week = Int32.Parse(_weekNo) % 4;
            if (_week == 0)
                _week = 4;

            _dayNo = place.DayNo;
            _morning = place.Morning;
            _sectionOfDay = (_morning) ? "morning" : "evening";
            _dayInMonth = place.DayInMonth;
            _month = place.Month;
            _domSeason = place.DomSeason;
            _filename = place.Filename;

            // Need to sort this oput before it will work on ios
            //if (Device.RuntimePlatform.ToLower().Equals("ios"))
            //{
            //    _singleLE = "\n";
            //    _doubleLE = "\n\n";
            //    _doubleSpacedLE = "\n \n";
            //}

            foreach (PrayerSect prayerSect in baseNames.Keys)
            {
                _baseName[prayerSect] = //(Device.RuntimePlatform == Device.iOS) ?
                                        // baseNames[prayerSect].Replace("_", "") :
                                         baseNames[prayerSect];
            }
        }

        public virtual FileDetails LoadText(PrayerSect pSect)
        {
            FileDetails fileDetails = new FileDetails();

            for (PrayerSect prayerSect = PrayerSect.WordOfGod; prayerSect <= PrayerSect.Conclusion; prayerSect++)
            {
                if (pSect != PrayerSect.AllSections)
                {
                    if (pSect != prayerSect)
                        continue;
                }

                if (_testMode)
                {
                    fileDetails.AddText("<p/><b>"+prayerSect.ToString()+ "</b><p/>");
                }
                    

                if (prayerSect == PrayerSect.Canticles)
                {
                    fileDetails.Add(LoadCanticles(_fileEnd));
                }
                else
                {
                    string filename = string.Format("{0}.{1}.{2}", _base, _baseName[prayerSect], _fileEnd);
                    fileDetails.Add(LoadFile(filename, prayerSect));
                }
            }

            return fileDetails;
        }

        protected FileDetails LoadIntro(string filenamePart2)
        {
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 1. Intro</b><p/>");
            fileDetails.AddText("<b><font color =\"red\">Giáo đầu</font></b><p/>");
            // Load 1.Intro.morning/evening_opening.txt
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Intro]);
            string filename = string.Format("{0}.{1}_opening.txt", filebase, _sectionOfDay);

            fileDetails.Add(LoadFile(filename, PrayerSect.Intro));

            if (_morning)
            {
                filename = string.Format("{0}.{1}", _base, filenamePart2);
                fileDetails.Add(LoadFile(filename, PrayerSect.Intro));
                fileDetails.Add(LoadPsalm(filebase));
            }

            return fileDetails;
        }

        virtual protected FileDetails LoadPraise()
        {
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 2. Praise</b><p/>");

            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Praise]);

            string openHymnFilename = string.Format("{0}.opening_hymn.{1}", filebase, _fileEnd);
            string psalmsFilename = string.Format("{0}.psalms.{1}", filebase, _fileEnd);
            string refrainsFilename = string.Format("{0}.refrains.{1}", filebase, _fileEnd);
            Debug.WriteLine($"{_Tag}.LoadPraise({openHymnFilename}, \n\t\t{psalmsFilename}, \n\t\t{refrainsFilename})");

            fileDetails.Add(LoadFile(openHymnFilename, PrayerSect.Praise));
            fileDetails.Add(LoadRefrainAndPsalms(refrainsFilename, psalmsFilename));

            return fileDetails;
        }

        virtual protected FileDetails LoadWordOfGod()
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.WordOfGod]);
            string filename = string.Format("{0}.{1}", filebase, _fileEnd);

            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 3. Word Of God</b><p/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.WordOfGod));

            return fileDetails;
        }

        virtual protected FileDetails LoadResponse()
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Response]);
            string filename = string.Format("{0}.{1}", filebase, _fileEnd);

            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 4. Response</b><p/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.Response));
            //string text = LoadFile(filename);

            return fileDetails;
        }

        enum CSection { Intro = 0, Yearly, DC };

        virtual protected FileDetails LoadCanticles(string fileEnd)
        {
            CSection csection = CSection.Intro;

            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Canticles]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);

            // canticles r in 1 of 2 forms
            // they can b seperated into an intro, and then 3 sections A, B, and C
            // and we want the intro, and the section which matches the year
            // or, there might not b any sections
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 5. Canticles</b><p/>");
            string[] lines = new string[4];
            string text = ReadFile(filename);
            if (text.StartsWith(FileDetails.Error))
            {
                fileDetails.AddText(text);
                fileDetails.AddFilename(filename);
                return fileDetails;
            }

            int idx = 0;
            string intro = string.Empty;
            string textDC = string.Empty;
            bool bFirstLine = true;
            string[] lineArray = Regex.Split(text, _singleLE);
            foreach (string line in lineArray)
            {
                if (bFirstLine)
                {
                    intro = "<p/><b><font color =\"red\">";           // bold and red for first line
                    intro += line;
                    intro += "</font></b><p/>";
                    bFirstLine = false;
                    continue;
                }

                //Debug.WriteLine($"{line}");
                if (line.StartsWith("A") || line.StartsWith("B") || line.StartsWith("C"))
                {
                    string l = line.Trim();
                    if (l.StartsWith("A.") || l.StartsWith("B.") || l.StartsWith("C.") ||
                        l == "A" || l == "B" || l == "C")
                    {
                        csection = CSection.Yearly;
                        lines[idx] += "<p/>";               // add blank line at end
                        idx++;

                        // chop off the year indicator
                        if (l.StartsWith("A.") || l.StartsWith("B.") || l.StartsWith("C."))
                            l = l.Substring(2).Trim();
                        else
                            l = l.Substring(1).Trim();
                        if (String.IsNullOrEmpty(l))                                // empty - ignore
                            continue;

                        lines[idx] += l + "<br/>";
                        continue;
                    }
                }
                if (line.StartsWith(_DC))
                {
                    if (csection == CSection.Intro)
                    {
                        csection = CSection.DC;
                        textDC = FormatDC2(line) + "<br/>";
                    }
                    else
                    {
                        textDC = FormatDC2(line) + "<br/>";
                        lines[idx] += textDC;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(line.Trim()))
                    {
                        lines[idx] += "<p/>";
                        if (!string.IsNullOrEmpty(textDC))
                        {
                            textDC += "<p/>";
                        }
                    }
                    else
                    {
                        if (csection == CSection.Yearly)
                        {
                            lines[idx] += line + "<br/>";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(textDC))
                            {
                                textDC += line + "<br/>";
                            }
                        }
                    }
                }
            }

            string refrain = string.Empty;
            string canticles = intro;
            if (idx > 0)
            {
                int yearIdx = (_place.YearChar - 'A') + 1;
                refrain = lines[yearIdx];
            }
            else
                refrain = textDC;

            canticles += refrain;
            canticles += "<p/>";

            //fileDetails.AddText($"<p/>Canticles<br/>");                             // **************

            fileDetails.AddText(canticles);

            //fileDetails.AddFilename(filename);                                      // **************

            filename = string.Format("{0}.{1}.txt", filebase, (_morning) ? "benedictus" : "magnificat");
            //fileDetails.AddText($"<p/>{filename}<br/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.Canticles));

            //fileDetails.AddText($"<p/>Refrain<br/>");                               // **************
            if (!string.IsNullOrEmpty(refrain))
            {
                fileDetails.AddText(refrain + "<p/>");
            }


            // **************
            //for (int i = 0; i < 3; i++)
            //{
            //    fileDetails.AddText($"<p/>Refrain {i + 1}<br/>");
            //    fileDetails.AddText(lines[i]);
            //}

            return fileDetails;
        }

        virtual protected FileDetails LoadPrayers()
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Prayers]);
            string filename = string.Format("{0}.{1}", filebase, _fileEnd);

            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 6. Prayers</b><p/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.Prayers));
            //string text = LoadFile(filename);

            return fileDetails;
        }

        virtual protected FileDetails LoadConclusion(string fileEnd)
        {
            string filebase = string.Format("{0}.{1}", _base, _baseName[PrayerSect.Conclusion]);
            string filename = string.Format("{0}.{1}", filebase, fileEnd);

            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - 7. Prayers</b><p/>");
            fileDetails.Add(LoadFile(filename, PrayerSect.Conclusion));
            //string text = LoadFile(filename);

            return fileDetails;
        }

        protected FileDetails LoadFile(string filename)
        {
            return LoadFile(filename, PrayerSect.AllSections);
        }

        protected FileDetails LoadFile(string filename, PrayerSect prayerSect)
        {
            //Debug.WriteLine(_Tag + ".LoadFile( " + filename + ", " + prayerSect.ToString() + " )");
            string text = "";       // "<p/>" + filename.Substring(30) + "<br/>";
            //if (!PrayerPageModel.DisplayFilenamesOnly)
            //{
                string fileText = ReadFile(filename);
                if (fileText.StartsWith(FileDetails.Error))
                {
                    text = fileText;
                }
                else
                {
                    text += HtmlifyText(fileText, prayerSect);
                }
            //}
            FileDetails fileDetails = new FileDetails();
            fileDetails.Add(filename, text);

            return fileDetails;
        }

        protected static string ReadFile(string filename)
        {
            Debug.WriteLine(_Tag + ".ReadFile( " + filename + " )");
            string text = "";
            try
            {
                var assembly = typeof(PrayerSeason).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream(filename))
                {
                    if (null == stream)
                    {
                        throw (new Exception(" not found"));
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            text = sr.ReadToEnd().Trim();
                            text = text.Replace(_doubleLE, _doubleSpacedRN);
                            text = text.Replace(_doubleSpacedRN, _doubleLE);
                            text = text.Replace(_doubleSpacedLE, _doubleSpacedRN);
                            text = text.Replace(_singleRN, _singleLE);
                            text = text.Replace(_tripleCR, _doubleLE);
                            text = text.Replace(_doubleCR, _doubleLE);
                            text = text.Replace(_doubleR, _doubleLE);
                            text = text.Replace(_Dc, _DC);
                            text = text.Replace('Đ', 'Ð');                        // I know they look the same, but they aren't ...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrayerModel prayerModel = FreshIOC.Container.Resolve<IPrayerModel>() as PrayerModel;
                prayerModel.NotFounds.Add(filename.Substring(30));

                Debug.WriteLine(filename + ": Error - File not found" + ex.Message);
                text = FileDetails.Error + " file not found<br>" + filename.Substring(30) + "<p>";
            }

            return text;
        }

        string HtmlifyText(string text, PrayerSect prayerSect)
        {
            string formattedText = "";
            if (string.IsNullOrEmpty(text))
                return formattedText;

            try
            {
                //if (text.Contains("Tv"))
                //    Debug.WriteLine($"\nHtmlifyText Tv - {prayerSect.ToString()}\n{text}");

                if (prayerSect == PrayerSect.Prayers)
                {
                    //Debug.WriteLine($"\nHtmlifyText - {prayerSect.ToString()}\n{text}");
                    formattedText = FormatPrayers(text);
                    return formattedText;
                }
                else if (prayerSect == PrayerSect.Conclusion)
                {
                    // Kinh Lạy Cha has been doubled up in lots of the data files
                    // so this gets rid of the second
                    if (text.StartsWith(_kinhLayCha))
                    {
                        text = text.Substring(_kinhLayCha.Length).Trim();
                    }
                }

                if (text.StartsWith(_DC))
                    text = FormatDC2(text);
                else if (text.StartsWith("X: ") || text.StartsWith("Ð: "))
                    text = FormatXD(text);
                else if (text.StartsWith("XÐ: ") || text.StartsWith("X: ") || text.StartsWith("* "))
                    text = FormatRed(text);
                else
                {
                    formattedText = "<b><font color =\"red\">";           // bold and red for first line

                    //bool bPrint = false;
                    /*char a = text[0];
                    if (a >= '0' && a <= '9')                           // starts with a number
                    {
                        int n = text.IndexOf(_singleLE);
                        string text3 = text.Substring(n + 2);
                        text = text3.TrimStart();
                    }*/
                    int m = text.IndexOf(_singleLE);                      // end of first line
                    if (m > -1)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(text.Substring(0, m));
                        builder.Append("</font></b><p/>");
                        builder.Append(text.Substring(m + 2));
                        text = builder.ToString();
                    }

                    text = FormatDC2(text);
                }

                if (prayerSect == PrayerSect.Intro)
                {
                    // nasty hack to get rid of the 1., and 2. in the text
                    text = text.Replace("1.\r\n", "\r\n").Replace("2.\r\n", "\r\n");
                }
                else if (prayerSect == PrayerSect.Prayers)
                {
                    string colon = ":";
                    string colonLineEnd = ":\r\n";
                    string checkString = colonLineEnd;
                    int i = text.IndexOf(checkString);
                    if (i == -1)
                    {
                        checkString = colon;
                        i = text.IndexOf(checkString);
                    }
                    if (i > -1)
                    {
                        text = text.Insert(i + checkString.Length, "<b>");
                        int j = text.IndexOf(_singleLE, i + checkString.Length + 1);
                        text = (j == -1) ? text + "</b>" : text.Insert(j, "</b>");
                    }
                }
                else if (prayerSect == PrayerSect.Conclusion)
                {
                    text = text.Replace("Lời Nguyện", "<b><font color =\"red\">Lời Nguyện</font></b>");
                    text = text.Replace("Lời nguyện", "<b><font color =\"red\">Lời Nguyện</font></b>");
                }

                text = ConvertLineEnds(text);
                formattedText += text + "<p/>";     // "Cuối Phần</p>";

                //if (_testMode2)
                //    Debug.WriteLine("\nHtmlifyText 2 - \n" + formattedText);
            }
            catch (Exception ex)
            {
                formattedText = $"{_Tag}.Htmlify: error - {ex.ToString()}";
                Debug.WriteLine(formattedText);
            }


            return formattedText;
        }

        string ConvertLineEnds(string text)
        {
            string retText = text.Replace(_doubleLE, "<p/>");
            retText = retText.Replace(_singleLE, "<br/>");
            retText = retText.Replace("<p/><br/>", "<p/>");
            if (!retText.EndsWith("<p/>"))
                retText += "<p/>";

            return retText;
        }

        const string _redBoldCenterStart = "<center><b><font color =\"red\">";
        const string _centerEnd = "</center>";
        const string _lineEnd = "<br/>";
        String FormatPraise(String text)
        {
            //int paraCnt = 0;
            bool bCenterNext = false;
            string retText = "";
            string[] paragraphArray = Regex.Split(text, _doubleLE);

            // iter over the paragraphs
            // center/bold/red first 1 or 2 lines
            foreach (string paragraph in paragraphArray)
            {
                //Debug.WriteLine("FormatPraise\n" + paragraph + "\n");

                if (bCenterNext)
                {
                    retText += _redBoldCenterStart;

                    string[] lineArray = Regex.Split(paragraph, _singleLE);
                    if (lineArray.Count<string>() == 1)
                    {
                        retText += lineArray[0].Trim();
                        retText += _redBoldEnd + _lineEnd;
                    }
                    else
                    {
                        int lineCnt = 0;
                        int numLines = lineArray.Count<string>();
                        foreach (string line in lineArray)
                        {
                            lineCnt++;
                            retText += _lineEnd;
                            if (lineCnt == numLines - 1)
                                retText += _redBoldEnd + _lineEnd;
                        }
                    }
                    retText += _centerEnd + "<p/>";
                    bCenterNext = false;
                }
                else
                if (paragraph.StartsWith("I"))
                {
                    if (paragraph.StartsWith("I.") || paragraph.StartsWith("II.") || paragraph.StartsWith("III."))
                    {
                        retText += "<b>" + paragraph.Replace("I.", "I.</b>");
                        bCenterNext = true;
                    }
                }
                else
                {
                    string text2 = ConvertLineEnds(paragraph);
                    if (text2.StartsWith(_DC))
                        text2 = FormatDC2(text2);
                    retText += text2;
                }
            }

            return retText;
        }


        const string _redBoldStart = "<b><font color =\"red\">";
        const string _redBoldEnd = "</font></b>";
        const string _kinhLayCha = "Kinh Lạy Cha";
        //string[] KinhLayCha = { _kinhLayCha };

        String FormatPrayers(String inText)
        {
            // red/bold first line, bold 3rd, blank line betw each line
            string retText = "";
            try
            {
                List<string> lineList = new List<string>(Regex.Split(inText, _singleLE));
                string lastLine = lineList.Last().Trim();
                if (lastLine != _kinhLayCha)
                {
                    // no longer necessary ???
                    lastLine = lastLine.Replace(_kinhLayCha, "").Trim();
                    lineList[lineList.Count - 1] = lastLine;
                    lineList.Add(_kinhLayCha);
                    //Debug.WriteLine($"FormatPrayers {lastLine}");
                }
                int lineCnt = 0;
                int lineCnt2 = 0;
                int totalLines = lineList.Count;

                foreach (string l in lineList)
                {
                    lineCnt++;
                    string line = l.Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    lineCnt2++;
                    if ((lineCnt2 == 1) || (lineCnt == totalLines))
                    {
                        retText += _redBoldStart;
                        retText += line;
                        retText += _redBoldEnd;
                    }
                    else if (lineCnt2 == 3)
                    {
                        retText += "<b>" + line + "</b>";
                    }
                    else
                    {
                        retText += line;
                    }

                    retText += "<p/>";
                }

            }
            catch (Exception ex)
            {
                retText = $"{_Tag}.FormatPrayers: error - {ex.ToString()}";
                Debug.WriteLine(retText);
            }

            return retText;
        }

        protected FileDetails LoadFile2(string filename)
        {
            Debug.WriteLine(_Tag + ".LoadText( " + filename + " )");
            string text = "";
            FileDetails fileDetails = new FileDetails();

            bool bPrint = false;
            try
            {
                var assembly = typeof(PrayerSeason).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream(filename))
                {
                    if (null != stream)
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            //text = "<br>" + filename.Substring(30) + "<br>";
                            text += "<b><font color =\"red\">";
                            string text2 = sr.ReadToEnd().Trim();
                            if (string.IsNullOrEmpty(text2))
                            {
                                fileDetails.Add(filename, text);
                                return fileDetails;
                                //return text;
                            }
                            char a = text2[0];
                            if (a >= '0' && a <= '9')
                            {
                                int n = text2.IndexOf(_singleLE);
                                string text3 = text2.Substring(n + 2);
                                text2 = text3.TrimStart();
                            }
                            int m = text2.IndexOf(_singleLE);
                            if (m > -1)
                            {
                                StringBuilder builder = new StringBuilder();
                                builder.Append(text2.Substring(0, m));
                                builder.Append("</font></b><p/>");
                                builder.Append(text2.Substring(m + 2));
                                text2 = builder.ToString();
                            }

                            text2 = text2.Replace(_doubleSpacedLE, "<p/>");
                            text2 = text2.Replace(_doubleLE, "<p/>");
                            text2 = text2.Replace(_singleLE, "<br/>");
                            text += text2;

                            text += "<p/>";     // "Cuối Phần</p>";
                        }
                    }
                    else
                    {
                        if (_testMode)
                            text = filename + ": Error - File not found\n";
                        else
                            Debug.WriteLine(filename + ": Error - File not found");
                    }
                }
            }
            catch (Exception ex)
            {
                if (_testMode)
                    text = filename + ": Error - File not found\n";
                else
                    Debug.WriteLine(filename + ": Error - File not found\n" + ex.Message);
            }

            fileDetails.Add(filename, text);

            if (bPrint)
                Debug.WriteLine("if (text2.IndexOf(\"ÐC:\") > -1)\n" + text);


            return fileDetails;
        }

        public FileDetails LoadPsalm(string filebase)
        {
            string psalm = "";
            //if ("3" == _dayNo)
            //    psalm = "psalm23.txt";
            //else if ("4" == _dayNo)
            psalm = "psalm94.txt";
            //else if ("6" == _dayNo)
            //    psalm = "psalm66.txt";
            //else
            //    psalm = "psalm99.txt";
            string filename = string.Format("{0}.psalms.{1}", filebase, psalm);

            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - Psalm</b><p/>");
            fileDetails.Add(LoadFile(filename));
            //string text = LoadFile(filename);

            return fileDetails;
        }

        protected FileDetails LoadRefrainAndPsalms(string refrainFilename, string psalmsFilename)
        {
            FileDetails fileDetails = new FileDetails();
            if (_testMode) fileDetails.AddText($"<p/><b>{_Tag} - RefrainAndPsalms</b><p/>");

            fileDetails.AddFilename(refrainFilename);
            fileDetails.AddFilename(psalmsFilename);

            string refrainText = ReadFile(refrainFilename);
            string psalmsText = ReadFile(psalmsFilename);
            //Debug.WriteLine("PrayerSeason.LoadRefrainAndPsalms: " + psalmsFilename + ", " + psalmsFilename);
            //Debug.WriteLine();

            string text = "";
            if (string.IsNullOrEmpty(refrainText) || string.IsNullOrEmpty(psalmsText) ||
                psalmsText.StartsWith(FileDetails.Error) || refrainText.StartsWith(FileDetails.Error))
            {
                text = string.Format("Refrain/Psalms file(s) not found:");
                if (refrainText.StartsWith(FileDetails.Error))
                    text += "<br/>" + refrainFilename.Substring(30);
                if (psalmsText.StartsWith(FileDetails.Error))
                    text += "<br/>" + psalmsFilename.Substring(30);
                text += "<p/>";
            }
            else
            {
                if (!InterleaveRefrainsAndPsalms(refrainText, psalmsText, ref text))
                    text = string.Format($"Refrain/Psalms files not found:<br/>{refrainFilename},<br/>{psalmsFilename}<p/>");
            }

            //fileDetails.AddText("<br>" + refrainFilename.Substring(30) + "<br>" + psalmsFilename.Substring(30) + "<br>");
            fileDetails.AddText(text);

            return fileDetails;
        }

        bool InterleaveRefrainsAndPsalms(string refrainText, string psalmsText, ref string text)
        {
            //Debug.WriteLine($"\n{_Tag }.InterleaveRefrainsAndPsalms()\n{refrainText}\n\n{psalmsText}\n");

            int idxB = 0;
            int len = 0;
            string keyB = "";
            string heading = ".";

            if (string.IsNullOrEmpty(refrainText) || string.IsNullOrEmpty(psalmsText))
                return false;

            //text = "<b><font color =\"red\">Ca vịnh</font></b><p/>";
            text = "<b>Ca vịnh</b><p/>";
            try
            {
                for (int i = 1; i <= 3; i++)
                {
                    heading = "I" + heading;
                    text += "\n\n<span style=\"text-decoration: underline; color:red\"><b>" + heading + "</b></span>\n";
                    //if (_testMode)
                    //    text += "<br/><b>Refrain</b><br/>";

                    // extract xa. --> xb. from refrainsX.txt
                    len = 4;
                    string keyA = string.Format("{0}a.", i);
                    int idxA = refrainText.IndexOf(keyA, idxB);
                    //if (_testMode2)
                    //    Debug.WriteLine("Refrain Key: " + keyA + "\tIdx: " + idxA.ToString());
                    if (idxA == -1)
                    {
                        len = 3;
                        keyA = string.Format("{0}.", i);
                        idxA = refrainText.IndexOf(keyA);
                        //if (_testMode2)
                        //    Debug.WriteLine("Refrain Key: " + keyA + "\tIdx: " + idxA.ToString());
                    }
                    //len = 0;
                    if (idxA > -1)
                    {
                        idxA += len;           // add 4 so we skip over the actual bx.
                        keyB = string.Format("{0}b.", i);
                        idxB = (idxA < refrainText.Length) ? refrainText.IndexOf(keyB, idxA) : -1;
                        //if (_testMode2)
                        //    Debug.WriteLine("Refrain Key: " + keyB + "\tIdx: " + idxB.ToString());
                        if (idxB > -1)
                        {
                            string refrain = refrainText.Substring(idxA, idxB - idxA).Trim();
                            text += FormatRefrain(refrain);
                        }
                        text += _doubleLE;
                    }

                    //if (i == 3)
                    //    break;
                    //if (_testMode)  text += "<b>Psalm</b><br/>";

                    string key = string.Format("{0}.", i);
                    int idx = psalmsText.IndexOf(key);
                    if (idx > -1)
                    {
                        idx += 3;                   // add 3 so we skip over the actual bx.
                        string key2 = string.Format("{0}.", i + 1);
                        int idx2 = (idx < psalmsText.Length) ? psalmsText.IndexOf(key2, idx) : -1;
                        //if (_testMode2)
                        //    Debug.WriteLine("Psalms Key: " + key2 + "\tIdx: " + idx2.ToString());
                        if (idx2 > -1)
                        {
                            text += psalmsText.Substring(idx, idx2 - idx).Trim();
                        }
                        else if (i == 3)
                        {
                            text += psalmsText.Substring(idx).Trim();
                        }
                        text += _doubleLE;
                    }

                    // extract xb. --> (x+1)a. from refrainsX.txt
                    //if (_testMode)
                    //    text += "<b>Refrain</b><br/>";
                    keyB = string.Format("{0}b.", i);
                    idxB = refrainText.IndexOf(keyB);
                    //if (_testMode2)
                    //    Debug.WriteLine("Refrain Key: " + keyB + "\tIdx: " + idxB.ToString());
                    if (idxB > -1)
                    {
                        string refrain = "";
                        idxB += 4;                      // add 4 so we skip over the actual bx.
                        keyA = string.Format("{0}a.", i + 1);
                        idxA = (idxB < refrainText.Length) ? refrainText.IndexOf(keyA, idxB) : -1;
                        //if (_testMode2)
                        //    Debug.WriteLine("Refrain Key: " + keyA + "\tIdx: " + idxA.ToString());
                        if (idxA == -1)
                        {
                            keyA = string.Format("{0}.", i + 1);
                            idxA = (idxB < refrainText.Length) ? refrainText.IndexOf(keyA, idxB) : -1;
                            //if (_testMode2)
                            //    Debug.WriteLine("Refrain Key: " + keyA + "\tIdx: " + idxA.ToString());
                        }
                        if (idxA > -1)
                        {
                            refrain = refrainText.Substring(idxB, idxA - idxB).Trim();
                        }
                        else if (i == 3)
                        {
                            refrain = refrainText.Substring(idxB).Trim();
                        }
                        text += FormatRefrain(refrain);
                        text += _doubleLE;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.ToString());
                text = ex.ToString();
                return false;
            }

            text = text.Replace(_doubleLE, "<p/>");
            text = text.Replace(_singleLE, "<br/>");
            text = text.Replace("<p/><br/>", "<p/>");

            //if (_testMode2)
            //Debug.WriteLine("InterleaveRefrainsAndPsalms:\n " + text);

            return true;
        }

        string FormatRefrain(string refrain)
        {
            string text = "";
            if (string.IsNullOrEmpty(refrain))
                return text;

            //Debug.WriteLine("{_Tag}.FormatRefrain:\n " + refrain);

            bool bFirst = true;
            //bool bCenterOn = false;
            bool bItalicsOn = false;
            //CultureInfo cultureInfo = new CultureInfo("vi");
            refrain = refrain.Replace("  ", " ").Replace("  ", " ");
            refrain = refrain.Replace(_doubleSpacedLE, _doubleLE);

            bool startsWithTvc = refrain.StartsWith("Tv") || refrain.StartsWith("Tc");

            string[] verseArray = Regex.Split(refrain, _doubleLE);
            foreach (string v in verseArray)
            {
                //string verse = vrs.Trim().Replace('Đ', 'Ð');                                    // I know they look the same, but they aren't ...
                if (string.IsNullOrEmpty(v))
                    continue;
                string verse = v.Trim();
                bool startsWithDC = verse.StartsWith(_DC);
                string text2 = "";

                if (startsWithDC)
                {
                    if (bItalicsOn)
                    {
                        text2 += "</i>";
                        bItalicsOn = false;
                    }
                    text2 += "</center><p/>" + FormatDC(verse);
                }
                else
                {
                    text2 = "<p/>";
                    string[] lines = Regex.Split(verse, _singleLE);
                    if (startsWithTvc)
                    {
                        // red and bold 4 first 2 lines, italics 4 rest of verse
                        //bCenterOn = true;
                        text2 += "\n<center><font color=\"red\"><b>" + lines[0];

                        if (lines.Count() == 1)
                        {
                            text2 += "</b></font></center>\n";
                        }
                        else
                        {
                            for (int i = 1; i < lines.Count(); i++)
                            {
                                text2 += "<br/>" + lines[i];
                                if (i == 1)
                                {
                                    text2 += "</b></font><i>";          // <br/>+ lines[i];
                                    bItalicsOn = true;
                                }
                            }
                            if (lines.Count() > 2)
                            {
                                text2 += "</i>";
                                bItalicsOn = false;
                            }
                            text2 += "</center><br/>\n";
                        }
                    }
                    else
                    {
                        // black, ordinary 4 all of verse
                        for (int i = 0; i < lines.Count(); i++)
                        {
                            if (bFirst)
                            {
                                bFirst = false;
                                text2 += "<b>" + lines[i] + "</b>";
                            }
                            else
                                text2 += "<br/>" + lines[i];
                        }
                    }
                }
                text += text2;
            }

            if (bItalicsOn)
            {
                text += "</i>";
                bItalicsOn = false;
            }
            //if (bCenterOn)
            //{
            //    text += "</center><p/>";
            //    bCenterOn = false;
            //}
            text += "<p/>";


            return text;
        }

        string FormatDC2(string text)
        {
            // iter over string, converting all places where "ÐC:" occurs
            int lastPos = 0;
            string formattedText = "";
            int dcPos = text.IndexOf(_DC, lastPos);
            if (dcPos > -1)
            {
                //Debug.WriteLine("\nFormatDC2( \n"+text+"\n )");
                int c = text.IndexOf(_doubleLE, dcPos);
                if (c == -1)
                {
                    // Canticles, Sunday
                    string format = "<font color =\"red\"><b>ÐC:</b></font>";
                    text = text.Replace(_DC, format);
                    text = text.Replace("A.", "<b>A.</b>");
                    text = text.Replace("B.", "<b>B.</b>");
                    text = text.Replace("C.", "<b>C.</b>");
                    return text;
                }
                while (dcPos > -1)
                {
                    c = text.IndexOf(_doubleLE, dcPos);
                    int len = c - dcPos;
                    string text2 = (len > 0) ? text.Substring(dcPos, len) : text.Substring(dcPos);
                    string text3 = FormatDC(text2);
                    formattedText += text.Substring(lastPos, dcPos - lastPos) + text3;
                    //Debug.WriteLine("\nFormatDC2 loop1( \n" + formattedText + "\n )");
                    if (c > 0)
                    {
                        formattedText += text.Substring(c);
                        //Debug.WriteLine("\nFormatDC2 loop2( \n" + formattedText + "\n )");
                    }

                    lastPos = dcPos + 1;
                    dcPos = text.IndexOf(_DC, lastPos);
                }
                text = formattedText;
                //Debug.WriteLine("\nFormatDC2 2( \n" + text + "\n )");
            }

            return text;
        }

        string FormatDC(string text)
        {
            //Debug.WriteLine("\nFormatDC( \n" + text + "\n )");
            string[] lines = Regex.Split(text, _singleLE);

            // red and italics 4 ÐC:, black and italics 4 rest of verse
            //string text2 = "<i><font color=\"red\">ÐC:</font>";
            string text2 = "<font color =\"red\"><b>ÐC:</b></font>";
            text2 += lines[0].Replace(_DC, "");
            if (lines.Length > 1)
            {
                text2 += "<br/>";
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    text2 += lines[i] + "<br/>";
                }
                text2 += lines[lines.Length - 1];
            }
            text2 += "<p/>";

            /*bool startsWithDC = text.StartsWith("ÐC:");
            string[] dcArray = Regex.Split(text, "ÐC:");
            foreach (string dcText in dcArray)
            {
                if (string.IsNullOrEmpty(dcText))
                    continue;
                string text = "";
                if (bFirst)
                {
                    if (startsWithDC)
                        text = "<i><font color=\"red\">ÐC:</font>";
                    else
                        text = dcText;
                }
                else
                {
                    text = "<i><font color=\"red\">ÐC:</font>";
                }

                if (startsWithDC || !bFirst)
                {
                    int k = dcText.IndexOf(_singleLE);
                    int l = dcText.IndexOf(_doubleLE);
                    if (k > -1)
                    {
                        int len = (k == l) ? 4 : 2;
                        StringBuilder builder = new StringBuilder();
                        builder.Append(dcText.Substring(0, k));
                        Debug.WriteLine("\n\n(text.IndexOf(ÐC: ) > -1)\n" + builder.ToString());
                        builder.Append("</i>").Append((4 == len) ? "<p/>" : "<br/>");
                        Debug.WriteLine("\n\nafter append2\n" + builder.ToString());
                        builder.Append(dcText.Substring(k + len));
                        text += builder.ToString();
                        Debug.WriteLine("\ntext2 = \n" + text + "\n****************");
                    }
                    else
                    {
                        text += dcText + "</i>";
                        text += (l > -1) ? "<p/>" : "<br/>";
                    }
                }

                bFirst = false;

                Debug.WriteLine("\n\nFormatDC\n" + text);

                formattedText += text;
            }*/
            //Debug.WriteLine("\nFormatDC Fin( \n" + text2 + "\n )");

            return text2;
        }

        string FormatXD(string text)
        {
            string[] lines = Regex.Split(text, _singleLE);

            // red and italics 4 ÐC:, black and italics 4 rest of verse
            string formattedText = "";      // "<i><font color=\"red\">";   //    "ÐC:</font>";
            foreach (string line in lines)
            {
                int x = line.IndexOf(": ");
                if (x == -1)
                {
                    formattedText += line + "<br/>";
                    continue;
                }

                formattedText += "<i><font color=\"red\">";
                formattedText += line.Substring(0, x + 2) + "</font>" + line.Substring(x + 2) + "</i><br/>";
            }

            formattedText = formattedText.Substring(0, formattedText.Length - 5) + "<p/>";

            return formattedText;
        }

        string FormatRed(string text)
        {
            // red and bold 4 first 'word', black and italics 4 rest of verse
            string text2 = "<b><font color=\"red\">";
            text = text.TrimStart();
            int i = text.IndexOf(' ');
            text2 += text.Insert(i, "</font><b/>");
            return text2;
        }

        public static string LoadEndHtml()
        {
            string filename = _base + "." + "ending.txt";
            string endHtml = ReadFile(filename);

            return endHtml;
        }

        public string VietnameseName(Place place, string satSunExtra)
        {
            string name = "";
            string morning = (place.Morning) ? _vietAmPrayer : _vietPmPrayer;
            if (place.DomSeason == DailyPrayer.DominicanSeasons.Feasts)
            {
                string amPm = morning;
                if (!string.IsNullOrEmpty(satSunExtra))
                    amPm += " " + satSunExtra;
                amPm += ",";
                name = string.Format($"{place.Title},\r\n{amPm} {_vietYear} {place.YearChar}");
            }
            else
            {
                string season = place.DomSeason.ToString();
                _vietnameseSeason.TryGetValue(place.DomSeason, out season);
                string day = _vietnameseDays[place.DayNo];
                string afterAshWedWeek = (place.DomSeason == DominicanSeasons.Ash_Wednesday_Week) ? _afterAshWedWeek : "";
                //string name = $"{_vietWeek} {place.WeekNo} {season}, {_vietYear} {place.YearChar}<br/>{day}<br/>{morning}";
                name = $"{_vietWeek} {place.WeekNo} {season}\r\n{_vietYear} {place.YearChar}, {day}{afterAshWedWeek}, {morning}{satSunExtra}";
            }

            return name;
        }

        public string EnglishName(Place place, string satSunExtra)
        {
            string name = "";
            string morning = (place.Morning) ? "AmPrayer" : "PmPrayer";
            if (place.DomSeason == DailyPrayer.DominicanSeasons.Feasts)
            {
                string amPm = morning;
                if (!string.IsNullOrEmpty(satSunExtra))
                    amPm += " " + satSunExtra;
                amPm += ",";
                name = string.Format($"{place.Title},\r\n{amPm} {_vietYear} {place.YearChar}");
            }
            else
            {
                string season = place.DomSeason.ToString();
                string day = _englishDays[place.DayNo];
                string afterAshWedWeek = (place.DomSeason == DominicanSeasons.Ash_Wednesday_Week) ? _afterAshWedWeek : "";
                name = $"Week {place.WeekNo} {season}\r\nYear {place.YearChar}, {day}{afterAshWedWeek}, {morning}{satSunExtra}";
            }

            return name;
        }



        protected string GetSeasonString(string sectionOfDay)
        {
            string season = "";
            if (string.IsNullOrEmpty(_filename))
                season = string.Format("{0}_{1}.txt", _domSeason.ToString().ToLower().Replace("_ii", ""), sectionOfDay);
            else
                season = _filename;
            if (!season.EndsWith(".txt"))
            {
                //Debug.WriteLine($"{_Tag}.GetSeasonString() season = {season}");
                season += ".txt";
                //season = season.Replace(".txt", "");
            }

            return season;
        }

        public override string ToString()
        {
            string toString = string.Format("Season: {3}, Week {2}, Day {1}, {0}", _sectionOfDay, _dayNo, _weekNo, _domSeason.ToString());
            toString += string.Format("<br/>Date: {0:00}/{1:00}/{2}", _place.DayInMonth, _place.Month, _place.YearChar);
            //string toString = string.Format("{_sectionOfDay}, Day {_dayNo}, Week {_weekNo}, Season {_domSeason.ToString()}");
            return toString;
        }
    }
}
