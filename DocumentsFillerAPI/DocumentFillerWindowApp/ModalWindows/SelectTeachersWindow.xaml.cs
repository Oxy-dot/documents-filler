using DocumentFillerWindowApp.UserModels;
using DocumentFillerWindowApp.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DocumentFillerWindowApp.ModalWindows
{
	/// <summary>
	/// Окно выбора преподавателей на основе TeachersControl DataGrid
	/// </summary>
	public partial class SelectTeachersWindow : Window
	{
		private readonly TeachersControlViewModel _viewModel;

		public IReadOnlyList<TeacherRecord> SelectedTeachers { get; private set; } = Array.Empty<TeacherRecord>();

		public SelectTeachersWindow(List<TeacherRecord> notInclude = default)
		{
			InitializeComponent();
			_viewModel = new TeachersControlViewModel();
			_viewModel.RecreateCollection(_viewModel.Teachers.Except(notInclude ?? new List<TeacherRecord>()).ToList());
			DataContext = _viewModel;
		}

		private void SelectionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var count = SelectionGrid.SelectedItems.Count;
			SelectionStatus.Text = $"Выбрано: {count}";
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			if (SelectionGrid.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выберите хотя бы одну запись.", "Выбор преподавателей", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			SelectedTeachers = SelectionGrid.SelectedItems.Cast<TeacherRecord>().ToList();
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}

