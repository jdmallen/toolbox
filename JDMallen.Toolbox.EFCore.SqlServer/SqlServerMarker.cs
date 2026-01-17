namespace JDMallen.Toolbox.EFCore.SqlServer
{
	/// <summary>
	/// Marker class for the JDMallen.Toolbox.EFCore.SqlServer assembly.
	/// This package brings in Microsoft.EntityFrameworkCore.SqlServer for SQL Server support.
	/// </summary>
	public static class SqlServerMarker
	{
		/// <summary>
		/// Gets the assembly name.
		/// </summary>
		public static string AssemblyName => typeof(SqlServerMarker).Assembly.GetName().Name;
	}
}
