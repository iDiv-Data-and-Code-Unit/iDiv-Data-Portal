using System.Collections.Generic;
using System.Linq;
using System.IO;
using Vaiona.Utils.Cfg;
using System.Xml;
using System;

namespace IDIV.Modules.Idiv.UI.Importer
{
    public class MetadataImporter
    {
        public static List<Dictionary<string, string>> loadPublications()
        {
            FileStream idivPub = null;
            FileStream iDivDatasetsList = null;

            try
            {
                List<Dictionary<string, string>> publications = new List<Dictionary<string, string>>();
                Dictionary<string, string> rows = null;
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), Path.Combine("import", "idivPub1.txt"));
                idivPub = System.IO.File.Open(path, FileMode.Open);
                StreamReader sr = new StreamReader(idivPub);
                string line = "";
                string key = "";
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        rows = new Dictionary<string, string>();
                        while (!string.IsNullOrEmpty(line))
                        {
                            string[] values = line.Split(':');
                            if (values.Count() == 2)
                            {
                                key = values[0].Trim();
                                rows.Add(key, values[1].Trim());
                            }
                            else if (values.Count() < 2)
                            {
                                rows[key] += "\n" + line.Trim();
                            }
                            else if (values.Count() > 2)
                            {
                                string value = "";
                                key = values[0].Trim();
                                for (int i = 1; i < values.Count(); i++)
                                {
                                    if (i > 1)
                                        value += ": " + values[i];
                                    else
                                        value = values[i];
                                }
                                rows.Add(key, value);
                            }
                            line = sr.ReadLine();
                        }
                        publications.Add(rows);
                    }
                }
                path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), Path.Combine("import", "iDivDatasetsList.txt"));
                iDivDatasetsList = System.IO.File.Open(path, FileMode.Open);
                sr = new StreamReader(iDivDatasetsList);
                line = "";
                key = "";
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        while (!string.IsNullOrEmpty(line))
                        {
                            string[] values = line.Split('\t');
                            if (values.Count() > 2)
                            {
                                foreach (Dictionary<string, string> d in publications)
                                {
                                    if (d["Record Number"] == values[0].Trim())
                                        d.Add("Dataset DOI/URL", values[1].Trim());
                                }
                            }
                            line = sr.ReadLine();
                        }
                    }
                }
                return publications;
            }
            finally
            {
                idivPub.Close();
                iDivDatasetsList.Close();
            }
        }

        public static XmlDocument GeneratePublicationXml(Dictionary<string, string> publication)
        {
            string value = "";

            XmlDocument xml = new XmlDocument();

            XmlNode header = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            xml.PrependChild(header);

            XmlElement Metadata = xml.CreateElement("Metadata");
            Metadata.SetAttribute("xmlns", "http://www.w3.org/2001/XMLSchema");
            xml.AppendChild(Metadata);

            XmlElement DatasetDetails = xml.CreateElement("DatasetDetails");
            Metadata.AppendChild(DatasetDetails);

            XmlElement DatasetPublicationTitle = xml.CreateElement("DatasetPublicationTitle");
            if (publication.TryGetValue("Title", out value))
                DatasetPublicationTitle.InnerText = value;
            DatasetDetails.AppendChild(DatasetPublicationTitle);

            XmlElement Description = xml.CreateElement("Description");
            if (publication.TryGetValue("Abstract", out value))
                Description.InnerText = value;
            DatasetDetails.AppendChild(Description);

            XmlElement DateOfPublication = xml.CreateElement("DateOfPublication");
            if (publication.TryGetValue("Date", out value))
                DateOfPublication.InnerText = value;
            if (publication.TryGetValue("Year", out value))
                DateOfPublication.InnerText += " " + value;
            DatasetDetails.AppendChild(DateOfPublication);

            XmlElement Publication_DOI_Or_URL = xml.CreateElement("Publication_DOI_Or_URL");
            if (publication.TryGetValue("DOI", out value))
            {
                value = value.Replace(" ", "");
                value = value.Replace("\t", "");
                value = value.Replace("\n", "");
                foreach (string s in value.Split(','))
                {
                    if (!String.IsNullOrEmpty(Publication_DOI_Or_URL.InnerText))
                        Publication_DOI_Or_URL.InnerText += "\n" + s.Trim();
                    else
                        Publication_DOI_Or_URL.InnerText = s.Trim();
                }
            }
            if (publication.TryGetValue("URL", out value))
            {
                value = value.Replace(" ", "");
                value = value.Replace("\t", "");
                value = value.Replace("\n", "");
                foreach (string s in value.Split(','))
                {
                    if (!String.IsNullOrEmpty(Publication_DOI_Or_URL.InnerText))
                        Publication_DOI_Or_URL.InnerText += "\n" + s.Trim();
                    else
                        Publication_DOI_Or_URL.InnerText = s.Trim();
                }
            }
            DatasetDetails.AppendChild(Publication_DOI_Or_URL);

            XmlElement Dataset_DOI_Or_URL = xml.CreateElement("Dataset_DOI_Or_URL");
            if (publication.TryGetValue("Dataset DOI/URL", out value))
            {
                value = value.Replace(" ", "");
                value = value.Replace("\t", "");
                value = value.Replace("\n", "");
                foreach (string s in value.Split(new string[] { "http" }, StringSplitOptions.None))
                {
                    if (!String.IsNullOrEmpty(s))
                    {
                        if (!String.IsNullOrEmpty(Dataset_DOI_Or_URL.InnerText))
                            Dataset_DOI_Or_URL.InnerText += "\n" + ("http" + s).Trim();
                        else
                            Dataset_DOI_Or_URL.InnerText = ("http" + s).Trim();
                    }
                }
            }
            DatasetDetails.AppendChild(Dataset_DOI_Or_URL);

            XmlElement Dataset_Access_Policy = xml.CreateElement("Dataset_Access_Policy");
            Dataset_Access_Policy.InnerText = "Open Access";
            //if (publication.TryGetValue("Access Policy", out value))
            //    Dataset_Access_Policy.InnerText = value;
            DatasetDetails.AppendChild(Dataset_Access_Policy);

            XmlElement Keywords = xml.CreateElement("Keywords");
            if (publication.TryGetValue("Keywords", out value))
                Keywords.InnerText = value;
            DatasetDetails.AppendChild(Keywords);

            XmlElement Associated_Groupe = xml.CreateElement("Associated_Groupe");
            if (publication.TryGetValue("PMCID", out value))
                if (value.Substring(0, 3) != "PMC")
                    Associated_Groupe.InnerText = value;
            DatasetDetails.AppendChild(Associated_Groupe);

            XmlElement AuthorDetails = xml.CreateElement("AuthorDetails");
            Metadata.AppendChild(AuthorDetails);

            XmlElement AuthorName = xml.CreateElement("AuthorName");
            if (publication.TryGetValue("Author", out value))
                foreach (string s in value.Split(','))
                {
                    if (!String.IsNullOrEmpty(s))
                    {
                        if (value.Split(',').ToList().IndexOf(s) <= 1)
                        {
                            if (value.Split(',').ToList().First().Equals(s))
                            {
                                AuthorName.InnerText += s.Trim() + ", ";
                            }
                            else
                            {
                                AuthorName.InnerText += s.Trim();
                                break;
                            }
                        }
                    }
                }
            AuthorDetails.AppendChild(AuthorName);

            XmlElement Email = xml.CreateElement("Email");
            if (publication.TryGetValue("Email", out value))
                Email.InnerText = value;
            AuthorDetails.AppendChild(Email);

            XmlElement Author_Status = xml.CreateElement("Author_Status");
            if (publication.TryGetValue("Author_Status", out value))
                Author_Status.InnerText = value;
            else
                Author_Status.InnerText = "Main Author";
            AuthorDetails.AppendChild(Author_Status);

            XmlElement PrimaryContact = xml.CreateElement("PrimaryContact");
            if (publication.TryGetValue("PrimaryContact", out value))
                PrimaryContact.InnerText = value;
            else
                PrimaryContact.InnerText = "true";
            AuthorDetails.AppendChild(PrimaryContact);

            XmlElement Association = xml.CreateElement("Association");
            if (publication.TryGetValue("Association", out value))
                Association.InnerText = value;
            AuthorDetails.AppendChild(Association);

            XmlElement CoauthorList = xml.CreateElement("CoauthorList");
            Metadata.AppendChild(CoauthorList);

            XmlElement Coauthors_Name = xml.CreateElement("Coauthors_Name");
            if (publication.TryGetValue("Author", out value))
                foreach (string s in value.Split(','))
                {
                    if (!String.IsNullOrEmpty(s))
                    {
                        if (value.Split(',').ToList().IndexOf(s) >= 2)
                        {
                            if (value.Split(',').Last() == s)
                                Coauthors_Name.InnerText += s.Trim();
                            else
                                Coauthors_Name.InnerText += s.Trim() + ", ";
                        }
                    }
                }
            CoauthorList.AppendChild(Coauthors_Name);

            string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), Path.Combine("import", publication["Record Number"] + ".xml"));
            xml.Save(path);

            return xml;
        }
    }
}