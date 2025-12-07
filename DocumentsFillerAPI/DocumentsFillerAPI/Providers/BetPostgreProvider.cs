using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class BetPostgreProvider
	{
		private string connectionString = "Host=localhost;Port=5432;Database=document_filler;Username=postgres;Password=root";

		public async Task<ResultMessage> Delete(IEnumerable<Guid> bets)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql = 
					$@"
					UPDATE public.bet
					SET is_deleted = True
					WHERE id IN ('{string.Join("','", bets)}')
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

		public async Task<ResultMessage> Insert(IEnumerable<BetStruct> bets)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.bet(id, bet, hours_amount, teacher_id, department_id, is_excessive)
					VALUES (@id, @bet, @hours_amount, @teacher_id, @department_id, @is_excessive);
					";

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (BetStruct bet in bets)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
							cmd.Parameters.AddWithValue("@bet", bet.BetAmount);
							cmd.Parameters.AddWithValue("@hours_amount", bet.HoursAmount);
							cmd.Parameters.AddWithValue("@teacher_id", bet.TeacherID);
							cmd.Parameters.AddWithValue("@department_id", bet.DepartmentID);
							cmd.Parameters.AddWithValue("@is_excessive", bet.IsExcessive);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								//and bet = { bet.IsAdditional } and bet = { bet.IsExcessive }
								throw new Exception($"Row with bet={bet.BetAmount} and bet={bet.HoursAmount} for te {bet.TeacherID} wasnt inserted");
						}
						catch (Exception ex)
						{
							errors.Add(ex.Message);
						}
					}
				}

				string message = errors.Count == 0 ? "Success" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", errors)}";

				return new ResultMessage() { Message = message, IsSuccess = true };
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

		public async Task<(ResultMessage Message, List<UpdateBetStruct> BetsResult)> Update(IEnumerable<BetStruct> bets)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
					$@"
					UPDATE public.bet
					SET bet=@bet, hours_amount=@hours_amount, teacher_id=@teacher_id, department_id=@department_id, is_excessive=@is_excessive
					WHERE id = @id
					";

				List<UpdateBetStruct> results = new List<UpdateBetStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (BetStruct bet in bets)
					{
						try
						{
							cmd.Parameters.Clear();
							cmd.Parameters.AddWithValue("@id", bet.ID);
							cmd.Parameters.AddWithValue("@bet", bet.BetAmount);
							cmd.Parameters.AddWithValue("@hours_amount", bet.HoursAmount);
							cmd.Parameters.AddWithValue("@teacher_id", bet.TeacherID);
							cmd.Parameters.AddWithValue("@department_id", bet.DepartmentID);
							cmd.Parameters.AddWithValue("@is_excessive", bet.IsExcessive);

							int cnt = cmd.ExecuteNonQuery();
							if (cnt != 1)
								throw new Exception($"Rows with bet id={bet.ID} wasnt updated");

							results.Add(new UpdateBetStruct { Bet = bet, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateBetStruct { Bet = bet, IsSuccess = false, Message = ex.Message });
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

				return (message, new List<UpdateBetStruct>());
			}
		}

		public async Task<(ResultMessage, List<BetStruct>)> List(uint count, uint startIndex)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SELECT *, ROW_NUMBER() OVER (ORDER BY bet_id ASC, is_deleted DESC) AS row_id FROM betv2

				string sql = 
					$@"
					SELECT id,
						   bet,
						   hours_amount,
						   teacher_id,
						   department_id,
						   is_excessive,
						   ROW_NUMBER() OVER (ORDER BY id ASC, is_deleted DESC) AS row_id
					FROM public.bet
					WHERE is_deleted = False
					OFFSET {startIndex}
					LIMIT {(count == 0 ? "NULL" : count.ToString())}";

				List<BetStruct> results = new List<BetStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						results.Add(new BetStruct
						{
							ID = reader.GetGuid(0),
							BetAmount = reader.GetDouble(1),
							HoursAmount = reader.GetInt32(2),
							TeacherID = reader.IsDBNull(3) ? Guid.Empty : reader.GetGuid(3),
							DepartmentID = reader.IsDBNull(4) ? Guid.Empty : reader.GetGuid(4),
							IsExcessive = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Success" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<BetStruct>());
			}
		}

		public record UpdateBetStruct
		{
			public BetStruct Bet { get; init; }
			public string Message { get; init; }
			public bool IsSuccess { get; init; }
		}
	}
}
