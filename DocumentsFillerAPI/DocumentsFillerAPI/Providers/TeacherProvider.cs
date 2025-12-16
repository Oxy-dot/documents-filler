using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;
using NpgsqlTypes;
using System.Linq;

namespace DocumentsFillerAPI.Providers
{
	public class TeacherProvider
	{
		private string connectionString = ConfigProvider.Get<string>("ConnectionStrings:PgSQL");

		public async Task<(ResultMessage Message, List<DeleteTeachersRecord> DeleteResults)> Delete(IEnumerable<Guid> teachers)
		{
			var teachersList = teachers.ToList();
			if (teachersList.Count == 0)
			{
				return new (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, new List<DeleteTeachersRecord>());
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.teacher
					SET is_deleted = True
					WHERE id = @id
					";

				List<DeleteTeachersRecord> deleteResults = new List<DeleteTeachersRecord>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					cmd.Parameters.Add(idParam);

					foreach (Guid teacherID in teachersList)
					{
						try
						{
							idParam.Value = teacherID;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с ИД={teacherID} не была добавлена");

							deleteResults.Add(new DeleteTeachersRecord { TeacherID = teacherID, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							deleteResults.Add(new DeleteTeachersRecord { TeacherID = teacherID, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				await transaction.CommitAsync();

				ResultMessage message = new ResultMessage
				{
					Message = deleteResults.Count(a => !a.IsSuccess) == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", deleteResults)}",
					IsSuccess = true,
				};

				return new (message, deleteResults);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<DeleteTeachersRecord>());
			}
		}

		public async Task<ResultMessage> Insert(IEnumerable<TeacherFullInfoStruct> teachers)
		{
			var teachersList = teachers.ToList();
			if (teachersList.Count == 0)
			{
				return new ResultMessage { IsSuccess = true, Message = "Успешно" };
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.teacher(id, first_name, second_name, patronymic, academic_title)
					VALUES (@id, @firstName, @secondName, @patronymic, @academicTitle);
					";

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var firstNameParam = new NpgsqlParameter("@firstName", NpgsqlTypes.NpgsqlDbType.Text);
					var secondNameParam = new NpgsqlParameter("@secondName", NpgsqlTypes.NpgsqlDbType.Text);
					var patronymicParam = new NpgsqlParameter("@patronymic", NpgsqlTypes.NpgsqlDbType.Text);
					var academicTitleParam = new NpgsqlParameter("@academicTitle", NpgsqlTypes.NpgsqlDbType.Uuid);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(firstNameParam);
					cmd.Parameters.Add(secondNameParam);
					cmd.Parameters.Add(patronymicParam);
					cmd.Parameters.Add(academicTitleParam);

					foreach (TeacherFullInfoStruct teacher in teachersList)
					{
						try
						{
							idParam.Value = Guid.NewGuid();
							firstNameParam.Value = teacher.FirstName;
							secondNameParam.Value = teacher.SecondName;
							patronymicParam.Value = teacher.Patronymic;
							academicTitleParam.Value = (object)teacher.AcademicTitle?.ID ?? DBNull.Value;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с именем: {teacher.FirstName}, фамилией: {teacher.SecondName}, отчеством: {teacher.Patronymic}");
						}
						catch (Exception ex)
						{
							errors.Add(ex.Message);
						}
					}
				}

				await transaction.CommitAsync();

				string message = errors.Count == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", errors)}";

				return new ResultMessage { IsSuccess = true, Message = message };
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new ResultMessage { IsSuccess = false, Message = ex.Message };
			}
		}

		public void Search()
		{
			throw new NotImplementedException();
		}

		public async Task<(ResultMessage Message, List<UpdateTeacherRecord> TeachersResult)> Update(IEnumerable<TeacherFullInfoStruct> teachers)
		{
			var teachersList = teachers.ToList();
			if (teachersList.Count == 0)
			{
				return (new ResultMessage { Message = "Успешно", IsSuccess = true }, new List<UpdateTeacherRecord>());
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.teacher
					SET first_name=@firstName, second_name=@secondName, patronymic=@patronymic, academic_title=@academicTitle
					WHERE id = @id
					";

				List<UpdateTeacherRecord> results = new List<UpdateTeacherRecord>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var firstNameParam = new NpgsqlParameter("@firstName", NpgsqlTypes.NpgsqlDbType.Text);
					var secondNameParam = new NpgsqlParameter("@secondName", NpgsqlTypes.NpgsqlDbType.Text);
					var patronymicParam = new NpgsqlParameter("@patronymic", NpgsqlTypes.NpgsqlDbType.Text);
					var academicTitleParam = new NpgsqlParameter("@academicTitle", NpgsqlTypes.NpgsqlDbType.Uuid);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(firstNameParam);
					cmd.Parameters.Add(secondNameParam);
					cmd.Parameters.Add(patronymicParam);
					cmd.Parameters.Add(academicTitleParam);

					foreach (TeacherFullInfoStruct teacher in teachersList)
					{
						try
						{
							idParam.Value = teacher.ID;
							firstNameParam.Value = teacher.FirstName;
							secondNameParam.Value = teacher.SecondName;
							patronymicParam.Value = teacher.Patronymic;
							academicTitleParam.Value = (object)teacher.AcademicTitle?.ID ?? DBNull.Value;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с ИД={teacher.ID} не была обновлена");

							results.Add(new UpdateTeacherRecord { Teacher = teacher, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateTeacherRecord { Teacher = teacher, IsSuccess = false, Message = ex.Message });
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

				return (message, new List<UpdateTeacherRecord>());
			}
		}

		public async Task<(ResultMessage Message, List<TeacherFullInfoStruct> Teachers)> FullList(uint count, uint startIndex)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SELECT *, ROW_NUMBER() OVER (ORDER BY bet_id ASC, is_deleted DESC) AS row_id FROM betv2

				string sql =
					$@"
					SELECT teacher.id,
						   first_name, 
						   second_name,
						   patronymic,
						   public.academic_title.id,
						   public.academic_title.name,
						   ROW_NUMBER() OVER (ORDER BY public.teacher.id ASC) AS row_id
					FROM public.teacher LEFT JOIN
						   public.academic_title ON public.teacher.academic_title = public.academic_title.id AND
													public.academic_title.is_deleted = False
					WHERE public.teacher.is_deleted = False
					OFFSET {startIndex}
					LIMIT {(count == 0 ? "NULL" : count.ToString())}";

				//main_bet_id, 
				//		   second_bet_id, 
				//		   excessive_bet_id

				List<TeacherFullInfoStruct> results = new List<TeacherFullInfoStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						results.Add(new TeacherFullInfoStruct
						{
							ID = reader.GetGuid(0),
							FirstName = reader.GetString(1),
							SecondName = reader.GetString(2),
							Patronymic = reader.GetString(3),
							AcademicTitle = reader.IsDBNull(4) ? (AcademicTitleStruct?)null :  new AcademicTitleStruct
							{
								ID = reader.GetGuid(4),
								Name = reader.GetString(5)
							}
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<TeacherFullInfoStruct>());
			}
		}

		public async Task<(ResultMessage Result, Guid TeacherID)> FindTeacherByShortName(string shortName)
		{
			try
			{
				var splittedShortName = shortName.Split(' ');
				string secondName = string.Empty;
				string firstNameLetter = string.Empty;
				string patronymicLetter = string.Empty;
				if (splittedShortName.Length == 3)
				{
					secondName = splittedShortName[0];
					firstNameLetter = splittedShortName[1].First().ToString();
					patronymicLetter = splittedShortName[2].First().ToString();
				}
				else 
				{
					secondName = splittedShortName[0];

					var firstNameWithPatronymic = splittedShortName[1].Split('.');
					firstNameLetter = firstNameWithPatronymic[0].First().ToString();
					patronymicLetter = firstNameWithPatronymic.Length == 2 ? firstNameWithPatronymic[1].First().ToString() : "";
				}

				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SELECT *, ROW_NUMBER() OVER (ORDER BY bet_id ASC, is_deleted DESC) AS row_id FROM betv2

				string sql =
					$@"
					SELECT id
					FROM public.teacher
					WHERE is_deleted = False AND
						  second_name = @secondName AND
					      first_name like '{firstNameLetter}%' AND
						  patronymic like '{patronymicLetter}%'";


				await using (var cmd = dataSource.CreateCommand(sql))
				{
					cmd.Parameters.AddWithValue("@secondName", secondName);
					var reader = await cmd.ExecuteReaderAsync();

					if (await reader.ReadAsync())
					{
						return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, reader.GetGuid(0));
					}
					return (new ResultMessage() { IsSuccess = false, Message = "Преподаватель не найден" }, default);
				}
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, default);
			}
		}

		public record UpdateTeacherRecord
		{
			public TeacherFullInfoStruct Teacher { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}

		public record DeleteTeachersRecord
		{
			public Guid TeacherID { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
