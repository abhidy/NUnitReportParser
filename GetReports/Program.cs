using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace GetReports
{
    class Program
    {
        public static string GetDirNameBasedOnDateAndTime()
        {
            return string.Format("{0:MM_dd_yyy_HH_mm_ss_ffff_tt}", DateTime.Now);
        }
        static void Main(string[] args)
        {
            string searchPattern = "*";

            if (args.Length < 1)
            {
                System.Console.WriteLine("Not Appropriate Input Arguments");
                return;
            }

            string inputDirectory = args[0];
            string destinationDirectory = @"C:\" + GetDirNameBasedOnDateAndTime();

            if (Directory.Exists(destinationDirectory))
            {
                destinationDirectory = @"C:\" + GetDirNameBasedOnDateAndTime();
            }

            Directory.CreateDirectory(destinationDirectory);

            if (args.Length > 1)
            {
                string inputSearchPattern = args[1];
                searchPattern = inputSearchPattern + "*";
            }

            DirectoryInfo inDirectoryInfo = new DirectoryInfo(inputDirectory);

            if (inDirectoryInfo.Exists == false)
            {
                System.Console.WriteLine("InputDirectory Does not Exist");
                return;
            }

            DirectoryInfo desDirectoryInfo = new DirectoryInfo(destinationDirectory);

            if (desDirectoryInfo.Exists == false)
            {
                System.Console.WriteLine("DestinationDirectory Does not Exist");
                return;
            }

            System.Console.WriteLine("Output Generated at " + destinationDirectory);

            string logFilePath = desDirectoryInfo.FullName + Path.DirectorySeparatorChar + "GeneratedLog.txt";

            StreamWriter swLog = new StreamWriter(logFilePath);

            foreach (var jenkinsJobDir in inDirectoryInfo.GetDirectories(searchPattern))
            {
                string jenkinsJobDirDestPath = desDirectoryInfo.FullName + Path.DirectorySeparatorChar + jenkinsJobDir.Name;

                if (Directory.Exists(jenkinsJobDirDestPath))
                {
                    //Directory.Delete(jenkinsJobDirDestPath, true);
                }
                else
                {
                    Directory.CreateDirectory(jenkinsJobDirDestPath);
                }

                FileInfo[] nextBuildNumberFins = jenkinsJobDir.GetFiles("nextBuildNumber");

                if ((nextBuildNumberFins.Length == 1) && (File.Exists(nextBuildNumberFins[0].FullName)))
                {
                    string nextBuildNumberDestFile =
                        jenkinsJobDirDestPath + Path.DirectorySeparatorChar + "nextBuildNumber";

                    string stringNumber = File.ReadAllText(nextBuildNumberFins[0].FullName);
                    UInt32 nextBuildNumber = UInt32.Parse(stringNumber, NumberStyles.Integer);
                    UInt32 lastBuildNumber = nextBuildNumber - 1;

                    string lastBuildFolder = jenkinsJobDir.FullName + "\\builds\\" + lastBuildNumber.ToString();

                    string lastBuildFolderLog =
                        jenkinsJobDir.FullName + "\\builds\\" + lastBuildNumber.ToString() + "\\log";
                    string lastBuildFolderArchieve =
                        jenkinsJobDir.FullName + "\\builds\\" + lastBuildNumber.ToString() + "\\archive";

                    bool newLogAvailable = false;

                    if (File.Exists(nextBuildNumberDestFile))
                    {
                        string stringNumberDestFile = File.ReadAllText(nextBuildNumberDestFile);
                        UInt32 nextBuildNumberDestFileParsed = UInt32.Parse(stringNumberDestFile, NumberStyles.Integer);

                        if (nextBuildNumberDestFileParsed < nextBuildNumber)
                        {
                            newLogAvailable = true;
                        }
                        else
                        {
                            newLogAvailable = false;
                        }

                    }
                    else
                    {
                        newLogAvailable = true;
                    }

                    if (newLogAvailable)
                    {
                        File.Copy(nextBuildNumberFins[0].FullName,
                            jenkinsJobDirDestPath + "\\" + nextBuildNumberFins[0].Name,true);

                        if (lastBuildNumber != 0)
                        {

                            File.Copy(lastBuildFolderLog, jenkinsJobDirDestPath + "\\log",true);

                            DirectoryInfo din1 = new DirectoryInfo(lastBuildFolderArchieve);

                            if (din1.Exists)
                            {

                                FileInfo[] fin1 = din1.GetFiles("*.xml");

                                foreach (var fin2 in fin1)
                                {
                                    File.Copy(fin2.FullName, jenkinsJobDirDestPath + "\\" + fin2.Name,true);

                                }

                                swLog.WriteLine("nextBuildNumber[NewFound]" + "\t" + nextBuildNumber + "\t" +
                                                  lastBuildNumber + "\t" + fin1.Length+ "NUNIT" + "\t" + jenkinsJobDir.Name);
                            }
                            else
                            {

                                swLog.WriteLine("nextBuildNumber[NewFound]" + "\t" + nextBuildNumber + "\t" +
                                                  lastBuildNumber + "\t" + "NONUNIT" +"\t" + jenkinsJobDir.Name);
                            }
                        }
                        else
                        {
                            swLog.WriteLine("nextBuildNumber[NewFound01]" + "\t1\t0\tNONUNIT01\t" + jenkinsJobDir.Name);
                        }
                    }
                    else
                    {
                        swLog.WriteLine("nextBuildNumber[SameFound]" + "\t" + nextBuildNumber + "\t" + lastBuildNumber +
                                          "\t*NUNIT*\t" + jenkinsJobDir.Name);
                    }
                }
                else
                {
                    swLog.WriteLine("nextBuildNumber[NotFound]" + "\t0\t0\t" + jenkinsJobDir.Name);
                }
                swLog.Flush();
            }
            swLog.Close();
        }

    }
}
namespace GetReports1
{
    class Program
    {
        public static string GetDirNameBasedOnDateAndTime()
        {
            return string.Format("{0:MM_dd_yyy_HH_mm_ss_ffff_tt}", DateTime.Now);
        }

        static void Main(string[] args)
        {
            string searchPattern = "*";

            if (args.Length < 1)
            {
                System.Console.WriteLine("Not Appropriate Input Arguments");
                return;
            }

            string inputDirectory = args[0];
            string destinationDirectory = @"C:\" + GetDirNameBasedOnDateAndTime();

            if (Directory.Exists(destinationDirectory))
            {
                destinationDirectory = @"C:\" + GetDirNameBasedOnDateAndTime();
            }

            Directory.CreateDirectory(destinationDirectory);

            if (args.Length > 1)
            {
                string inputSearchPattern = args[1];
                searchPattern = inputSearchPattern + "*";
            }

            DirectoryInfo inDirectoryInfo = new DirectoryInfo(inputDirectory);

            if (inDirectoryInfo.Exists == false)
            {
                System.Console.WriteLine("InputDirectory Does not Exist");
                return;
            }

            DirectoryInfo desDirectoryInfo = new DirectoryInfo(destinationDirectory);

            if (desDirectoryInfo.Exists == false)
            {
                System.Console.WriteLine("DestinationDirectory Does not Exist");
                return;
            }

            System.Console.WriteLine("Output Generated at " + destinationDirectory);

            string logFilePath = desDirectoryInfo.FullName + Path.DirectorySeparatorChar + "GeneratedLog.txt";

            StreamWriter swLog = new StreamWriter(logFilePath);
            foreach (DirectoryInfo inner1 in inDirectoryInfo.GetDirectories())
            {
                string localJobFolder = inner1.FullName + Path.DirectorySeparatorChar + "jobs";
                DirectoryInfo din = new DirectoryInfo(localJobFolder);
                string outDirName = desDirectoryInfo.FullName + Path.DirectorySeparatorChar + inner1.Name;
                Directory.CreateDirectory(outDirName);
                DirectoryInfo dout = new DirectoryInfo(outDirName);
                Main1(din, dout, ref swLog, searchPattern);

                
            }
            
            swLog.Close();
        }
        static void Main1(DirectoryInfo inDirectoryInfo,DirectoryInfo desDirectoryInfo, ref StreamWriter swLog, string searchPattern)
        {

            foreach (var jenkinsJobDir in inDirectoryInfo.GetDirectories(searchPattern))
            {
                string jenkinsJobDirDestPath = desDirectoryInfo.FullName + Path.DirectorySeparatorChar + jenkinsJobDir.Name;

                if (Directory.Exists(jenkinsJobDirDestPath))
                {
                    //Directory.Delete(jenkinsJobDirDestPath, true);
                }
                else
                {
                    Directory.CreateDirectory(jenkinsJobDirDestPath);
                }

                FileInfo[] nextBuildNumberFins = jenkinsJobDir.GetFiles("nextBuildNumber");

                if ((nextBuildNumberFins.Length == 1) && (File.Exists(nextBuildNumberFins[0].FullName)))
                {
                    string nextBuildNumberDestFile =
                        jenkinsJobDirDestPath + Path.DirectorySeparatorChar + "nextBuildNumber";

                    string stringNumber = File.ReadAllText(nextBuildNumberFins[0].FullName);
                    UInt32 nextBuildNumber = UInt32.Parse(stringNumber, NumberStyles.Integer);
                    UInt32 lastBuildNumber = nextBuildNumber - 1;

                    string lastBuildFolder = jenkinsJobDir.FullName + "\\builds\\" + lastBuildNumber.ToString();

                    string lastBuildFolderLog =
                        jenkinsJobDir.FullName + "\\builds\\" + lastBuildNumber.ToString() + "\\log";
                    string lastBuildFolderArchieve =
                        jenkinsJobDir.FullName + "\\builds\\" + lastBuildNumber.ToString() + "\\archive";

                    bool newLogAvailable = false;

                    if (File.Exists(nextBuildNumberDestFile))
                    {
                        string stringNumberDestFile = File.ReadAllText(nextBuildNumberDestFile);
                        UInt32 nextBuildNumberDestFileParsed = UInt32.Parse(stringNumberDestFile, NumberStyles.Integer);

                        if (nextBuildNumberDestFileParsed < nextBuildNumber)
                        {
                            newLogAvailable = true;
                        }
                        else
                        {
                            newLogAvailable = false;
                        }

                    }
                    else
                    {
                        newLogAvailable = true;
                    }

                    if (newLogAvailable)
                    {
                        File.Copy(nextBuildNumberFins[0].FullName,
                            jenkinsJobDirDestPath + "\\" + nextBuildNumberFins[0].Name,true);

                        if (lastBuildNumber != 0)
                        {

                            File.Copy(lastBuildFolderLog, jenkinsJobDirDestPath + "\\log",true);

                            DirectoryInfo din1 = new DirectoryInfo(lastBuildFolderArchieve);

                            if (din1.Exists)
                            {

                                FileInfo[] fin1 = din1.GetFiles("*.xml");

                                foreach (var fin2 in fin1)
                                {
                                    File.Copy(fin2.FullName, jenkinsJobDirDestPath + "\\" + fin2.Name,true);

                                }

                                swLog.WriteLine(inDirectoryInfo.Parent.Name+"\tnextBuildNumber[NewFound]" + "\t" + nextBuildNumber + "\t" +
                                                  lastBuildNumber + "\t" + fin1.Length+ "NUNIT" + "\t" + jenkinsJobDir.Name);
                            }
                            else
                            {

                                swLog.WriteLine(inDirectoryInfo.Parent.Name + "\tnextBuildNumber[NewFound]" + "\t" + nextBuildNumber + "\t" +
                                                  lastBuildNumber + "\t" + "NONUNIT" +"\t" + jenkinsJobDir.Name);
                            }
                        }
                        else
                        {
                            swLog.WriteLine(inDirectoryInfo.Parent.Name + "\tnextBuildNumber[NewFound01]" + "\t1\t0\tNONUNIT01\t" + jenkinsJobDir.Name);
                        }
                    }
                    else
                    {
                        swLog.WriteLine(inDirectoryInfo.Parent.Name + "\tnextBuildNumber[SameFound]" + "\t" + nextBuildNumber + "\t" + lastBuildNumber +
                                          "\t*NUNIT*\t" + jenkinsJobDir.Name);
                    }
                }
                else
                {
                    swLog.WriteLine(inDirectoryInfo.Parent.Name + "\tnextBuildNumber[NotFound]" + "\t0\t0\t" + jenkinsJobDir.Name);
                }
                swLog.Flush();
            }
        }

    }
}