using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;

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

		public async Task<(ResultMessage Message, List<TeacherStruct> Inserted, List<string> NotInserted)> Insert(IEnumerable<TeacherStruct> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					INSERT INTO public.teacher(id, first_name, second_name, patronymic, main_bet_id, second_bet_id, excessive_bet_id)
					VALUES (@id, @firstName, @secondName, @patronymic, @mainBetID, @secondBetID, @excessiveBetID) RETURNING *;
					";

				List<TeacherStruct> insertedTeachers = new List<TeacherStruct>();
				List<string> notInsertedTeachers = new List<string>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (TeacherStruct teacher in teachers)
					{
						try
						{
							cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
							cmd.Parameters.AddWithValue("@firstName", teacher.FirstName);
							cmd.Parameters.AddWithValue("@secondName", teacher.SecondName);
							cmd.Parameters.AddWithValue("@patronymic", teacher.Patronymic);
							cmd.Parameters.AddWithValue("@mainBetID", teacher.MainBetID); //??
							cmd.Parameters.AddWithValue("@secondBetID", teacher.SecondBetID); //??
							cmd.Parameters.AddWithValue("@excessiveBetID", teacher.ExcessiveBetID); //??

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Unkown error");

							insertedTeachers.Add(teacher);
						}
						catch (Exception ex)
						{
							notInsertedTeachers.Add($"Row with firstName={teacher.FirstName} and secondName={teacher.SecondName} and patronymic={teacher.Patronymic} wasnt inserted, erorr: {ex.Message}");
						}
					}
				}

				return new (new ResultMessage() { Message = "Success", IsSuccess = true }, insertedTeachers, notInsertedTeachers);
			}
			catch (Exception ex)
			{
				return new (new ResultMessage() { Message = ex.Message, IsSuccess = false }, new List<TeacherStruct>(), new List<string>());
			}
		}

		public void Search()
		{
			throw new NotImplementedException();
		}

		public async Task<(ResultMessage Message, List<UpdateTeacherRecord> TeachersResult)> Update(IEnumerable<TeacherStruct> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.teacher
					SET first_name=@firstName, second_name=@secondName, patronymic=@patronymic, main_bet_id=@mainBetID, second_bet_id=@secondBetID, excessive_bet_id=@excessiveBetID
					WHERE id = @id
					";

				List<UpdateTeacherRecord> results = new List<UpdateTeacherRecord>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (TeacherStruct teacher in teachers)
					{
						try
						{
							cmd.Parameters.AddWithValue("@id", teacher.ID);
							cmd.Parameters.AddWithValue("@firstName", teacher.FirstName);
							cmd.Parameters.AddWithValue("@secondName", teacher.SecondName);
							cmd.Parameters.AddWithValue("@patronymic", teacher.Patronymic);
							cmd.Parameters.AddWithValue("@mainBetID", teacher.MainBetID);
							cmd.Parameters.AddWithValue("@secondBetID", teacher.SecondBetID);
							cmd.Parameters.AddWithValue("@excessiveBetID", teacher.ExcessiveBetID);

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
						   main_bet.id,
						   main_bet.bet,
						   main_bet.hours_amount,
						   second_bet.id,
						   second_bet.bet,
						   second_bet.hours_amount,
						   excessive_bet.id,
						   excessive_bet.hours_amount,
						   ROW_NUMBER() OVER (ORDER BY public.teacher.id ASC) AS row_id
					FROM public.teacher LEFT JOIN
						   public.bet AS main_bet ON public.teacher.main_bet_id = main_bet.id AND
												     main_bet.is_deleted = False LEFT JOIN
						   public.bet AS second_bet ON public.teacher.second_bet_id = second_bet.id AND
													   second_bet.is_deleted = False LEFT JOIN
						   public.bet AS excessive_bet ON public.teacher.excessive_bet_id = excessive_bet.id AND
														  excessive_bet.is_deleted = False
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
							MainBet = new BetStruct
							{
								ID = reader.GetGuid(4),
								BetAmount = reader.GetDouble(5),
								HoursAmount = reader.GetInt32(6),
							},
							SecondBet = new BetStruct
							{
								ID = reader.GetGuid(7),
								BetAmount = reader.GetDouble(8),
								HoursAmount = reader.GetInt32(9),
							},
							ExcessiveBet = new BetStruct
							{
								ID = reader.GetGuid(10),
								HoursAmount = reader.GetInt32(11)
							},
							//MainBetID = reader.GetGuid(4),
							//SecondBetID = reader.GetGuid(5),
							//ExcessiveBetID = reader.GetGuid(6),
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

		public async Task<(ResultMessage Message, List<TeacherStruct> Teachers)> List(uint count, uint startIndex)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SELECT *, ROW_NUMBER() OVER (ORDER BY bet_id ASC, is_deleted DESC) AS row_id FROM betv2

				string sql =
					$@"
					SELECT id,
						   first_name, 
						   second_name,
						   patronymic, 
						   main_bet_id, 
						   second_bet_id, 
						   excessive_bet_id,
						   ROW_NUMBER() OVER (ORDER BY id ASC, is_deleted DESC) AS row_id
					FROM public.teacher
					WHERE is_deleted = False
					OFFSET {startIndex}
					LIMIT {count}";

				List<TeacherStruct> results = new List<TeacherStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new TeacherStruct
						{
							ID = reader.GetGuid(0),
							FirstName = reader.GetString(1),
							SecondName = reader.GetString(2),
							Patronymic = reader.GetString(3),
							MainBetID = reader.GetGuid(4),
							SecondBetID = reader.GetGuid(5),
							ExcessiveBetID = reader.GetGuid(6),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<TeacherStruct>());
			}
		}

		public record UpdateTeacherRecord
		{
			public TeacherStruct Teacher { get; init; }
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
