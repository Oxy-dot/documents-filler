using DocumentFillerWindowApp.UserModels;
using System.Windows;
using System.Windows.Controls;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для FilesControl.xaml
	/// </summary>
	public partial class FilesControl : UserControl
	{
		private FilesControlViewModel _viewModel;

		public FilesControl()
		{
			InitializeComponent();
			_viewModel = new FilesControlViewModel();
			DataContext = _viewModel;
		}

		private async void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.DataContext is FileRecord file)
			{
				await _viewModel.DownloadFile(file);
			}
		}
	}
}







