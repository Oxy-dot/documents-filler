using DocumentFillerWindowApp.UserModels;
using System.Windows;
using System.Windows.Controls;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для TeachersControl.xaml
	/// </summary>
	public partial class TeachersControl : UserControl
	{
		private TeachersControlViewModel _viewModel;

		public TeachersControl()
		{
			_viewModel = new TeachersControlViewModel();
			DataContext = _viewModel;
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (MainGrid.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выбрано 0 записей", "Удаление преподавателей", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			_viewModel.Delete((MainGrid.SelectedItems as List<TeacherRecord>)!);
			//var window = new AddNewAcademicTitle();
			//window.Owner = Window.GetWindow(this);
			//window.ShowDialog();		}
		}
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			_viewModel.FindChangesAndUpdate();
		}
	}
}
