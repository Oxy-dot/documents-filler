using DocumentFillerWindowApp.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
				MessageBox.Show("Выбрано 0 записей", "Удаление должностей", MessageBoxButton.OK, MessageBoxImage.Warning);
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
