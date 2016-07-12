using System;
using System.ComponentModel;

namespace Epi.Cloud.Common.Metadata
{
    [DesignerCategory("code")]
    public class Template
    {
        private Template _templateCloneSource;
        private int _templateGeneration;
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreateDate { get; set; }
        public string Level { get; set; }
        public Project Project { get; set; }
        public SourceTable[] SourceTables { get; set; }

        public Template Clone()
        {
            var clone = (Template)MemberwiseClone();
            clone._templateCloneSource = this;
            clone._templateGeneration++;
            clone.Project = Project != null ? Project.Clone() : null;
            return clone;
        }
    }

    [DesignerCategory("code")]
    public class Project
    {
        private int _projectGeneration;
        private Project _projectCloneSource;
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
        public View[] Views { get; set; }
        public ProjectDigest[] Digest { get; set; }
 
        //  public ProjectCollectedData CollectedData { get; set; }

        public Project Clone()
        {
            var clone = (Project)MemberwiseClone();
            clone._projectCloneSource = this;
            clone._projectGeneration++;
            clone.Views = new View[Views != null ? Views.Length : 0];
            for (int i = 0; i < clone.Views.Length; ++i)
            {
                clone.Views[i] = Views[i].Clone();
            }
            return clone;
        }
    }

    [DesignerCategory("code")]
    public partial class View
    {
        private View _viewCloneSource;
        private int _viewGeneration;
        public int ViewId { get; set; }
        public string Name { get; set; }
        public bool IsRelatedView { get; set; }
        public string CheckCode { get; set; }
        public string CheckCodeBefore { get; set; }
        public string CheckCodeAfter { get; set; }
        public string RecordCheckCodeBefore { get; set; }
        public string RecordCheckCodeAfter { get; set; }
        public string CheckCodeVariableDefinitions { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Orientation { get; set; }
        public string LabelAlign { get; set; }
        public string EIWSOrganizationKey { get; set; }
        public string EIWSFormId { get; set; }
        public string EWEOrganizationKey { get; set; }
        public string EWEFormId { get; set; }

        public Page[] Pages { get; set; }

        public View Clone()
        {
            var clone = (View)MemberwiseClone();
            clone._viewCloneSource = this;
            clone._viewGeneration++;
            clone.Pages = new Page[Pages != null ? Pages.Length : 0];
            for (int i = 0; i < clone.Pages.Length; ++i)
            {
                clone.Pages[i] = Pages[i];
            }
            return clone;
        }
    }

    [DesignerCategory("code")]
    public partial class Page
    {
        private Page _pageCloneSource;
        private int _pageGeneration;
        public int? PageId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int BackgroundId { get; set; }
        public int ViewId { get; set; }
        public Field[] Fields { get; set; }
        public Page Clone()
        {
            var clone = (Page)MemberwiseClone();
            clone._pageCloneSource = this;
            clone._pageGeneration++;
            return clone;
        }
    }

    [DesignerCategory("code")]
    public partial class Field
    {
        public int ViewId { get; set; }
        public string PageName { get; set; }
        public int? PageId { get; set; }
        public int? PagePosition { get; set; }

        public int FieldId { get; set; }
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        public int FieldTypeId { get; set; }
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
        public string List { get; set; }
        public string SourceTableName { get; set; }
        public string CodeColumnName { get; set; }
        public string TextColumnName { get; set; }
        public bool? Sort { get; set; }
        public bool? IsExclusiveTable { get; set; }
        public int TabIndex { get; set; }
        public bool? HasTabStop { get; set; }
        public int? SourceFieldId { get; set; }
        public int? BackgroundColorSpecified { get; set; }
        public string RelatedViewName { get; set; }

        public string ControlAfterCheckCode { get; set; }
        public string ControlBeforeCheckCode { get; set; }
        public string PageBeforeCheckCode { get; set; }
        public string PageAfterCheckCode { get; set; }

        public string Expr1015 { get; set; }
        public string Expr1016 { get; set; }
        public string Expr1017 { get; set; }

        public string[] SourceTableItems { get; set; }
    }

    [DesignerCategory("code")]
    public partial class SourceTable
    {
        public string TableName { get; set; }
        public string[] Items { get; set; }
    }

    [DesignerCategory("code")]
    public partial class SourceTableItem
    {
        public string Code { get; set; }
        public string Text { get; set; }
    }

    [DesignerCategory("code")]
    public partial class CollectedData
    {
        public Database Database { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    public partial class Database
    {
        public string Source { get; set; }
        public string DataDriver { get; set; }
    }
}

