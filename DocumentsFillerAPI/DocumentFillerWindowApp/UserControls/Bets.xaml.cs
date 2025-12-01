using DocumentFillerWindowApp.ModalWindows;
using DocumentFillerWindowApp.UserModels;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentFillerWindowApp.UserControls
{
	/// <summary>
	/// Логика взаимодействия для Bets.xaml
	/// </summary>
	public partial class Bets : UserControl
	{
		private BetsControlViewModel _viewModel;

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

			_viewModel.Delete((MainGrid.SelectedItems as List<BetDisplayRecord>)!);
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{			
			_viewModel.FindChangesAndUpdate();
		}
	}
}



