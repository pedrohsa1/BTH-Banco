using System.Data.Common;
using Bth.Common.Application.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Bth.Common.Infrastructure.Data;

internal sealed class DbConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(configuration.GetConnectionString("Database"));
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
