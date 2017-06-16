using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace GenerateMetaDestroyScript
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            #region Variables

            var vmaxSerialNumbers = new List<string>();
            string userSerialNumber = null;
            string userMetaVolumes = null;
            List<string> devicesToScript;
            SymCliHelper helper = new SymCliHelper();
            string desktopPath;
            string fullPath;

     
            XmlDocument doc;
            XmlNodeList nodeList;
            XmlNode node;


            #endregion

            //Print the porgram header
            Console.WriteLine("VMAX Meta Device Cleanup Script Creator - v1.0");
            Console.WriteLine();

            Console.WriteLine("[1] - Checking what VMAX's are in the symapi database.");
            Console.WriteLine();
            System.Threading.Thread.Sleep(500);

            if (args.Length > 1)
            {
                userSerialNumber = args[0];
                userMetaVolumes = args[1];
            }
            else
            {
                // Determine what VMAX arrays symcli see's by generate a node list of all symmetrix entries from the symcli output
                doc = helper.runSymCliCommand("symcfg.exe", "list -output xml_e");
                nodeList = doc.SelectNodes("/SymCLI_ML/Symmetrix");
                Console.WriteLine("There are: " + nodeList.Count + " VMAX arrays visible: ");

                //Sort through the symmetrix entries and extract each serial number
                int counter = 1;
                foreach (XmlNode xn in nodeList)
                {
                    XmlNode symmNode = xn.SelectSingleNode("Symm_Info/symid");
                    Console.WriteLine("(" + counter + ") - " + symmNode.InnerText);
                    counter++;
                }
                
                //Prompt the user for what serial number and metadevice they would like to create a script for
                Console.WriteLine();
                Console.Write("Enter the last four digits of the VMAX serial number: ");
                userSerialNumber = Console.ReadLine();
                Console.Write("Enter the meta volume(s) you wish to destroy: ");
                userMetaVolumes = Console.ReadLine();
                Console.WriteLine();
            }

            //split out devices into a list of devices to be checked and scripted
            devicesToScript = new List<string>(userMetaVolumes.Split(','));

            //Set the desktop path
            desktopPath = helper.setDesktopPath();

            // Write the commands to a file.
            Console.WriteLine("[2] - Creating commands script file at: " + desktopPath);
            fullPath = helper.createCommandScript(desktopPath, userSerialNumber);
            Console.WriteLine();

            //for each volume passed to the cli
            int devCounter = 1;
            foreach (string userMetaVolume in devicesToScript)
            {
                Console.WriteLine("[Device " + devCounter + "] - Creating commands for " + userMetaVolume + ".");
                Dictionary<string, string> volumeAttributes;
                List<string> metaMembers;
                List<string> previewCommands;
                List<string> commitCommands;
                Boolean isMeta = false;
                Boolean hasRDF = false;
                Boolean hasClone = false;
                Boolean isMapped = false;

                //Load the symcli output into a XML document so that we can parse through to find the nodes we need
                doc = helper.runSymCliCommand("symdev.exe", "-sid " + userSerialNumber + " show " + userMetaVolume + " -output xml_e -offline");

                //Begin device checks
                node = doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device");

                //Check if the device is a meta
                Console.WriteLine("[Device " + devCounter + "] - Checking if " + userMetaVolume + " is a meta.");
                isMeta = helper.checkDeviceConfig(node, "Meta");
                System.Threading.Thread.Sleep(500);

                //Check if the device is mapped down any FA's
                Console.WriteLine("[Device " + devCounter + "] - Checking if " + userMetaVolume + " is masked/mapped.");
                isMapped = helper.checkDeviceConfig(node, "Front_End");
                System.Threading.Thread.Sleep(500);

                //Check if the device has a BCV
                Console.WriteLine("[Device " + devCounter + "] - Checking if " + userMetaVolume + " is part of a TimeFinder session.");
                hasClone = helper.checkDeviceConfig(node, "CLONE_Device");
                System.Threading.Thread.Sleep(500);

                //Check if the device has RDF
                Console.WriteLine("[Device " + devCounter + "] - Checking if " + userMetaVolume + " is part of an RDF session.");
                Console.WriteLine();
                hasRDF = helper.checkDeviceConfig(node, "RDF");
                System.Threading.Thread.Sleep(500);

                //Perform check on LUN, do not continue if it is not a meta, else continute with warnings
                if (isMeta == false)
                {
                    Console.WriteLine("[Device " + devCounter + "][Error] - " + userMetaVolume + " is not a meta device. The program will now terminate.");
                    Console.WriteLine();
                }
                else
                {
                    //find meta members under meta informaiton
                    node = doc.SelectSingleNode("SymCLI_ML/Symmetrix/Device/Meta");
                    metaMembers = helper.getMetaMembers(node);

                    //Populate the attributes directory with information on the meta device
                    volumeAttributes = helper.getDeviceAttributes(doc);

                    //Console.WriteLine("[5] - Generating script to destory the meta volume " + userMetaVolume + " from VMAX " + userSerialNumber + ".");
                    //System.Threading.Thread.Sleep(500);

                    //Generate Commands
                    previewCommands = helper.generateCleanupCommands("preview", userSerialNumber, userMetaVolume, volumeAttributes, metaMembers);
                    commitCommands = helper.generateCleanupCommands("commit", userSerialNumber, userMetaVolume, volumeAttributes, metaMembers);


                    //Write commands to file
                    helper.writeCommandScript(fullPath, userSerialNumber, previewCommands, commitCommands);

                    //Print warnings if the device is mapped, part of a BCV session, or RDF session
                    if (isMapped == true)
                        Console.WriteLine("[Device " + devCounter + "][Warning] - " + userMetaVolume + " is mapped down FA ports. You will need to unmask/umap it before running the cleanup commands.");
                        System.Threading.Thread.Sleep(500);
                    if (hasClone == true)
                        Console.WriteLine("[Device " + devCounter + "][Warning] - " + userMetaVolume + "  is part of a TimeFinder session. You will need remove the TimeFinder relationship before running the cleanup commands.");
                        System.Threading.Thread.Sleep(500);
                    if (hasRDF == true)
                        Console.WriteLine("[Device " + devCounter + "][Warning] - " + userMetaVolume + "  is part of a RDF session. You will need remove the SRDF relationship before running the cleanup commands.");
                        System.Threading.Thread.Sleep(500);

                }//end if check for meta or not
                devCounter++;
            }
            Console.WriteLine("[3] - Script generation complete, you can now close the command line window.");
            Console.WriteLine();
        }
    }
}
