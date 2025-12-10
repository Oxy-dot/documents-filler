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
				if (string.IsNullOrWhiteSpace(StartYearTextBoxText))
				{
					MessageBox.Show("Не указано начало учебного года", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (string.IsNullOrWhiteSpace(EndYearTextBoxText))
				{
					MessageBox.Show("Не указан конец учебного года", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (string.IsNullOrWhiteSpace(_protocolNumberText))
				{
					MessageBox.Show("Не указан номер протокола", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (_selectedDepartment == null)
				{
					MessageBox.Show("Не выбран отдел", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (!int.TryParse(StartYearTextBoxText, out int firstYear))
				{
					MessageBox.Show("Неверный формат начала учебного года", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (!int.TryParse(EndYearTextBoxText, out int secondYear))
				{
					MessageBox.Show("Неверный формат конца учебного года", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (!int.TryParse(_protocolNumberText, out int protocolNumber))
				{
					MessageBox.Show("Неверный формат номера протокола", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				var data = new FilesAPI.StaffingTemplateData()
				{
					DepartmentName = _selectedDepartment.FullName,
					FirstAcademicYear = firstYear,
					SecondAcademicYear = secondYear,
					ProtocolDate = _protocolDate,
					ProtocolNumber = protocolNumber,
					MainStaff = MainStaff.Select(a => new FilesAPI.StaffingTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()}. {a.Patronymic.First()}.",
						AcademicTitle = a.AcademicTitle.Name,
						Bet = 0, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
					ExternalStaff = ExternalStaff.Select(a => new FilesAPI.StaffingTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()}. {a.Patronymic.First()}.",
						AcademicTitle = a.AcademicTitle.Name,
						Bet = 0, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
					InternalStaff = InternallStaff.Select(a => new FilesAPI.StaffingTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()}. {a.Patronymic.First()}.",
						AcademicTitle = a.AcademicTitle.Name,
						Bet = 0, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
				};

				var fileName = string.IsNullOrWhiteSpace(_fileName) ? "testFile" : _fileName;
				var result = await _filesAPI.GenerateStaffingTable(data, fileName);
				if (!string.IsNullOrEmpty(result.Message))
				{
					MessageBox.Show(result.Message, "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				var excelDir = Path.Combine(Directory.GetCurrentDirectory(), "excel");
				if (!Directory.Exists(excelDir))
				{
					Directory.CreateDirectory(excelDir);
				}

				var filePath = Path.Combine(excelDir, $"{fileName}.xlsx");
				var workbook = new XSSFWorkbook(result.Result);
				using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

				workbook.Write(fileStream);
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = filePath,
					UseShellExecute = true
				});
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
