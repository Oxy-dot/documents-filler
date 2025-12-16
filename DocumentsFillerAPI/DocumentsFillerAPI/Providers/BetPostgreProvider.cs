using DocumentsFillerAPI.Controllers;
using DocumentsFillerAPI.Structures;
using Npgsql;
using NpgsqlTypes;
using System.Linq;

namespace DocumentsFillerAPI.Providers
{
	public class BetPostgreProvider
	{
		private string connectionString = ConfigProvider.Get<string>("ConnectionStrings:PgSQL");

		public async Task<ResultMessage> Delete(IEnumerable<Guid> bets)
		{
			try
			{
				
				//await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql = 
					$@"
					UPDATE public.bet
					SET is_deleted = True
					WHERE id IN ('{string.Join("','", bets)}')
					";

				await using (var cmd = StaticHelper.dataSource.CreateCommand(sql))
				{
					await cmd.ExecuteNonQueryAsync();
				}

				return new ResultMessage() { IsSuccess = true, Message = "Успешно" };
			}
			catch (Exception ex)
			{
				return new ResultMessage() { IsSuccess = false, Message = ex.Message };
			}
		}

		public async Task<ResultMessage> Insert(IEnumerable<BetStruct> bets)
		{
			var betsList = bets.ToList();
			if (betsList.Count == 0)
			{
				return new ResultMessage() { Message = "Успешно", IsSuccess = true };
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				List<string> errors = new List<string>();

				string sql =
					$@"
					INSERT INTO public.bet(id, bet, hours_amount, teacher_id, department_id, is_excessive)
					VALUES (@id, @bet, @hours_amount, @teacher_id, @department_id, @is_excessive);
					";

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var betParam = new NpgsqlParameter("@bet", NpgsqlTypes.NpgsqlDbType.Double);
					var hoursParam = new NpgsqlParameter("@hours_amount", NpgsqlTypes.NpgsqlDbType.Integer);
					var teacherIdParam = new NpgsqlParameter("@teacher_id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var departmentIdParam = new NpgsqlParameter("@department_id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var isExcessiveParam = new NpgsqlParameter("@is_excessive", NpgsqlTypes.NpgsqlDbType.Boolean);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(betParam);
					cmd.Parameters.Add(hoursParam);
					cmd.Parameters.Add(teacherIdParam);
					cmd.Parameters.Add(departmentIdParam);
					cmd.Parameters.Add(isExcessiveParam);

					foreach (BetStruct bet in betsList)
					{
						try
						{
							idParam.Value = Guid.NewGuid();
							betParam.Value = bet.BetAmount;
							hoursParam.Value = bet.HoursAmount;
							teacherIdParam.Value = bet.TeacherID;
							departmentIdParam.Value = bet.DepartmentID;
							isExcessiveParam.Value = bet.IsExcessive;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								//and bet = { bet.IsAdditional } and bet = { bet.IsExcessive }
								throw new Exception($"Строка со ставкой={bet.BetAmount} и часовой ставкой={bet.HoursAmount} для учителя с ID={bet.TeacherID} не была добавлена");
						}
						catch (Exception ex)
						{
							errors.Add(ex.Message);
						}
					}
				}

				await transaction.CommitAsync();

				string message = errors.Count == 0 ? "Успешно" : $"Успешно, но с ошибками\nОшибки: {string.Join(";\n", errors)}";

				return new ResultMessage() { Message = message, IsSuccess = true };
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new ResultMessage() { Message = ex.Message, IsSuccess = false };
			}
		}

		public void Search()
		{
			throw new NotImplementedException();
		}

		public async Task<(ResultMessage Message, List<UpdateBetStruct> BetsResult)> Update(IEnumerable<BetStruct> bets)
		{
			var betsList = bets.ToList();
			if (betsList.Count == 0)
			{
				return (new ResultMessage { Message = "Успешно", IsSuccess = true }, new List<UpdateBetStruct>());
			}

			await using var dataSource = NpgsqlDataSource.Create(connectionString);
			await using var connection = await dataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					UPDATE public.bet
					SET bet=@bet, hours_amount=@hours_amount, teacher_id=@teacher_id, department_id=@department_id, is_excessive=@is_excessive
					WHERE id = @id
					";

				List<UpdateBetStruct> results = new List<UpdateBetStruct>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var betParam = new NpgsqlParameter("@bet", NpgsqlTypes.NpgsqlDbType.Double);
					var hoursParam = new NpgsqlParameter("@hours_amount", NpgsqlTypes.NpgsqlDbType.Integer);
					var teacherIdParam = new NpgsqlParameter("@teacher_id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var departmentIdParam = new NpgsqlParameter("@department_id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var isExcessiveParam = new NpgsqlParameter("@is_excessive", NpgsqlTypes.NpgsqlDbType.Boolean);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(betParam);
					cmd.Parameters.Add(hoursParam);
					cmd.Parameters.Add(teacherIdParam);
					cmd.Parameters.Add(departmentIdParam);
					cmd.Parameters.Add(isExcessiveParam);

					foreach (BetStruct bet in betsList)
					{
						try
						{
							idParam.Value = bet.ID;
							betParam.Value = bet.BetAmount;
							hoursParam.Value = bet.HoursAmount;
							teacherIdParam.Value = bet.TeacherID;
							departmentIdParam.Value = bet.DepartmentID;
							isExcessiveParam.Value = bet.IsExcessive;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с ИД={bet.ID} не была обновлена");

							results.Add(new UpdateBetStruct { Bet = bet, IsSuccess = true, Message = "" });
						}
						catch (Exception ex)
						{
							results.Add(new UpdateBetStruct { Bet = bet, IsSuccess = false, Message = ex.Message });
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
					SELECT bet.id,
						   bet,
						   hours_amount,
						   teacher_id,
						   department_id,
						   is_excessive,
						   ROW_NUMBER() OVER (ORDER BY bet.id ASC, bet.is_deleted DESC) AS row_id
					FROM public.bet INNER JOIN
						   public.teacher ON public.bet.teacher_id = public.teacher.id AND
										     public.teacher.is_deleted = False INNER JOIN
						   public.department ON public.bet.department_id = public.department.id AND
											    public.department.is_deleted = False
					WHERE bet.is_deleted = False
					OFFSET {startIndex}
					LIMIT {(count == 0 ? "NULL" : count.ToString())}";

				List<BetStruct> results = new List<BetStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
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

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<BetStruct>());
			}
		}

		public async Task<(ResultMessage, BetStruct?)> Get(Guid teacherID, Guid departmentID, bool isExcessive)
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
						   is_excessive
					FROM public.bet
					WHERE teacher_id = '{teacherID}' AND
						  department_id = '{departmentID}' AND
						  is_excessive = '{isExcessive}' AND
						  is_deleted = False";

				List<BetStruct> results = new List<BetStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
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

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results.FirstOrDefault());
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, default);
			}
		}

		public async Task<(ResultMessage, Dictionary<(Guid TeacherID, Guid DepartmentID, bool IsExcessive), BetStruct>)> GetMultiple(List<(Guid TeacherID, Guid DepartmentID, bool IsExcessive)> criteria)
		{
			try
			{
				if (criteria == null || criteria.Count == 0)
				{
					return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, new Dictionary<(Guid, Guid, bool), BetStruct>());
				}

				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				var teacherIds = criteria.Select(c => c.TeacherID).Distinct().ToArray();
				var departmentIds = criteria.Select(c => c.DepartmentID).Distinct().ToArray();
				var isExcessiveValues = criteria.Select(c => c.IsExcessive).Distinct().ToArray();

				string sql = @"
					SELECT id,
						   bet,
						   hours_amount,
						   teacher_id,
						   department_id,
						   is_excessive
					FROM public.bet
					WHERE teacher_id = ANY(@teacher_ids) AND
						  department_id = ANY(@department_ids) AND
						  is_excessive = ANY(@is_excessive_values) AND
						  is_deleted = False";

				Dictionary<(Guid, Guid, bool), BetStruct> results = new Dictionary<(Guid, Guid, bool), BetStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					cmd.Parameters.Add(new NpgsqlParameter("@teacher_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid) { Value = teacherIds });
					cmd.Parameters.Add(new NpgsqlParameter("@department_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid) { Value = departmentIds });
					cmd.Parameters.Add(new NpgsqlParameter("@is_excessive_values", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Boolean) { Value = isExcessiveValues });

					var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						var bet = new BetStruct
						{
							ID = reader.GetGuid(0),
							BetAmount = reader.GetDouble(1),
							HoursAmount = reader.GetInt32(2),
							TeacherID = reader.IsDBNull(3) ? Guid.Empty : reader.GetGuid(3),
							DepartmentID = reader.IsDBNull(4) ? Guid.Empty : reader.GetGuid(4),
							IsExcessive = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
						};

						var key = (bet.TeacherID, bet.DepartmentID, bet.IsExcessive);
						if (!results.ContainsKey(key))
						{
							results[key] = bet;
						}
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, results);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new Dictionary<(Guid, Guid, bool), BetStruct>());
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
