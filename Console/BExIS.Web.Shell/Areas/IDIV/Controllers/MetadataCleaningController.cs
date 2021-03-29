using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;
using IDIV.Modules.Idiv.UI.Importer;
using BExIS.Xml.Helpers.Mapping;

namespace IDIV.Modules.Idiv.UI.Controllers
{
    public class MetadataCleaningController : Controller
    {
        // GET: MetadataCleaning
        public void Index()
        {
            DatasetManager datasetManager = null;
            EntityPermissionManager entityPermissionManager = null;
            EntityManager entityManager = null;
            try
            {
                datasetManager = new DatasetManager();
                entityPermissionManager = new EntityPermissionManager();
                entityManager = new EntityManager();

                

                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                
                string xPath = "";

                List<Dataset> datasets = new List<Dataset>();
                List<string> lines = new List<string>();
                

                string dataAccessPolicy = "";
                long entityId = entityManager.Entities.Where(e => e.Name.ToLower().Equals("dataset")).FirstOrDefault().Id;
                
                string line = "Dataset ID;Title;Access Policy;Email";
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), "CleanedDatasets.txt");              
                bool check = false;

                datasets = datasetManager.DatasetRepo.Get().OrderBy(ds => ds.Id).ToList();
                int DsCount = datasets.Count;
                int count = 0;

                lines.Add(line);

                foreach (Dataset ds in datasets)
                {
                    xPath = "";

                    
                    dataAccessPolicy = MappingUtils.GetValuesFromMetadata((int)Key.DataAccessPolicy, LinkElementType.Key, ds.MetadataStructure.Id, XmlUtility.ToXDocument(ds.Versions.OrderByDescending(dv => dv.Id).FirstOrDefault().Metadata)).FirstOrDefault();
                    check = false;

                    if (!String.IsNullOrEmpty(dataAccessPolicy) && (dataAccessPolicy.ToLower().Contains("open") || dataAccessPolicy.ToLower().Contains("public")))
                        check = true;
                    else if (ds.MetadataStructure.Id == 25 && String.IsNullOrEmpty(dataAccessPolicy))
                        check = true;

                    if (check)
                    {
                        List<Mapping> mappings = new List<Mapping>();
                        Mapping temp = null;
                        mappings = MappingUtils.GetMappingsWhereTarget((int)Key.DataAccessPolicy, LinkElementType.Key);
                        foreach (Mapping m in mappings)
                        {                          
                            temp = m;
                            do
                            {
                                if (temp.Parent == null && temp.Source.ElementId == ds.MetadataStructure.Id)
                                {
                                    xPath = m.Source.XPath;
                                    break;
                                }

                                temp = temp.Parent;
                            }
                            while (temp != null);
                        }

                        if (!String.IsNullOrEmpty(xPath) && (dataAccessPolicy != "Open Access" && dataAccessPolicy != "Open (CC BY 4.0)" && !dataAccessPolicy.Contains("(CC BY 4.0)")))
                        {
                            editMetadata(ds.Id, xPath);
                        }
                        EntityPermission entityPermission = entityPermissionManager.Find(null, entityId, ds.Id);

                        if (entityPermission == null)
                        {
                            entityPermissionManager.Create(null, entityId, ds.Id, (int)RightType.Read + (int)RightType.Read);
                        }
                        else
                        {
                            entityPermission.Rights = (int)RightType.Read + (int)RightType.Read;
                            entityPermissionManager.Update(entityPermission);
                        }
                        line = ds.Id + ";" + xmlDatasetHelper.GetInformationFromVersion(ds.Versions.OrderByDescending(dv => dv.Id).FirstOrDefault().Id, NameAttributeValues.title) + ";" + dataAccessPolicy + ";" + MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, ds.MetadataStructure.Id, XmlUtility.ToXDocument(ds.Versions.OrderByDescending(dv => dv.Id).FirstOrDefault().Metadata)).FirstOrDefault();
                        lines.Add(line);
                    }
                }
                System.IO.File.WriteAllLines(path, lines);
                Debug.WriteLine(count++ + "|" + DsCount);

                if (this.IsAccessible("DDM", "SearchIndex", "ReIndex"))
                {
                    var x = this.Run("DDM", "SearchIndex", "ReIndex");
                }
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                entityManager.Dispose();
            }
        }

        private string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private XmlDocument editMetadata (long datasetId, string xPath, XmlDocument metadata = null)
        {

            DatasetManager datasetManager = null;
            try
            {
                datasetManager = new DatasetManager();
                if (datasetManager.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || datasetManager.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                {
                    XDocument mdata = new XDocument();
                    XElement accessPolicy = null;
                    XmlDocument xml = new XmlDocument();

                    List<string> MdXml = new List<string>();

                    DatasetVersion workingCopy = new DatasetVersion();
                    workingCopy = datasetManager.GetDatasetWorkingCopy(datasetId);
                    workingCopy.Materialize();

                    MdXml.Add(workingCopy.Dataset.Id.ToString());
                    MdXml.Add("");
                    MdXml.Add(xPath);
                    MdXml.Add("");
                    MdXml.Add(workingCopy.Metadata.InnerText);

                    if (metadata == null)
                        mdata = XmlUtility.ToXDocument(workingCopy.Metadata);
                    else
                        mdata = XmlUtility.ToXDocument(metadata);

                    accessPolicy = XmlUtility.GetXElementByXPath(xPath, mdata);
                    accessPolicy.Value = "Open Access";
                    xml.LoadXml(mdata.ToString());

                    workingCopy.Metadata = xml;

                    if (workingCopy.StateInfo == null)
                        workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo();

                    workingCopy = datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                    datasetManager.CheckInDataset(workingCopy.Dataset.Id, "Metadata was submited.", GetUsernameOrDefault(), ViewCreationBehavior.None);

                    MdXml.Add("");
                    MdXml.Add(workingCopy.Metadata.InnerText);
                    MdXml.Add("");
                    System.IO.File.WriteAllLines(Path.Combine(AppConfiguration.GetModuleWorkspacePath("IDIV"), datasetId + ".txt"), MdXml);
                    Debug.WriteLine("metadata edit");

                    return workingCopy.Metadata;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        public void repair()
        {
            List<long> datasetIDs = new List<long>();
            long start = 535;
            long end = 1766;
            do
            {
                datasetIDs.Add(start);
                start++;
            }
            while (start <= end);
            repairMetadata(datasetIDs);
        }

        private XmlDocument repairMetadata(List<long> datasetIDs)
        {
            datasetIDs.Sort();
            List<Dictionary<string, string>> publications = MetadataImporter.loadPublications();
            Dataset dataset = new Dataset();
            string xPath = "";
            int count = 0;
            bool check = false;
            List<Mapping> mappings = new List<Mapping>();
            Mapping temp = null;
            XmlDocument metadata = new XmlDocument();
            string path_mappingFile = "";

            DatasetManager datasetManager = null;

            try
            {
                datasetManager = new DatasetManager();

                foreach (Dictionary<string, string> d in publications)
                {
                    check = false;

                    dataset = datasetManager.DatasetRepo.Get(datasetIDs.ElementAt(count));

                    if (dataset.MetadataStructure.Id == 25)
                        check = true;

                    if (check)
                    {
                        mappings = MappingUtils.GetMappingsWhereTarget((int)Key.DataAccessPolicy, LinkElementType.Key);
                        foreach (Mapping m in mappings)
                        {
                            temp = m;
                            do
                            {
                                if (temp.Parent == null && temp.Source.ElementId == dataset.MetadataStructure.Id)
                                {
                                    xPath = m.Source.XPath;
                                    break;
                                }

                                temp = temp.Parent;
                            }
                            while (temp != null);
                        }
                    }

                    metadata = MetadataImporter.GeneratePublicationXml(d);

                    path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(dataset.MetadataStructure.Id, TransmissionType.mappingFileImport, dataset.MetadataStructure.Name));

                    XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.ExternToIntern);
                    xmlMapperManager.Load(path_mappingFile, "");

                    XmlDocument metadataResult = xmlMapperManager.Generate(metadata, 1, true);

                    XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
                    XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(dataset.MetadataStructure.Id, XmlUtility.ToXDocument(metadataResult));

                    XmlDocument metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

                    XmlDocument completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult, metadataXmlTemplate);

                    editMetadata(dataset.Id, xPath, completeMetadata);
                    Debug.WriteLine(count++ + "|" + publications.Count);
                }
                return null;
            }
            finally
            {
                datasetManager.Dispose();
            }          
        }
    }
}