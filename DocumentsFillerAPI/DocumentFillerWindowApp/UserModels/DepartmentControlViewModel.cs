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

		public void FindChangesAndUpdate()
		{
			List<DepartmentRecord> changes = new List<DepartmentRecord>();

			foreach (var item in Departments)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originaItem = _lastDepartments.First(a => a.ID == item.ID);
				if (item.Name != originaItem.Name || 
					item.FullName != originaItem.FullName)
				{
					changes.Add(item);
					continue;
				}
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				InsertDepartments(toInsert.Select(a => a.Name).ToList());

			if (toUpdate.Count > 0)
				UpdateDepartments(toUpdate);

			UpdateDepartmentsFromAPI();
			_saveChangesShowButton = Visibility.Hidden;
		}

		public void InsertDepartments(List<string> names)
		{
			var results = _departmentAPI.InsertDepartments(names).Result;
			if (results.Messages.Count > 0)
			{
				string message = string.Join("\r\n", results.Messages);
				MessageBox.Show(message, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void UpdateDepartments(List<DepartmentRecord> recordsToUpdate)
		{
			var results = _departmentAPI.Update(recordsToUpdate).Result;
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (!string.IsNullOrEmpty(results.Message))
				MessageBox.Show(results.Message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);

			if (errorResults.Count > 0)
			{
				string message = string.Join("\r\n", errorResults.Select(a => $"Название: {a.Name}, Ошибка: {a.Message}"));
				MessageBox.Show(message, "Ошибка при вставке", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Delete(List<DepartmentRecord> recordsToDelete)
		{
			var results = _departmentAPI.Delete(recordsToDelete).Result;
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

	public record DepartmentRecord : INotifyPropertyChanged
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
	}
}
