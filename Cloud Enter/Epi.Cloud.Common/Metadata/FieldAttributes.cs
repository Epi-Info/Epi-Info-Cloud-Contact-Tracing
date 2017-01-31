using System.Collections.Generic;
using System.Linq;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.Constants;
using Epi.FormMetadata.DataStructures;
using Epi.FormMetadata.DataStructures.Interfaces;

namespace Epi.Cloud.Common.Metadata
{
    public class FieldAttributes : IAbridgedFieldInfo
    {
        public FieldAttributes()
        {
        }

        public int ViewId { get; set; }
        public int PageId { get; set; }
        public int PagePosition { get; set; }
        public string PageName { get; set; }

        public string UniqueId { get; set; }
        public FieldTypes FieldType { get; set; }
        public string RequiredMessage { get; set; }
        public string FieldName { get; set; }
        public string TrueCaseFieldName { get; set; }
        public int TabIndex { get; set; }

        public string PromptText { get; set; }
        public double PromptTopPositionPercentage { get; set; }
        public double PromptLeftPositionPercentage { get; set; }

        public double ControlTopPositionPercentage { get; set; }
        public double ControlLeftPositionPercentage { get; set; }
        public double ControlWidthPercentage { get; set; }
        public double ControlHeightPercentage { get; set; }
        public string Checkcode { get; set; }
        public double PromptFontSize { get; set; }
        public string PromptFontStyle { get; set; }
        public string PromptFontFamily { get; set; }

        public double ControlFontSize { get; set; }
        public string ControlFontStyle { get; set; }
        public string ControlFontFamily { get; set; }

        public int MaxLength { get; set; }
        public string Pattern { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }

        public bool IsRequired { get; set; }
        public bool Required { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsDisabled { get; set; }

        public string ShowTextOnRight { get; set; }
        public string List { get; set; }
        public string BackgroundColor { get; set; }
        public string[] SourceTableValues { get; set; }
        public string RelatedViewId { get; set; }

        public string RelateCondition { get; set; }

        /// <summary>
        /// Value is set / retrieved at runtime by SurveyHelper
        /// </summary>
        public string Value { get; set; }


        public static Dictionary<string, FieldAttributes> MapFieldMetadataToFieldAttributes(Page page, string formCheckcode)
        {
            var fields = page.Fields;
            return MapFieldMetadataToFieldAttributes(fields, formCheckcode);
        }

        public static Dictionary<string, FieldAttributes> MapFieldMetadataToFieldAttributes(Field[] fields, string formCheckcode)
        {
            var sourceTableFields = fields.Where(f => f.SourceTableValues != null).ToArray();
            var results = fields.Select(f => new FieldAttributes
            {
                RequiredMessage = "This field is required",

                ViewId = f.ViewId,
                PageId = f.PageId.ValueOrDefault(),
                PageName = f.PageName,
                PagePosition = f.PagePosition.ValueOrDefault(),
                Checkcode = formCheckcode,
                UniqueId = f.UniqueId.ToString("D"),
                FieldType = (FieldTypes)f.FieldTypeId,
                TrueCaseFieldName = f.Name,
                FieldName = f.Name.ToLower(),
                TabIndex = (int)f.TabIndex,

                PromptText = f.PromptText,
                PromptTopPositionPercentage = f.PromptTopPositionPercentage.ValueOrDefault(),
                PromptLeftPositionPercentage = f.PromptLeftPositionPercentage.ValueOrDefault(),
                PromptFontStyle = f.PromptFontStyle,
                PromptFontSize = (double)f.PromptFontSize.ValueOrDefault(),
                PromptFontFamily = f.PromptFontFamily,

                ControlTopPositionPercentage = f.ControlTopPositionPercentage.ValueOrDefault(),
                ControlLeftPositionPercentage = f.ControlLeftPositionPercentage.ValueOrDefault(),
                ControlWidthPercentage = f.ControlWidthPercentage.ValueOrDefault(),
                ControlHeightPercentage = f.ControlHeightPercentage.ValueOrDefault(),
                ControlFontStyle = f.ControlFontStyle,
                ControlFontSize = (double)f.ControlFontSize.ValueOrDefault(),
                ControlFontFamily = f.ControlFontFamily,

                MaxLength = f.MaxLength.ValueOrDefault(),
                Pattern = f.Pattern,
                Lower = f.Lower,
                Upper = f.Upper,
                IsRequired = f.IsRequired ?? false,
                Required = f.IsRequired ?? false,
                IsReadOnly = f.IsReadOnly ?? false,
                IsHidden = false,
                IsHighlighted = false,
                IsDisabled = false,
                List = f.List,
                SourceTableValues = f.SourceTableValues,
                RelatedViewId = f.RelatedViewId.ToString(),
                RelateCondition = f.RelateCondition

            })/*.OrderBy(x => x.ControlTopPositionPercentage).ThenByDescending(x => x.ControlLeftPositionPercentage)*/;
            return results.ToDictionary(f => f.FieldName, f => f);
        }
    }
}