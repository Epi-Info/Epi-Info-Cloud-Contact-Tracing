using System;

namespace Epi.Common.Core.Interfaces
{
    public interface IResponseContext
    {
        string ResponseId { get; set; }
        string FormId { get; set; }
        string FormName { get; set; }

        string ParentResponseId { get; set; }
        string ParentFormId { get; set; }
        string ParentFormName { get; set; }

        string RootResponseId { get; set; }
        string RootFormId { get; set; }
        string RootFormName { get; set; }

        bool IsNewRecord { get; set; }

        int UserId { get; set; }
        string UserName { get; set; }

        bool IsChildResponse { get; }
        bool IsRootResponse { get; }
    }
}
