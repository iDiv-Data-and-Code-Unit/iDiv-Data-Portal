using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace IDIV.Modules.Idiv.UI.Controllers
{
    public class ImportController : Controller
    {
        // GET: Import
        public ActionResult Index()
        {
            List<Dictionary<string, string>> publications = new List<Dictionary<string, string>>();
            Dictionary<string, string> rows = null;
            string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), Path.Combine("import", "idivPub.txt"));
            FileStream csv = null;

            try
            {
                csv = System.IO.File.Open(path, FileMode.Open);
                StreamReader sr = new StreamReader(csv);
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
                return View(publications);
            }
            finally
            {
                csv.Close();
            }
        }
    }
}