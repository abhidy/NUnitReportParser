using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace ResultParser
{
    class Program
    {
        static void Main(string[] args)
        {

            
            if (args.Length != 1)
            {
                System.Console.WriteLine("Improper number of argument");
            }
            else
            {
                string DirPath = args[0];
                if (Directory.Exists(DirPath))
                {
                    string NUnitFileExtn = ConfigurationManager.AppSettings["InputFileName"].ToString();
                    string OutputFileNamePostfix = ConfigurationManager.AppSettings["OutputFileNamePostfix"].ToString();
                    string OutputPath = DirPath + @"\" + OutputFileNamePostfix;
                    DirectoryInfo din = new DirectoryInfo(DirPath);
                    BaseCode CodeLogic = new BaseCode();
                    GenerateReport.Instance.CreateNewExcelHandle(OutputPath);
                    CodeLogic.GoInteration(din, NUnitFileExtn);
                    CodeLogic.GenerateResults();

                    GenerateReport.Instance.ClearExcelHandler(OutputPath);
                }
                else
                {
                    System.Console.WriteLine("Directory Does not exist!");
                }
            }
        }
    }
}
