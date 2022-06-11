using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using System.Text;
using System.Web;
using BExIS.Xml.Helpers;
using BExIS.Utils.Models;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace IDIV.Modules.Idiv.UI.Controllers
{
    public class StatisticController : Controller
    {
        public ActionResult MetadataFieldUsageCount()
        {
            Dictionary<string, double> rows = new Dictionary<string, double>();
            int count = 0;
            XmlMetadataHelper xmlMetadataHelper = new XmlMetadataHelper();
            MetadataStructureManager metadataStructureManager = null;
            try
            {
                metadataStructureManager = new MetadataStructureManager();
                List<MetadataStructure> metadataStructures = metadataStructureManager.Repo.Get().OrderBy(m => m.Id).ToList();
                rows.Add("Number of metadata structures", metadataStructures.Count());
                foreach (MetadataStructure ms in metadataStructures)
                {
                    rows.Add(ms.Name + "(" + ms.Id + ")", -1);
                    rows.Add("Number of datasets using metadata structures " + ms.Name + "(" + ms.Id + ")", ms.Datasets.Count());
                    rows.Add("Usage of Fields in metadata structures " + ms.Name + "(" + ms.Id + ")", -1);
                    List<SearchMetadataNode> searchMetadataNodes = xmlMetadataHelper.GetAllXPathsOfSimpleAttributes(ms.Id);
                    int fieldsUsed = 0;
                    foreach (SearchMetadataNode smn in searchMetadataNodes)
                    {
                        count = 0;
                        foreach (Dataset ds in ms.Datasets)
                        {
                            DatasetVersion dsv = ds.Versions.OrderBy(d => d.Id).Last();
                            if (dsv.Metadata != null && !String.IsNullOrEmpty(dsv.Metadata.SelectSingleNode(smn.XPath).InnerText))
                                count++;
                        }
                        rows.Add("(" + ms.Id + ")" + smn.DisplayName, count);
                        if (count > 0)
                            fieldsUsed++;
                    }
                    rows.Add("Field Usage of metadata structures " + ms.Name + "(" + ms.Id + ")", -1);
                    rows.Add("Fields in metadata structures " + ms.Name + "(" + ms.Id + ")", searchMetadataNodes.Count());
                    rows.Add("Fields used in metadata structures " + ms.Name + "(" + ms.Id + ")", fieldsUsed);
                    rows.Add("Fields used/fields in metadata structures " + ms.Name + "(" + ms.Id + ")", Math.Round((double)fieldsUsed / (double)searchMetadataNodes.Count(), 3));
                }
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
            return View("Statistic", rows);
        }

        public ActionResult Datasets()
        {
            Dictionary<string, double> rows = new Dictionary<string, double>(); ;
            
            DatasetManager datasetManager = null;
            DataStructureManager dataStructureManager = null;
            try
            {
                datasetManager = new DatasetManager();
                dataStructureManager = new DataStructureManager();
                List<Dataset> datasets = datasetManager.DatasetRepo.Get().Where(ds => ds.Status.Equals(DatasetStatus.CheckedIn)).ToList();
                rows.Add("Number of datasets", datasets.Count());
                List<long> dataStructureIds = dataStructureManager.StructuredDataStructureRepo.Get().Select(sds => sds.Id).ToList();
                datasets = datasetManager.DatasetRepo.Get().Where(ds => dataStructureIds.Contains(ds.DataStructure.Id) && ds.Status.Equals(DatasetStatus.CheckedIn)).ToList();
                rows.Add("Number of structured datasets", datasets.Count());
                rows.Add("Number of structured datasets with data", datasets.Where(ds => datasetManager.GetDatasetVersionEffectiveTupleCount(datasetManager.GetDatasetLatestVersion(ds)) != 0).Count());
                rows.Add("Number of structured datasets without data", datasets.Where(ds => datasetManager.GetDatasetVersionEffectiveTupleCount(datasetManager.GetDatasetLatestVersion(ds)) == 0).Count());
                double average = 0;
                long min = 0;
                long max = 0;
                long total = 0;
                foreach (Dataset ds in datasets)
                {
                    long temp = datasetManager.GetDatasetVersionEffectiveTupleCount(datasetManager.GetDatasetLatestVersion(ds));
                    if (min == 0 || min > temp)
                        min = temp;
                    if (max == 0 || max < temp)
                        max = temp;

                    total += temp;
                }
                if(total != 0 && datasets.Count() != 0)
                    average = (double)total / (double)datasets.Count();
                rows.Add("Minimum records per structured datasets with data", min);
                rows.Add("Average records per structured datasets with data", Math.Round(average, 3));
                rows.Add("Maximum records per structured datasets with data", max);
                rows.Add("Total records per structured datasets with data", total);
                average = 0;
                min = 0;
                max = 0;
                total = 0;
                foreach (Dataset ds in datasets)
                {
                    long temp = dataStructureManager.StructuredDataStructureRepo.Get(ds.DataStructure.Id).Variables.Count();
                    if (min == 0 || min > temp)
                        min = temp;
                    if (max == 0 || max < temp)
                        max = temp;

                    total += temp;
                }
                if (total != 0 && datasets.Count() != 0)
                    average = (double)total / (double)datasets.Count();
                rows.Add("Minimum variables per structured datasets with data", min);
                rows.Add("Average variables per structured datasets with data", Math.Round(average, 3));
                rows.Add("Maximum variables per structured datasets with data", max);
                rows.Add("Total variables per structured datasets with data", total);

                dataStructureIds = dataStructureManager.UnStructuredDataStructureRepo.Get().Select(sds => sds.Id).ToList();
                datasets = datasetManager.DatasetRepo.Get().Where(ds => dataStructureIds.Contains(ds.DataStructure.Id) && ds.Status.Equals(DatasetStatus.CheckedIn)).ToList();
                rows.Add("Number of unstructured datasets", datasets.Count());
                rows.Add("Number of unstructured datasets with data", datasets.Where(ds => datasetManager.GetDatasetLatestVersion(ds).ContentDescriptors.Count() != 0).Count());
                rows.Add("Number of unstructured datasets without data", datasets.Where(ds => datasetManager.GetDatasetLatestVersion(ds).ContentDescriptors.Count() == 0).Count());
            }
            finally
            {
                datasetManager.Dispose();
                dataStructureManager.Dispose();
            }
            return View("Statistic",rows);
        }

        public ActionResult DataStructures()
        {
            Dictionary<string, double> rows = new Dictionary<string, double>(); ;

            DatasetManager datasetManager = null;
            DataStructureManager dataStructureManager = null;
            DataContainerManager dataContainerManager = null;
            try
            {
                datasetManager = new DatasetManager();
                dataStructureManager = new DataStructureManager();
                dataContainerManager = new DataContainerManager();
                List<StructuredDataStructure> structuredDataStructures = dataStructureManager.StructuredDataStructureRepo.Get().ToList();
                List<UnStructuredDataStructure> unStructuredDataStructures = dataStructureManager.UnStructuredDataStructureRepo.Get().ToList();
                rows.Add("Number of data structures", structuredDataStructures.Count() + unStructuredDataStructures.Count());
                rows.Add("Number of structured data structures (table)", structuredDataStructures.Count());
                rows.Add("Number of unstructured data structures (file)", unStructuredDataStructures.Count());
                double average = 0;
                long min = 0;
                long max = 0;
                long total = 0;
                foreach (StructuredDataStructure sds in structuredDataStructures)
                {
                    long temp = sds.Variables.Count();
                    if (min == 0 || min > temp)
                        min = temp;
                    if (max == 0 || max < temp)
                        max = temp;

                    total += temp;
                }
                if (total != 0 && structuredDataStructures.Count() != 0)
                    average = (double)total / (double)structuredDataStructures.Count();
                rows.Add("Minimum variables per structured data structure", min);
                rows.Add("Average variables per structured data structure", Math.Round(average, 3));
                rows.Add("Maximum variables per structured data structure", max);
                rows.Add("Total variables per structured data structure", total);
                average = 0;
                min = 0;
                max = 0;
                total = 0;
                foreach (StructuredDataStructure sds in structuredDataStructures)
                {
                    long temp = sds.Datasets.Where(ds=> ds.Status.Equals(DatasetStatus.CheckedIn)).Count();
                    if (min == 0 || min > temp)
                        min = temp;
                    if (max == 0 || max < temp)
                        max = temp;

                    total += temp;
                }
                foreach (UnStructuredDataStructure sds in unStructuredDataStructures)
                {
                    long temp = sds.Datasets.Where(ds => ds.Status.Equals(DatasetStatus.CheckedIn)).Count();
                    if (min == 0 || min > temp)
                        min = temp;
                    if (max == 0 || max < temp)
                        max = temp;

                    total += temp;
                }
                if (total != 0 && structuredDataStructures.Count() + unStructuredDataStructures.Count() != 0)
                    average = (double)total / (double)structuredDataStructures.Count() + (double)unStructuredDataStructures.Count();
                rows.Add("Minimum datasets per structured data structure", min);
                rows.Add("Average datasets per structured data structure", Math.Round(average, 3));
                rows.Add("Maximum datasets per structured data structure", max);
                rows.Add("Total datasets per structured data structure", total);

                List<DataAttribute> dataAttributes = dataContainerManager.DataAttributeRepo.Get().ToList();
                rows.Add("Number of data attributes", dataAttributes.Count());
                List<DataAttribute> usedDataAttributes = dataAttributes.Where(da => structuredDataStructures.Where(sds => sds.Variables.Where(v => v.DataAttribute.Id.Equals(da.Id)).Count() > 0).Count() > 0).ToList();
                rows.Add("Number of used data attributes", usedDataAttributes.Count());
                List<DataAttribute> reusedDataAttributes = usedDataAttributes.Where(da => structuredDataStructures.Where(sds => sds.Variables.Where(v => v.DataAttribute.Id.Equals(da.Id)).Count() > 0).Count() > 1).ToList();
                rows.Add("Number of reused data attributes", reusedDataAttributes.Count());
                rows.Add("Number of reused data attributes/used data attributes", Math.Round((double)reusedDataAttributes.Count() / (double)usedDataAttributes.Count(), 3));
                rows.Add("Number of reused data attributes/total data attributes", Math.Round((double)reusedDataAttributes.Count() / (double)dataAttributes.Count(), 3));
            }
            finally
            {
                datasetManager.Dispose();
                dataStructureManager.Dispose();
                dataContainerManager.Dispose();
            }
            return View("Statistic", rows);
        }
    }
}