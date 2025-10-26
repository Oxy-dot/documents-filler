using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class TeacherProvider
	{
		private string connectionString = "";

		public async Task<ResultMessage> Delete(IEnumerable<Guid> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.teacher
					SET is_deleted = True
					WHERE id IN ('{string.Join("','", teachers)}')
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

		public async Task<ResultMessage> Insert(IEnumerable<TeacherStruct> teachers)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					INSERT INTO public.teacher(id, first_name, second_name, patronymic, main_bet_id, second_bet_id, excessive_bet_id)
					VALUES (@id, @firstName, @secondName, @patronymic, @mainBetID, @secondBetID, @excessiveBetID);
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (TeacherStruct teacher in teachers)
					{
						cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
						cmd.Parameters.AddWithValue("@firstName", teacher.FirstName);
						cmd.Parameters.AddWithValue("@secondName", teacher.SecondName);
						cmd.Parameters.AddWithValue("@patronymic", teacher.Patronymic);
						cmd.Parameters.AddWithValue("@mainBetID", teacher.MainBetID);
						cmd.Parameters.AddWithValue("@secondBetID", teacher.SecondBetID);
						cmd.Parameters.AddWithValue("@excessiveBetID", teacher.ExcessiveBetID);

						int cnt = cmd.ExecuteNonQuery();
						if (cnt != 1)
							throw new Exception($"Row with firstName={teacher.FirstName} and secondName={teacher.SecondName} and patronymic={teacher.Patronymic} wasnt inserted");
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

		public async Task<(ResultMessage Message, List<UpdateTeacherStruct> BetsResult)> Update(IEnumerable<TeacherStruct> teachers)
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

				List<UpdateTeacherStruct> results = new List<UpdateTeacherStruct>();

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

							results.Add(new UpdateTeacherStruct { Teacher = teacher, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateTeacherStruct { Teacher = teacher, IsSuccess = false, Message = ex.Message });
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

				return (message, new List<UpdateTeacherStruct>());
			}
		}

		public async Task<(ResultMessage, List<TeacherStruct>)> List(uint count, uint startIndex)
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
						   excessive_bet_id
						   ROW_NUMBER() OVER (ORDER BY id ASC, is_deleted DESC) AS row_id
					FROM public.bet
					WHERE row_id >= {startIndex} AND 
						  is_deleted = False
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

		public record UpdateTeacherStruct
		{
			public TeacherStruct Teacher { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
