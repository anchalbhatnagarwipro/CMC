using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace RemoveSavedQueryTags
{
    class RemoveSavedQueryTags
    {
        static void Main(string[] args)
        {
            string folderPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

            //foreach (folderPath in args)
            //{
            string[] filePathCollection = Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories);

            foreach (string filePath in filePathCollection)
            {
                XDocument XMLDoc = XDocument.Load(filePath);

                List<XElement> elementColl = (from xml1 in XMLDoc.Descendants("savedqueries")
                                              select xml1).ToList();

                foreach (XElement xmlElement in elementColl)
                {
                    if (xmlElement.FirstNode != null && xmlElement.FirstNode.ToString().StartsWith("<visualization>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var visualization = XMLDoc.Descendants("visualization");
                        XMLDoc = new XDocument(visualization);
                        XMLDoc.Save(filePath);
                    }
                }
            }
            //}
        }
    }
}
