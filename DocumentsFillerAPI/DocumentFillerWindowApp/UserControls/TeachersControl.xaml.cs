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
				// Устанавливаем ItemsSource, если он еще не установлен
				if (comboBox.ItemsSource == null)
				{
					comboBox.ItemsSource = _viewModel.AcademicTitles;
				}
				// Устанавливаем начальное значение в ComboBox
				comboBox.SelectedItem = _editingTeacher.AcademicTitle;
			}
		}

		private void MainGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			if (e.Column.Header?.ToString() == "Должность" && e.Row.DataContext is TeacherRecord teacher)
			{
				if (e.EditingElement is ComboBox comboBox)
				{
					// Если значение стало null, а старое было не null, восстанавливаем старое значение
					if (teacher.AcademicTitle == null && _oldAcademicTitle != null)
					{
						// Отменяем редактирование и восстанавливаем старое значение
						e.Cancel = true;
						teacher.AcademicTitle = _oldAcademicTitle;
					}
					_oldAcademicTitle = null; // Очищаем сохраненное значение
					_editingTeacher = null;
				}
			}
		}

		private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && sender is ComboBox comboBox)
			{
				// При нажатии Escape восстанавливаем старое значение
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
				// Если после потери фокуса значение null, а старое было не null, восстанавливаем
				if (_editingTeacher.AcademicTitle == null && _oldAcademicTitle != null)
				{
					_editingTeacher.AcademicTitle = _oldAcademicTitle;
					comboBox.SelectedItem = _oldAcademicTitle;
				}
				// Также проверяем, если SelectedItem в ComboBox null, но старое значение было не null
				else if (comboBox.SelectedItem == null && _oldAcademicTitle != null)
				{
					_editingTeacher.AcademicTitle = _oldAcademicTitle;
					comboBox.SelectedItem = _oldAcademicTitle;
				}
			}
		}
	}
}
