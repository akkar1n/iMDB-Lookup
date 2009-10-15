using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ImdbLookup
{
    [Serializable]
    public  class iMDBParser
    {
        public bool found = true;
        private string m_mainlink;
        private string m_country = "";
        private string m_genre = "";
        private string m_language = "";
        private string m_openweekend = "";
        private string m_rating = "0";
        private string m_title = "";
        private string m_votes = "0";
        private string m_year = "0";
        private int m_runtime = 0;

        private Regex m_openingWeekendRegex;
        private Regex m_countryScreensRegex;
        private Regex m_searchHtmlRegex;

        public iMDBParser()
        {
            m_openingWeekendRegex = new Regex(@"<h5>Opening Weekend</h5>\s*(.|\n)*<h5>Gross</h5>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            m_countryScreensRegex = new Regex(@"[0-9,]*\s+\((?<country>.+?)\).+?\s+\((?<screens>[0-9,]*)", RegexOptions.Compiled);
            m_searchHtmlRegex = new Regex(@"<p><b>Popular Titles</b>\s+\(Displaying [0-9]{1,2}\s+Result[s]{0,1}\).+?<a\s+href=""(?<suburl>.+?)""|<p><b>Titles \(Exact Matches\)</b>\s+\(Displaying [0-9]{1,2}\s+Result[s]{0,1}\).+?<a\s+href=""(?<suburl>.+?)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public iMDB Parse(string title)
        {
            string url = "http://www.imdb.com/find?q=";
            title = title.Replace(" ", "%20");
            url = url + title + ";s=all";
            Query(url);

            return new iMDB(m_mainlink, 
                m_country, 
                m_genre, 
                m_language, 
                m_openweekend, 
                m_rating, 
                m_title, 
                m_votes, 
                m_year,
                m_runtime);
        }


        public iMDB ParseUrl(string url)
        {
            Query(url);

            return new iMDB(m_mainlink,
                m_country,
                m_genre,
                m_language,
                m_openweekend,
                m_rating,
                m_title,
                m_votes,
                m_year,
                m_runtime);
        }

        private void AnalyzeBoxOffice()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_mainlink + "business");
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            if (m_mainlink == null)
            {
                m_mainlink = response.ResponseUri.ToString();
            }
            byte[] BufferRead = new byte[1024];

            StringBuilder requestData = new StringBuilder();

            int bytesRead = 0;
            while ((bytesRead = responseStream.Read(BufferRead, 0, 1024)) > 0)
            {
                requestData.Append(Encoding.ASCII.GetString(BufferRead, 0, bytesRead));
            }

            response.Close();
            AnalyzeBoxOfficeHtml(requestData.ToString());
        }


        private void AnalyzeBoxOfficeHtml(string html)
        {
            html = m_openingWeekendRegex.Match(html).ToString();
            foreach (Match match2 in m_countryScreensRegex.Matches(html))
            {
                string str = m_openweekend;
                m_openweekend = str + match2.Groups["country"].Value + ": " + match2.Groups["screens"].Value + " screens.\n";
            }
        }


        private void AnalyzeHtml(string html)
        {
            if (Regex.IsMatch(html, "<b>No Matches.</b>"))
            {
                found = false;
            }
            else if (Regex.IsMatch(html, "<title>IMDb  Search</title>"))
            {
                AnalyzeSearchHtml(html);
            }
            else if (Regex.IsMatch(html, "<title>Business Data for"))
            {
                AnalyzeBoxOfficeHtml(html);
            }
            else
            {
                Match titleYearMatch = new Regex(@"<title>(?<title>.*)\s*\((?<year>\d+)\).*</title>").Match(html);
                m_title = titleYearMatch.Groups["title"].Value;
                m_year = titleYearMatch.Groups["year"].Value;

                Regex genreMatch = new Regex("<a href=\"/Sections/Genres/(?<genre>.{0,20})/\">");
                foreach (Match match in genreMatch.Matches(html))
                {
                    m_genre += string.Format("{0}{1}/", m_genre, match.Groups["genre"].Value);
                }
                m_genre = m_genre.TrimEnd(new [] { '/' });

                Match ratingVotesMatch = new Regex(@"<b>(?<rating>\d.\d)/10</b>.+>(?<votes>\d+(,\d+)?)\s+votes</a>", RegexOptions.Singleline).Match(html);
                m_rating = ratingVotesMatch.Groups["rating"].Value.Replace('.', ',');
                m_votes = ratingVotesMatch.Groups["votes"].Value;

                Match runtimeMatch = new Regex(@"(?<runtime>\d+)\s+min.+").Match(html);
                int.TryParse(runtimeMatch.Groups["runtime"].Value, out m_runtime);

                Regex languageMatch = new Regex("<a href=\"/Sections/Languages/(?<language>.{0,30})/\">");
                foreach (Match match in languageMatch.Matches(html))
                {
                    m_language += string.Format("{0}{1}/", m_language, match.Groups["language"].Value);
                }
                m_language = m_language.TrimEnd(new [] { '/' });

                Regex countryMatch = new Regex("<a href=\"/Sections/Countries/(?<country>.{0,30})/\">");
                foreach (Match match in countryMatch.Matches(html))
                {
                    m_country += string.Format("{0}{1}/", m_country, match.Groups["country"].Value);
                }
                m_country = m_country.TrimEnd(new [] { '/' });

                AnalyzeBoxOffice();
            }
        }

        private void AnalyzeSearchHtml(string html)
        {
            Match match = m_searchHtmlRegex.Match(html);
            string requestUriString = "http://www.imdb.com" + match.Groups["suburl"];
            m_mainlink = requestUriString;

            GetHtmlSite(requestUriString);
        }


        private void Query(string url)
        {
            GetHtmlSite(url);
        }


        private void GetHtmlSite(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            
            if (m_mainlink == null)
            {
                m_mainlink = response.ResponseUri.ToString();
            }
            byte[] BufferRead = new byte[1024];
            StringBuilder requestData = new StringBuilder();

            int bytesRead = 0;
            while ((bytesRead = responseStream.Read(BufferRead, 0, 1024)) > 0)
            {
                requestData.Append(Encoding.ASCII.GetString(BufferRead, 0, bytesRead));
            }
            response.Close();
            AnalyzeHtml(requestData.ToString());
        }
    }
}