using DocumentFillerWindowApp.APIProviders;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DocumentFillerWindowApp.UserModels
{
	internal class FilesControlViewModel : INotifyPropertyChanged
	{
		private FilesAPI _filesAPI;

		public ObservableCollection<FileRecord> Files { get; set; } = new ObservableCollection<FileRecord>();

		public FilesControlViewModel()
		{
			_filesAPI = new FilesAPI();
			UpdateFilesFromAPI();
		}

		private async void UpdateFilesFromAPI()
		{
			var filesResult = await _filesAPI.Get();
			if (!string.IsNullOrEmpty(filesResult.Message))
			{
				MessageBox.Show(filesResult.Message, "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Files = new ObservableCollection<FileRecord>(filesResult.Files);
			OnPropertyChanged("Files");
		}

		public async Task DownloadFile(FileRecord file)
		{
			try
			{
				var result = await _filesAPI.DownloadFile(file.FileID);
				if (!string.IsNullOrEmpty(result.Message))
				{
					MessageBox.Show(result.Message, "Ошибка скачивания файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (result.Stream == null)
				{
					MessageBox.Show("Поток файла пуст", "Ошибка скачивания файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// Извлекаем имя файла из пути
				var fileName = System.IO.Path.GetFileName(file.Path);
				if (fileName.Contains('_'))
				{
					fileName = fileName.Substring(fileName.IndexOf('_') + 1);
				}

				// Открываем диалог сохранения файла
				var saveFileDialog = new Microsoft.Win32.SaveFileDialog
				{
					FileName = fileName,
					Filter = "All files (*.*)|*.*"
				};

				if (saveFileDialog.ShowDialog() == true)
				{
					using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
					{
						await result.Stream.CopyToAsync(fileStream);
					}
					MessageBox.Show("Файл успешно скачан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
				}

				result.Stream.Dispose();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка скачивания файла", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public record FileRecord
	{
		public Guid FileID { get; set; }
		public DateTime CreationDate { get; set; }
		public string Path { get; set; } = "";
		public string FileType { get; set; } = "";
		public string FileName => System.IO.Path.GetFileName(Path).Contains('_') 
			? System.IO.Path.GetFileName(Path).Substring(System.IO.Path.GetFileName(Path).IndexOf('_') + 1)
			: System.IO.Path.GetFileName(Path);
	}
}

