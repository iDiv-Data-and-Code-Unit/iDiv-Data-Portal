using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class DataCiteDoiController : BaseController
    {
        public ActionResult index()
        {
            List<PublicationModel> model = new List<PublicationModel>();
            PublicationManager publicationManager = null;
            try
            {
                publicationManager = new PublicationManager();
                Broker broker = publicationManager.RepositoryRepo.Get().Where(b => b.Name.ToLower().Equals(ConfigurationManager.AppSettings["doiProvider"].ToLower())).FirstOrDefault().Broker;
                List<Publication> publications = publicationManager.GetPublication().Where(p => p.Broker.Name.ToLower().Equals(broker.Name.ToLower())).ToList();

                foreach (Publication p in publications)
                {
                    model.Add(new PublicationModel()
                    {
                        Broker = new BrokerModel(broker.Name, new List<string>() { p.Repository.Name }, broker.Link),
                        DataRepo = p.Repository.Name,
                        DatasetVersionId = p.DatasetVersion.Id,
                        CreationDate = p.Timestamp,
                        ExternalLink = p.ExternalLink,
                        FilePath = p.FilePath,
                        Status = p.Status
                       // Doi = p.Doi
                    });
                }
            }
            finally
            {
                publicationManager.Dispose();
            }

            return View(model);
        }

        public ActionResult _grantDoi(long datasetVersionId)
        {
            PublicationManager publicationManager = null;
            DatasetManager datasetManager = null;
            EntityPermissionManager entityPermissionManager = null;
            EntityManager entityManager = null;

            try
            {
                datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                long versionNo = datasetManager.GetDatasetVersions(datasetVersion.Dataset.Id).OrderBy(d => d.Timestamp).Count();

                publicationManager = new PublicationManager();
                Publication publication = publicationManager.GetPublication().Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + datasetVersion.Dataset.Id).ToString()).ToString();
                
                string doi = new DataCiteDoiHelper().issueDoi(datasetVersion, datasetUrl, versionNo);

                publication.DatasetVersion = datasetVersion;
              //  publication.Doi = doi;
                publication.ExternalLink = doi;
                publication.Status = "DOI Registered";

                publication = publicationManager.UpdatePublication(publication);

                EmailService es = new EmailService();
                List<string> tmp = null;
                string title = new XmlDatasetHelper().GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                string subject = "DOI Request for Dataset " + title + "(" + datasetVersion.Dataset.Id + ")";
                string body = "<p>DOI reqested for dataset <a href=\"" + datasetUrl + "\">" + title + "(" + datasetVersion.Dataset.Id + ")</a>, was granted by the Datamanager.</p>" +
                    "<p>The doi is<a href=\"https://doi.org/"+ doi +"\">" + doi + "</a></p>";

                tmp = new List<string>();
                tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                foreach (string s in tmp)
                {
                    string e = s.Trim();
                    es.Send(subject, body, e);
                }

                entityPermissionManager = new EntityPermissionManager();
                entityManager = new EntityManager();
                long entityId = entityManager.Entities.Where(e => e.Name.ToLower().Equals("dataset")).FirstOrDefault().Id;

                EntityPermission entityPermission = entityPermissionManager.Find(null, entityId, datasetVersion.Dataset.Id);

                if (entityPermission == null)
                {
                    entityPermissionManager.Create(null, entityId, datasetVersion.Dataset.Id, (int)RightType.Read);
                }
                else
                {
                    entityPermission.Rights = (int)RightType.Read;
                    entityPermissionManager.Update(entityPermission);
                }

                if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                {
                    var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetVersion.Dataset.Id } });
                }

                return PartialView("_requestRow", new PublicationModel()
                {
                    Broker = new BrokerModel(publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                    DataRepo = publication.Repository.Name,
                    DatasetVersionId = publication.DatasetVersion.Id,
                    CreationDate = publication.Timestamp,
                    ExternalLink = publication.ExternalLink,
                    FilePath = publication.FilePath,
                    Status = publication.Status
                    //Doi = publication.Doi
                });
            }
            finally
            {
                publicationManager.Dispose();
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                entityManager.Dispose();
            }
        }

        public ActionResult _denyDoi(long datasetVersionId)
        {
            PublicationManager publicationManager = null;
            DatasetManager datasetManager = null;

            try
            {
                datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                publicationManager = new PublicationManager();
                Publication publication = publicationManager.GetPublication().Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                publication.Status = "DOI Denied";

                publication = publicationManager.UpdatePublication(publication);

                EmailService es = new EmailService();
                List<string> tmp = null;
                string title = new XmlDatasetHelper().GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                string subject = "DOI Request for Dataset " + title + "(" + datasetVersion.Dataset.Id + ")";
                string body = "<p>DOI reqested for dataset " + title + "(" + datasetVersion.Dataset.Id + "), was denied by the Datamanager.</p>";

                tmp = new List<string>();
                tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                foreach (string s in tmp)
                {
                    string e = s.Trim();
                    es.Send(subject, body, e);
                }

                return PartialView("_requestRow", new PublicationModel()
                {
                    Broker = new BrokerModel(publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                    DataRepo = publication.Repository.Name,
                    DatasetVersionId = publication.DatasetVersion.Id,
                    CreationDate = publication.Timestamp,
                    ExternalLink = publication.ExternalLink,
                    FilePath = publication.FilePath,
                    Status = publication.Status
                    //Doi = publication.Doi
                });
            }
            finally
            {
                publicationManager.Dispose();
                datasetManager.Dispose();
            }
        }
    }
}
