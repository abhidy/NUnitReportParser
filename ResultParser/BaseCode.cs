using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultParser
{
    public class BaseCode
    {
        public static string SpacingChar = "\t";
        public ArrayList NUnitResultConsolidation = new ArrayList();
        public void GoInteration(DirectoryInfo dinInput,string fileExtn)
        {
            foreach (FileInfo fin in dinInput.GetFiles(fileExtn))
            {
                NUnitResultClass NUnitResultClassObj = (NUnitResultClass)(Serializer.DeserializeObject(fin.FullName, typeof(NUnitResultClass)));
                NUnitResultClassObj.XmlFilePath = fin.Name;
                NUnitResultClassObj.XmlDirPath = fin.DirectoryName;
                NUnitResultConsolidation.Add(NUnitResultClassObj);
            }

            foreach (DirectoryInfo din in dinInput.GetDirectories())
            {
                GoInteration(din, fileExtn);
            }
        }
        public void GenerateResults()
        {

            System.Console.WriteLine("FileName" + BaseCode.SpacingChar + "Directory" + BaseCode.SpacingChar + "ReferencePath" + BaseCode.SpacingChar + "Name" + BaseCode.SpacingChar + "Type" + BaseCode.SpacingChar + "Executed" + BaseCode.SpacingChar + "Result" + BaseCode.SpacingChar + "Success" + BaseCode.SpacingChar + "Reason" + BaseCode.SpacingChar + "Failure");
            string[] ArrayData = new string[12];

            ArrayData[0] = "FileName";
            ArrayData[1] = "Directory";
            ArrayData[2] = "Date";
            ArrayData[3] = "Time";
            ArrayData[4] = "ReferencePath";
            ArrayData[5] = "Name";
            ArrayData[6] = "Type";
            ArrayData[7] = "Executed";
            ArrayData[8] = "Result";
            ArrayData[9] = "Success";
            ArrayData[10] = "Reason";
            ArrayData[11] = "Failure";

            GenerateReport.Instance.WriteToExcel(ArrayData);


            foreach (NUnitResultClass resultClass in NUnitResultConsolidation)
            {
                GoNUnitInteration(resultClass.TestSuite, resultClass.XmlFilePath, resultClass.XmlDirPath,string.Empty,resultClass.Date,resultClass.Time);
            }
        }
        public void GoNUnitInteration(TestSuiteClass TSClass,String FileName,String DirName,string Prefix, string date,string time)
        {
            System.Console.WriteLine(FileName + BaseCode.SpacingChar + DirName + BaseCode.SpacingChar + Prefix + BaseCode.SpacingChar + TSClass.ToString());
            string[] ArrayData1 = new string[12];
            ArrayData1[0] = FileName;
            ArrayData1[1] = DirName;
            ArrayData1[2] = date;
            ArrayData1[3] = time;
            ArrayData1[4] = Prefix;
            ArrayData1[5] = TSClass.GetAppName;
            ArrayData1[6] = TSClass.Type;

            ArrayData1[7] = TSClass.Executed;
            ArrayData1[8] = TSClass.Result;
            ArrayData1[9] = TSClass.Success;
            GenerateReport.Instance.WriteToExcel(ArrayData1);
            
            if (TSClass.Results != null)
            {
                if (TSClass.Results.TestCases != null)
                {
                    for (int count = 0; count < (TSClass.Results.TestCases.Length); count++)
                    {
                        TestCaseClass tsc = TSClass.Results.TestCases[count];
                        string[] ArrayData = new string[12];
                        ArrayData[0] = FileName;
                        ArrayData[1] = DirName;
                        ArrayData[2] = date;
                        ArrayData[3] = time;
                        ArrayData[4] = Prefix + "\\" + TSClass.GetAppName;
                        ArrayData[5] = tsc.Name;
                        ArrayData[6] = tsc.Type;
                        ArrayData[7] = tsc.Executed;
                        ArrayData[8] = tsc.Result;
                        ArrayData[9] = tsc.Success;
                        
                        string ReasonMessage = String.Empty;
                        if (tsc.Reason != null)
                        {
                            ReasonMessage = tsc.Reason.Message.Replace(Environment.NewLine, " ");
                            ReasonMessage = ReasonMessage.Replace("\n", " ");
                        }
                        ArrayData[10] = ReasonMessage;
                        string FailMessage = String.Empty;
                        if (tsc.Failure != null)
                        {
                            FailMessage = tsc.Failure.Message.Replace(Environment.NewLine, " ");
                            FailMessage = FailMessage.Replace("\n", " ");
                        }
                        ArrayData[11] = FailMessage; ;


                        GenerateReport.Instance.WriteToExcel(ArrayData);
                        System.Console.WriteLine(FileName + BaseCode.SpacingChar + DirName + BaseCode.SpacingChar + Prefix + BaseCode.SpacingChar + tsc.ToString());
                    }
                }
                if (TSClass.Results.TestSuites != null)
                {                    
                    for (int count = 0; count < (TSClass.Results.TestSuites.Length); count++)
                    {
                        GoNUnitInteration(TSClass.Results.TestSuites[count], FileName, DirName,Prefix + "\\" + TSClass.GetAppName,date,time);
                    }
                }
            }
        }
    }
}
