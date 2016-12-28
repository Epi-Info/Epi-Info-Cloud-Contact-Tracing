using System.Configuration;
using System;
using Epi.Common.Security;
using Epi.Cloud.Common.Configuration;

namespace Epi.Web.EF
{
    /// <summary>
    /// DataObjectFactory caches the connectionstring so that the context can be created quickly.
    /// </summary>
    public static class DataObjectFactory
    {
        private static readonly string _connectionString;

        private static readonly string _eweAdoConnectionString;

        /// <summary>
        /// Static constructor. Reads the connectionstring from web.config just once.
        /// </summary>
        static DataObjectFactory()
        {
            try
            {
                // Connection strings here
                string EWEEntitiesConnectionStringName = ConfigurationHelper.GetEnvironmentResourceKey("EWEEntities");
                string EWEADOconnectionStringName = ConfigurationHelper.GetEnvironmentResourceKey("EWEADO");

                //Decrypt connection string here
                var connectionInfo = ConfigurationManager.ConnectionStrings[EWEEntitiesConnectionStringName];
                _connectionString = connectionInfo != null ?Cryptography.Decrypt(connectionInfo.ConnectionString) : null;

                connectionInfo = ConfigurationManager.ConnectionStrings[EWEADOconnectionStringName];
                _eweAdoConnectionString = connectionInfo != null ? Cryptography.Decrypt(connectionInfo.ConnectionString) : null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Creates the Context using the current connectionstring.
        /// </summary>
        /// <remarks>
        /// Gof pattern: Factory method. 
        /// </remarks>
        /// <returns>Action Entities context.</returns>
        public static Epi.Web.EF.OSELS_EWEEntities CreateContext()
        {
            return new Epi.Web.EF.OSELS_EWEEntities(_connectionString);
        }
        public static Epi.Web.EF.SurveyMetaData CreateSurveyMetaData()
        {
            return new Epi.Web.EF.SurveyMetaData();
        }

        public static Epi.Web.EF.SurveyResponse CreateSurveyResponse()
        {
            return new Epi.Web.EF.SurveyResponse();
        }

        /// <summary>
        /// Property to read connection string without meta information
        /// </summary>
        public static string EWEADOConnectionString
        {
            get
            {
                return _eweAdoConnectionString;
            }
        }
    }
}
