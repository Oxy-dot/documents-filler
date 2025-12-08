using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;
using NpgsqlTypes;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;

namespace DocumentsFillerAPI.Providers
{
	public class TeacherProvider
	{
		private string connectionString = "Host=localhost;Port=5432;Database=document_filler;Username=postgres;Password=root";

		public async Task<(ResultMessage Message, List<DeleteTeachersRecord> DeleteResults)> Delete(IEnumerable<Guid> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//IN('{string.Join("', '", teachers)}')
				string sql =
					$@"
					UPDATE public.teacher
					SET is_deleted = True
					WHERE id = @id
					";

				List<DeleteTeachersRecord> deleteResults = new List<DeleteTeachersRecord>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (Guid teacherID in teachers)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", teacherID);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with teacherID={teacherID} wasnt updated");

							deleteResults.Add(new DeleteTeachersRecord { TeacherID = teacherID, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							deleteResults.Add(new DeleteTeachersRecord { TeacherID = teacherID, IsSuccess = false, Message = ex.Message });
						}
					}
				}

				ResultMessage message = new ResultMessage
				{
					Message = deleteResults.Count(a => !a.IsSuccess) == 0 ? "Success" : "Success with errors",
					IsSuccess = deleteResults.Count(a => !a.IsSuccess) == 0,
				};

				return new (message, deleteResults);
			}
			catch (Exception ex)
			{
				return new (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<DeleteTeachersRecord>());
			}
		}

		public async Task<ResultMessage> Insert(IEnumerable<TeacherFullInfoStruct> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.teacher(id, first_name, second_name, patronymic, academic_title)
					VALUES (@id, @firstName, @secondName, @patronymic, @academicTitle);
					";


				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (TeacherFullInfoStruct teacher in teachers)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
							cmd.Parameters.AddWithValue("@firstName", teacher.FirstName);
							cmd.Parameters.AddWithValue("@secondName", teacher.SecondName);
							cmd.Parameters.AddWithValue("@patronymic", teacher.Patronymic);
							cmd.Parameters.AddWithValue("@academicTitle", NpgsqlTypes.NpgsqlDbType.Uuid, (object)teacher.AcademicTitle?.ID ?? DBNull.Value);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Строка с именем: {teacher.FirstName}, фамилией: {teacher.SecondName}, отчеством: {teacher.Patronymic}");
						}
						catch (Exception ex)
						{
							errors.Add(ex.Message);
						}
					}
				}

				string message = errors.Count == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", errors)}";

				return new ResultMessage { IsSuccess = true, Message = message };
			}
			catch (Exception ex)
			{
				return new ResultMessage { IsSuccess = false, Message = ex.Message };
			}
		}

		public void Search()
		{
			throw new NotImplementedException();
		}

		public async Task<(ResultMessage Message, List<UpdateTeacherRecord> TeachersResult)> Update(IEnumerable<TeacherFullInfoStruct> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.teacher
					SET first_name=@firstName, second_name=@secondName, patronymic=@patronymic, academic_title=@academicTitle
					WHERE id = @id
					";

				List<UpdateTeacherRecord> results = new List<UpdateTeacherRecord>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (TeacherFullInfoStruct teacher in teachers)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", teacher.ID);
							cmd.Parameters.AddWithValue("@firstName", teacher.FirstName);
							cmd.Parameters.AddWithValue("@secondName", teacher.SecondName);
							cmd.Parameters.AddWithValue("@patronymic", teacher.Patronymic);
							cmd.Parameters.AddWithValue("@academicTitle", NpgsqlDbType.Uuid, (object)teacher.AcademicTitle?.ID ?? DBNull.Value);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with teacher id={teacher.ID} wasnt updated");

							results.Add(new UpdateTeacherRecord { Teacher = teacher, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateTeacherRecord { Teacher = teacher, IsSuccess = false, Message = ex.Message });
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
					var reader = cmd.ExecuteReader();
					while (reader.Read())
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

				return (new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
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
				string secondName = splittedShortName[0];

				var firstNameWithPatronymic = splittedShortName[1].Split('.');
				string firstNameLetter = splittedShortName[0];
				string patronymicLetter = splittedShortName.Length == 2 ? splittedShortName[1] : "";

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
					var reader = cmd.ExecuteReader();

					reader.Read();
					return (new ResultMessage() { IsSuccess = true, Message = "Success" }, reader.GetGuid(0));
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
