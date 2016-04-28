using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epi.Cloud.DBAccessService.Models
{
    public class metaFieldTypes
    {
        public int FieldTypeId { get; set; }
        public string Name { get; set; }
        public bool HasFont { get; set; }
        public bool HasRepeatLast { get; set; }
        public bool HasRequired { get; set; }
        public bool HasReadOnly { get; set; }
        public bool HasRetainImageSize { get; set; }
        public bool IsDropDown { get; set; }
        public bool IsGridColumn { get; set; }
        public bool IsSystem { get; set; }
        public int DefaultPatternId { get; set; }
        public int DataTypeId { get; set; }
    }
}