using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Mf.Service.WebSpider;
using Mf.Util;
using System.IO;

namespace ContractGrabber
{
    /// <summary>
    /// Contract Grabber
    /// (1) Involves WebSpider libary (by David Cruwis http://www.codeproject.com/Articles/6438/C-VB-Automated-WebSpider-WebRobot) to get all relevant urls
    /// (2) Performs an analyze of record pages in order to find out basic info and file links
    /// (3) Downloads files related to the record and writes record pack on disk
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// A container that keeps string interval data
        /// </summary>
        private struct StringInterval
        {
            internal readonly string First;
            internal readonly string Last;
            internal StringInterval(string first, string last)
            {
                First = first;
                Last = last;
            }
        }

        private string outputDirectory = @"D:\Smlouvy";

        private const string LOG_NAME = "log.txt";
        private const string START_PAGE = "https://portal.gov.cz/portal/rejstriky/data/10013/2014-01.html";
        private const string REQUIRED_URL_BODY = "https://portal.gov.cz/portal/rejstriky/data/";
        private const string FILE_DOWNLOAD_URL = "https://portal.gov.cz/";
        private const string REQUIRED_URL_BODY_WITHOUT = "portal.gov.cz/portal/rejstriky/data/";

        private const string RECORD_PAGE_IDENTIFIER = "rec-";
        private StringInterval INTERVAL_SUBJECT = new StringInterval("<tr title=\"Popis obsahu záznamu.\"><th>Předmět: </th><td><strong>", "</strong>");
        private StringInterval INTERVAL_DATE = new StringInterval("<th>Datum zveřejnění: </th>", "</strong>");
        private StringInterval INTERVAL_AMOUNT = new StringInterval("Částka vč. DPH: </th><td><strong>", "<");
        private StringInterval INTERVAL_ID = new StringInterval("<title>", " ");

        private string[] SELECTION_TAGS = new string[] { " it ", "informati", "aplika", "vývoj", "ware", " sw ", " hw ", " itc ", " ict ", " hp ", " dell", "mail", "web", "kyber", "cyber", " vlákn", "počítač", "wifi", "systém", "síť", "microsoft", "licen", "internet", "app" };
        private const bool USE_SELECTION_TAGS = false;
        private const int NUMBER_OF_PAGES_LIMIT = 100000;

        private int collectionIndex;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Choose button handler
        /// </summary>
        private void buttonChoose_Click(object sender, EventArgs e)
        {
            using (var browser = new FolderBrowserDialog())
            {
                browser.ShowDialog();
                if (!Directory.Exists(browser.SelectedPath))
                    return;
                outputDirectory = browser.SelectedPath;
            }

            label1.Text = "Output dir: " + outputDirectory;
        }

        /// <summary>
        /// Grab button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGrab_Click(object sender, EventArgs e)
        {
            collectionIndex = 0;

            if (Directory.Exists(outputDirectory))
                startSpider(START_PAGE);
        }

        /// <summary>
        /// Starts spider to get the list of all relevant urls
        /// Than analyses pages recognized as a record page
        /// </summary>
        /// <param name="firstUrl"></param>
        private void startSpider(string firstUrl)
        {
            WebSpider spider = new WebSpider(firstUrl, REQUIRED_URL_BODY, NUMBER_OF_PAGES_LIMIT);
            spider.Execute();

            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (DictionaryEntry entry in spider.WebPages)
            {
                var page = ((System.Uri)entry.Key).ToString();
                if (!page.Contains(REQUIRED_URL_BODY_WITHOUT))
                    continue;

                if (page.Contains(RECORD_PAGE_IDENTIFIER))
                    analyzeContent(page);

                builder.AppendLine(page);

                i++;
                if (i % 10 == 0)
                    Console.Out.WriteLine(string.Format("Progress {0} %", Math.Round((i / (double)spider.WebPages.Count * 100)), 2));
            }

            File.WriteAllText(outputDirectory + LOG_NAME, builder.ToString());
            Console.Out.WriteLine("Processed urls: " + collectionIndex);
        }

        /// <summary>
        /// Downloads and parses record page
        /// </summary>
        /// <param name="url"></param>
        private void analyzeContent(string url)
        {
            string content = "";
            using (var webClient = new WebClient())
                content = System.Text.Encoding.UTF8.GetString(webClient.DownloadData(url));

            Console.Out.WriteLine("Analyzing: " + url);

            string subject = findPart(content, INTERVAL_SUBJECT.First, INTERVAL_SUBJECT.Last);
            var loweredSubject = subject.ToLower();

            if ((!USE_SELECTION_TAGS || (USE_SELECTION_TAGS && contains(loweredSubject, SELECTION_TAGS))) && (content.Contains("pdf") || content.Contains("xml")))
            {
                string amount = findPart(content, INTERVAL_AMOUNT.First, INTERVAL_AMOUNT.Last);
                string id = findPart(content, INTERVAL_ID.First, INTERVAL_ID.Last);
                string year = findPart(content, INTERVAL_DATE.First, INTERVAL_DATE.Last);
                year = year.Substring(year.Length - 4, 4); // date -> year

                string[] fileAddresses = findAllPdfs(content).Concat(findAllPdfs(content, ".xml")).Where(x => x.StartsWith("/portal")).Distinct().ToArray();
                try
                {
                    for (int i = 0; i < fileAddresses.Length; i++)
                    {
                        string filePath = outputDirectory + "\\" + cleanFileName(string.Format("{0} {1} ({2}) [{3}{4}{5}] {6}{7}", year, id, amount, i + 1, "-", fileAddresses.Length, subject, System.IO.Path.GetExtension(fileAddresses[i])));
                        if (!System.IO.File.Exists(filePath))
                            using (var client = new WebClient())
                                client.DownloadFile(FILE_DOWNLOAD_URL + fileAddresses[i], filePath);
                    }
                }
                catch
                {
                    Console.Out.WriteLine("Handled Exception");
                }
                collectionIndex++;
            }
            else
                Console.Out.WriteLine("Rejected: " + url + " " + subject);
        }

        /// <summary>
        /// Detects all valid pdf urls
        /// TODO: Regex approach
        private string[] findAllPdfs(string content, string extension = ".pdf")
        {
            string text = content;
            System.Collections.Generic.HashSet<string> pdfUrls = new System.Collections.Generic.HashSet<string>();
            while (text.ToLower().Contains(extension))
            {
                int index = text.ToLower().IndexOf(extension);
                int start = 0;
                for (int i = index; i > 0; i--)
                    if (text[i] == '\"' || text[i] == '\'')
                    {
                        start = i;
                        break;
                    }
                pdfUrls.Add(text.Substring(start + 1, index - (start + 1)) + extension);
                text = text.Substring(index + 5);
            }
            return pdfUrls.ToArray();
        }

        /// <summary>
        /// Remove illegal characters from future file name
        /// </summary>
        private static string cleanFileName(string fileName)
        {
            return System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        /// <summary>
        /// Returns true if any of keys is present in the body
        /// </summary>
        private bool contains(string body, string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
                if (body.Contains(keys[i]))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns a substring dentoted by string interval
        /// </summary>
        private string findPart(string content, string p1, string p2)
        {
            int index = content.IndexOf(p1);
            if (index == -1)
                return "";

            string part = content.Substring(index + p1.Length);
            int index2 = part.IndexOf(p2);
            if (index2 == -1)
                return "";

            return part.Substring(0, index2);
        }


    }
}
