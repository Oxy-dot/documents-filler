using DocumentFillerWindowApp.APIProviders;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

		public void FindChangesAndUpdate()
		{
			List<AcademicTitleRecord> changes = new List<AcademicTitleRecord>();

			foreach (var item in AcademicTitles)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originaItem = _lastAcademicTitles.First(a => a.ID == item.ID);
				if (item.Name != originaItem.Name)
				{
					changes.Add(item);
					continue;
				}	
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				InsertTitles(toInsert.Select(a => a.Name).ToList());

			if (toUpdate.Count > 0)
				UpdateTitles(toUpdate);

			UpdateTitlesFromAPI();
			_saveChangesShowButton = Visibility.Hidden;
		}

		public void InsertTitles(List<string> names)
		{
			var results = _titlesAPI.InsertTitles(names).Result;
			if (results.Messages.Count > 0)
			{
				string message = string.Join("\r\n", results.Messages);
				MessageBox.Show(message, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void UpdateTitles(List<AcademicTitleRecord> recordsToUpdate)
		{
			var results = _titlesAPI.Update(recordsToUpdate).Result;
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (!string.IsNullOrEmpty(results.Message))
				MessageBox.Show(results.Message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);
			
			if (errorResults.Count > 0)
			{
				string message = string.Join("\r\n", errorResults.Select(a => $"Название: {a.Name}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Delete(List<AcademicTitleRecord> recordsToDelete)
		{
			var results = _titlesAPI.Delete(recordsToDelete).Result;
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (!string.IsNullOrEmpty(results.Message))
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

	public record AcademicTitleRecord : INotifyPropertyChanged
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
	}
}
