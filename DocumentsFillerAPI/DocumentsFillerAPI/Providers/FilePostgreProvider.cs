using DocumentsFillerAPI.Controllers;
using Npgsql;

namespace DocumentsFillerAPI.Providers
{
	public class FilePostgreProvider
	{
		private string connectionString = "Host=localhost;Port=5432;Database=document_filler;Username=postgres;Password=root";

		public async Task<(ResultMessage Message, List<FileForListStruct> Files)> List(uint count, uint startIndex)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

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

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					var reader = cmd.ExecuteReader();
					while (reader.Read())
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

				return (new ResultMessage() { IsSuccess = true, Message = "Success" }, files);
			}
			catch (Exception ex)
			{
				return (new ResultMessage() { IsSuccess = false, Message = ex.Message }, new List<FileForListStruct>());
			}
		}

		public async Task<(ResultMessage Message, List<MinimalFileInfoStruct> InsertedFiles, List<string> NotInsertedFiles)> InsertFiles(List<FileStruct> filesToInsert)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);
				//SET id =?, file_name =?, creation_date =?, content =?, type_id =?, is_deleted =?

				string sql =
					$@"
					INSERT INTO public.files(id, creation_date, type_id, path)
					VALUES (@id, @name, @date, @content, @typeId) RETURNING *;
					";

				List<MinimalFileInfoStruct> insertedFiles = new List<MinimalFileInfoStruct>();
				List<string> notInsertedFiles = new List<string>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (FileStruct file in filesToInsert)
					{
						try
						{
							cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
							cmd.Parameters.AddWithValue("@date", file.CreationDate);
							cmd.Parameters.AddWithValue("@typeId", file.FileType);
							cmd.Parameters.AddWithValue("@path", file.Path);

							var reader = cmd.ExecuteReader();
							insertedFiles.Add(new MinimalFileInfoStruct
							{
								FileID = reader.GetGuid(0),
								FileName = reader.GetString(1)
							});
						}
						catch (Exception ex)
						{
							notInsertedFiles.Add($"Row with name={file.Path} wasnt created, erorr: {ex.Message}");
						}
					}
				}

				return new (new ResultMessage() { Message = "Success", IsSuccess = true }, insertedFiles, notInsertedFiles);
			}
			catch (Exception ex)
			{
				return new (new ResultMessage() { Message = ex.Message, IsSuccess = false }, new(), new());
			}
		}

		public async Task<(ResultMessage Message, List<DeleteFilesStruct> DeleteResults)> DeleteFiles(List<Guid> files)
		{
			try
			{
				await using var dataSource = NpgsqlDataSource.Create(connectionString);

				string sql =
				$@"
					UPDATE public.files
					SET is_deleted = True
					WHERE id = @id;
				";

				List<DeleteFilesStruct> deleteResults = new List<DeleteFilesStruct>();

				await using (var cmd = dataSource.CreateCommand(sql))
				{
					foreach (Guid fileID in files)
					{
						try
						{
							cmd.Parameters.AddWithValue("@id", fileID);

							var reader = cmd.ExecuteReader();
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

				return new (new ResultMessage() { Message = deleteResults.Count == 0 ? "Success" : "Success with errors" }, deleteResults);
			}
			catch (Exception ex)
			{
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
