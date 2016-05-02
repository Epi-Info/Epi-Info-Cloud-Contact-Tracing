namespace Epi.Cloud.MetadataServices.MetadataService.Interface
{
    public interface IMetadataFieldAttributes
    {
        string ProjectId { get; set; }
        string ProjectName { get; set; }
        string Name { get; set; }
        int? PageId { get; set; }
        int FieldId { get; set; }
        string UniqueId { get; set; }
        int? FieldTypeId { get; set; }
        string ControlAfterCheckCode { get; set; }
        string ControlBeforeCheckCode { get; set; }
        string PageName { get; set; }
        string PageBeforeCheckCode { get; set; }
        string PageAfterCheckCode { get; set; }
        int Position { get; set; }
        double? ControlTopPositionPercentage { get; set; }
        double? ControlLeftPositionPercentage { get; set; }
        double? ControlHeightPercentage { get; set; }
        double? ControlWidthPercentage { get; set; }
        string ControlFontFamily { get; set; }
        decimal? ControlFontSize { get; set; }
        string ControlFontStyle { get; set; }
        string ControlScriptName { get; set; }
        double? PromptTopPositionPercentage { get; set; }
        double? PromptLeftPositionPercentage { get; set; }
        string PromptText { get; set; }
        decimal? PromptFontSize { get; set; }
        string PromptFontFamily { get; set; }
        string PromptFontStyle { get; set; }
        string PromptScriptName { get; set; }
        bool? ShouldRepeatLast { get; set; }
        bool? IsRequired { get; set; }
        bool? IsReadOnly { get; set; }
        bool? ShouldRetainImageSize { get; set; }
        string Pattern { get; set; }
        int? MaxLength { get; set; }
        bool? ShowTextOnRight { get; set; }
        string Lower { get; set; }
        string Upper { get; set; }
        string RelateCondition { get; set; }
        bool? ShouldReturnToParent { get; set; }
        int? RelatedViewId { get; set; }
        string SourceTableName { get; set; }
        string CodeColumnName { get; set; }
        string TextColumnName { get; set; }
        int? BackgroundColor { get; set; }
        bool? IsExclusiveTable { get; set; }
        decimal? TabIndex { get; set; }
        bool? HasTabStop { get; set; }
        int? SourceFieldId { get; set; }
        string Expr1015 { get; set; }
        double? Expr1016 { get; set; }
        string Expr1017 { get; set; }
        bool Sort { get; set; }
        string List { get; set; }
        string RequiredMessage { get; set; }
    }
}