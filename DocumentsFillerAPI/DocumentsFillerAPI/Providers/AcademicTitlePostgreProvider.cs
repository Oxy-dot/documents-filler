using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;
using NpgsqlTypes;
using System.Linq;

namespace DocumentsFillerAPI.Providers
{
	public class AcademicTitlePostgreProvider
	{
		private string connectionString = ConfigProvider.Get<string>("ConnectionStrings:PgSQL");

		public async Task<ResultMessage> Insert(IEnumerable<AcademicTitleStruct> titles)
		{
			var titlesList = titles.ToList();
			if (titlesList.Count == 0)
			{
				return new ResultMessage() { IsSuccess = true, Message = "Успешно" };
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.academic_title(id, name)
					VALUES (@id, @name);
					";

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var nameParam = new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(nameParam);

					foreach (AcademicTitleStruct title in titlesList)
					{
						try
						{
							idParam.Value = Guid.NewGuid();
							nameParam.Value = title.Name;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с названием={title.Name} и коротким названием={title.ShortName} не была вставлена");
						}
						catch (Exception ex)
						{
							errors.Add(ex.Message);
						}
					}
				}

				await transaction.CommitAsync();

				string message = errors.Count == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", errors)}";

				return new ResultMessage() { IsSuccess = true, Message = message };
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new ResultMessage() { Message = ex.Message };
			}
		}

		public async Task<(ResultMessage Message, List<AcademicTitleStruct> Titles)> Search(string searchText)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					SELECT public.academic_title.id,
						   public.academic_title.name
					FROM public.academic_title
					WHERE public.academic_title.id like '%@seachText%' OR
						  public.academic_title.name like '%@seachText%'";

				List<AcademicTitleStruct> results = new List<AcademicTitleStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@seachText", searchText);

					var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						results.Add(new AcademicTitleStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1)
						});
					}
				}

				return new (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return new(new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<AcademicTitleStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<UpdateAcademicTitleStruct> AcademicTitlesResult)> Update(IEnumerable<AcademicTitleStruct> titles)
		{
			var titlesList = titles.ToList();
			if (titlesList.Count == 0)
			{
				return (new ResultMessage { Message = "Успешно", IsSuccess = true }, new List<UpdateAcademicTitleStruct>());
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.academic_title
					SET name=@name
					WHERE id = @id
					";

				List<UpdateAcademicTitleStruct> results = new List<UpdateAcademicTitleStruct>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var nameParam = new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(nameParam);

					foreach (AcademicTitleStruct title in titlesList)
					{
						try
						{
							idParam.Value = title.ID;
							nameParam.Value = title.Name;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с названием={title.Name} не была обновлена");

							results.Add(new UpdateAcademicTitleStruct { Title = title, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateAcademicTitleStruct { Title = title, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				await transaction.CommitAsync();

				ResultMessage message = new ResultMessage
				{
					Message = results.Count(a => !a.IsSuccess) == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", results.Where(r => !r.IsSuccess).Select(r => r.Message))}",
					IsSuccess = results.Count(a => !a.IsSuccess) == 0,
				};

				return (message, results);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				ResultMessage message = new ResultMessage
				{
					Message = ex.Message,
					IsSuccess = false,
				};

				return (message, new List<UpdateAcademicTitleStruct>());
			}
		}

		public async Task<(ResultMessage, List<AcademicTitleStruct>)> List(uint count, uint startIndex)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SELECT *, ROW_NUMBER() OVER (ORDER BY bet_id ASC, is_deleted DESC) AS row_id FROM betv2

				string sql =
					$@"
					SELECT id,
						   name,
						   ROW_NUMBER() OVER (ORDER BY id ASC, is_deleted DESC) AS row_id
					FROM public.academic_title
					WHERE is_deleted = False
					OFFSET {startIndex}
					{(count == 0 ? "" : $"LIMIT {count}")}";

				List<AcademicTitleStruct> results = new List<AcademicTitleStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						results.Add(new AcademicTitleStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<AcademicTitleStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<DeleteAcademicTitleStruct> Results)> Delete(List<Guid> titles)
		{
			if (titles == null || titles.Count == 0)
			{
				return new (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, new());
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.academic_title
					SET is_deleted = True
					WHERE id = @id
					";

				List<DeleteAcademicTitleStruct> results = new List<DeleteAcademicTitleStruct>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					cmd.Parameters.Add(idParam);

					foreach (Guid titleID in titles)
					{
						try
						{
							idParam.Value = titleID;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с ИД={titleID} не была обновлена");

							results.Add(new DeleteAcademicTitleStruct { TitleID = titleID, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new DeleteAcademicTitleStruct { TitleID = titleID, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				await transaction.CommitAsync();

				ResultMessage message = new ResultMessage
				{
					Message = results.Count(a => !a.IsSuccess) == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", results)}",
					IsSuccess = results.Count(a => !a.IsSuccess) == 0,
				};

				return (message, results);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				ResultMessage message = new ResultMessage
				{
					Message = ex.Message,
					IsSuccess = false
				};

				return new (message, new());
			}
		}

		public record UpdateAcademicTitleStruct
		{
			public AcademicTitleStruct Title { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}

		public record DeleteAcademicTitleStruct
		{
			public Guid TitleID { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
