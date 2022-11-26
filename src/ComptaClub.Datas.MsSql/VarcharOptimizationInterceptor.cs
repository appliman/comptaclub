using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;

namespace ComptaClub.Datas;

internal class VarcharOptimizationInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        AdjustVarCharParametersSize(command);

        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        AdjustVarCharParametersSize(command);

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void AdjustVarCharParametersSize(DbCommand command)
    {
        foreach (SqlParameter parameter in command.Parameters)
        {
            if ((parameter.SqlDbType == System.Data.SqlDbType.NVarChar || parameter.SqlDbType == System.Data.SqlDbType.VarChar)
                && parameter.Value != null && !(parameter.Value is DBNull)
                 )
            {
                var value = (string)parameter.Value;
                parameter.Size = value.Length;
            }
        }
    }
}
