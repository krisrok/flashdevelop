using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ApplicationXMLParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new[] {@"C:\dev\as3\workspaces\backgrounder - Copy\application.xml", @"C:\dev\as3\workspaces\backgrounder - Copy\obj\backgrounderConfig.xml", @"C:\dev\as3\workspaces\backgrounder - Copy\obj\application.xml" };

            if(args.Length != 3)
            {
                Console.WriteLine("Error: Unexpected number of arguments");
                Console.WriteLine(
                    "Usage: ApplicationXMLParser.exe <input-application.xml> <config.xml> <output-application.xml>");

                Environment.Exit(1);
            }

            var appXmlInputFilename = args[0];
            var configXmlFilename = args[1];
            var appXmlOutputFilename = args[2];

            var appXmlDoc = new XmlDocument();
            try
            {
                appXmlDoc.Load(appXmlInputFilename);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading application.xml:", appXmlInputFilename);
                Console.WriteLine(ex.Message);
                Environment.Exit(2);
            }
            
            var nodesWithCondition = appXmlDoc
                .SelectNodes("//node()[@condition]")
                ?.Cast<XmlNode>()
                .Select(n => new KeyValuePair<string, XmlNode>(n.Attributes["condition"].Value, n));
            
            if(nodesWithCondition == null)
            {
                File.Copy(appXmlInputFilename, appXmlOutputFilename);
                Environment.Exit(0);
            }

            var configXmlDoc = new XmlDocument();
            Dictionary<string, string> defineMap = null;
            try
            {
                configXmlDoc.Load(configXmlFilename);
                defineMap = configXmlDoc.SelectNodes("//compiler/define")
                    ?.Cast<XmlNode>()
                    .ToDictionary(n => n.SelectSingleNode("name").InnerText, node => node.SelectSingleNode("value").InnerText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading config.xml:");
                Console.WriteLine(ex.Message);
                Environment.Exit(2);
            }
            
            foreach(var nwc in nodesWithCondition)
            {
                if(defineMap != null && defineMap.ContainsKey(nwc.Key) && defineMap[nwc.Key] == "true")
                {
                    nwc.Value.Attributes.RemoveNamedItem("condition");
                }
                else
                {
                    nwc.Value.ParentNode.RemoveChild(nwc.Value);
                }
            }

            try
            {
                if(Directory.Exists(Path.GetDirectoryName(appXmlOutputFilename)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(appXmlOutputFilename));

                appXmlDoc.Save(appXmlOutputFilename);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing application.xml");
                Console.WriteLine(ex.Message);
                Environment.Exit(3);
            }
        }
    }
}
