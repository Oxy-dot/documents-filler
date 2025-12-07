using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class AcademicTitlePostgreProvider
	{
		private string connectionString = "Host=localhost;Port=5432;Database=document_filler;Username=postgres;Password=root";

		public async Task<ResultMessage> Insert(IEnumerable<AcademicTitleStruct> titles)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.academic_title(id, name)
					VALUES (@id, @name);
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (AcademicTitleStruct title in titles)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
							cmd.Parameters.AddWithValue("@name", title.Name);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Row with name={title.Name} and short name={title.ShortName} wasnt inserted");
						}
						catch (Exception ex)
						{
							errors.Add(ex.Message);
						}
					}
				}

				string message = errors.Count == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", errors)}";

				return new ResultMessage() { IsSuccess = true, Message = message };
			}
			catch (Exception ex)
			{
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

					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new AcademicTitleStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1)
						});
					}
				}

				return new (new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
			}
			catch (Exception ex)
			{
				return new(new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<AcademicTitleStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<UpdateAcademicTitleStruct> AcademicTitlesResult)> Update(IEnumerable<AcademicTitleStruct> titles)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.academic_title
					SET name=@name
					WHERE id = @id
					";

				List<UpdateAcademicTitleStruct> results = new List<UpdateAcademicTitleStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (AcademicTitleStruct title in titles)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", title.ID);
							cmd.Parameters.AddWithValue("@name", title.Name);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with title name={title.Name} wasnt updated");

							results.Add(new UpdateAcademicTitleStruct { Title = title, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateAcademicTitleStruct { Title = title, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				ResultMessage message = new ResultMessage
				{
					Message = results.Count(a => !a.IsSuccess) == 0 ? "Success" : "Success with errors",
					IsSuccess = results.Count(a => !a.IsSuccess) == 0,
				};

				return (message, results);
			}
			catch (Exception ex)
			{
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
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new AcademicTitleStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<AcademicTitleStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<DeleteAcademicTitleStruct> Results)> Delete(List<Guid> titles)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.academic_title
					SET is_deleted = True
					WHERE id = @id
					";

				List<DeleteAcademicTitleStruct> results = new List<DeleteAcademicTitleStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (Guid titleID in titles)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", titleID);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with titleID={titleID} wasnt updated");

							results.Add(new DeleteAcademicTitleStruct { TitleID = titleID, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new DeleteAcademicTitleStruct { TitleID = titleID, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				ResultMessage message = new ResultMessage
				{
					Message = results.Count(a => !a.IsSuccess) == 0 ? "Success" : "Success with errors",
					IsSuccess = results.Count(a => !a.IsSuccess) == 0,
				};

				return (message, results);
			}
			catch (Exception ex)
			{
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
