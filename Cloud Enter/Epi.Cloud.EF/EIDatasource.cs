//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Epi.Data.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class EIDatasource
    {
        public int DatasourceID { get; set; }
        public string DatasourceServerName { get; set; }
        public string DatabaseType { get; set; }
        public string InitialCatalog { get; set; }
        public string PersistSecurityInfo { get; set; }
        public Nullable<System.Guid> SurveyId { get; set; }
        public string DatabaseUserID { get; set; }
        public string Password { get; set; }
    
        public virtual SurveyMetaData SurveyMetaData { get; set; }
    }
}