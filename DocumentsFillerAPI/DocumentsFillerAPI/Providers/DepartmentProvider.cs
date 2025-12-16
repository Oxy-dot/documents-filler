using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;
using NpgsqlTypes;
using System.Linq;

namespace DocumentsFillerAPI.Providers
{
	public class DepartmentProvider
	{
		private static NpgsqlDataSource DataSource => StaticHelper.DataSource;

		public async Task<(ResultMessage Message, List<DeleteDepartmentStruct> Results)> Delete(List<Guid> departmentIDs)
		{
			if (departmentIDs == null || departmentIDs.Count == 0)
			{
				return new(new ResultMessage() { IsSuccess = true, Message = "Успешно" }, new());
			}

			await using var connection = await DataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.department
					SET is_deleted = True
					WHERE id = @id
					";

				List<DeleteDepartmentStruct> results = new List<DeleteDepartmentStruct>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					cmd.Parameters.Add(idParam);

					foreach (Guid departmentID in departmentIDs)
					{
						try
						{
							idParam.Value = departmentID;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с ИД={departmentID} не была обновлена");

							results.Add(new DeleteDepartmentStruct { DepartmentID = departmentID, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new DeleteDepartmentStruct { DepartmentID = departmentID, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				await transaction.CommitAsync();

				ResultMessage message = new ResultMessage
				{
					Message = results.Count(a => !a.IsSuccess) == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", results)}",
					IsSuccess = true,
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

				return new(message, new());
			}
		}

		public async Task<ResultMessage> Insert(IEnumerable<DepartmentStruct> departments)
		{
			var departmentsList = departments.ToList();
			if (departmentsList.Count == 0)
			{
				return new ResultMessage() { IsSuccess = true, Message = "Успешно" };
			}

			await using var connection = await DataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.department(id, name, full_name)
					VALUES (@id, @name, @fullName);
					";

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var nameParam = new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text);
					var fullNameParam = new NpgsqlParameter("@fullName", NpgsqlTypes.NpgsqlDbType.Text);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(nameParam);
					cmd.Parameters.Add(fullNameParam);

					foreach (DepartmentStruct department in departmentsList)
					{
						try
						{
							idParam.Value = Guid.NewGuid();
							nameParam.Value = department.Name;
							fullNameParam.Value = department.FullName ?? (object)DBNull.Value;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с именем={department.Name} не была добавлена");
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

		public async Task<(ResultMessage Message, List<DepartmentStruct> Departments)> Search(string searchText)
		{
			try
			{
				string sql =
					$@"
					SELECT public.departments.id,
						   public.departments.name,
						   public.departments.full_name
					FROM public.departments
					WHERE public.departments.id like '%@seachText%' OR
						  public.departments.name like '%@seachText%'";

				List<DepartmentStruct> results = new List<DepartmentStruct>();

				await using (var cmd = DataSource.CreateCommand(sql))
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@seachText", searchText);

					await using var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						results.Add(new DepartmentStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
							FullName = reader.IsDBNull(2) ? "" : reader.GetString(2)
						});
					}
				}

				return new(new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return new(new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<DepartmentStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<UpdateDepartmentStruct> DepartmentsResults)> Update(IEnumerable<DepartmentStruct> departments)
		{
			var departmentsList = departments.ToList();
			if (departmentsList.Count == 0)
			{
				return (new ResultMessage { Message = "Успешно", IsSuccess = true }, new List<UpdateDepartmentStruct>());
			}

			await using var connection = await DataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.department
					SET name=@name, full_name=@fullName
					WHERE id = @id
					";

				List<UpdateDepartmentStruct> results = new List<UpdateDepartmentStruct>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var nameParam = new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text);
					var fullNameParam = new NpgsqlParameter("@fullName", NpgsqlTypes.NpgsqlDbType.Text);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(nameParam);
					cmd.Parameters.Add(fullNameParam);

					foreach (DepartmentStruct department in departmentsList)
					{
						try
						{
							idParam.Value = department.ID;
							nameParam.Value = department.Name;
							fullNameParam.Value = department.FullName ?? (object)DBNull.Value;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с названием={department.Name} не была обновлена");

							results.Add(new UpdateDepartmentStruct { Department  = department, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateDepartmentStruct { Department = department, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				await transaction.CommitAsync();

				ResultMessage message = new ResultMessage
				{
					Message = results.Count(a => !a.IsSuccess) == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", results.Where(r => !r.IsSuccess).Select(r => r.Message))}",
					IsSuccess = true,
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

				return (message, new List<UpdateDepartmentStruct>());
			}
		}

		public async Task<(ResultMessage, List<DepartmentStruct>)> List(uint count, uint startIndex)
		{
			try
			{
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

				await using (var cmd = DataSource.CreateCommand(sql))
				{
					await using var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						results.Add(new DepartmentStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
							FullName = reader.IsDBNull(2) ? "" : reader.GetString(2),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<DepartmentStruct>());
			}
		}

		public async Task<(ResultMessage, DepartmentStruct?)> Get(string name)
		{
			try
			{
				string sql =
					$@"
					SELECT id,
						   name,
						   full_name
					FROM public.department
					WHERE (full_name = '{name}' OR
						  name = '{name}') AND
						  is_deleted = False";

				List<DepartmentStruct> results = new List<DepartmentStruct>();

				await using (var cmd = DataSource.CreateCommand(sql))
				{
					await using var reader = await cmd.ExecuteReaderAsync();
					if (await reader.ReadAsync())
					{
						results.Add(new DepartmentStruct
						{
							ID = reader.GetGuid(0),
							Name = reader.GetString(1),
							FullName = reader.IsDBNull(2) ? "" : reader.GetString(2),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results.FirstOrDefault());
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, default);
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
