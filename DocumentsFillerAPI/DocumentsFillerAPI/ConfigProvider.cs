using Microsoft.Extensions.Primitives;

namespace DocumentsFillerAPI
{
	public static class ConfigProvider
	{
		public static string ContentRootPath { get; set; }
		public static IConfiguration Configuration { get; set; }
		public static string GetFile() => File.ReadAllTextAsync(GetPath()).Result;
		public static string GetPath() => Path.Combine(ContentRootPath, "appsettings.json");
		public static List<KeyValuePair<string, string>> GetProperties() => Configuration.AsEnumerable().ToList();
		public static T Get<T>(string path, T defaultValue = default) => Configuration.GetValue(path, defaultValue);
		public static void Set<T>(string key, T value) => Configuration.GetSection(key).Value = Convert.ToString(value);
		public static void OnReload(Action action) => ChangeToken.OnChange(() => Configuration.GetReloadToken(), () => action());
		public static T GetSafely<T>(string path, T defaultValue = default) { try { return Get(path, defaultValue); } catch { return defaultValue; } }
		public static List<string> GetValues(string path) => Configuration.GetSection(path).AsEnumerable().Where(a => !string.IsNullOrEmpty(a.Value)).Select(a => a.Value).ToList();
		public static List<KeyValuePair<string, string>> GetProperties(string path) => Configuration.GetSection(path).AsEnumerable().Where(a => !string.IsNullOrEmpty(a.Value)).ToList();
		public static void Reload(string file, out IChangeToken token, float timeout = 1f) { File.WriteAllText(GetPath(), file); token = Configuration.GetReloadToken(); Thread.Sleep((int)(1000 * timeout)); }
		public static T GetValue<T>(this List<KeyValuePair<string, string>> properties, string propertyName) => (T)Convert.ChangeType(properties.Where(prop => prop.Key.EndsWith(propertyName)).FirstOrDefault().Value, typeof(T));
		public static List<IGrouping<string, KeyValuePair<string, string>>> GroupByKey(this List<KeyValuePair<string, string>> properties) => properties.GroupBy(prop => prop.Key.Replace(prop.Key.Split(':').Last(), "")).ToList();
		public static List<T> GetPropertyValues<T>(this List<KeyValuePair<string, string>> properties, string propertyName) => properties.Where(prop => prop.Key.EndsWith(propertyName)).Select(prop => (T)Convert.ChangeType(prop.Value, typeof(T))).ToList();
		public static List<IGrouping<string, KeyValuePair<string, string>>> PickByName(this List<IGrouping<string, KeyValuePair<string, string>>> groups, string name) => groups.Where(group => (group.Where(prop => prop.Key.EndsWith(name)).Any())).ToList();
		public static List<IGrouping<string, KeyValuePair<string, string>>> PickByNameValue(this List<IGrouping<string, KeyValuePair<string, string>>> groups, string name, string value) => groups.Where(group => (group.Where(prop => prop.Key.EndsWith(name)).FirstOrDefault().Value == value)).ToList();
	}
}
