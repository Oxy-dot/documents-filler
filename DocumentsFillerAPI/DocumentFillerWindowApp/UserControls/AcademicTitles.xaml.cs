using DocumentFillerWindowApp.UserModels;
using System.Windows;
using System.Windows.Controls;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для AcademicTitles.xaml
	/// </summary>
	public partial class AcademicTitles : UserControl
	{
		private AcademicTitlesControlViewModel _viewModel;

		public AcademicTitles()
		{
			InitializeComponent();

			_viewModel = new AcademicTitlesControlViewModel();
			DataContext = _viewModel;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (MainGrid.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выбрано 0 записей", "Удаление должностей", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			_viewModel.Delete(new List<AcademicTitleRecord>(MainGrid.SelectedItems.Cast<AcademicTitleRecord>()));
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
