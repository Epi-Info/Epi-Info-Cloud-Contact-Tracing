using System;
using System.Runtime.Serialization;


namespace Epi.Cloud.MVC.Models
{
    [Serializable]
    public class SearchBoxModel
    {
        public string SearchCol1 { get; set; }
        public string SearchCol2 { get; set; }
        public string SearchCol3 { get; set; }
        public string SearchCol4 { get; set; }
        public string SearchCol5 { get; set; }

        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string Op3 { get; set; }
        public string Op4 { get; set; }
        public string Op5 { get; set; }

        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
        public string Value5 { get; set; }
    }
}