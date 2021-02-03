using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using Vaiona.Web.Extensions;

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

        public ActionResult FAQ()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("FAQ", this.Session.GetTenant());
            return View();
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