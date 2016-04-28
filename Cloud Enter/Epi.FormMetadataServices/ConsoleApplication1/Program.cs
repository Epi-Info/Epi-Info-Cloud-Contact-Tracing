using Epi.Cloud.MetadataServices.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        { 
            string pageid = "1"; 
            ProjectMetadataProvider p = new ProjectMetadataProvider();
            List<MetadataFieldAttribute> fieldattributes = new List<MetadataFieldAttribute>();
            fieldattributes = p.GetProxy(pageid);  
        }
    }
 
}
