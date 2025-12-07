using DocumentFillerWindowApp.APIProviders;
using DocumentFillerWindowApp.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DocumentFillerWindowApp.UserModels
{
	internal class DepartmentControlViewModel : INotifyPropertyChanged
	{
		private List<DepartmentRecord> _lastDepartments;
		private DepartmentsAPI _departmentAPI;

		public ObservableCollection<DepartmentRecord> Departments { get; set; } = new ObservableCollection<DepartmentRecord>();
		private Visibility _saveChangesShowButton = Visibility.Hidden;
		public Visibility SaveChangesShowButton
		{
			get => _saveChangesShowButton;
		}

		public DepartmentControlViewModel()
		{
			_departmentAPI = new DepartmentsAPI();
			UpdateDepartmentsFromAPI();
		}

		public async Task FindChangesAndUpdate()
		{
			List<DepartmentRecord> changes = new List<DepartmentRecord>();

			foreach (var item in Departments)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originalItem = _lastDepartments.FirstOrDefault(a => a.ID == item.ID);
				if (originalItem == null)
					continue;

				// Сравниваем все поля
				if (item.Name != originalItem.Name || 
					item.FullName != originalItem.FullName)
				{
					changes.Add(item);
					continue;
				}
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				await InsertDepartments(toInsert);

			if (toUpdate.Count > 0)
				await UpdateDepartments(toUpdate);

			UpdateDepartmentsFromAPI();
			_saveChangesShowButton = Visibility.Hidden;
		}

		public async Task InsertDepartments(List<DepartmentRecord> departments)
		{
			var result = await _departmentAPI.InsertDepartments(departments);
			if (!string.IsNullOrEmpty(result))
			{
				MessageBox.Show(result, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task UpdateDepartments(List<DepartmentRecord> recordsToUpdate)
		{
			var results = await _departmentAPI.Update(recordsToUpdate);
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (!string.IsNullOrEmpty(results.Message))
				MessageBox.Show(results.Message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);

			if (errorResults.Count > 0)
			{
				string message = string.Join("\r\n", errorResults.Select(a => $"Название: {a.Name}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task Delete(List<DepartmentRecord> recordsToDelete)
		{
			var results = await _departmentAPI.Delete(recordsToDelete);
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

			UpdateDepartmentsFromAPI();
		}

		private void UpdateDepartmentsFromAPI()
		{
			Departments.CollectionChanged -= OnCollectionChanged;
			Departments = new ObservableCollection<DepartmentRecord>(_departmentAPI.Get().Result.Departments);
			Departments.CollectionChanged += OnCollectionChanged;
			OnPropertyChanged("Departments");
			// Клонируем записи для сохранения исходных значений
			_lastDepartments = Departments.Select(d => (DepartmentRecord)d.Clone()).ToList();

			foreach (var department in Departments)
			{
				department.PropertyChanged += OnInternalPropertyChanged;
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

	public class DepartmentRecord : INotifyPropertyChanged, ICloneable
	{
		private Guid _ID;
		private string _name;
		private string _fullName;

		public Guid ID 
		{ 
			get => _ID; 
			set 
			{
				_ID = value;
				OnPropertyChanged();
			} 
		}

		public string Name 
		{ 
			get => _name; 
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public string FullName 
		{
			get => _fullName; 
			set
			{
				_fullName = value;
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
			return new DepartmentRecord
			{
				ID = this.ID,
				Name = this.Name,
				FullName = this.FullName
			};
		}
	}
}
