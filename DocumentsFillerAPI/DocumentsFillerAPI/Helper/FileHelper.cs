using DocumentsFillerAPI.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocumentsFillerAPI.Helper
{
	public static class FileHelper
	{
		public static async Task AddNewFile(Stream fileStream, string name, Guid fileType)
		{
			try
			{
				// Создаем папку Files, если её нет
				string filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");
				if (!Directory.Exists(filesDirectory))
				{
					Directory.CreateDirectory(filesDirectory);
				}

				// Генерируем уникальное имя файла
				string fileName = $"{Guid.NewGuid()}_{name}";
				string filePath = Path.Combine(filesDirectory, fileName);

				// Сохраняем файл
				using (var fileStreamToWrite = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					fileStream.Position = 0;
					await fileStream.CopyToAsync(fileStreamToWrite);
				}

				// Создаем FileStruct для добавления в БД
				var fileStruct = new FileStruct
				{
					CreationDate = DateTime.UtcNow,
					Path = filePath,
					FileType = fileType
				};

				// Добавляем информацию о файле в БД
				var fileProvider = new FilePostgreProvider();
				await fileProvider.InsertFiles(new List<FileStruct> { fileStruct });
			}
			catch (Exception ex)
			{
				throw new Exception($"Ошибка при сохранении файла: {ex.Message}", ex);
			}
		}
	}
}
