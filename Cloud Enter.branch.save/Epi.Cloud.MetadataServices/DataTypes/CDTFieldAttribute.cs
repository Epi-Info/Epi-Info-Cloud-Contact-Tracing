using Epi.Cloud.MetadataServices.MetadataService.Interface;
using System.Collections.Generic;

namespace Epi.Cloud.MetadataServices.DataTypes
{
    public class MetadataFieldAttributes : CDTBase, IMetadataFieldAttributes
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public int? PageId { get; set; }
        public int FieldId { get; set; }
        public string UniqueId { get; set; }
        public int? FieldTypeId { get; set; }
        public string ControlAfterCheckCode { get; set; }
        public string ControlBeforeCheckCode { get; set; }
        public string PageName { get; set; }
        public string PageBeforeCheckCode { get; set; }
        public string PageAfterCheckCode { get; set; }
        public int Position { get; set; }
        public double? ControlTopPositionPercentage { get; set; }
        public double? ControlLeftPositionPercentage { get; set; }
        public double? ControlHeightPercentage { get; set; }
        public double? ControlWidthPercentage { get; set; }
        public string ControlFontFamily { get; set; }
        public decimal? ControlFontSize { get; set; }
        public string ControlFontStyle { get; set; }
        public string ControlScriptName { get; set; }
        public double? PromptTopPositionPercentage { get; set; }
        public double? PromptLeftPositionPercentage { get; set; }
        public string PromptText { get; set; }
        public decimal? PromptFontSize { get; set; }
        public string PromptFontFamily { get; set; }
        public string PromptFontStyle { get; set; }
        //publi string ControlFontFamily{get;set;}
        // public string ControlFontSize { get; set; } 
        public string PromptScriptName { get; set; }
        public bool? ShouldRepeatLast { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsReadOnly { get; set; }
        public bool? ShouldRetainImageSize { get; set; }
        public string Pattern { get; set; }
        public int? MaxLength { get; set; }
        public bool? ShowTextOnRight { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }
        public string RelateCondition { get; set; }
        public bool? ShouldReturnToParent { get; set; }
        public int? RelatedViewId { get; set; }
        public string SourceTableName { get; set; }
        public string CodeColumnName { get; set; }
        public string TextColumnName { get; set; }
        public int? BackgroundColor { get; set; }
        public bool? IsExclusiveTable { get; set; }
        public decimal? TabIndex { get; set; }
        public bool? HasTabStop { get; set; }
        public int? SourceFieldId { get; set; }
        public string Expr1015 { get; set; }
        public double? Expr1016 { get; set; }
        public string Expr1017 { get; set; }
        public bool Sort { get; set; }
        public string List { get; set; }
        public string RequiredMessage { get; set; }
        public List<string> SourceTableValues { get; set; }
    }
}
