using System.IO.Enumeration;

if (args.Length <= 0 || args.All(arg => arg.StartsWith('-'))) {
	var executableName = AppDomain.CurrentDomain.FriendlyName;
	Console.WriteLine($"Usage: {executableName} <Path> [-y]");
	return;
}

var root = args.First(arg => !arg.StartsWith('-'));
var forceDelete = args.Any(arg => arg == "-y");

// 指定フォルダ以下のファイル、フォルダを列挙する
FileSystemEnumerable<(string Path, bool IsDirectory)> fileSystemEnumerable = new(
	directory: root,
	transform: (ref FileSystemEntry entry) => (entry.ToSpecifiedFullPath(), entry.IsDirectory),
	options: new() {
		RecurseSubdirectories = true
	}
);
var entries = fileSystemEnumerable.ToHashSet();

foreach (var (path, isDirectory) in entries) {
	if (!isDirectory) {
		// ファイルは消さない
		entries.Remove((path, isDirectory));
		continue;
	}

	var childPrefix = $"{path}{Path.DirectorySeparatorChar}";

	// path以下にあるファイル、フォルダかどうか
	bool isChild((string Path, bool IsDirectory) entry) =>
		path != entry.Path &&
		entry.Path.StartsWith(childPrefix, StringComparison.Ordinal);

	// このフォルダ内のファイルとフォルダ
	var containsFile = entries.Where(isChild).Any(entry => !entry.IsDirectory);
	if (containsFile) {
		// ファイルが含まれるフォルダは消さない
		entries.Remove((path, isDirectory));
	} else {
		// 空フォルダ内のフォルダは除外する
		entries.RemoveWhere(isChild);
	}
}

var folders = entries.Select(entry => entry.Path).ToArray();

if (folders.Length <= 0) {
	Console.WriteLine("No empty folders found.");
	return;
}

var newLine = Environment.NewLine;
Console.WriteLine(
	$"Empty folders:{newLine}{newLine}{string.Join(newLine, folders)}{newLine}"
);

bool delete;
if (forceDelete) {
	delete = true;
} else {
	ConsoleKey key;
	do {
		Console.Write("Are you sure you want to delete folders? [y/n] ");
		key = Console.ReadKey(intercept: false).Key;
		if (key != ConsoleKey.Enter) {
			Console.WriteLine();
		}
	} while (key is not (ConsoleKey.Y or ConsoleKey.N));
	delete = key == ConsoleKey.Y;
}

if (delete) {
	foreach (var folder in folders) {
		Directory.Delete(folder, recursive: true);
		Console.WriteLine($"Deleted: {folder}");
	}
}
