using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace JDMallen.Toolbox.EFCore.Extensions
{
	public static class ServiceExtensions
	{
		/// <summary>
		/// CAREFUL with this! It does exactly as it describes with no warning:
		/// drops all the tables and then recreates them. Useful for designing
		/// the structure of your entities/DB, but disastrous for production.
		/// </summary>
		public static async Task DropTablesAndEnsureCreated(
			this DbContext dbContext,
			bool dropTables = true,
			IEnumerable<string> orderedTablesToDrop = null,
			CancellationToken cancellationToken = default)
		{
			if (dropTables)
			{
				List<string> orderOfDroppage;
				if (orderedTablesToDrop != null)
				{
					orderOfDroppage = orderedTablesToDrop.ToList();
				}
				else
				{
					orderOfDroppage = dbContext.Model.GetEntityTypes()
					                           .Select(
						                           et => et.GetAnnotations()
						                                   .FirstOrDefault(
							                                   x => x.Name
								                                   == "Relational:TableName")
						                                   ?.Value.ToString()
						                                   .ToLowerInvariant()
						                                 ?? et.ClrType.Name.ToLowerInvariant())
					                           .ToList();
				}

				var conn = dbContext.Database.GetDbConnection();
				await conn.OpenAsync(cancellationToken);
				orderOfDroppage.ForEach(
					async tableName =>
					{
						bool tableExists;
						await using (var cmd = conn.CreateCommand())
						{
							cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES "
							                  + "WHERE TABLE_CATALOG = "
							                  + $"\'{dbContext.Database.GetDbConnection().Database}\' "
							                  + $"AND TABLE_NAME = \'{tableName.ToLowerInvariant()}\';";
							cmd.CommandType = CommandType.Text;
							await using (var reader =
								await cmd.ExecuteReaderAsync(cancellationToken))
							{
								await reader.ReadAsync(cancellationToken);
								tableExists = reader.GetInt32(0) > 0;
								await reader.CloseAsync();
							}
						}

						if (!tableExists)
							return;
						var dropTable = $"DROP TABLE {tableName.ToLowerInvariant()};";
						await dbContext.Database.ExecuteSqlRawAsync(dropTable, cancellationToken);
					});
				await conn.CloseAsync();
			}

			await dbContext.Database.EnsureCreatedAsync(cancellationToken);
		}
	}
}
