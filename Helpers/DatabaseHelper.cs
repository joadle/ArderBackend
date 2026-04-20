using System.Text.RegularExpressions;

namespace ArderBackend.Helpers
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            // Intenta obtener la URL de conexión que Railway inyecta de forma automática
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                bool isUri = Uri.TryCreate(databaseUrl, UriKind.Absolute, out Uri? databaseUri);
                if (isUri && databaseUri != null)
                {
                    // Formato habitual de DATABASE_URL en Railway:
                    // postgresql://user:password@host:port/dbname
                    var userInfo = databaseUri.UserInfo.Split(':');
                    var builder = new Npgsql.NpgsqlConnectionStringBuilder
                    {
                        Host = databaseUri.Host,
                        Port = databaseUri.Port,
                        Username = userInfo[0],
                        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                        Database = databaseUri.LocalPath.TrimStart('/')
                    };
                    
                    return builder.ToString();
                }
            }

            // Fallback al connection string en appsettings.json si estamos en local y la variable no existe
            return configuration.GetConnectionString("DefaultConnection") 
                   ?? throw new InvalidOperationException("No se encontró cadena de conexión y DATABASE_URL no está configurada.");
        }
    }
}
