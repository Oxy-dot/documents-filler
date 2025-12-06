using DocumentFillerWindowApp.APIProviders;
using DocumentFillerWindowApp.UserModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace DocumentFillerWindowApp.ModalWindows
{
	/// <summary>
	/// Окно для парсинга PPS Excel файла
	/// </summary>
	public partial class ParsePPSFileWindow : Window
	{
		private FilesAPI _filesAPI = new FilesAPI();
		private TeachersAPI _teachersAPI = new TeachersAPI();
		private DepartmentsAPI _departmentsAPI = new DepartmentsAPI();
		private string? _selectedFilePath;
		private List<FilesAPI.PPSParsedRow> _parsedRows = new List<FilesAPI.PPSParsedRow>();

		public ParsePPSFileWindow()
		{
			InitializeComponent();
			LoadDepartments();
		}

		private async void LoadDepartments()
		{
			try
			{
				var result = await _departmentsAPI.Get();
				if (!string.IsNullOrEmpty(result.Message))
				{
					MessageBox.Show($"Ошибка при загрузке кафедр: {result.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				DepartmentComboBox.ItemsSource = result.Departments;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при загрузке кафедр: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void SelectFileButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "Excel files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All files (*.*)|*.*",
				FilterIndex = 1,
				RestoreDirectory = true
			};

			if (openFileDialog.ShowDialog() == true)
			{
				_selectedFilePath = openFileDialog.FileName;
				StatusTextBlock.Text = $"Выбран файл: {Path.GetFileName(_selectedFilePath)}";
				ParseButton.IsEnabled = true;
			}
		}

		private async void ParseButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
			{
				MessageBox.Show("Файл не выбран или не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			try
			{
				ParseButton.IsEnabled = false;
				StatusTextBlock.Text = "Парсинг файла...";

				var result = await _filesAPI.ParsePPSExcelFile(_selectedFilePath);

				if (!string.IsNullOrEmpty(result.Message))
				{
					MessageBox.Show($"Ошибки при парсинге:\n{result.Message}", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
				}

				_parsedRows = result.Rows;
				ResultsGrid.ItemsSource = _parsedRows;
				StatusTextBlock.Text = $"Получено строк: {result.Rows.Count}";
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при разборе файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				StatusTextBlock.Text = "Ошибка при разборе";
			}
			finally
			{
				ParseButton.IsEnabled = true;
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			if (_parsedRows == null || _parsedRows.Count == 0)
			{
				MessageBox.Show("Нет данных для сохранения. Сначала выполните парсинг файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (DepartmentComboBox.SelectedItem == null)
			{
				MessageBox.Show("Выберите кафедру для сохранения данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var selectedDepartment = (DepartmentRecord)DepartmentComboBox.SelectedItem;
			if (selectedDepartment.ID == Guid.Empty)
			{
				MessageBox.Show("Выбрана некорректная кафедра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			try
			{
				StatusTextBlock.Text = "Сохранение данных...";
				(sender as System.Windows.Controls.Button)!.IsEnabled = false;

				var teachersInfo = _parsedRows.Select(row => (
					FullName: row.ShortFullName,
					MainBet: row.MainBet,
					MainBetHours: row.MainBetHours,
					ExcessiveBet: row.ExcessibeBet,
					ExcessiveBetHours: row.ExcessiveBetHours.HasValue ? (int?)Math.Round(row.ExcessiveBetHours.Value, MidpointRounding.AwayFromZero) : null
				)).ToList();

				var result = await _teachersAPI.InsertTeachersFullInfo(selectedDepartment.ID, teachersInfo);

				if (result.IsSuccess)
				{
					MessageBox.Show($"Данные успешно сохранены.\n{result.Message}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
					StatusTextBlock.Text = $"Сохранено строк: {_parsedRows.Count}";
				}
				else
				{
					MessageBox.Show($"Ошибка при сохранении данных:\n{result.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					StatusTextBlock.Text = "Ошибка при сохранении";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				StatusTextBlock.Text = "Ошибка при сохранении";
			}
			finally
			{
				(sender as System.Windows.Controls.Button)!.IsEnabled = true;
			}
		}
    }
}


