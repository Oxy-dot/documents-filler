using DocumentFillerWindowApp.APIProviders;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace DocumentFillerWindowApp.UserModels
{
	internal class AcademicTitlesControlViewModel : INotifyPropertyChanged
	{
		private List<AcademicTitleRecord> _lastAcademicTitles;
		private AcademicTitlesAPI _titlesAPI;

		public ObservableCollection<AcademicTitleRecord> AcademicTitles { get; set; } = new ObservableCollection<AcademicTitleRecord>();
		private Visibility _saveChangesShowButton = Visibility.Hidden;
		public Visibility SaveChangesShowButton
		{
			get => _saveChangesShowButton;
		}
		public AcademicTitlesControlViewModel()
		{
			_titlesAPI = new AcademicTitlesAPI();
			UpdateTitlesFromAPI();
		}

		public async Task FindChangesAndUpdate()
		{
			List<AcademicTitleRecord> changes = new List<AcademicTitleRecord>();

			foreach (var item in AcademicTitles)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originalItem = _lastAcademicTitles.FirstOrDefault(a => a.ID == item.ID);
				if (originalItem == null)
					continue;

				// Сравниваем все поля
				if (item.Name != originalItem.Name)
				{
					changes.Add(item);
					continue;
				}	
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty && !string.IsNullOrEmpty(a.Name)).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				await InsertTitles(toInsert.Select(a => a.Name).ToList());

			if (toUpdate.Count > 0)
				await UpdateTitles(toUpdate);

			UpdateTitlesFromAPI();
			_saveChangesShowButton = Visibility.Hidden;
		}

		public async Task InsertTitles(List<string> names)
		{
			var result = await _titlesAPI.InsertTitles(names);
			if (!string.IsNullOrEmpty(result) && result != "Успешно")
			{
				MessageBox.Show(result, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task UpdateTitles(List<AcademicTitleRecord> recordsToUpdate)
		{
			var results = await _titlesAPI.Update(recordsToUpdate);
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (results.Message != "Успешно")
				MessageBox.Show(results.Message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);
			
			if (errorResults.Count > 0)
			{
				string message = string.Join("\r\n", errorResults.Select(a => $"Название: {a.Name}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task Delete(List<AcademicTitleRecord> recordsToDelete)
		{
			var results = await _titlesAPI.Delete(recordsToDelete);
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (results.Message != "Успешно")
				MessageBox.Show(results.Message, "Ошибка при удалении", MessageBoxButton.OK, MessageBoxImage.Error);

			if (errorResults.Count > 0)
			{
				var toShow = (from errorTitleID in errorResults
							  join recordInfo in recordsToDelete on errorTitleID.TitleID equals recordInfo.ID
							  select new
							  {
								  TitleID = errorTitleID,
								  Name = recordInfo.Name,
								  Message = errorTitleID.Message
							  }).ToList();

				string message = string.Join("\r\n", toShow.Select(a => $"Название: {a.Name}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при удалении", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			UpdateTitlesFromAPI();
		}

		private void UpdateTitlesFromAPI()
		{
			AcademicTitles.CollectionChanged -= OnCollectionChanged;
			AcademicTitles = new ObservableCollection<AcademicTitleRecord>(_titlesAPI.Get().Result.Titles);
			AcademicTitles.CollectionChanged += OnCollectionChanged;
			OnPropertyChanged("AcademicTitles");
			// Клонируем записи для сохранения исходных значений
			_lastAcademicTitles = AcademicTitles.Select(t => (AcademicTitleRecord)t.Clone()).ToList();

			foreach (var academicTitle in AcademicTitles)
			{
				academicTitle.PropertyChanged += OnInternalPropertyChanged; 
			}
		}

		private void OnInternalPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_saveChangesShowButton == Visibility.Hidden)
			{
				_saveChangesShowButton = Visibility.Visible;
				OnPropertyChanged("SaveChangesShowButton");
			}
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_saveChangesShowButton == Visibility.Hidden)
			{
				_saveChangesShowButton = Visibility.Visible;
				OnPropertyChanged("SaveChangesShowButton");
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class AcademicTitleRecord : INotifyPropertyChanged, ICloneable
	{
		public Guid ID { get; set; }
		private string _name;
		public string Name 
		{ 
			get => _name; 
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public object Clone()
		{
			return new AcademicTitleRecord
			{
				ID = this.ID,
				Name = this.Name
			};
		}
	}
}
