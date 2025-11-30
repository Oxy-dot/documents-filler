using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DocumentFillerWindowApp.ModalWindows;
using DocumentFillerWindowApp.UserControls;

namespace DocumentFillerWindowApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void NavigationRailControl_ItemSelected(object? sender, NavigationRailItem e)
		{
			// Update page title and description based on selected item
			PageTitle.Text = e.Title;
			//PageDescription.Text = $"You have selected: {e.Title} (ID: {e.Id})";

			// Switch content based on selected item
			switch (e.Id)
			{
				case "Teachers":
					ContentArea.Content = new UserControls.TeachersControl();
					break;
				case "AcademicTitles":
					ContentArea.Content = new UserControls.AcademicTitles();
					break;
				case "Departments":
					ContentArea.Content = new UserControls.DepartmentControl();
					break;
				case "CreateStaffingTable":
					ContentArea.Content = new UserControls.CreateStaffingTableControl();
					break;
				case "CreateServiceMemoTable":
					ContentArea.Content = new UserControls.CreateServiceMemoTableControl();
					break;
				case "Staffing":
					ContentArea.Content = CreatePlaceholderContent("Штатное расписание", "Staffing");
					break;
				case "Rates":
					ContentArea.Content = CreatePlaceholderContent("Ставки преподавателей", "Rates");
					break;
				case "Accounting":
					ContentArea.Content = CreatePlaceholderContent("Учет", "Accounting");
					break;
				default:
					ContentArea.Content = CreatePlaceholderContent(e.Title, e.Id);
					break;
			}
		}

		private UIElement CreatePlaceholderContent(string title, string id)
		{
			var stackPanel = new StackPanel
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			var textBlock = new TextBlock
			{
				Text = $"Content for {title}",
				FontSize = 18,
				HorizontalAlignment = HorizontalAlignment.Center,
				Margin = new Thickness(0, 0, 0, 8)
			};

			var description = new TextBlock
			{
				Text = $"This is a placeholder for the {id} section.",
				FontSize = 14,
				HorizontalAlignment = HorizontalAlignment.Center,
				TextWrapping = TextWrapping.Wrap,
				TextAlignment = TextAlignment.Center
			};

			stackPanel.Children.Add(textBlock);
			stackPanel.Children.Add(description);

			return stackPanel;
		}

		private void OpenSelectTeachers_Click(object sender, RoutedEventArgs e)
		{
			var window = new SelectTeachersWindow
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				var selected = window.SelectedTeachers;
				PageTitle.Text = $"Выбрано преподавателей: {selected.Count}";

				ContentArea.Content = new TextBlock
				{
					Text = string.Join(Environment.NewLine, selected.Select(t => $"{t.SecondName} {t.FirstName} {t.Patronymic}")),
					FontSize = 16,
					TextWrapping = TextWrapping.Wrap,
					Margin = new Thickness(16)
				};
			}
		}
	}
}