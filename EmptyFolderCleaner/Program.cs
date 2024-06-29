
if (args.Length <= 0) {
	var executableName = AppDomain.CurrentDomain.FriendlyName;
	Console.WriteLine($"Usage: {executableName} <Path>");
	return;
}

var path = args[0];

const string searchPattern = "*";
const SearchOption searchOption = SearchOption.AllDirectories;

// ファイルが1つもないフォルダ (空フォルダ) を列挙する
var folders = Directory.EnumerateDirectories(path, searchPattern, searchOption)
	.Where(path => !Directory.EnumerateFiles(path, searchPattern, searchOption).Any())
	.ToHashSet();

if (folders.Count <= 0) {
	Console.WriteLine("No empty folders found.");
	return;
}

// 空フォルダ内のフォルダは無視する
foreach (var folder in folders) {
	folders.RemoveWhere(path =>
		path != folder &&
		path.StartsWith($"{folder}{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
	);
}

var newLine = Environment.NewLine;
Console.WriteLine(
	$"Empty folders:{newLine}{newLine}{string.Join(newLine, folders)}{newLine}"
);

ConsoleKey key;
do {
	Console.Write("Are you sure you want to delete folders? [y/n] ");
	key = Console.ReadKey(intercept: false).Key;
	if (key != ConsoleKey.Enter) {
		Console.WriteLine();
	}
} while (key is not (ConsoleKey.Y or ConsoleKey.N));

if (key == ConsoleKey.Y) {
	foreach (var folder in folders) {
		Directory.Delete(folder, recursive: true);
		Console.WriteLine($"Deleted: {folder}");
	}
}
