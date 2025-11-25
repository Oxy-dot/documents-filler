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
	internal class TeachersControlViewModel : INotifyPropertyChanged
	{
		private TeachersAPI _teachersAPI;
		private List<TeacherRecord> _lastTeachers;

		public ObservableCollection<TeacherRecord> Teachers { get; set; } = new ObservableCollection<TeacherRecord>();
		private Visibility _saveChangesShowButton = Visibility.Hidden;
		public Visibility SaveChangesShowButton
		{
			get => _saveChangesShowButton;
		}

		public TeachersControlViewModel() 
		{
			_teachersAPI = new TeachersAPI();
			UpdateTeachersFromAPI();
		}

		public void FindChangesAndUpdate()
		{
			List<TeacherRecord> changes = new List<TeacherRecord>();

			foreach (var item in Teachers)
			{
				if (item.ID == Guid.Empty)
				{
					changes.Add(item);
					continue;
				}

				var originaItem = _lastTeachers.First(a => a.ID == item.ID);
				if (item.Equals(originaItem)) /*item.Name != originaItem.Name*/
				{
					changes.Add(item);
					continue;
				}
			}

			var toInsert = changes.Where(a => a.ID == Guid.Empty).ToList();
			var toUpdate = changes.Where(a => a.ID != Guid.Empty).ToList();

			if (toInsert.Count > 0)
				InsertTeachers(toInsert);

			if (toUpdate.Count > 0)
				UpdateTeachers(toUpdate);

			UpdateTeachersFromAPI();
		}

		public void InsertTeachers(List<TeacherRecord> teachers)
		{
			var results = _teachersAPI.InsertTeachers(teachers.Select(a => new MinimalTeacherRecord(a)).ToList()).Result;
			if (results.Messages.Count > 0)
			{
				string message = string.Join("\r\n", results.Messages);
				MessageBox.Show(message, "Ошибка вставки данных", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void UpdateTeachers(List<TeacherRecord> recordsToUpdate)
		{
			var results = _teachersAPI.Update(recordsToUpdate.Select(a => new MinimalTeacherRecord(a)).ToList()).Result;
			var errorResults = results.Messages.Where(a => !a.IsSuccess).ToList();

			if (!string.IsNullOrEmpty(results.Message))
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

		public void Delete(List<TeacherRecord> recordsToDelete)
		{
			var results = _teachersAPI.Delete(recordsToDelete.Select(a => new MinimalTeacherRecord(a)).ToList()).Result;
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
			Teachers = new ObservableCollection<TeacherRecord>(_teachersAPI.GetFullInfo().Result.Teachers);
			Teachers.CollectionChanged += OnCollectionChanged;
			OnPropertyChanged("Teachers");
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

	public record TeacherRecord
	{
		public Guid ID { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
		public BetRecord MainBet { get; set; }
		public BetRecord SecondBet { get; set; }
		public BetRecord ExcessiveBet { get; set; }
	}

	public record BetRecord
	{
		public Guid ID { get; set; }
		public double BetAmount { get; set; }
		public int HoursAmount { get; set; }
	}

	public record MinimalTeacherRecord
	{
		public Guid ID { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
		public Guid MainBet { get; set; }
		public Guid SecondBet { get; set; }
		public Guid ExcessiveBet { get; set; }
		public MinimalTeacherRecord(TeacherRecord teacher)
		{
			ID = teacher.ID;
			FirstName = teacher.FirstName;
			SecondName = teacher.SecondName;
			Patronymic = teacher.Patronymic;
			MainBet = teacher.MainBet.ID;
			SecondBet = teacher.SecondBet.ID;
			ExcessiveBet = teacher.ExcessiveBet.ID;
		}

		public MinimalTeacherRecord()
		{

		}
	}
}
