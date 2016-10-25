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
    
    public partial class metaView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public metaView()
        {
            this.metaFields = new HashSet<metaField>();
            this.metaFields1 = new HashSet<metaField>();
            this.metaPages = new HashSet<metaPage>();
        }
    
        public int ViewId { get; set; }
        public string Name { get; set; }
        public bool IsRelatedView { get; set; }
        public string CheckCode { get; set; }
        public string CheckCodeBefore { get; set; }
        public string CheckCodeAfter { get; set; }
        public string RecordCheckCodeBefore { get; set; }
        public string RecordCheckCodeAfter { get; set; }
        public string CheckCodeVariableDefinitions { get; set; }
        public Nullable<int> Width { get; set; }
        public Nullable<int> Height { get; set; }
        public string Orientation { get; set; }
        public string LabelAlign { get; set; }
        public string EIWSOrganizationKey { get; set; }
        public string EIWSFormId { get; set; }
        public string EWEOrganizationKey { get; set; }
        public string EWEFormId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<metaField> metaFields { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<metaField> metaFields1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<metaPage> metaPages { get; set; }
    }
}