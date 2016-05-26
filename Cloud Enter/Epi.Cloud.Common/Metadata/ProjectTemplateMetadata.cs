using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Epi.Cloud.Common.Metadata
{
    [Serializable()]
    [DesignerCategory("code")]
    public class ProjectTemplateMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreateDate { get; set; }
        public string Level { get; set; }
        public ProjectMetadata Project { get; set; }
        public SourceTable[] SourceTables { get; set; }

        public ProjectTemplateMetadata Clone()
        {
            var clone = (ProjectTemplateMetadata)MemberwiseClone();
            clone.Project = Project != null ? Project.Clone() : null;
            return clone;
        }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public class ProjectMetadata
    {
        public string MetadataSource { get; set; }
        public string EnterMakeviewInterpreter { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string EpiVersion { get; set; }
        public DateTime CreateDate { get; set; }
        public bool ControlFontBold { get; set; }
        public bool ControlFontItalics { get; set; }
        public string ControlFontName { get; set; }
        public decimal ControlFontSize { get; set; }
        public string DefaultLabelAlign { get; set; }
        public decimal DefaultPageHeight { get; set; }
        public string DefaultPageOrientation { get; set; }
        public decimal DefaultPageWidth { get; set; }
        public bool EditorFontBold { get; set; }
        public bool EditorFontItalics { get; set; }
        public string EditorFontName { get; set; }
        public decimal EditorFontSize { get; set; }
        public ViewMetadata View { get; set; }

        //  public ProjectCollectedData CollectedData { get; set; }

        public ProjectMetadata Clone()
        {
            var clone = (ProjectMetadata)MemberwiseClone();
            clone.View = View != null ? View.Clone() : null;
            return clone;
        }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public partial class ViewMetadata
    {
        public int ViewId { get; set; }
        public string Name { get; set; }
        public bool IsRelatedView { get; set; }
        public string CheckCode { get; set; }
        public string CheckCodeBefore { get; set; }
        public string CheckCodeAfter { get; set; }
        public string RecordCheckCodeBefore { get; set; }
        public string RecordCheckCodeAfter { get; set; }
        public string CheckCodeVariableDefinitions { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Orientation { get; set; }
        public string LabelAlign { get; set; }
        public int[] PageIds{ get; set; }
        public PageMetadata[] Pages { get; set; }

        public ViewMetadata Clone()
        {
            var clone = (ViewMetadata)MemberwiseClone();
            return clone;
        }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public partial class PageMetadata
    {
        public int PageId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int BackgroundId { get; set; }
        public int ViewId { get; set; }
        public FieldMetdata[] Fields { get; set; }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public partial class FieldMetdata
    {
        public int FieldId { get; set; }
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        public int? PageId { get; set; }
        public int FieldTypeId { get; set; }
        public string PageName { get; set; }
        public int Position { get; set; }
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
        public string PromptFontFamily { get; set; }
        public double? PromptFontSize { get; set; }
        public string PromptFontStyle { get; set; }
        public string PromptScriptName { get; set; }
        public bool ShouldRepeatLast { get; set; }
        public string IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool ShouldRetainImageSize { get; set; }
        public string Pattern { get; set; }
        public int? MaxLength { get; set; }
        public bool ShowTextOnRight { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }
        public string RelateCondition { get; set; }
        public bool? ShouldReturnToParent { get; set; }
        public int? RelatedViewId { get; set; }
        public string List { get; set; }
        public string SourceTableName { get; set; }
        public string CodeColumnName { get; set; }
        public string TextColumnName { get; set; }
        public bool? Sort { get; set; }
        public bool? IsExclusiveTable { get; set; }
        public int TabIndex { get; set; }
        public bool? HasTabStop { get; set; }
        public string SourceFieldId { get; set; }
        public bool BackgroundColorSpecified { get; set; }
        public string RelatedViewName { get; set; }

        public string ControlAfterCheckCode { get; set; }
        public string ControlBeforeCheckCode { get; set; }
        public string PageBeforeCheckCode { get; set; }
        public string PageAfterCheckCode { get; set; }

        public string Expr1015 { get; set; }
        public string Expr1016 { get; set; }
        public string Expr1017 { get; set; }

        /// <summary>
        /// The SourceTableItems are not serialized at the PageMetadata level.
        /// Serialization is performed at the ProjectTemplateMetadata level and
        /// copied here later for page level access convienence.
        /// </summary>
        //[JsonIgnore]
        public SourceTableItem[] Items { get; set; }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public partial class SourceTable
    {
        public string TableName { get; set; }
        public SourceTableItem[] Items { get; set; }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public partial class SourceTableItem
    {
        public string Code { get; set; }
        public string Text { get; set; }
    }

    //[Serializable()]
    //[DesignerCategory("code")]
    //public partial class ProjectCollectedData
    //{
    //    public CollectedDataDatabase Database { get; set; }
    //}

    ///// <remarks/>
    //[Serializable()]
    //[DesignerCategory("code")]
    //public partial class CollectedDataDatabase
    //{
    //    public string Source { get; set; }
    //    public string DataDriver { get; set; }
    //}
}

