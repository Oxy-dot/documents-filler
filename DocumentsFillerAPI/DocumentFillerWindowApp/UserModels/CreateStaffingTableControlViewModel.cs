using DocumentFillerWindowApp.APIProviders;
using NPOI.OpenXmlFormats.Shared;
using NPOI.XSSF.UserModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DocumentFillerWindowApp.UserModels
{
	internal class CreateStaffingTableControlViewModel : INotifyPropertyChanged
	{
		private FilesAPI _filesAPI = new FilesAPI();
		private DepartmentsAPI _departmentsAPI = new DepartmentsAPI();

		private string _startYearTextBoxText = "";
		private string _endYearTextBoxText = "";

		private List<TeacherRecord> _mainStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _internallStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _externalStaff = new List<TeacherRecord>();
		private List<DepartmentRecord> _departments = new List<DepartmentRecord>();
		private DepartmentRecord _selectedDepartment;
		private DateTime _protocolDate;
		private string _protocolNumberText;
		private string _fileName = "";

		public string FileName
		{
			get => _fileName;
			set
			{
				_fileName = value;
				OnPropertyChanged();
			}
		}

		public string ProtocolNumberText
		{
			get => _protocolNumberText;
			set
			{
				_protocolNumberText = value;
				OnPropertyChanged();
			}
		}

		public DateTime ProtocolDate
		{
			get => _protocolDate;
			set
			{
				_protocolDate = value;
				OnPropertyChanged();
			}
		}

		public DepartmentRecord SelectedDepartment
		{
			get => _selectedDepartment;
			set
			{
				_selectedDepartment = value;
				OnPropertyChanged();
			}
		}

		public List<DepartmentRecord> Departments
		{
			get => _departments;
			set
			{
				_departments = value;
				OnPropertyChanged();
			}
		}

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

		public string StartYearTextBoxText
		{
			get => _startYearTextBoxText;
			set
			{
				_startYearTextBoxText = value;
				OnPropertyChanged();
			}
		}

		public string EndYearTextBoxText
		{
			get => _endYearTextBoxText;
			set
			{
				_endYearTextBoxText = value;
				OnPropertyChanged();
			}
		}

		public CreateStaffingTableControlViewModel()
		{
			Departments = _departmentsAPI.Get().Result.Departments;
		}

		public async Task GenerateStaffingTable()
		{
			try
			{
				var data = new FilesAPI.StaffingTemplateData()
				{
					DepartmentName = _selectedDepartment.Name,
					FirstAcademicYear = int.Parse(StartYearTextBoxText),
					SecondAcademicYear = int.Parse(EndYearTextBoxText),
					ProtocolDate = _protocolDate,
					ProtocolNumber = int.Parse(_protocolNumberText),
					MainStaff = MainStaff.Select(a => new FilesAPI.StaffingTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						Bet = a.MainBet.BetAmount,
					}).ToList(),
					ExternalStaff = ExternalStaff.Select(a => new FilesAPI.StaffingTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						Bet = a.MainBet.BetAmount,
					}).ToList(),
					InternallStaff = InternallStaff.Select(a => new FilesAPI.StaffingTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						Bet = a.MainBet.BetAmount,
					}).ToList(),
				};

				var fileName = string.IsNullOrWhiteSpace(_fileName) ? "testFile" : _fileName;
				var result = await _filesAPI.GenerateStaffingTable(data, fileName);
				if (!string.IsNullOrEmpty(result.Message))
				{
					MessageBox.Show(result.Message, "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				var workbook = new XSSFWorkbook(result.Result);
				using var fileStream = new FileStream($"/excel/{fileName}", FileMode.OpenOrCreate, FileAccess.Write);

				workbook.Write(fileStream);
				System.Diagnostics.Process.Start($"./{fileName}.xlsx");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
