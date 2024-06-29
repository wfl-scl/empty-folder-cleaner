using FluentAssertions;
using System.Runtime.CompilerServices;

namespace EmptyFolderCleaner.Tests;

public class CleaningTest {

	private static readonly Action<string[]> mainDelegate;


	static CleaningTest() {
		var entryPoint = typeof(Program).Assembly.EntryPoint!;
		mainDelegate = entryPoint.CreateDelegate<Action<string[]>>();
	}

	[Fact]
	public void FolderOnly() {
		run([
			(Path.Combine("TestFolder1", "TestFolder2", "TestFolder3"), ShouldBeDeleted: true),
			(Path.Combine("TestFolder111"), ShouldBeDeleted: true)
		]);
	}

	[Fact]
	public void FileAndFolder() {
		run([
			(Path.Combine("TestFolder1", "TestFolder2", "TestFolder3"), ShouldBeDeleted: true),
			(Path.Combine("TestFolder111"), ShouldBeDeleted: true),
			(Path.Combine("TestFolderA", "TestFolderB", "TestFolderC"), ShouldBeDeleted: true),
			(Path.Combine("TestFolderA", "TestFolderB", "Test.txt"), ShouldBeDeleted: false)
		]);
	}

	private static void run(
		IEnumerable<(string Path, bool ShouldBeDeleted)> entries,
		[CallerMemberName] string memberName = ""
	) {
		var root = memberName;
		if (Directory.Exists(root)) {
			Directory.Delete(root, recursive: true);
		}
		foreach (var (path, _) in entries) {
			createFileOrDirectory(Path.Combine(root, path));
		}

		mainDelegate([root, "-y"]);

		Directory.EnumerateFileSystemEntries(root, searchPattern: "*", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(root, path))
			.Intersect(entries.Select(entry => entry.Path))
			.Should()
			.BeEquivalentTo(entries.Where(entry => !entry.ShouldBeDeleted).Select(entry => entry.Path));

		Directory.Delete(root, recursive: true);
	}

	private static void createFileOrDirectory(string path) {
		if (!string.IsNullOrEmpty(Path.GetExtension(path))) {
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, contents: string.Empty);
		} else {
			Directory.CreateDirectory(path);
		}
	}

}
