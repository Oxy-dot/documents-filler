using DocumentFillerWindowApp.APIProviders;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DocumentFillerWindowApp.UserModels
{
	internal class TeachersControlViewModel : INotifyPropertyChanged
	{
		private TeachersAPI _teachersAPI;
		private AcademicTitlesAPI _titlesAPI;
		private List<TeacherRecord> _lastTeachers;

		public ObservableCollection<TeacherRecord> Teachers { get; set; } = new ObservableCollection<TeacherRecord>();
		private Visibility _saveChangesShowButton = Visibility.Hidden;
		public Visibility SaveChangesShowButton
		{
			get => _saveChangesShowButton;
		}

		public List<AcademicTitleRecord> AcademicTitles { get; set; } = new List<AcademicTitleRecord>();
		private bool _onlyWithAcademicTitle;
		public TeachersControlViewModel(bool onlyWithAcademicTitle) 
		{
			_teachersAPI = new TeachersAPI();
			_titlesAPI = new AcademicTitlesAPI();

			_onlyWithAcademicTitle = onlyWithAcademicTitle;

			UpdateAcademicTitlesFromAPI();
			UpdateTeachersFromAPI();
		}

		public async Task FindChangesAndUpdate()
		{
			List<TeacherRecord> changes = new List<TeacherRecord>();

			foreach (var item in Teachers)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originalItem = _lastTeachers.FirstOrDefault(a => a.ID == item.ID);
				if (originalItem == null)
					continue;

				// Сравниваем все поля
				if (item.FirstName != originalItem.FirstName ||
					item.SecondName != originalItem.SecondName ||
					item.Patronymic != originalItem.Patronymic ||
					(item.AcademicTitle?.ID ?? Guid.Empty) != (originalItem.AcademicTitle?.ID ?? Guid.Empty))
				{
					changes.Add(item);
					continue;
				}
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty && (!string.IsNullOrEmpty(a.FirstName) || !string.IsNullOrEmpty(a.SecondName) || !string.IsNullOrEmpty(a.Patronymic))).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				await InsertTeachers(toInsert);

			if (toUpdate.Count > 0)
				await UpdateTeachers(toUpdate);

			UpdateTeachersFromAPI();
			_saveChangesShowButton = Visibility.Hidden;
		}

		public async Task InsertTeachers(List<TeacherRecord> teachers)
		{
			var result = await _teachersAPI.InsertTeachers(teachers);
			if (result != "Успешно")
			{
				MessageBox.Show(result, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task UpdateTeachers(List<TeacherRecord> recordsToUpdate)
		{
			var results = await _teachersAPI.Update(recordsToUpdate);
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (results.Message != "Успешно")
				MessageBox.Show(results.Message, "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);

			if (errorResults.Count > 0)
			{
				List<string> messages = new List<string>();
				errorResults.ForEach(a =>
				{
					var notUpdatedRecord = recordsToUpdate.First(b => b.ID == a.ID);
					string message = $"{notUpdatedRecord.SecondName} {notUpdatedRecord.FirstName.First()} {notUpdatedRecord.Patronymic.First()}, Ошибка: {a.Message}";
					messages.Add(message);
				});

				string message = string.Join("\r\n", messages);
				MessageBox.Show(message, "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task Delete(List<TeacherRecord> recordsToDelete)
		{
			var results = await _teachersAPI.Delete(recordsToDelete);
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
								  FIO = $"{recordInfo.SecondName} {recordInfo.FirstName.First()} {recordInfo.Patronymic.First()}",
								  Message = errorTitleID.Message
							  }).ToList();

				string message = string.Join("\r\n", toShow.Select(a => $"ФИО: {a.FIO}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при удалении", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			UpdateTeachersFromAPI();
		}

		private void UpdateTeachersFromAPI()
		{
			Teachers.CollectionChanged -= OnCollectionChanged;
			var teachersFromAPI = _teachersAPI.GetFullInfo().Result.Teachers;
			if (_onlyWithAcademicTitle)
			{
				teachersFromAPI = teachersFromAPI.Where(a => a.AcademicTitle != null).ToList();
			}
			foreach (var teacher in teachersFromAPI)
			{
				if (teacher.AcademicTitle != null)
				{
					var matchingTitle = AcademicTitles.FirstOrDefault(t => t.ID == teacher.AcademicTitle.ID);
					if (matchingTitle != null)
					{
						teacher.AcademicTitle = matchingTitle;
					}
				}
			}
			
			Teachers = new ObservableCollection<TeacherRecord>(teachersFromAPI);
			Teachers.CollectionChanged += OnCollectionChanged;
			OnPropertyChanged("Teachers");
			_lastTeachers = Teachers.Select(t => (TeacherRecord)t.Clone()).ToList();
			foreach (var teacher in Teachers)
			{
				teacher.PropertyChanged += OnInternalPropertyChanged;
			}
		}

		private void UpdateAcademicTitlesFromAPI()
		{
			var getTitlesResult = _titlesAPI.Get().Result;
			if (getTitlesResult.Message == "Успешно")
				AcademicTitles = getTitlesResult.Titles;
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

		public void RecreateCollection(List<TeacherRecord> teachers)
		{
			Teachers.CollectionChanged -= OnCollectionChanged;
			Teachers = new ObservableCollection<TeacherRecord>(teachers);
			Teachers.CollectionChanged += OnCollectionChanged;
			OnPropertyChanged("Teachers");
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class TeacherRecord : INotifyPropertyChanged, ICloneable
	{
		public Guid ID { get; set; }
		private string _firstName;
		public string FirstName 
		{
			get => _firstName;
			set
			{
				_firstName = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FullName));
			}
		}
		private string _secondName;
		public string SecondName 
		{
			get => _secondName;
			set
			{
				_secondName = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FullName));
			}
		}
		private string _patronymic;
		public string Patronymic 
		{
			get => _patronymic;
			set
			{
				_patronymic = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FullName));
			}
		}
		public string FullName => $"{SecondName} {FirstName} {Patronymic}".Trim();
		private AcademicTitleRecord? _academicTitle;
		public AcademicTitleRecord? AcademicTitle 
		{
			get => _academicTitle;
			set
			{
				_academicTitle = value;
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
			return new TeacherRecord
			{
				ID = this.ID,
				FirstName = this.FirstName,
				SecondName = this.SecondName,
				Patronymic = this.Patronymic,
				AcademicTitle = this.AcademicTitle
			};
		}

		public override bool Equals(object? obj)
		{
			try
			{
				if (obj == null)
					return false;

				return ((TeacherRecord)obj)!.ID == ID;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}
	}

	public record BetRecord
	{
		public Guid ID { get; set; }
		public double BetAmount { get; set; }
		public int HoursAmount { get; set; }
		public Guid TeacherID { get; set; }
		public Guid DepartmentID { get; set; }
		public bool IsExcessive { get; set; }
	}

}
