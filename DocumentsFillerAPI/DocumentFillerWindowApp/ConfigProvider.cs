using System.IO;

namespace DocumentFillerWindowApp
{
	public static class ConfigProvider
	{
		public static string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
		public static string GetFile() => File.ReadAllTextAsync(GetPath()).Result;
		public static string GetPath() => Path.Combine(ContentRootPath, "appsettings.json");
		public static T GetValue<T>(this List<KeyValuePair<string, string>> properties, string propertyName) => (T)Convert.ChangeType(properties.Where(prop => prop.Key.EndsWith(propertyName)).FirstOrDefault().Value, typeof(T));
		public static List<IGrouping<string, KeyValuePair<string, string>>> GroupByKey(this List<KeyValuePair<string, string>> properties) => properties.GroupBy(prop => prop.Key.Replace(prop.Key.Split(':').Last(), "")).ToList();
		public static List<T> GetPropertyValues<T>(this List<KeyValuePair<string, string>> properties, string propertyName) => properties.Where(prop => prop.Key.EndsWith(propertyName)).Select(prop => (T)Convert.ChangeType(prop.Value, typeof(T))).ToList();
		public static List<IGrouping<string, KeyValuePair<string, string>>> PickByName(this List<IGrouping<string, KeyValuePair<string, string>>> groups, string name) => groups.Where(group => (group.Where(prop => prop.Key.EndsWith(name)).Any())).ToList();
		public static List<IGrouping<string, KeyValuePair<string, string>>> PickByNameValue(this List<IGrouping<string, KeyValuePair<string, string>>> groups, string name, string value) => groups.Where(group => (group.Where(prop => prop.Key.EndsWith(name)).FirstOrDefault().Value == value)).ToList();
	}
}