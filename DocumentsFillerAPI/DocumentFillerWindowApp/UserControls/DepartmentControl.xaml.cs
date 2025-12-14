using DocumentFillerWindowApp.UserModels;
using System.Windows;
using System.Windows.Controls;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для DepartmentControl.xaml
	/// </summary>
	public partial class DepartmentControl : UserControl
	{
		private DepartmentControlViewModel _viewModel;

		public DepartmentControl()
		{
			InitializeComponent();

			_viewModel = new DepartmentControlViewModel();
			DataContext = _viewModel;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (MainGrid.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выбрано 0 записей", "Удаление кафедры", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			_viewModel.Delete(new List<DepartmentRecord>(MainGrid.SelectedItems.Cast<DepartmentRecord>()));
			//var window = new AddNewAcademicTitle();
			//window.Owner = Window.GetWindow(this);
			//window.ShowDialog();
		}

		private async void Button_Click_1(object sender, RoutedEventArgs e)
		{
			await _viewModel.FindChangesAndUpdate();
		}
	}
}
