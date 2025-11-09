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
	/// Логика взаимодействия для NavigationRail.xaml
	/// </summary>
	public partial class NavigationRail : UserControl
	{
		public List<NavigationRailItem> AllTables { get; set; }

		public NavigationRail()
		{
			InitializeComponent();
			DataContext = this;

			AllTables = new List<NavigationRailItem>()
			{
				new NavigationRailItem { Title = "Преподаватели" },
				new NavigationRailItem { Title = "Должности" },
				new NavigationRailItem { Title = "Кафедры" },
				new NavigationRailItem { Title = "Штатное расписание" },
				new NavigationRailItem { Title = "Ставки преподавателей" },
				new NavigationRailItem { Title = "Учет" },

			};
		}
	}

	public class NavigationRailItem
	{
		public string Title { get; set; }
	}
}
