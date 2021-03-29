using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using Vaiona.Web.Extensions;
using System.IO;
using System.Web;
using BExIS.IO;
using Vaiona.Utils.Cfg;
using BExIS.Ddm.Providers.LuceneProvider.Helpers;

namespace IDIV.Modules.Idiv.UI.Controllers
{
    public class ContentController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());
            return View();
        }

        public ActionResult DataPolicy()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Data Policy", this.Session.GetTenant());
            return View();
        }

        public ActionResult QuickGuide()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Quick Guide", this.Session.GetTenant());
            return View();
        }

        public FileResult getFile(string path)
        {
            path = Server.UrlDecode(path);
            if (BExIS.IO.FileHelper.FileExist(Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), path)))
            {
                try
                {
                    path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), path);
                    FileInfo fileInfo = new FileInfo(path);
                    return File(path, MimeMapping.GetMimeMapping(fileInfo.Name), fileInfo.Name);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public FileResult getFileStreamResult(string path)
        {

            path = Server.UrlDecode(path);
            if (BExIS.IO.FileHelper.FileExist(Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), path)))
            {
                try
                {
                    path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), path);
                    FileInfo fileInfo = new FileInfo(path);
                    return new FileStreamResult(new FileStream(path, FileMode.Open), MimeMapping.GetMimeMapping(fileInfo.Name));
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public ActionResult FAQ()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("FAQ", this.Session.GetTenant());
            return View();
        }
    
        public ActionResult sharingPolicyFAQ()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Data Sharing and Usage Policy", this.Session.GetTenant());
            return View();
        }

        public void Helpdesk()
        {
            Response.Redirect("http://bdusupport.idiv.de/");
        }

        public ActionResult Contact()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Contact BDU", this.Session.GetTenant());
            return View();
        }

        public ActionResult TermsOfCondition()
        {
            return View();
        }

        public ActionResult MyDatasets()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Dashboard", this.Session.GetTenant());
            return View();
        }

        public ActionResult Databases()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Databases", this.Session.GetTenant());
            return View();
        }

        public ActionResult Home()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());
            //List<long> ids = new List<long>();
            //IList<DataTuple> dts = new List<DataTuple>();
            //DatasetManager dsm = new DatasetManager();

            //ids = dsm.DataTupleRepo.Query().Select(p => p.Id).ToList();
            //ids.Sort();

            //for (int i = 0; i <= ids.Count; i+=1000)
            //{
            //    dts = new List<DataTuple>();
            //    dts = getDataTuplePage(ids, i);
            //    foreach (DataTuple dt in dts)
            //    {
            //        Debug.WriteLine(dt.Id + "|" + dt.XmlVariableValues.InnerXml);
            //    }
            //}

            return PartialView("_home");
        }

        public List<DataTuple> getDataTuplePage(List<long> ids, int offset)
        {
            Debug.WriteLine(offset);
            DatasetManager dsm = new DatasetManager();
            IList<DataTuple> dts = new List<DataTuple>();
            DataTuple dt = new DataTuple();
            for (int i = 0 + offset; i < 1000 + offset; i++)
            {
                dt = dsm.DataTupleRepo.Query().Where(p => p.Id == ids[i]).ToList().FirstOrDefault();
                
                if (dt != null)
                    dts.Add(dsm.DataTupleRepo.Query().Where(p => p.Id == ids[i]).ToList().FirstOrDefault());
                else
                    Debug.WriteLine(dt);
            }
            return dts.ToList();
        }
    }
}