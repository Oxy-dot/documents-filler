using DocumentsFillerAPI.Controllers;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class FilePostgreProvider
	{
		private static NpgsqlDataSource DataSource => StaticHelper.DataSource;

		public async Task<(ResultMessage Message, List<FileForListStruct> Files)> List(uint count, uint startIndex)
		{
			try
			{
				string sql =
					$@"
					SELECT file.id AS file_id,
						   file.creation_date,
						   file.path,
						   file_type.type_name,
						   ROW_NUMBER() OVER (ORDER BY file.id ASC, file.is_deleted DESC) AS row_id
					FROM public.file INNER JOIN
						   public.file_type ON public.file.type_id = public.file_type.id AND
											   public.file.is_deleted = False AND
											   public.file_type.is_deleted = False
					OFFSET {startIndex}
					{(count == 0 ? "" : $"LIMIT {count}")}";

				List<FileForListStruct> files = new List<FileForListStruct>();

				await using (var cmd = DataSource.CreateCommand(sql))
				{
					await using var reader = await cmd.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						files.Add(new FileForListStruct
						{
							FileID = reader.GetGuid(0),
							CreationDate = reader.GetDateTime(1),
							Path = reader.GetString(2),
							FileType = reader.GetString(3),
						});
					}
				}

				return (new ResultMessage() { IsSuccess = true, Message = "Успешно" }, files);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<FileForListStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<MinimalFileInfoStruct> InsertedFiles, List<string> NotInsertedFiles)> InsertFiles(List<FileStruct> filesToInsert)
		{
			if (filesToInsert == null || filesToInsert.Count == 0)
			{
				return new (new ResultMessage() { Message = "Success", IsSuccess = true }, new(), new());
			}

			await using var connection = await DataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
					$@"
					INSERT INTO public.file(id, creation_date, type_id, path)
					VALUES (@id, @date, @typeId, @path) RETURNING *;
					";

				List<MinimalFileInfoStruct> insertedFiles = new List<MinimalFileInfoStruct>();
				List<string> notInsertedFiles = new List<string>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					var dateParam = new NpgsqlParameter("@date", NpgsqlTypes.NpgsqlDbType.Date);
					var typeIdParam = new NpgsqlParameter("@typeId", NpgsqlTypes.NpgsqlDbType.Uuid);
					var pathParam = new NpgsqlParameter("@path", NpgsqlTypes.NpgsqlDbType.Text);

					cmd.Parameters.Add(idParam);
					cmd.Parameters.Add(dateParam);
					cmd.Parameters.Add(typeIdParam);
					cmd.Parameters.Add(pathParam);

					foreach (FileStruct file in filesToInsert)
					{
						try
						{
							var fileId = Guid.NewGuid();
							idParam.Value = fileId;
							dateParam.Value = file.CreationDate;
							typeIdParam.Value = file.FileType;
							pathParam.Value = file.Path;

							await using (var reader = await cmd.ExecuteReaderAsync())
							{
								if (await reader.ReadAsync())
								{
									insertedFiles.Add(new MinimalFileInfoStruct
									{
										FileID = reader.GetGuid(0),
										FileName = Path.GetFileName(file.Path)
									});
								}
							}
						}
						catch (Exception ex)
						{
							notInsertedFiles.Add($"Строка с путем={file.Path} не была создана, ошибка: {ex.Message}");
						}
					}
				}

				await transaction.CommitAsync();

				return new (new ResultMessage() { Message = "Success", IsSuccess = true }, insertedFiles, notInsertedFiles);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new (new ResultMessage() { Message = ex.Message, IsSuccess = false }, new(), new());
			}
		}

		public async Task<(ResultMessage Message, List<DeleteFilesStruct> DeleteResults)> DeleteFiles(List<Guid> files)
		{
			if (files == null || files.Count == 0)
			{
				return new (new ResultMessage() { Message = "Успешно", IsSuccess = true }, new List<DeleteFilesStruct>());
			}

			await using var connection = await DataSource.OpenConnectionAsync();
			await using var transaction = await connection.BeginTransactionAsync();

			try
			{
				string sql =
				$@"
					UPDATE public.files
					SET is_deleted = True
					WHERE id = @id;
				";

				List<DeleteFilesStruct> deleteResults = new List<DeleteFilesStruct>();

				await using (var cmd = new NpgsqlCommand(sql, connection, transaction))
				{
					var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid);
					cmd.Parameters.Add(idParam);

					foreach (Guid fileID in files)
					{
						try
						{
							idParam.Value = fileID;

							int cnt = await cmd.ExecuteNonQueryAsync();
							if (cnt != 1)
								throw new Exception($"Строка с ИД={fileID} не была обновлена");

							deleteResults.Add(new DeleteFilesStruct
							{
								FileID = fileID,
								Message = "",
								IsSuccess = true
							});
						}
						catch (Exception ex)
						{
							deleteResults.Add(new DeleteFilesStruct
							{
								FileID = fileID,
								Message = ex.Message,
								IsSuccess = false
							});
						}
					}
				}

				await transaction.CommitAsync();

				return new (new ResultMessage() { Message = deleteResults.Count(a => !a.IsSuccess) == 0 ? "Успешно" : "Успешно, но с ошибками", IsSuccess = true }, deleteResults);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new(new ResultMessage() { Message = ex.Message, IsSuccess = false }, new List<DeleteFilesStruct>());
			}
		}
	}

	public readonly struct DeleteFilesStruct
	{
		public Guid FileID { get; init; }
		public string Message { get; init; }
		public bool IsSuccess { get; init; }
	}

	public readonly struct MinimalFileInfoStruct
	{
		public Guid FileID { get; init; }
		public string FileName { get; init; }
	}

	public record FileStruct
	{
		public Guid FileID { get; init; }
		public DateTime CreationDate { get; init; }
		public string Path { get; init; }
		public Guid FileType { get; init; }
	}

	public record FileForListStruct
	{
		public Guid FileID { get; init; }
		public DateTime CreationDate { get; init; }
		public string Path { get; init; }
		public string FileType { get; init; }
	}
}
