using System.Data;

namespace AuthService.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection(string connectionName = "DefaultConnection");
}

