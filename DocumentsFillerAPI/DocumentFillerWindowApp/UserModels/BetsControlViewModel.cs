using DocumentFillerWindowApp.APIProviders;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

			if (teachersResult.Message != "Успешно")
			{
				MessageBox.Show(teachersResult.Message, "Ошибка загрузки преподавателей", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				Teachers = teachersResult.Teachers;
				OnPropertyChanged("Teachers");
			}

			if (departmentsResult.Message != "Успешно")
			{
				MessageBox.Show(departmentsResult.Message, "Ошибка загрузки кафедр", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				Departments = departmentsResult.Departments;
				OnPropertyChanged("Departments");
			}
		}

		public async Task FindChangesAndUpdate()
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
				await InsertBets(toInsert);

			if (toUpdate.Count > 0)
				await UpdateBets(toUpdate);

			UpdateBetsFromAPI();
			_saveChangesShowButton = Visibility.Hidden;
		}

		public async Task InsertBets(List<BetDisplayRecord> recordsToInsert)
		{
			var betsToInsert = recordsToInsert.Select(a => new BetRecord
			{
				BetAmount = a.BetAmount,
				HoursAmount = a.HoursAmount,
				TeacherID = a.TeacherID,
				DepartmentID = a.DepartmentID,
				IsExcessive = a.IsExcessive
			}).ToList();

			var results = await _betsAPI.Insert(betsToInsert);
			if (results.Message != "Успешно")
			{
				MessageBox.Show(results.Message, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task UpdateBets(List<BetDisplayRecord> recordsToUpdate)
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

			var results = await _betsAPI.Update(betsToUpdate);
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (results.Message != "Успешно")
				MessageBox.Show(results.Message, "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);

			if (errorResults.Count > 0)
			{
				string message = string.Join("\r\n", errorResults.Select(a => $"ID: {a.BetID}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task Delete(List<BetDisplayRecord> recordsToDelete)
		{
			var betsToDelete = recordsToDelete.Select(a => new BetRecord { ID = a.ID }).ToList();
			var results = await _betsAPI.Delete(betsToDelete);

			if (results != "Успешно")
				MessageBox.Show(results, "Ошибка при удалении", MessageBoxButton.OK, MessageBoxImage.Error);

			UpdateBetsFromAPI();
		}

		private void UpdateBetsFromAPI()
		{
			var betsResult = _betsAPI.Get().Result;
			var teachersResult = _teachersAPI.GetFullInfo().Result;
			var departmentsResult = _departmentsAPI.Get().Result;

			if (betsResult.Message != "Успешно")
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
			
			foreach (var bet in Bets)
			{
				bet.PropertyChanged += OnInternalPropertyChanged;
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

	public class BetDisplayRecord : INotifyPropertyChanged, ICloneable
	{
		public Guid ID { get; set; }
		private double _betAmount;
		public double BetAmount 
		{ 
			get => _betAmount; 
			set
			{
				_betAmount = value;
				OnPropertyChanged();
			}
		}
		private int _hoursAmount;
		public int HoursAmount 
		{ 
			get => _hoursAmount; 
			set
			{
				_hoursAmount = value;
				OnPropertyChanged();
			}
		}
		private Guid _teacherID;
		public Guid TeacherID 
		{ 
			get => _teacherID; 
			set
			{
				_teacherID = value;
				OnPropertyChanged();
			}
		}
		private Guid _departmentID;
		public Guid DepartmentID 
		{ 
			get => _departmentID; 
			set
			{
				_departmentID = value;
				OnPropertyChanged();
			}
		}
		private bool _isExcessive;
		public bool IsExcessive 
		{ 
			get => _isExcessive; 
			set
			{
				_isExcessive = value;
				OnPropertyChanged();
			}
		}
		public string TeacherFullName { get; set; } = "";
		public string DepartmentName { get; set; } = "";

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public object Clone()
		{
			return new BetDisplayRecord
			{
				ID = this.ID,
				BetAmount = this.BetAmount,
				HoursAmount = this.HoursAmount,
				TeacherID = this.TeacherID,
				DepartmentID = this.DepartmentID,
				IsExcessive = this.IsExcessive,
				TeacherFullName = this.TeacherFullName,
				DepartmentName = this.DepartmentName
			};
		}
	}
}




