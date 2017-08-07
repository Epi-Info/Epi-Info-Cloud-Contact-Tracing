using System.Collections.Generic;

namespace Epi.Cloud.MVC.Models
{
    public class RelateModel
    {
        private string _rootFormId;
        private string _formId;
        private List<SurveyAnswerModel> _responseIds;
        private bool _isRoot;
        private int _viewId;
        private bool _isSqlProject;
        public string RootFormId
        {
            get { return _rootFormId; }
            set { _rootFormId = value; }
        }
        public string FormId
        {
            get { return _formId; }
            set { _formId = value; }
        }

        public List<SurveyAnswerModel> ResponseIds
        {
            get { return _responseIds; }
            set { _responseIds = value; }
        }

        public bool IsRoot
        {
            get { return _isRoot; }
            set { _isRoot = value; }
        }

        public int ViewId
        {
            get { return _viewId; }
            set { _viewId = value; }
        }
        public bool IsSqlProject
        {
            get { return _isSqlProject; }
            set { _isSqlProject = value; }
        }
    }
}