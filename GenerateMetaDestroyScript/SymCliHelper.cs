using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GenerateMetaDestroyScript
{
    class SymCliHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public XmlDocument runSymCliCommand(string command, string args)
        {
            Process cliProcess = new Process();
            cliProcess.StartInfo.UseShellExecute = false;
            cliProcess.StartInfo.RedirectStandardOutput = true;
            cliProcess.StartInfo.FileName = command;
            cliProcess.StartInfo.Arguments = args;
            cliProcess.Start();
            string output = cliProcess.StandardOutput.ReadToEnd();
            cliProcess.WaitForExit();

            //Debug - Write command output to console
            //Console.WriteLine(output);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(output);
            return doc;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Boolean checkDeviceConfig(XmlNode node, string nodeType)
        {
            Boolean result = false;
            XmlNodeList nodeList = node.SelectNodes(nodeType);

            if (nodeList.Count > 0)
            {
                if (nodeList[0].ChildNodes.Count > 1)
                {
                    // Console.WriteLine("Meta list: " + nodeList[0].ChildNodes.Count);
                    result = true;
                }
            }
            return result;    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Dictionary<string, string> getDeviceAttributes(XmlDocument doc)
        {
            Dictionary<string, string> volumeAttributes = new Dictionary<string, string>();
            volumeAttributes.Add("devMetaID", doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Dev_Info/dev_name").InnerXml);
            volumeAttributes.Add("devType", doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Dev_Info/configuration").InnerXml);
            volumeAttributes.Add("devBoundPool", doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Dev_Info/thin_pool_name").InnerXml);
            volumeAttributes.Add("devMetaLocation", doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Flags/meta").InnerXml);
            volumeAttributes.Add("metaType", doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Meta/configuration").InnerXml);
            volumeAttributes.Add("metaMemberCount", doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Meta/members").InnerXml);

            return volumeAttributes;


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<string> getMetaMembers(XmlNode node)
        {
            List<string> metaMembers = new List<string>();

            foreach (XmlNode xmlNode in node)
            {
                if (xmlNode.FirstChild.LocalName == "dev_name")
                {
                    metaMembers.Add(xmlNode.FirstChild.InnerXml);
                }
            }
            return metaMembers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSN"></param>
        /// <param name="userDev"></param>
        /// <param name="volumeAttributes"></param>
        /// <param name="metaMembers"></param>
        /// <returns></returns>
        public List<string> generateCleanupCommands(string type, string userSN, string userDev, Dictionary<string, string> volumeAttributes, List<string> metaMembers)
        {
            List<string> commands = new List<string>();
            commands.Add("symconfigure -sid " + userSN + " -cmd \"unbind tdev " + userDev + " from pool " + volumeAttributes["devBoundPool"] + ";\" " + type);
            commands.Add("symconfigure -sid " + userSN + " -cmd \"dissolve meta dev " + userDev + ";\" " + type);
            foreach (string tdev in metaMembers)
            {
                commands.Add("symconfigure -sid " + userSN + " -cmd \"delete dev " + tdev + ";\" " + type);
            }
            return commands;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        public void printCleanupCommands(List<string> commands)
        {
           Console.WriteLine();
           foreach (string command in commands)
            {
                Console.WriteLine(command);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string setDesktopPath()
        {
            string desktopPath;
            if (Environment.OSVersion.Version.Equals("6.2.9200.0"))
            {
                desktopPath = @"C:\Users\" + Environment.UserName + @"\Desktop\";
            }
            else
            {
                desktopPath = @"C:\Documents and Settings\" + Environment.UserName + @"\Desktop\";
            }
            return desktopPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="desktopPath"></param>
        /// <param name="userSerialNumber"></param>
        public string createCommandScript(string desktopPath,string userSerialNumber)
        {
            string fullpath = desktopPath + DateTime.Now.DayOfYear + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + "_VMAX" + userSerialNumber;
            System.IO.StreamWriter file = new System.IO.StreamWriter(fullpath + "_preview_commands.txt");
            file.WriteLine("# Preview Commands");
            file.Close();

            System.IO.StreamWriter file2 = new System.IO.StreamWriter(fullpath + "_commit_commands.txt");
            file2.WriteLine("# Commit Commands");
            file2.Close();
            return fullpath;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="desktopPath"></param>
        /// <param name="userSerialNumber"></param>
        /// <param name="previewCommands"></param>
        /// <param name="commitCommands"></param>
        public void writeCommandScript(string fullPath, string userSerialNumber, List<string> previewCommands, List<string> commitCommands)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(fullPath + "_preview_commands.txt", append: true);
            foreach (string command in previewCommands)
            {
                file.WriteLine(command);
            }
            file.Close();

            System.IO.StreamWriter file2 = new System.IO.StreamWriter(fullPath + "_commit_commands.txt", append: true);
            foreach (string command in commitCommands)
            {
                file2.WriteLine(command);
            }
            file2.Close();
        }
    }
}
