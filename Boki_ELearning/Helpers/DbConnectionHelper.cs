using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;

namespace Boki_ELearning.Helpers
{
    public static class DbConnectionHelper
    {
        public static SqlConnection CreateSqlConnection()
        {
            var entityConnectionString = ConfigurationManager.ConnectionStrings["BokiDBEntities1"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(entityConnectionString))
            {
                throw new ConfigurationErrorsException("Missing connection string BokiDBEntities1.");
            }

            var builder = new EntityConnectionStringBuilder(entityConnectionString);
            return new SqlConnection(builder.ProviderConnectionString);
        }
    }
}
