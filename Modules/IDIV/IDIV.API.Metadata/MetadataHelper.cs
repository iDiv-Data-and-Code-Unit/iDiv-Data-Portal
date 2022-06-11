using System.Collections.Generic;

using System.Xml;
using Newtonsoft.Json;

namespace IDIV.API.Metadata
{
    public class MetadataHelper
    {

        public MetadataHelper()
        {
        }

        public static XmlElement createRootElement(XmlDocument doc)
        {
            XmlElement root = doc.CreateElement("iDivDataApiResult");

            XmlAttribute vers = doc.CreateAttribute("apiVersion");
            vers.Value = "1.0.0";
            root.Attributes.Append(vers);

            XmlAttribute url = doc.CreateAttribute("url");
            url.Value = "http://idata.idiv.de";
            root.Attributes.Append(url);
            return root;
        }

        public static XmlElement createMetatdataElement(XmlDocument doc, XmlElement root, long datasetId, long metadataStructureId, string metadataStructureName, XmlNode metadataStructureExtra, XmlDocument metadataXML)
        {

            if (metadataXML != null && metadataXML.InnerText != "")
            {
                XmlElement metadata = doc.CreateElement("Metadata");

                XmlAttribute datasetIdXmlAttribute = doc.CreateAttribute("datasetId");
                datasetIdXmlAttribute.Value = datasetId.ToString();
                metadata.Attributes.Append(datasetIdXmlAttribute);

                XmlAttribute metadataStructureNameXmlAttribute = doc.CreateAttribute("metadataStructureName");
                metadataStructureNameXmlAttribute.Value = metadataStructureName;
                metadata.Attributes.Append(metadataStructureNameXmlAttribute);

                XmlAttribute metadataStructureIdXmlAttribute = doc.CreateAttribute("metadataStructureId");
                metadataStructureIdXmlAttribute.Value = metadataStructureId.ToString();
                metadata.Attributes.Append(metadataStructureIdXmlAttribute);

                XmlAttribute metadataStructureTitlePathXmlAttribute = doc.CreateAttribute("metadataStructureTitlePath");
                XmlAttribute metadataStructureDescriptionPathXmlAttribute = doc.CreateAttribute("metadataStructureDescriptionPath");
                List<XmlNode> metadataStructureExtraNodes = new List<XmlNode>();
                foreach (XmlNode xn in metadataStructureExtra.FirstChild.SelectSingleNode("nodeReferences").ChildNodes)
                {
                    if (xn.Attributes["name"].Value == "title")
                        metadataStructureTitlePathXmlAttribute.Value = xn.Attributes["value"].Value;
                    if (xn.Attributes["name"].Value == "description")
                        metadataStructureDescriptionPathXmlAttribute.Value = xn.Attributes["value"].Value;
                }
                metadata.Attributes.Append(metadataStructureTitlePathXmlAttribute);
                metadata.Attributes.Append(metadataStructureDescriptionPathXmlAttribute);

                metadata.InnerXml = metadataXML.FirstChild.InnerXml;
                return metadata;
            }
            else
            {
                XmlElement metadata = doc.CreateElement("Metadata");

                XmlAttribute datasetIdXmlAttribute = doc.CreateAttribute("datasetId");
                datasetIdXmlAttribute.Value = datasetId.ToString();
                metadata.Attributes.Append(datasetIdXmlAttribute);
                metadata.InnerXml = "<error>404 not found</error>";
                return metadata;
            }
        }

        public static string xmlToJson(XmlDocument doc)
        {
            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
