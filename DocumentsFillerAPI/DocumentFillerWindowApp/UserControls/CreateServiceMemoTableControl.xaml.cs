using DocumentFillerWindowApp.ModalWindows;
using DocumentFillerWindowApp.UserModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для CreateServiceMemoTableControl.xaml
	/// </summary>
	public partial class CreateServiceMemoTableControl : UserControl
	{
		private SelectTeachersWindow _selectTeacherWindow;
		private CreateServiceMemoTableControlViewModel _viewModel;
		public CreateServiceMemoTableControl()
		{
			InitializeComponent();
			_viewModel = new CreateServiceMemoTableControlViewModel();
			DataContext = _viewModel;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.InternallStaff.Concat(_viewModel.ExternalStaff).Concat(_viewModel.HourlyWorkers).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.MainStaff = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.MainStaff.Concat(_viewModel.ExternalStaff).Concat(_viewModel.HourlyWorkers).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.InternallStaff = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.MainStaff.Concat(_viewModel.InternallStaff).Concat(_viewModel.HourlyWorkers).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.ExternalStaff = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}

		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.MainStaff.Concat(_viewModel.InternallStaff).Concat(_viewModel.ExternalStaff).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.HourlyWorkers = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}

		private void StartYearNumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			// Разрешаем только цифры
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text) && (sender as TextBox).Text /*_viewModel.StartYearTextBoxText*/.Length <= 2;
		}

		private void EndYearNumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			// Разрешаем только цифры
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text) && _viewModel.EndYearTextBoxText.Length <= 2;
		}

		private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			// Разрешаем только цифры
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text) /*&& _viewModel.EndYearTextBoxText.Length <= 2*/;
		}

		private async void Button_Click_3(object sender, RoutedEventArgs e)
		{
			await _viewModel.GenerateServiceMemoTable();
		}
	}
}

