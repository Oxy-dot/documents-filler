using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class DepartmentProvider
	{
		private string connectionString = "";

		public async Task<ResultMessage> Delete(IEnumerable<Guid> departments)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.department
					SET is_deleted = True
					WHERE id IN ('{string.Join("','", departments)}')
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

		public async Task<ResultMessage> Insert(IEnumerable<DepartmentStruct> departments)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					INSERT INTO public.department(id, name, short_name)
					VALUES (@id, @name, @short_name);
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (DepartmentStruct department in departments)
					{
						cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
						cmd.Parameters.AddWithValue("@name", department.Name);
						cmd.Parameters.AddWithValue("@short_name", department.ShortName);

						int cnt = cmd.ExecuteNonQuery();
						if (cnt != 1)
							throw new Exception($"Row with name={department.Name} and short name={department.ShortName} wasnt inserted");
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

		public async Task<(ResultMessage Message, List<UpdateDepartmentStruct> DepartmentsResults)> Update(IEnumerable<DepartmentStruct> departments)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.department
					SET name=@name, short_name=@shortName
					WHERE id = @id
					";

				List<UpdateDepartmentStruct> results = new List<UpdateDepartmentStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (DepartmentStruct department in departments)
					{
						try
						{
							cmd.Parameters.AddWithValue("@id", department.ID);
							cmd.Parameters.AddWithValue("@name", department.Name);
							cmd.Parameters.AddWithValue("@short_name", department.ShortName);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with department id={department.ID} wasnt updated");

							results.Add(new UpdateDepartmentStruct { Department = department, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateDepartmentStruct { Department = department, IsSuccess = false, Message = ex.Message });
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

				return (message, new List<UpdateDepartmentStruct>());
			}
		}

		public async Task<(ResultMessage, List<DepartmentStruct>)> List(uint count, uint startIndex)
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
					FROM public.department
					WHERE row_id >= {startIndex} AND 
						  is_deleted = False
					LIMIT {count}";

				List<DepartmentStruct> results = new List<DepartmentStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new DepartmentStruct
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
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<DepartmentStruct>());
			}
		}

		public record UpdateDepartmentStruct
		{
			public DepartmentStruct Department { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
