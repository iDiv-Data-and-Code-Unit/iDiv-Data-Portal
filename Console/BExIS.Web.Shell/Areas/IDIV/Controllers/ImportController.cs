using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;
using IDIV.Modules.Idiv.UI.Importer;
using Vaiona.Entities.Common;

namespace IDIV.Modules.Idiv.UI.Controllers
{
    public class ImportController : Controller
    {
        // GET: Import
        public ActionResult Index()
        {
            long datasetId = 0;
            int count = 0;
            List<Dictionary<string, string>> publications = MetadataImporter.loadPublications();

            foreach (Dictionary<string, string> d in publications)
            {
                Debug.WriteLine(count + "/" + publications.Count);
                datasetId = createDataset(152, 25, 1);
                addMetadata(datasetId, MetadataImporter.GeneratePublicationXml(d));
                count++;
            }

            if (this.IsAccessible("DDM", "SearchIndex", "ReIndex"))
            {
                var x = this.Run("DDM", "SearchIndex", "ReIndex");
            }

            return View(publications);
        }

        private long createDataset(long dataStructureId, long metadataStructureId, long researchPlanId)
        {
            DatasetManager dm = null;
            DataStructureManager dsm = null;
            ResearchPlanManager rpm = null;
            MetadataStructureManager msm = null;

            try
            {
                dm = new DatasetManager();
                dsm = new DataStructureManager();
                rpm = new ResearchPlanManager();
                msm = new MetadataStructureManager();

                DataStructure ds = dsm.UnStructuredDataStructureRepo.Get(dataStructureId);
                ResearchPlan rp = rpm.Repo.Get(researchPlanId);
                MetadataStructure ms = msm.Repo.Get(metadataStructureId);
                Dataset d = new Dataset();

                d = dm.CreateEmptyDataset(ds, rp, ms);

                if (GetUsernameOrDefault() != "DEFAULT")
                {
                    EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
                    entityPermissionManager.Create<User>(GetUsernameOrDefault(), "Publication", typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                }

                return d.Id;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
                rpm.Dispose();
                msm.Dispose();
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

            return !string.IsNullOrWhiteSpace(username) ? username : "EndNote Importer";
        }

        private XmlDocument addMetadata(long datasetId, XmlDocument metadata)
        {
            DatasetManager dm = null;

            try
            {
                dm = new DatasetManager();
                Dataset d = dm.DatasetRepo.Get(datasetId);

                string path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(d.MetadataStructure.Id, TransmissionType.mappingFileImport, d.MetadataStructure.Name));

                XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.ExternToIntern);
                xmlMapperManager.Load(path_mappingFile, "");

                XmlDocument metadataResult = xmlMapperManager.Generate(metadata, 1, true);

                XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(d.MetadataStructure.Id, XmlUtility.ToXDocument(metadataResult));

                XmlDocument metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

                XmlDocument completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult, metadataXmlTemplate);

                if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    workingCopy.Metadata = completeMetadata;

                    if (workingCopy.StateInfo == null)
                    {
                        workingCopy.StateInfo = new EntityStateInfo();
                        workingCopy.StateInfo.State = DatasetStateInfo.Valid.ToString();
                    }

                    workingCopy.ModificationInfo = new EntityAuditInfo()
                    {
                        Performer = "EndNote Importer",
                        Comment = "Metadata",
                        ActionType = AuditActionType.Create
                    };

                    dm.EditDatasetVersion(workingCopy, null, null, null);
                    dm.CheckInDataset(datasetId, "", GetUsernameOrDefault(), ViewCreationBehavior.None);
                }
                return completeMetadata;
            }
            finally
            {
                dm.Dispose();
            }
            
        }
    }
}