using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

using MvcContrib.ActionResults;
using IDIV.API.Metadata;
using System.Xml;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Ddm.Api;
//using BExIS.Ddm.Model;
using Vaiona.IoC;
using System.Data;
using BExIS.Utils.Models;

namespace IDIV.Modules.Idiv.UI.Controllers

{
    public class MetadataController : Controller
    {

        //
        // GET: /IDIV/Metadata/

        public ActionResult get(string type, string ids, string keywords)
        {
            XmlDocument doc = new XmlDocument();

            if (ids != null)
            {
                if (keywords != null)
                    doc = getByIdFilteredXml(keywords, ids);
                else
                    doc = getByIdXml(ids);
            }
            else
            {
                if (keywords != null)
                    doc = getFilteredXml(keywords);
                else
                    doc = getAllXml();
            }

            if (type != null)
            {
                if (type.ToLower() == "xml")
                    return new XmlResult(doc);
                if (type.ToLower() == "json")
                    return new ContentResult { Content = MetadataHelper.xmlToJson(doc), ContentType = "application/json" };

                return new ContentResult { Content = "400 bad request (return type not supported)" };
            }
            else
            {
                return new XmlResult(doc);
            }
        }

        public XmlDocument getAllXml()
        {
            DatasetManager datasetmanager = new DatasetManager();
            List<DatasetVersion> datasets = datasetmanager.GetDatasetLatestVersions();

            XmlDocument doc = new XmlDocument();
            XmlElement root = MetadataHelper.createRootElement(doc);

            foreach (DatasetVersion dsv in datasets)
            {
                root.AppendChild(MetadataHelper.createMetatdataElement(doc, root, dsv.Dataset.Id, dsv.Dataset.MetadataStructure.Id, dsv.Dataset.MetadataStructure.Name, dsv.Dataset.MetadataStructure.Extra, dsv.Metadata));
            }
            doc.AppendChild(root);

            return doc;
        }

        public XmlDocument getByIdXml(string ids)
        {
            DatasetManager datasetmanager = new DatasetManager();
            DatasetVersion datasetVersion = new DatasetVersion();

            if (ids == null)
                ids = "";

            String[] datasetIds = ids.Split(',');

            List<long> tempIds = datasetmanager.GetDatasetLatestIds();

            XmlDocument doc = new XmlDocument();
            XmlElement root = MetadataHelper.createRootElement(doc);

            foreach (string s in datasetIds)
            {
                try
                {
                    if (tempIds.Contains(Convert.ToInt64(s)))
                    {
                        datasetVersion = datasetmanager.GetDatasetLatestVersion(Convert.ToInt64(s));
                        root.AppendChild(MetadataHelper.createMetatdataElement(doc, root, datasetVersion.Dataset.Id, datasetVersion.Dataset.MetadataStructure.Id, datasetVersion.Dataset.MetadataStructure.Name, datasetVersion.Dataset.MetadataStructure.Extra, datasetVersion.Metadata));
                    }
                }
                catch
                { }
            }
            doc.AppendChild(root);

            return doc;
        }

        public XmlDocument getFilteredXml(string keywords)
        {

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            if (keywords == null)
                keywords = "";

            string[] kwords = keywords.Split(',');

            foreach (string s in kwords)
            {
                if (!provider.WorkingSearchModel.CriteriaComponent.ContainsSearchCriterion("All", s, SearchComponentBaseType.Category))
                {
                    provider.WorkingSearchModel.UpdateSearchCriteria("All", s, SearchComponentBaseType.Category);
                }
            }

            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            XmlDocument doc = new XmlDocument();
            XmlElement root = MetadataHelper.createRootElement(doc);

            DataTable dt = provider.WorkingSearchModel.ResultComponent.ConvertToDataTable();

            DatasetManager datasetmanager = new DatasetManager();
            DatasetVersion datasetVersion = new DatasetVersion();

            foreach (DataRow r in dt.Rows)
            {
                datasetVersion = datasetmanager.GetDatasetLatestVersion(Convert.ToInt64(r.ItemArray.First()));
                root.AppendChild(MetadataHelper.createMetatdataElement(doc, root, datasetVersion.Dataset.Id, datasetVersion.Dataset.MetadataStructure.Id, datasetVersion.Dataset.MetadataStructure.Name, datasetVersion.Dataset.MetadataStructure.Extra, datasetVersion.Metadata));
            }
            doc.AppendChild(root);

            return doc;
        }

        public XmlDocument getByIdFilteredXml(string keywords, string ids)
        {
            if (ids == null)
                ids = "";

            String[] datasetIds = ids.Split(',');

            if (keywords == null)
                keywords = "";

            string[] kwords = keywords.Split(',');

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            foreach (string s in kwords)
            {
                if (!provider.WorkingSearchModel.CriteriaComponent.ContainsSearchCriterion("All", s, SearchComponentBaseType.Category))
                {
                    provider.WorkingSearchModel.UpdateSearchCriteria("All", s, SearchComponentBaseType.Category);
                }
            }

            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            XmlDocument doc = new XmlDocument();
            XmlElement root = MetadataHelper.createRootElement(doc);

            DataTable dt = provider.WorkingSearchModel.ResultComponent.ConvertToDataTable();

            DatasetManager datasetmanager = new DatasetManager();
            DatasetVersion datasetVersion = new DatasetVersion();

            foreach (DataRow r in dt.Rows)
            {
                try
                {
                    if (datasetIds.Contains(r.ItemArray.First().ToString()))
                    {
                        datasetVersion = datasetmanager.GetDatasetLatestVersion(Convert.ToInt64(r.ItemArray.First()));
                        root.AppendChild(MetadataHelper.createMetatdataElement(doc, root, datasetVersion.Dataset.Id, datasetVersion.Dataset.MetadataStructure.Id, datasetVersion.Dataset.MetadataStructure.Name, datasetVersion.Dataset.MetadataStructure.Extra, datasetVersion.Metadata));
                    }
                }
                catch
                { }

            }
            doc.AppendChild(root);

            return doc;
        }
    }
}
