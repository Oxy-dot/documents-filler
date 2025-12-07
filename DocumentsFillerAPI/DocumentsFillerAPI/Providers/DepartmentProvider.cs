using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;
using static DocumentsFillerAPI.Providers.AcademicTitlePostgreProvider;

namespace DocumentsFillerAPI.Providers
{
	public class DepartmentProvider
	{
		private string connectionString = "Host=localhost;Port=5432;Database=document_filler;Username=postgres;Password=root";

		public async Task<(ResultMessage Message, List<DeleteDepartmentStruct> Results)> Delete(List<Guid> departmentIDs)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.department
					SET is_deleted = True
					WHERE id = @id
					";

				List<DeleteDepartmentStruct> results = new List<DeleteDepartmentStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (Guid departmentID in departmentIDs)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", departmentID);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with departmentID={departmentID} wasnt updated");

							results.Add(new DeleteDepartmentStruct { DepartmentID = departmentID, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new DeleteDepartmentStruct { DepartmentID = departmentID, IsSuccess = false, Message = ex.Message });
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

				return new(message, new());
			}
		}

		public async Task<ResultMessage> Insert(IEnumerable<DepartmentStruct> departments)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.department(id, name, full_name)
					VALUES (@id, @name, @fullName);
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (DepartmentStruct department in departments)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
							cmd.Parameters.AddWithValue("@name", department.Name);
							cmd.Parameters.AddWithValue("@fullName", department.FullName ?? (object)DBNull.Value);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Row with name={department.Name} wasnt inserted");
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

		public async Task<(ResultMessage Message, List<DepartmentStruct> Departments)> Search(string searchText)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					SELECT public.departments.id,
						   public.departments.name,
						   public.departments.full_name
					FROM public.departments
					WHERE public.departments.id like '%@seachText%' OR
						  public.departments.name like '%@seachText%'";

				List<DepartmentStruct> results = new List<DepartmentStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@seachText", searchText);

					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new DepartmentStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
							FullName = reader.IsDBNull(2) ? "" : reader.GetString(2)
						});
					}
				}

				return new(new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
			}
			catch (Exception ex)
			{
				return new(new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<DepartmentStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<UpdateDepartmentStruct> DepartmentsResults)> Update(IEnumerable<DepartmentStruct> departments)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.department
					SET name=@name, full_name=@fullName
					WHERE id = @id
					";

				List<UpdateDepartmentStruct> results = new List<UpdateDepartmentStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (DepartmentStruct department in departments)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", department.ID);
							cmd.Parameters.AddWithValue("@name", department.Name);
							cmd.Parameters.AddWithValue("@fullName", department.FullName ?? (object)DBNull.Value);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with title name={department.Name} wasnt updated");

							results.Add(new UpdateDepartmentStruct { Department  = department, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateDepartmentStruct { Department = department, IsSuccess = false, Message = ex.Message });
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
						   full_name,
						   ROW_NUMBER() OVER (ORDER BY id ASC, is_deleted DESC) AS row_id
					FROM public.department
					WHERE is_deleted = False
					OFFSET {startIndex}
					{(count == 0 ? "" : $"LIMIT {count}")}";

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
							FullName = reader.IsDBNull(2) ? "" : reader.GetString(2),
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

		public record DeleteDepartmentStruct
		{
			public Guid DepartmentID { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
