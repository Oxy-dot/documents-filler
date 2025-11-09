using DocumentFillerWindowApp.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DocumentFillerWindowApp.UserModels
{
	internal class TeachersControlViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<TeacherRecord> Teachers { get; } = new ObservableCollection<TeacherRecord>();

		public TeachersControlViewModel() 
		{
			FillTeachersList();
		}

		private void FillTeachersList()
		{
			int i = 0;
			while (i++ < 5000)
				Teachers.Add(new TeacherRecord { FirstName = $"Test{i}", Patronymic = "Test", SecondName = "Test" });
			OnPropertyChanged("Teachers");
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public record TeacherRecord
	{
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
	}
}
