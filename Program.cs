using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Configuration;


namespace BulkLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            var inbox=Configuration.AppSettings["inbox"];
            var outbox=Configuration.AppSettings["outbox"];
            var file=Configuration.AppSettings["file"];
            var format=Configuration.AppSettings["format"];
            var dump=Configuration.AppSettings["multiples"];

            DirectoryInfo inboxFolder = new DirectoryInfo(inbox);
            Console.WriteLine("The inbox directory is " + inboxFolder);
            FileSystemInfo[] inboxFiles = inboxFolder.GetFileSystemInfos(files);
            Console.WriteLine("Number of Files " + inboxFiles.Length);
            foreach (var file in inboxFiles)
            {
                // get exclusive lock by renaming to <file>.<ext>.<processing>
                var procFileName = string.Format("{0}.{1}", file.FullName, "processing");

                if (format == "JSON")
                {
                    if (dump == "ON")
                    {
                        StreamReader reader = File.OpenText(file.FullName);
                        string line;
                        while (line = reader.ReadLine()) != null)
                        {
                            SendFileJSON.Post(Line);
                        }
                        reader.Close();
                    }
                    else {
                        var doc = File.ReadAllText(file.FullName);
                        SendFileJSON.Post(doc);
                    }
                }
                if (format == "XML")
                {
                    if (dump == "ON")
                    {
                        MultiLineXML.Seperate(file.FullName);
                    }
                    else {
                        var doc = File.ReadAllText(file.FullName);
                        SendFileXML.Post(doc);
                    }
                }
                if (format = "CSV")
                {
                    var lines = File.ReadAllLines(file.FullName);
                    string[] headers = lines[0].Split(',').Select(x => x.Trim('\"')).ToArray();

                    var xml = new XElement("TopElement",
                        lines.Where((line, index) => index > 0).Select(line => new XElement("Item",
                            line.Split(',').Select((column, index) => new XElement(headers[index],column)))));
                    
                    xml.Save(@"c:\bulkloader\new.xml");

                    MultiLineXml.Seperate(@"c:\bulkloader\new.xml");

                    File.Move(@"c:\bulkloader\new.xml", outbox);
                }

                var completedFileName = string.Format("{0}", Path.Combine(outbox, fileName));
                File.Move(file.FullName, procFileName);
                File.Move(procFileName, completedFileName);
                Console.WriteLine("All Files have been processed. hit Enter to close.");
                Console.Read();
            }
        }
    }

    public class SendFileJSON
    {
        public static void Post(string message)
        {
            var authkey = Configuration.AppSettings["authkey"];
            var url =Configuration.AppSettings["url"];
            var postRequest =  (HttpWebRequest)WebRequest.Create(url);
            postRequest.ContentType = "application/json; charset=utf-8";
            postRequest.Method = "POST";
            if (authkey.Length > 0)
            {
                postRequest.Headers.Add("Authorization", "Basic " + authkey);
            }

            using (var sw = new StreamWriter(postRequest.GetRequestStream()))
            {
                sw.Write(message);
                sw.Flush();
                sw.Close();
            }
            try
            {
                HttpWebResponse resp= (HttpWebResonse)postRequest.GetResponse();
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("There has been an error dufing JSON post.",e);
            }
        }
    }

    public class SendFileXML
    {
        public static void Post(string mesage)
        {
            var authkey = Configuration.AppSettings["authkey"];
            var url =Configuration.AppSettings["url"];
            var postRequest = (HttpWebRequest)WebRequest.Create(url);
            if (authkey>0)
            {
                postRequest.Headers.Add("Authorization", "Basic " + authkey);
            }
            postRequest.ContentType ="text/xml";
            postRequest.Method = "POST";
            try
            {
                StreamWriter writer = new StreamWriter(postRequest.GetRequestStream());
                writer.WriteLine(message);
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sening XML to  " + url +  ";", e);
            }
            try
            {
                HttpWebRequest resp = (HttpWebResonse)postRequest.GetResponse();
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There has been an error during XML post.", e);
                throw;
            }
        }
    }
    public static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument XDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.load(xmlReader);
            }
            return xmlDocument;
        }
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContext();
                return XDocument.Load(nodeReader);
            }
        }
    }
    public static class MultiLineXML
    {
        public static void Seperate(string messgae)
        {
            StreamReader reader = File.OpenText(message);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                SendFileXML.Post(Line);
            }
            reader.Close();
        }
    }
}