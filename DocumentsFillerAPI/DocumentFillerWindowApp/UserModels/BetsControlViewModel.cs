using DocumentFillerWindowApp.APIProviders;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DocumentFillerWindowApp.UserModels
{
	internal class BetsControlViewModel : INotifyPropertyChanged
	{
		private List<BetDisplayRecord> _lastBets;
		private BetsAPI _betsAPI;
		private TeachersAPI _teachersAPI;
		private DepartmentsAPI _departmentsAPI;

		public ObservableCollection<BetDisplayRecord> Bets { get; set; } = new ObservableCollection<BetDisplayRecord>();
		public List<TeacherRecord> Teachers { get; set; } = new List<TeacherRecord>();
		public List<DepartmentRecord> Departments { get; set; } = new List<DepartmentRecord>();
		private Visibility _saveChangesShowButton = Visibility.Hidden;
		public Visibility SaveChangesShowButton
		{
			get => _saveChangesShowButton;
		}

		public BetsControlViewModel()
		{
			_betsAPI = new BetsAPI();
			_teachersAPI = new TeachersAPI();
			_departmentsAPI = new DepartmentsAPI();
			UpdateTeachersAndDepartments();
			UpdateBetsFromAPI();
		}

		private async void UpdateTeachersAndDepartments()
		{
			var teachersResult = await _teachersAPI.GetFullInfo();
			var departmentsResult = await _departmentsAPI.Get();

			if (!string.IsNullOrEmpty(teachersResult.Message))
			{
				MessageBox.Show(teachersResult.Message, "Ошибка загрузки преподавателей", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				Teachers = teachersResult.Teachers;
				OnPropertyChanged("Teachers");
			}

			if (!string.IsNullOrEmpty(departmentsResult.Message))
			{
				MessageBox.Show(departmentsResult.Message, "Ошибка загрузки кафедр", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				Departments = departmentsResult.Departments;
				OnPropertyChanged("Departments");
			}
		}

		public void FindChangesAndUpdate()
		{
			List<BetDisplayRecord> changes = new List<BetDisplayRecord>();

			foreach (var item in Bets)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originalItem = _lastBets.FirstOrDefault(a => a.ID == item.ID);
				if (originalItem == null)
					continue;

				if (item.BetAmount != originalItem.BetAmount ||
					item.HoursAmount != originalItem.HoursAmount ||
					item.TeacherID != originalItem.TeacherID ||
					item.DepartmentID != originalItem.DepartmentID ||
					item.IsExcessive != originalItem.IsExcessive)
				{
					changes.Add(item);
					continue;
				}
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				InsertBets(toInsert);

			if (toUpdate.Count > 0)
				UpdateBets(toUpdate);

			UpdateBetsFromAPI();
		}

		public void InsertBets(List<BetDisplayRecord> recordsToInsert)
		{
			var betsToInsert = recordsToInsert.Select(a => new BetRecord
			{
				BetAmount = a.BetAmount,
				HoursAmount = a.HoursAmount,
				TeacherID = a.TeacherID,
				DepartmentID = a.DepartmentID,
				IsExcessive = a.IsExcessive
			}).ToList();

			var results = _betsAPI.Insert(betsToInsert).Result;
			if (results.Message != "Успешно")
			{
				MessageBox.Show(results.Message, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void UpdateBets(List<BetDisplayRecord> recordsToUpdate)
		{
			var betsToUpdate = recordsToUpdate.Select(a => new BetRecord
			{
				ID = a.ID,
				BetAmount = a.BetAmount,
				HoursAmount = a.HoursAmount,
				TeacherID = a.TeacherID,
				DepartmentID = a.DepartmentID,
				IsExcessive = a.IsExcessive
			}).ToList();

			var results = _betsAPI.Update(betsToUpdate).Result;
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (results.Message != "Успешно")
				MessageBox.Show(results.Message, "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);

			if (errorResults.Count > 0)
			{
				string message = string.Join("\r\n", errorResults.Select(a => $"ID: {a.BetID}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Delete(List<BetDisplayRecord> recordsToDelete)
		{
			var betsToDelete = recordsToDelete.Select(a => new BetRecord { ID = a.ID }).ToList();
			var results = _betsAPI.Delete(betsToDelete).Result;

			if (!string.IsNullOrEmpty(results))
				MessageBox.Show(results, "Ошибка при удалении", MessageBoxButton.OK, MessageBoxImage.Error);

			UpdateBetsFromAPI();
		}

		private async void UpdateBetsFromAPI()
		{
			var betsResult = await _betsAPI.Get();
			var teachersResult = await _teachersAPI.GetFullInfo();
			var departmentsResult = await _departmentsAPI.Get();

			if (!string.IsNullOrEmpty(betsResult.Message))
			{
				MessageBox.Show(betsResult.Message, "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var teachers = teachersResult.Teachers;
			var departments = departmentsResult.Departments;

			// Обновляем списки для ComboBox
			Teachers = teachers;
			Departments = departments;
			OnPropertyChanged("Teachers");
			OnPropertyChanged("Departments");

			var betsWithDisplayInfo = betsResult.Bets.Select(bet =>
			{
				var teacher = teachers.FirstOrDefault(t => t.ID == bet.TeacherID);
				var department = departments.FirstOrDefault(d => d.ID == bet.DepartmentID);

				return new BetDisplayRecord
				{
					ID = bet.ID,
					BetAmount = bet.BetAmount,
					HoursAmount = bet.HoursAmount,
					TeacherID = bet.TeacherID,
					DepartmentID = bet.DepartmentID,
					IsExcessive = bet.IsExcessive,
					TeacherFullName = teacher != null ? $"{teacher.SecondName} {teacher.FirstName} {teacher.Patronymic}" : "",
					DepartmentName = department != null ? department.Name : ""
				};
			}).ToList();

			Bets.CollectionChanged -= OnCollectionChanged;
			Bets = new ObservableCollection<BetDisplayRecord>(betsWithDisplayInfo);
			_lastBets = betsWithDisplayInfo.Select(a => new BetDisplayRecord
			{
				ID = a.ID,
				BetAmount = a.BetAmount,
				HoursAmount = a.HoursAmount,
				TeacherID = a.TeacherID,
				DepartmentID = a.DepartmentID,
				IsExcessive = a.IsExcessive,
				TeacherFullName = a.TeacherFullName,
				DepartmentName = a.DepartmentName
			}).ToList();
			Bets.CollectionChanged += OnCollectionChanged;
			OnPropertyChanged("Bets");
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

	public record BetDisplayRecord
	{
		public Guid ID { get; set; }
		public double BetAmount { get; set; }
		public int HoursAmount { get; set; }
		public Guid TeacherID { get; set; }
		public Guid DepartmentID { get; set; }
		public bool IsExcessive { get; set; }
		public string TeacherFullName { get; set; } = "";
		public string DepartmentName { get; set; } = "";
	}
}




