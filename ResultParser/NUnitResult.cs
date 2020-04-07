using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

/*
  <environment nunit-version="2.6.1.12217" clr-version="2.0.50727.8830" os-version="Microsoft Windows NT 6.2.9200.0" platform="Win32NT" cwd="C:\workspace\jenkins\jobs\Trunk-ConfLight CT ColorReproduction Tests\workspace" machine-name="LDRDINGBLRWS106" user="310083519" user-domain="LUX" />
  <culture-info current-culture="en-IN" current-uiculture="en-US" />
 * */
namespace ResultParser
{
    [XmlRoot(ElementName = "test-results")]
    public class NUnitResultClass
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlIgnore()]
        public string GetAppName
        {
            get
            {

                FileInfo fin = new FileInfo(this.Name);

                return fin.Name;
            }

        }
        [XmlAttribute("total")]
        public string Total;
        [XmlAttribute("errors")]
        public string Errors;
        [XmlAttribute("failures")]
        public string Failures;
        [XmlAttribute("not-run")]
        public string NotRun;
        [XmlAttribute("inconclusive")]
        public string InConclusive;
        [XmlAttribute("ignored")]
        public string Ignored;
        [XmlAttribute("skipped")]
        public string Skipped;
        [XmlAttribute("invalid")]
        public string Invalid;
        [XmlAttribute("date")]
        public string Date;
        [XmlAttribute("time")]
        public string Time;
        [XmlElement("test-suite")]
        public TestSuiteClass TestSuite;
        [XmlIgnore()]
        public string XmlFilePath = String.Empty;
        [XmlIgnore()]
        public string XmlDirPath = String.Empty;
    }

    public class TestSuiteClass
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlIgnore()]
        public string GetAppName
        {
            get
            {
                if (this.Type.Equals("Assembly"))
                {
                    FileInfo fin = new FileInfo(this.Name);

                    return fin.Name;
                }
                else
                {
                    return this.Name;
                }

            }
        }
        [XmlAttribute("type")]
        public string Type;
        [XmlAttribute("executed")]
        public string Executed;
        [XmlAttribute("result")]
        public string Result;
        [XmlAttribute("success")]
        public string Success;
        [XmlAttribute("time")]
        public string Time;
        [XmlAttribute("asserts")]
        public string Asserts;
        [XmlElement("results")]
        public ResultClass Results;

        public string HeaderToString()
        {
            return "Name" + BaseCode.SpacingChar + "Type" + BaseCode.SpacingChar + "Executed" + BaseCode.SpacingChar + "Result" + BaseCode.SpacingChar + "Success";
        }

        public override string ToString()
        {
            return this.Name + BaseCode.SpacingChar + this.Type + BaseCode.SpacingChar + this.Executed + BaseCode.SpacingChar + this.Result + BaseCode.SpacingChar + this.Success;
        }
    }
    public class ResultClass    
    {
        [XmlElement("test-suite")]
        public TestSuiteClass[] TestSuites;
        [XmlElement("test-case")]
        public TestCaseClass[] TestCases;
    }
    public class TestCaseClass
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("type")]
        public string Type = "TestCase";
        [XmlAttribute("executed")]
        public string Executed;
        [XmlAttribute("result")]
        public string Result;
        [XmlAttribute("success")]
        public string Success;
        [XmlAttribute("time")]
        public string Time;
        [XmlAttribute("asserts")]
        public string Asserts;
        [XmlElement("reason")]
        public ReasonClass Reason;
        [XmlElement("failure")]
        public FailureClass Failure;

        public string HeaderToString()
        {
            return "TestName" + BaseCode.SpacingChar + "Executed" + BaseCode.SpacingChar + "Result" + BaseCode.SpacingChar + "Success";
        }

        public override string ToString()
        {
            string FailMessage = String.Empty;
            if(this.Failure != null)
            {
                FailMessage = this.Failure.Message.Replace(Environment.NewLine, " ");
                FailMessage = FailMessage.Replace("\n", " ");
            }
            return this.Name + BaseCode.SpacingChar + this.Type + BaseCode.SpacingChar + this.Executed + BaseCode.SpacingChar + this.Result + BaseCode.SpacingChar + this.Success + BaseCode.SpacingChar + ((this.Reason != null) ? this.Reason.Message.Replace(Environment.NewLine, " ") : String.Empty) + BaseCode.SpacingChar + FailMessage;
        }
    }
    public class ReasonClass
    {
        [XmlElement("message")]
        public string Message;
    }
    public class FailureClass
    {
        [XmlElement("message")]
        public string Message;
        [XmlElement("stack-trace")]
        public string StackTrace;
    }
    
}