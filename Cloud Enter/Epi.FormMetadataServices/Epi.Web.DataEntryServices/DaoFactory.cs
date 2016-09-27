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
        private readonly IFormInfoDao _formInfoDao;
        private readonly ISurveyResponseDao _surveyResponseDao;
		private readonly ISurveyInfoDao _surveyInfoDao;


		public DaoFactory(IFormInfoDao formInfoDao,
                          ISurveyResponseDao surveyResponseDao,
						  ISurveyInfoDao surveyInfoDao)
        {
            _formInfoDao = formInfoDao;
            _surveyResponseDao = surveyResponseDao;
			_surveyInfoDao = surveyInfoDao;
        }

        /// <summary>
        /// Gets an Entity Framework specific Sur data access object.
        /// </summary>
        public ISurveyInfoDao SurveyInfoDao
        {
            get { return new EntitySurveyInfoDao(); }
        }

        public IFormInfoDao FormInfoDao
        {
            get { return _formInfoDao; } //EntityFormInfoDao(); }
        }

        public ISurveyResponseDao SurveyResponseDao
        {
            get { return _surveyResponseDao; }
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
