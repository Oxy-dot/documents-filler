using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class StaticHelper
	{
		public static string connectionString = ConfigProvider.Get<string>("ConnectionStrings:PgSQL");
		public static NpgsqlDataSource dataSource;
		private StaticHelper()
		{
			dataSource = NpgsqlDataSource.Create(connectionString);
		}
	}
}
