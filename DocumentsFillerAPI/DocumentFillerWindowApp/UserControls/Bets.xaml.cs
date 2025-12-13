using DocumentFillerWindowApp.UserModels;
using System.Windows;
using System.Windows.Controls;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для Bets.xaml
	/// </summary>
	public partial class Bets : UserControl
	{
		private BetsControlViewModel _viewModel;
		private Guid? _oldTeacherID;
		private Guid? _oldDepartmentID;
		private BetDisplayRecord? _editingBet;

		public Bets()
		{
			InitializeComponent();

			_viewModel = new BetsControlViewModel();
			DataContext = _viewModel;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (MainGrid.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выбрано 0 записей", "Удаление ставок", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			_viewModel.Delete(new List<BetDisplayRecord>(MainGrid.SelectedItems.Cast<BetDisplayRecord>()));
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{			
			_viewModel.FindChangesAndUpdate();
		}

		private void MainGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			if (e.Row.DataContext is BetDisplayRecord bet)
			{
				_oldTeacherID = bet.TeacherID;
				_oldDepartmentID = bet.DepartmentID;
				_editingBet = bet;
			}
		}

		private void MainGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
		{
			if (e.Column.Header?.ToString() == "Преподаватель" && e.EditingElement is ComboBox comboBox && _editingBet != null)
			{
				if (comboBox.ItemsSource == null)
				{
					comboBox.ItemsSource = _viewModel.Teachers;
				}
				comboBox.SelectedValue = _editingBet.TeacherID;
			}
			else if (e.Column.Header?.ToString() == "Кафедра" && e.EditingElement is ComboBox comboBoxDept && _editingBet != null)
			{
				if (comboBoxDept.ItemsSource == null)
				{
					comboBoxDept.ItemsSource = _viewModel.Departments;
				}
				comboBoxDept.SelectedValue = _editingBet.DepartmentID;
			}
		}

		private void MainGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			if (e.Row.DataContext is BetDisplayRecord bet)
			{
				if (e.Column.Header?.ToString() == "Преподаватель" && e.EditingElement is ComboBox comboBox)
				{
					if (bet.TeacherID == Guid.Empty && _oldTeacherID.HasValue && _oldTeacherID.Value != Guid.Empty)
					{
						e.Cancel = true;
						bet.TeacherID = _oldTeacherID.Value;
						comboBox.SelectedValue = _oldTeacherID.Value;
					}
					else
					{
						// Обновляем имя преподавателя
						var teacher = _viewModel.Teachers.FirstOrDefault(t => t.ID == bet.TeacherID);
						bet.TeacherFullName = teacher != null ? $"{teacher.SecondName} {teacher.FirstName} {teacher.Patronymic}" : "";
					}
				}
				else if (e.Column.Header?.ToString() == "Кафедра" && e.EditingElement is ComboBox comboBoxDept)
				{
					if (bet.DepartmentID == Guid.Empty && _oldDepartmentID.HasValue && _oldDepartmentID.Value != Guid.Empty)
					{
						e.Cancel = true;
						bet.DepartmentID = _oldDepartmentID.Value;
						comboBoxDept.SelectedValue = _oldDepartmentID.Value;
					}
					else
					{
						// Обновляем название кафедры
						var department = _viewModel.Departments.FirstOrDefault(d => d.ID == bet.DepartmentID);
						bet.DepartmentName = department != null ? department.Name : "";
					}
				}
				_oldTeacherID = null;
				_oldDepartmentID = null;
				_editingBet = null;
			}
		}
	}
}




