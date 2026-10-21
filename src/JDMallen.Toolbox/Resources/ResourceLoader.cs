using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace JDMallen.Toolbox.Resources;

/// <summary>
/// Loads embedded resources from the assembly.
/// </summary>
[UsedImplicitly]
public class ResourceLoader
{
	static ResourceLoader()
	{
		LoadResources();
	}

	/// <summary>
	/// Gets the collection of common passwords loaded from the embedded resource.
	/// </summary>
	public static IEnumerable<string> CommonPasswords { get; set; } = [];

	/// <summary>
	/// Gets the character frequency table loaded from the embedded resource.
	/// </summary>
	public static IEnumerable<string> FrequencyTable { get; set; } = [];

	private static void LoadResources()

	{
		typeof(ResourceLoader)
			.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
			.ToList()
			.ForEach(prop =>
			{
				prop.SetValue(typeof(ResourceLoader), ReadResourceFile(prop.Name + ".txt"));
			});
	}

	// Reads the whole file once into an array. The loop must end when ReadLine
	// returns null (EOF); substituting a non-null fallback there makes it run
	// forever.
	private static string[] ReadResourceFile(string fileName)
	{
		using Stream? rs = Assembly.GetExecutingAssembly()
			.GetManifestResourceStream(typeof(ResourceLoader), fileName);
		using var sr = new StreamReader(rs!, Encoding.UTF8);
		var lines = new List<string>();
		while (sr.ReadLine() is { } line)
		{
			lines.Add(line);
		}

		return [.. lines];
	}
}
