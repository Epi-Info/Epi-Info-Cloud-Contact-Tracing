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
    
    public partial class metaMapLayer
    {
        public int MapLayerId { get; set; }
        public string RenderField { get; set; }
        public Nullable<int> MarkerColor { get; set; }
        public Nullable<int> RampBeginColor { get; set; }
        public Nullable<int> RampEndColor { get; set; }
        public Nullable<int> ClassBreaks { get; set; }
        public string DataTableName { get; set; }
        public string DataTableKey { get; set; }
        public string FeatureKey { get; set; }
        public Nullable<int> LineColor { get; set; }
        public Nullable<int> FillColor { get; set; }
        public Nullable<int> PolygonOutlineColor { get; set; }
        public int MapId { get; set; }
        public int LayerId { get; set; }
        public int LayerRenderTypeId { get; set; }
    
        public virtual metaLayerRenderType metaLayerRenderType { get; set; }
        public virtual metaLayer metaLayer { get; set; }
        public virtual metaMap metaMap { get; set; }
    }
}
