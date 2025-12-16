using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public static class StaticHelper
	{
		private static readonly string connectionString = ConfigProvider.Get<string>("ConnectionStrings:PgSQL");
		private static readonly Lazy<NpgsqlDataSource> _dataSource = new Lazy<NpgsqlDataSource>(() => NpgsqlDataSource.Create(connectionString));
		
		public static NpgsqlDataSource DataSource => _dataSource.Value;
	}
}
