using DocumentFillerWindowApp.ModalWindows;
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
	/// Логика взаимодействия для SelectTeachersControl.xaml
	/// </summary>
	public partial class CreateStaffingTableControl : UserControl
	{
		private SelectTeachersWindow _selectTeacherWindow;
		private CreateStaffingTableControlViewModel _viewModel;
		public CreateStaffingTableControl()
		{
			InitializeComponent();
			_viewModel = new CreateStaffingTableControlViewModel();
			DataContext = _viewModel;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.InternallStaff.Concat(_viewModel.ExternalStaff).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.MainStaff = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.MainStaff.Concat(_viewModel.ExternalStaff).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.InternallStaff = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			_selectTeacherWindow = new SelectTeachersWindow(_viewModel.MainStaff.Concat(_viewModel.InternallStaff).ToList()) { Owner = Window.GetWindow(this) };
			if (_selectTeacherWindow.ShowDialog() == true)
			{
				_viewModel.ExternalStaff = _selectTeacherWindow.SelectedTeachers.ToList();
			}
		}
	}
}
