using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace ResultParser
{
    public class Serializer
    {
        public static object DeserializeObject(string filename,Type t)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            XmlSerializer x = new XmlSerializer(t);
            object g = x.Deserialize(fs);
            return g;
        }
    }
}