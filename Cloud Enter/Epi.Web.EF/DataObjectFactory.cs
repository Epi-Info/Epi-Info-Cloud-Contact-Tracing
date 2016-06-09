using System.Configuration;
using System;
using Epi.Web.Enter.Common.Security;

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
#if false
                //Unencrypted (Clear Text) connections string here
                string ctConnectionStringName = "ctEWEEntities";
                string ctEWEADOconnectionStringName = "ctEWEADO";
                try
                {
                    _connectionString = ConfigurationManager.ConnectionStrings[ctConnectionStringName].ConnectionString;
                    _eweAdoConnectionString = ConfigurationManager.ConnectionStrings[ctEWEADOconnectionStringName].ConnectionString;
                }
                catch
                {
                    _connectionString = null;
                    _eweAdoConnectionString = null;
                }
                if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(_eweAdoConnectionString))
#endif
                {
                    var environment = ConfigurationManager.AppSettings["Environment"];
                    var environmentSuffix = environment != null ? "@" + environment : string.Empty;
                    // Encrypted connection strings here
                    string EWEEntitiesConnectionStringName = "EWEEntities" + environmentSuffix;
                    string EWEADOconnectionStringName = "EWEADO" + environmentSuffix;


                    //Decrypt connection string here
                    _connectionString = Cryptography.Decrypt(ConfigurationManager.ConnectionStrings[EWEEntitiesConnectionStringName].ConnectionString);
                    _eweAdoConnectionString = Cryptography.Decrypt(ConfigurationManager.ConnectionStrings[EWEADOconnectionStringName].ConnectionString);
                }
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
