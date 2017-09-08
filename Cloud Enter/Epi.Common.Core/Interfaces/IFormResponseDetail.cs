using System;
using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;

namespace Epi.Common.Core.Interfaces
{
    public interface IFormResponseDetail : IResponseContext
    {
        int RecStatus { get; set; }
        string FirstSaveLogonName { get; set; }
        string LastSaveLogonName { get; set; }
        DateTime FirstSaveTime { get; set; }
        DateTime LastSaveTime { get; set; }
        bool IsRelatedView { get; set; }
        bool IsDraftMode { get; set; }
        bool IsLocked { get; set; }
        List<int> PageIds { get; set; }

        int LastPageVisited { get; set; }

        string RequiredFieldsList { get; set; }
        string HiddenFieldsList { get; set; }
        string HighlightedFieldsList { get; set; }
        string DisabledFieldsList { get; set; }

        List<PageResponseDetail> PageResponseDetailList { get; set; }

        List<FormResponseDetail> ChildFormResponseDetailList { get; set; }
    }
}
