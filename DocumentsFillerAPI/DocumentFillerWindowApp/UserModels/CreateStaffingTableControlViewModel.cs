using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DocumentFillerWindowApp.UserModels
{
	internal class CreateStaffingTableControlViewModel : INotifyPropertyChanged
	{
		private List<TeacherRecord> _mainStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _internallStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _externalStaff = new List<TeacherRecord>();

		public List<TeacherRecord> MainStaff 
		{ 
			get => _mainStaff; 
			set 
			{
				_mainStaff = value;
				OnPropertyChanged();
			} 
		}

		public List<TeacherRecord> InternallStaff
		{
			get => _internallStaff;
			set
			{
				_internallStaff = value;
				OnPropertyChanged();
			}
		}

		public List<TeacherRecord> ExternalStaff
		{
			get => _externalStaff;
			set
			{
				_externalStaff = value;
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
