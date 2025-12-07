using DocumentFillerWindowApp.ModalWindows;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
	/// Material Design Navigation Rail Example
	/// </summary>
	public partial class NavigationRail : UserControl, INotifyPropertyChanged
	{
		private NavigationRailItem? _selectedItem;

		public ObservableCollection<NavigationRailItem> NavigationItems { get; set; }
		public ObservableCollection<NavigationRailItem> FileNavigatonItems { get; set; }
		public NavigationRailItem? SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
				OnSelectionChanged();
			}
		}

		public event EventHandler<NavigationRailItem>? ItemSelected;
		public event PropertyChangedEventHandler? PropertyChanged;

		public NavigationRail()
		{
			InitializeComponent();
			DataContext = this;

			// Initialize navigation items with Material Design icons
			NavigationItems = new ObservableCollection<NavigationRailItem>()
			{
				new NavigationRailItem 
				{ 
					Title = "Преподаватели", 
					IconKind = PackIconKind.AccountGroup,
					Id = "Teachers"
				},
				new NavigationRailItem 
				{ 
					Title = "Должности", 
					IconKind = PackIconKind.BadgeAccount,
					Id = "AcademicTitles"
				},
				new NavigationRailItem 
				{ 
					Title = "Кафедры", 
					IconKind = PackIconKind.OfficeBuilding,
					Id = "Departments"
				},
				new NavigationRailItem 
				{ 
					Title = "Ставки преподавателей", 
					IconKind = PackIconKind.Calculator,
					Id = "Bets"
				},
				new NavigationRailItem
				{
					Title = "Создать штатное расписание",
					IconKind = PackIconKind.ClipboardText,
					Id = "CreateStaffingTable"
				},
				new NavigationRailItem
				{
					Title = "Создать служебную записку",
					IconKind = PackIconKind.ClipboardText,
					Id = "CreateServiceMemoTable"
				},
				//new NavigationRailItem
				//{
				//	Title = "Парсить PPS файл",
				//	IconKind = PackIconKind.FileExcel,
				//	Id = "ParsePPSFile"
				//},
			};

			FileNavigatonItems = new ObservableCollection<NavigationRailItem>()
			{
				new NavigationRailItem
				{
					Title = "Файлы"
				},
				new NavigationRailItem
				{
					Title = "Добавить новый файл"
				}
			};

			// Select first item by default
			if (NavigationItems.Count > 0)
			{
				SelectedItem = NavigationItems[0];
			}
		}

		private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (NavigationListBox.SelectedItem is NavigationRailItem item)
			{
				SelectedItem = item;
			}
		}

		private void OnSelectionChanged()
		{
			if (SelectedItem != null)
			{
				ItemSelected?.Invoke(this, SelectedItem);
			}
		}

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			// Toggle text visibility for all items
			bool isTextVisible = NavigationRailAssist.GetIsTextVisible(NavigationListBox);
			NavigationRailAssist.SetIsTextVisible(NavigationListBox, !isTextVisible);
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Settings clicked!", "Navigation Rail", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var window = new ParsePPSFileWindow
			{
				Owner = Window.GetWindow(this)
			};
			window.ShowDialog();
		}
    }

	public class NavigationRailItem : INotifyPropertyChanged
	{
		private string _title = string.Empty;
		private PackIconKind _iconKind;
		private string _id = string.Empty;

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		public PackIconKind IconKind
		{
			get => _iconKind;
			set
			{
				_iconKind = value;
				OnPropertyChanged();
			}
		}

		public string Id
		{
			get => _id;
			set
			{
				_id = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
