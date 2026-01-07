using DocumentFillerWindowApp.ModalWindows;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

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

			// Select first item by default
			if (NavigationItems.Count > 0)
			{
				SelectedItem = NavigationItems[0];
			}
		}

		private void OnSelectionChanged()
		{
			if (SelectedItem != null)
			{
				ItemSelected?.Invoke(this, SelectedItem);
			}
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

		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue is not StackPanel)
				return;
			switch ((e.NewValue as StackPanel)!.Name)
			{
				case "Teachers":
					SelectedItem = new NavigationRailItem
					{
						Title = "Преподаватели",
						IconKind = PackIconKind.AccountGroup,
						Id = "Teachers"
					};
					break;
				case "AcademicTitles":
					SelectedItem = new NavigationRailItem
					{
						Title = "Должности",
						IconKind = PackIconKind.BadgeAccount,
						Id = "AcademicTitles"
					};
					break;
				case "Departments":
					SelectedItem = new NavigationRailItem
					{
						Title = "Кафедры",
						IconKind = PackIconKind.OfficeBuilding,
						Id = "Departments"
					};
					break;
				case "Bets":
					SelectedItem = new NavigationRailItem
					{
						Title = "Ставки преподавателей",
						IconKind = PackIconKind.Calculator,
						Id = "Bets"
					};
					break;
				case "Files":
					SelectedItem = new NavigationRailItem
					{
						Title = "Файлы",
						IconKind = PackIconKind.Folder,
						Id = "Files"
					};
					break;
				case "CreateStaffingTable":
					SelectedItem = new NavigationRailItem
					{
						Title = "Создать штатное расписание",
						IconKind = PackIconKind.ClipboardText,
						Id = "CreateStaffingTable"
					};
					break;
				case "CreateServiceMemoTable":
					SelectedItem = new NavigationRailItem
					{
						Title = "Создать служебную записку",
						IconKind = PackIconKind.ClipboardText,
						Id = "CreateServiceMemoTable"
					};
					break;
			}
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
