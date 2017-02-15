using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Epi.Cloud.Common.BusinessObjects
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormsHierarchyBO
    {
        private string _rootFormId;
        private string _formId;
        private List<SurveyResponseBO> _responseIds;
        private bool _isRoot;
        private int _viewId;
        private bool _isSqlProject;
        private SurveyInfoBO _surveyInfo;

        [DataMember]
        public string RootFormId
        {
            get { return _rootFormId; }
            set { _rootFormId = value; }
        }
        [DataMember]
        public string FormId
        {
            get { return _formId; }
            set { _formId = value; }
        }
        [DataMember]
        public List<SurveyResponseBO> ResponseIds
        {
            get { return _responseIds; }
            set { _responseIds = value; }
        }
        [DataMember]
        public bool IsRoot
        {
            get { return _isRoot; }
            set { _isRoot = value; }
        }
        [DataMember]
        public int ViewId
        {
            get { return _viewId; }
            set { _viewId = value; }
        }
        [DataMember]
        public bool IsSqlProject
        {
            get { return _isSqlProject; }
            set { _isSqlProject = value; }
        }

        [DataMember]
        public SurveyInfoBO SurveyInfo
        {
            get { return _surveyInfo; }
            set { _surveyInfo = value; }

        }
    }
}
