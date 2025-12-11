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

				if (_studyPeriodDateStart == DateTime.MinValue || _studyPeriodDateEnd == DateTime.MinValue)
				{
					MessageBox.Show("Не выбраны начало периода обучения или конец периода обучения ", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
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

				var data = new FilesAPI.ServiceMemoTemplateData()
				{
					FirstAcademicYear = firstYear,
					SecondAcademicYear = secondYear,
					StudyPeriodDateStart = _studyPeriodDateStart,
					StudyPeriodDateEnd = _studyPeriodDateEnd,
					ProtocolNumber = protocolNumber,
					ProtocolDateTime = _protocolDateTime,
					MainStaff = MainStaff.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()}. {a.Patronymic.First()}.",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
						ExcessiveBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
					ExternalStaff = ExternalStaff.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
						ExcessiveBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
					InternallStaff = InternallStaff.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
						ExcessiveBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
					HourlyWorkers = HourlyWorkers.Select(a => new FilesAPI.ServiceMemoTemplateRowData
					{
						FullName = $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}",
						AcademicTitle = a.AcademicTitle.Name,
						MainBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
						ExcessiveBetInfo = null, // Bet теперь получается отдельно через BetsAPI по TeacherID
					}).ToList(),
				};

				var fileName = string.IsNullOrWhiteSpace(_fileName) ? "testFile" : _fileName;
				var result = await _filesAPI.GenerateServiceMemo(data, fileName);
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

				// Убеждаемся, что расширение .xlsx есть в имени файла
				if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
				{
					fileName = fileName + ".xlsx";
				}

				var filePath = Path.Combine(excelDir, fileName);
				
				// Копируем Stream в файл
				if (result.Result != null)
				{
					// Устанавливаем позицию Stream в начало, если это возможно
					if (result.Result.CanSeek)
					{
						result.Result.Position = 0;
					}
					
					using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
					{
						result.Result.CopyTo(fileStream);
					}
					
					// Закрываем Stream после использования
					result.Result.Dispose();
					
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
					{
						FileName = filePath,
						UseShellExecute = true
					});
				}
				else
				{
					MessageBox.Show("Результат пуст", "Ошибка при генерации файла", MessageBoxButton.OK, MessageBoxImage.Error);
				}
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

