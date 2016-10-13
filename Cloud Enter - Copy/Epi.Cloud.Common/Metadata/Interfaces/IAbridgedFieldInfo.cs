using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.Common.Metadata.Interfaces
{
    public interface IAbridgedFieldInfo
    {
        string FieldName { get; }
        FieldTypes FieldType { get; }
        string List { get; }
        bool IsReadOnly { get; }
        bool IsRequired { get; }
        bool IsHidden { get; }
        bool IsDisabled { get; }
        bool IsHighlighted { get; }
    }
}
