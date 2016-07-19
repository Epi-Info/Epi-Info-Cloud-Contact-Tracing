using Epi.Web.Enter.Interfaces.DataInterfaces;
using Epi.Web.Enter.Interfaces.DataInterface;
using Epi.Cloud.DataEntryServices.DAO;
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
        /// <summary>
        /// Gets an Entity Framework specific Sur data access object.
        /// </summary>
        public ISurveyInfoDao SurveyInfoDao
        {
            get { return new EntitySurveyInfoDao(); }
        }

        public IFormInfoDao FormInfoDao
        {
            get { return Cloud.Common.Configuration.DependencyHelper.GetService<IFormInfoDao>() ?? new EntityFormInfoDao(); }
        }

        public ISurveyResponseDao SurveyResponseDao
        {
            get { return Cloud.Common.Configuration.DependencyHelper.GetService<ISurveyResponseDao>(); }
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
