using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class AcademicTitlePostgreProvider
	{
		private string connectionString = "";

		public async Task<ResultMessage> Delete(IEnumerable<Guid> titles)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.academic_title
					SET is_deleted = True
					WHERE id IN ('{string.Join("','", titles)}')
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					cmd.ExecuteNonQuery();
				}

				return new ResultMessage() { IsSuccess = true, Message = "Success" };
			}
			catch (Exception ex)
			{
				return new ResultMessage() { IsSuccess = false, Message = ex.Message };
			}
		}

		public async Task<ResultMessage> Insert(IEnumerable<AcadimicTitleStruct> titles)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					INSERT INTO public.academic_title(id, name, short_name)
					VALUES (@id, @name, @short_name);
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (AcadimicTitleStruct title in titles)
					{
						cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
						cmd.Parameters.AddWithValue("@name", title.Name);
						cmd.Parameters.AddWithValue("@short_name", title.ShortName);

						int cnt = cmd.ExecuteNonQuery();
						if (cnt != 1)
							throw new Exception($"Row with name={title.Name} and short name={title.ShortName} wasnt inserted");
					}
				}

				return new ResultMessage() { Message = "Success", IsSuccess = true };
			}
			catch (Exception ex)
			{
				return new ResultMessage() { Message = ex.Message, IsSuccess = false };
			}
		}

		public void Search()
		{
			throw new NotImplementedException();
		}

		public async Task<(ResultMessage Message, List<UpdateAcademicTitleStruct> AcademicTitlesResult)> Update(IEnumerable<AcadimicTitleStruct> titles)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.academic_title
					SET name=@name, short_name=@shortName
					WHERE id = @id
					";

				List<UpdateAcademicTitleStruct> results = new List<UpdateAcademicTitleStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (AcadimicTitleStruct title in titles)
					{
						try
						{
							cmd.Parameters.AddWithValue("@id", title.ID);
							cmd.Parameters.AddWithValue("@name", title.Name);
							cmd.Parameters.AddWithValue("@short_name", title.ShortName);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with title id={title.ID} wasnt updated");

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
					Message = results.Count == 0 ? "Success" : "Success with errors",
					IsSuccess = results.Count == 0,
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

		public async Task<(ResultMessage, List<AcadimicTitleStruct>)> List(uint count, uint startIndex)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SELECT *, ROW_NUMBER() OVER (ORDER BY bet_id ASC, is_deleted DESC) AS row_id FROM betv2

				string sql =
					$@"
					SELECT id,
						   name,
						   short_name,
						   ROW_NUMBER() OVER (ORDER BY id ASC, is_deleted DESC) AS row_id
					FROM public.academic_title
					WHERE row_id >= {startIndex} AND 
						  is_deleted = False
					LIMIT {count}";

				List<AcadimicTitleStruct> results = new List<AcadimicTitleStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new AcadimicTitleStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
							ShortName = reader.GetString(2)
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<AcadimicTitleStruct>());
			}
		}

		public record UpdateAcademicTitleStruct
		{
			public AcadimicTitleStruct Title { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
