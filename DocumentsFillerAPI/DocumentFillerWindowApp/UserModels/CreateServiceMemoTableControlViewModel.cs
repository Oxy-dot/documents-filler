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
	internal class CreateServiceMemoTableControlViewModel : INotifyPropertyChanged
	{
		private FilesAPI _filesAPI = new FilesAPI();
		private DepartmentsAPI _departmentsAPI = new DepartmentsAPI();

		private string _startYearTextBoxText = "";
		private string _endYearTextBoxText = "";

		private List<TeacherRecord> _mainStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _internallStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _externalStaff = new List<TeacherRecord>();
		private List<TeacherRecord> _hourlyWorkers = new List<TeacherRecord>();
		private List<DepartmentRecord> _departments = new List<DepartmentRecord>();
		private DepartmentRecord _selectedDepartment;
		private DateTime _studyPeriodDateStart;
		private DateTime _studyPeriodDateEnd;
		private DateTime _protocolDateTime;
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

		public DateTime ProtocolDateTime
		{
			get => _protocolDateTime;
			set
			{
				_protocolDateTime = value;
				OnPropertyChanged();
			}
		}

		public DateTime StudyPeriodDateStart
		{
			get => _studyPeriodDateStart;
			set
			{
				_studyPeriodDateStart = value;
				OnPropertyChanged();
			}
		}

		public DateTime StudyPeriodDateEnd
		{
			get => _studyPeriodDateEnd;
			set
			{
				_studyPeriodDateEnd = value;
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

		public List<TeacherRecord> HourlyWorkers
		{
			get => _hourlyWorkers;
			set
			{
				_hourlyWorkers = value;
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

		public CreateServiceMemoTableControlViewModel()
		{
			Departments = _departmentsAPI.Get().Result.Departments;
		}

		public async Task GenerateServiceMemoTable()
		{
			try
			{
				if (string.IsNullOrEmpty(_fileName))
				{
					MessageBox.Show("Надо указать название файла", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (_studyPeriodDateStart == DateTime.MinValue || _studyPeriodDateEnd == DateTime.MinValue)
				{
					MessageBox.Show("Не выбраны начало периода обучения или конец периода обучения ", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				var data = new FilesAPI.ServiceMemoTemplateData()
				{
					FirstAcademicYear = int.Parse(StartYearTextBoxText),
					SecondAcademicYear = int.Parse(EndYearTextBoxText),
					StudyPeriodDateStart = _studyPeriodDateStart,
					StudyPeriodDateEnd = _studyPeriodDateEnd,
					ProtocolNumber = int.Parse(_protocolNumberText),
					ProtocolDateTime = _protocolDateTime,
					MainStaff = MainStaff.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = a.MainBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.MainBet.HoursAmount,
							Bet = a.MainBet.BetAmount,
						} : null,
						ExcessiveBetInfo = a.ExcessiveBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.ExcessiveBet.HoursAmount,
							Bet = a.ExcessiveBet.BetAmount,
						} : null,
					}).ToList(),
					ExternalStaff = ExternalStaff.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = a.MainBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.MainBet.HoursAmount,
							Bet = a.MainBet.BetAmount,
						} : null,
						ExcessiveBetInfo = a.ExcessiveBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.ExcessiveBet.HoursAmount,
							Bet = a.ExcessiveBet.BetAmount,
						} : null,
					}).ToList(),
					InternallStaff = InternallStaff.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = a.MainBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.MainBet.HoursAmount,
							Bet = a.MainBet.BetAmount,
						} : null,
						ExcessiveBetInfo = a.ExcessiveBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.ExcessiveBet.HoursAmount,
							Bet = a.ExcessiveBet.BetAmount,
						} : null,
					}).ToList(),
					HourlyWorkers = HourlyWorkers.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = a.MainBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.MainBet.HoursAmount,
							Bet = a.MainBet.BetAmount,
						} : null,
						ExcessiveBetInfo = a.ExcessiveBet != null ? new FilesAPI.ServiceMemoTemplateBetStructData()
						{
							HoursAmount = a.ExcessiveBet.HoursAmount,
							Bet = a.ExcessiveBet.BetAmount,
						} : null,
					}).ToList(),
				};

				var fileName = string.IsNullOrWhiteSpace(_fileName) ? "testFile" : _fileName;
				var result = await _filesAPI.GenerateServiceMemo(data, fileName);
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

