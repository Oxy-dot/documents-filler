using DocumentFillerWindowApp.UserModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для TeachersControl.xaml
	/// </summary>
	public partial class TeachersControl : UserControl
	{
		private TeachersControlViewModel _viewModel;
		private AcademicTitleRecord? _oldAcademicTitle;
		private TeacherRecord? _editingTeacher;

		public TeachersControl()
		{
			_viewModel = new TeachersControlViewModel(false);
			DataContext = _viewModel;
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			if (MainGrid.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выбрано 0 записей", "Удаление преподавателей", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			await _viewModel.Delete(new List<TeacherRecord>(MainGrid.SelectedItems.Cast<TeacherRecord>()));
		}
		private async void Button_Click_1(object sender, RoutedEventArgs e)
		{
			await _viewModel.FindChangesAndUpdate();
		}

		private void MainGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			if (e.Column.Header?.ToString() == "Должность" && e.Row.DataContext is TeacherRecord teacher)
			{
				_oldAcademicTitle = teacher.AcademicTitle;
				_editingTeacher = teacher;
			}
		}

		private void MainGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
		{
			if (e.Column.Header?.ToString() == "Должность" && e.EditingElement is ComboBox comboBox && _editingTeacher != null)
			{
				if (comboBox.ItemsSource == null)
				{
					comboBox.ItemsSource = _viewModel.AcademicTitles;
				}
				comboBox.SelectedItem = _editingTeacher.AcademicTitle;
			}
		}

		private void MainGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			if (e.Column.Header?.ToString() == "Должность" && e.Row.DataContext is TeacherRecord teacher)
			{
				if (e.EditingElement is ComboBox comboBox)
				{
					if (teacher.AcademicTitle == null && _oldAcademicTitle != null)
					{
						e.Cancel = true;
						teacher.AcademicTitle = _oldAcademicTitle;
					}
					_oldAcademicTitle = null;
					_editingTeacher = null;
				}
			}
		}

		private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && sender is ComboBox comboBox)
			{
				if (_editingTeacher != null && _oldAcademicTitle != null)
				{
					_editingTeacher.AcademicTitle = _oldAcademicTitle;
					comboBox.SelectedItem = _oldAcademicTitle;
				}
			}
		}

		private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (sender is ComboBox comboBox && _editingTeacher != null)
			{
				if (_editingTeacher.AcademicTitle == null && _oldAcademicTitle != null)
				{
					_editingTeacher.AcademicTitle = _oldAcademicTitle;
					comboBox.SelectedItem = _oldAcademicTitle;
				}
				else if (comboBox.SelectedItem == null && _oldAcademicTitle != null)
				{
					_editingTeacher.AcademicTitle = _oldAcademicTitle;
					comboBox.SelectedItem = _oldAcademicTitle;
				}
			}
		}
	}
}
