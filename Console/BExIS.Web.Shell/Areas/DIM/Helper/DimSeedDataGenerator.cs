﻿
using BExIS.Dim.Services;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class DimSeedDataGenerator
    {
        public static void GenerateSeedData()
        {

            PublicationManager publicationManager = new PublicationManager();
            if (!publicationManager.BrokerRepo.Get().Any(b => b.Name.ToLower().Equals("generic")))
                publicationManager.CreateBroker("Generic", "", "", "", "", "text/csv");

            #region SECURITY
            //workflows = größere sachen, vielen operation
            //operations = einzelne actions

            //1.controller -> 1.Operation


            FeatureManager featureManager = new FeatureManager();

            Feature DataDissemination = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Dissemination"));
            if (DataDissemination == null) DataDissemination = featureManager.Create("Data Dissemination", "Data Dissemination");

            Feature Mapping = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Mapping"));
            if (Mapping == null) Mapping = featureManager.Create("Mapping", "Mapping", DataDissemination);

            Feature Submission = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Submission"));
            if (Submission == null) Submission = featureManager.Create("Submission", "Submission", DataDissemination);


            //worklfows -> create dataset ->
            WorkflowManager workflowManager = new WorkflowManager();

            var operation = new Operation();
            Workflow workflow = new Workflow();
            OperationManager operationManager = new OperationManager();


            #region Help Workflow

            workflow =
                workflowManager.WorkflowRepository
                    .Get()
                    .FirstOrDefault(w => w.Name.Equals("Data Dissemination Help") && w.Feature.Id.Equals(DataDissemination.Id));
            if (workflow == null) workflow = workflowManager.Create("Data Dissemination Help", "", DataDissemination);

            operationManager.Create("DIM", "Help", "*", null, workflow);

            #endregion

            #region Admin Workflow

            workflow =
               workflowManager.WorkflowRepository
                   .Get()
                   .FirstOrDefault(w => w.Name.Equals("Data Dissemination Management") && w.Feature.Id.Equals(DataDissemination.Id));

            if (workflow == null) workflow = workflowManager.Create("Data Dissemination Management", "", DataDissemination);

            operationManager.Create("Dim", "Admin", "*", null, workflow);


            #endregion

            #region Mapping Workflow

            //ToDo add security after Refactoring DIM mapping workflow


            //workflow = new Workflow();
            //workflow.Name = "Mapping";
            //workflowManager.Create(workflow);

            //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            //workflow.Operations.Add(operation);

            //Mapping.Workflows.Add(workflow);

            #endregion

            #region Submission Workflow

            //ToDo add security after Refactoring DIM Submission workflow

            //workflow = new Workflow();
            //workflow.Name = "Submission";
            //workflowManager.Create(workflow);

            //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            //workflow.Operations.Add(operation);

            //Submission.Workflows.Add(workflow);

            #endregion

            #endregion

            ImportPartyTypes();

        }

        private static void ImportPartyTypes()
        {
            //PartyTypeManager partyTypeManager = new PartyTypeManager();
            //var filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BAM"), "partyTypes.xml");
            //XDocument xDoc = XDocument.Load(filePath);
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(xDoc.CreateReader());
            //var partyTypesNodeList = xmlDoc.SelectNodes("//PartyTypes");
            //if (partyTypesNodeList.Count > 0)
            //    foreach (XmlNode partyTypeNode in partyTypesNodeList[0].ChildNodes)
            //    {
            //        var title = partyTypeNode.Attributes["Name"].Value;
            //        //If there is not such a party type
            //        if (partyTypeManager.Repo.Get(item => item.Title == title).Count == 0)
            //        {
            //            //
            //            var partyType = partyTypeManager.Create(title, "Imported from partyTypes.xml", null);
            //            partyTypeManager.AddStatusType(partyType, "Create", "", 0);
            //            foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
            //            {
            //                var customAttrType = customAttrNode.Attributes["type"] == null ? "String" : customAttrNode.Attributes["type"].Value;
            //                var description = customAttrNode.Attributes["description"] == null ? "" : customAttrNode.Attributes["description"].Value;
            //                var validValues = customAttrNode.Attributes["validValues"] == null ? "" : customAttrNode.Attributes["validValues"].Value;
            //                var isValueOptional = customAttrNode.Attributes["isValueOptional"] == null ? true : Convert.ToBoolean(customAttrNode.Attributes["isValueOptional"].Value);
            //                partyTypeManager.CreatePartyCustomAttribute(partyType, customAttrType, customAttrNode.Attributes["Name"].Value, description, validValues, isValueOptional);
            //            }
            //        }
            //        //edit add other custom attr

            //    }

        }
    }
}