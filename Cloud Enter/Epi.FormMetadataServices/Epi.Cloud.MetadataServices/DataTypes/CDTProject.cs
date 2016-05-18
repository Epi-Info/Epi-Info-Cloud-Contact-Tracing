namespace Epi.Cloud.MetadataServices.DataTypes
{
    public class CDTProject : CDTBase
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public string PageId { get; set; }
        public string FieldId { get; set; }
        public string UniqueId { get; set; }
        public string FieldTypeId { get; set; }
        public string ControlAfterCheckCode { get; set; }
        public string ControlBeforeCheckCode { get; set; }
        public string PageName { get; set; }
        public string PageBeforeCheckCode { get; set; }
        public string PageAfterCheckCode { get; set; }
        public string Position { get; set; }
        public double? ControlTopPositionPercentage { get; set; }
        public double? ControlLeftPositionPercentage { get; set; }
        public double? ControlHeightPercentage { get; set; }
        public double? ControlWidthPercentage { get; set; }
        public string ControlFontFamily { get; set; }
        public double? ControlFontSize { get; set; }
        public string ControlFontStyle { get; set; }
        public string ControlScriptName { get; set; }
        public double? PromptTopPositionPercentage { get; set; }
        public double? PromptLeftPositionPercentage { get; set; }
        public string PromptText { get; set; }
        public double? PromptFontSize { get; set; }
        public string PromptFontFamily { get; set; }
        public string PromptFontStyle { get; set; }
        //publi string ControlFontFamily{get;set;}
        // public string ControlFontSize { get; set; } 
        public string PromptScriptName { get; set; }
        public string ShouldRepeatLast { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsReadOnly { get; set; }
        public string ShouldRetainImageSize { get; set; }
        public string Pattern { get; set; }
        public int? MaxLength { get; set; }
        public string ShowTextOnRight { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }
        public string RelateCondition { get; set; }
        public string ShouldReturnToParent { get; set; }
        public string RelatedViewId { get; set; }
        public string SourceTableName { get; set; }
        public string CodeColumnName { get; set; }
        public string TextColumnName { get; set; }
        public string BackgroundColor { get; set; }
        public string IsExclusiveTable { get; set; }
        public int? TabIndex { get; set; }
        public string HasTabStop { get; set; }
        public string SourceFieldId { get; set; }
    }


}
