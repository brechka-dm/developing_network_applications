//using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace HTTP
{
    class Program
    {
        static void Main(string[] args)
        {
            var addr1 = new Uri("http://fkn.omsu.ru/people/kafedri.htm"); //set URI
            var resp = getRequest(addr1); //get info from URI
            resp = resp.Replace("\n", "").Replace("\r", ""); //delete "\n" and "\r" in response
            var regex = new Regex("<!--(.*?)-->"); //regular expression for finding comments
            var matches = regex.Matches(resp); //find all comments in response
            foreach (Match match in matches) //delete every comment
            {
                resp = resp.Replace(match.Groups[1].Value, "");
            }
            regex = new Regex(@"<font size=4>([^<]*)</a>"); //regular expression for finding "<font size=4>...</a>" in response
            matches = regex.Matches(resp); //search strings
            var kafs = new Kaf[matches.Count]; //create array os structs
            for (int i = 0; i < matches.Count; i++)
            {
                kafs[i] = (new Kaf() { name = matches[i].Groups[1].Value }); //fill sruct "name" field
            }
            regex = new Regex("<a href=\"([^<]*)\"><font size=4>[^<]*</a>"); //regular expression for finding "<a href=...><font size=4>...</a>"
            matches = regex.Matches(resp); //search strings
            for (int i = 0; i < matches.Count; i++)
            {
                kafs[i].link = new Uri(addr1, matches[i].Groups[1].Value); //fill struct "link" field
            }
            for (int i = 0; i < kafs.Length; i++)
            {
                Console.WriteLine("{0} - {1}", i, kafs[i].name); //print all names
            }
            Console.Write("Enter kaf num: "); //ask user to enter number
            int kafn = -1;
            while (!Int32.TryParse(Console.ReadLine(), out kafn) || kafn < 0 || kafn >= kafs.Length) Console.WriteLine("Error, try again");
            resp = getRequest(kafs[kafn].link); //send request 
            resp = resp.Replace("\n", "").Replace("\r", "");
            resp = Regex.Match(resp, "<h1 align=center>Штатн[^<]*</h1>(.*?)</table>").Groups[1].Value; //regular expression for finding "<h1 align=center>Штатн...</h1>...</table>"
            regex = new Regex(@"<font size=4><br><b>([^<]*)</b></font>");
            matches = regex.Matches(resp);
            foreach (Match match in matches)
                Console.WriteLine(match.Groups[1].Value);
            Console.ReadLine();
        }
        public static string getRequest(Uri url) //send GET request to server
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url); //create HttpWebRequest
                httpWebRequest.AllowAutoRedirect = false; //disable redirect
                httpWebRequest.Method = "GET"; //set method GET
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) //create HttpWebResponse
                {
                    using (var stream = httpWebResponse.GetResponseStream()) //create Stream
                    {
                        using (var reader = new StreamReader(stream, Encoding.GetEncoding(1251))) //create StreamReader
                        {
                            return reader.ReadToEnd(); //read response and return
                        }
                    }
                }
            }
            catch
            {
                return String.Empty;
            }
        }
        struct Kaf
        {
            public string name;
            public Uri link;
        }
    }
}
