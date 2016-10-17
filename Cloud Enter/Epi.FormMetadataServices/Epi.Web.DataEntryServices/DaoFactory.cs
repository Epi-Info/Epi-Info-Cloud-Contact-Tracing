using Epi.Web.Enter.Interfaces.DataInterfaces;
using Epi.Web.Enter.Interfaces.DataInterface;
using Epi.Web.EF;

namespace Epi.Cloud.DataEntryServices
{
    /// <summary>
    /// Factory that creates data access objects.
    /// </summary>
    /// <remarks>
    /// GoF Design Patterns: Factory.
    /// </remarks>
    public class DaoFactory : IDaoFactory
    {
        private IFormInfoDao _formInfoDao;
        private ISurveyResponseDao _surveyResponseDao;
		private ISurveyInfoDao _surveyInfoDao;

        /// <summary>
        /// Gets an Entity Framework specific data access object.
        /// </summary>
        public ISurveyInfoDao SurveyInfoDao
        {
			get { return _surveyInfoDao ?? (_surveyInfoDao = Cloud.Common.Configuration.DependencyHelper.GetService<ISurveyInfoDao>()); }
		}

		public IFormInfoDao FormInfoDao
        {
			get { return _formInfoDao ?? (_formInfoDao = Cloud.Common.Configuration.DependencyHelper.GetService<IFormInfoDao>()); }
		}

		public ISurveyResponseDao SurveyResponseDao
		{
			get { return _surveyResponseDao ?? (_surveyResponseDao = Cloud.Common.Configuration.DependencyHelper.GetService<ISurveyResponseDao>()); }
		}

        public IOrganizationDao OrganizationDao
        {
            get { return new EntityOrganizationDao(); }

        }

        public IFormSettingDao FormSettingDao
        {
            get { return new EntityFormSettingDao(); }

        }

        public IUserDao UserDao
        {
            get { return new EntityUserDao(); }
        }
    }
}
