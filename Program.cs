using System.Collections.Concurrent;
using System.Runtime.Caching;

WriteLine("Parsing command line options");

string directoryToWatch = args[0];

ConcurrentDictionary<string, string> Files = new();

using Timer timer = new(ProcessSingleFile,null, 0, 1000);

if (Directory.Exists(directoryToWatch) == false)
{
    WriteLine($"ERROR: {directoryToWatch} doesn't exist");
    WriteLine("press enter to quit");
    ReadLine();
    return;
}

WriteLine($"Watching directory: {directoryToWatch}");

using FileSystemWatcher watcher = new(directoryToWatch);

watcher.IncludeSubdirectories = false;
watcher.InternalBufferSize = 32_768; // 32KB
watcher.Filter = "*.*"; // this is the default value
watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

watcher.Created += FileCreated;
watcher.Changed += FileChanged;
watcher.Deleted += FileDeleted;
watcher.Renamed += FileRenamed;
watcher.Error += WatcherError;

watcher.EnableRaisingEvents = true;

WriteLine("Press enter to quit");
ReadLine();

void FileCreated(object sender, FileSystemEventArgs e)
{
    WriteLine($"* File created: {e.Name} - type: {e.ChangeType}");
    Files.TryAdd(e.FullPath, e.FullPath);
    // ProcessSingleFile();
}

void FileChanged(object sender, FileSystemEventArgs e)
{
    WriteLine($"* File changed: {e.Name} - type: {e.ChangeType}");
    Files.TryAdd(e.FullPath, e.FullPath);
    // ProcessSingleFile();
}

void FileDeleted(object sender, FileSystemEventArgs e)
{
    WriteLine($"* File deleted: {e.Name} - type: {e.ChangeType}");
}

void FileRenamed(object sender, RenamedEventArgs e)
{
    WriteLine($"* File created: {e.OldName} to {e.Name} - type: {e.ChangeType}");
}

void WatcherError(object sender, ErrorEventArgs e)
{
    WriteLine($"ERROR: file system watching may no longer be active: {e.GetException()}");
}

void ProcessSingleFile(object? state)
{
    foreach (var file in Files.Values)
    {
        if (Files.TryRemove(file, out _))
        {
            FileProcessor fileProcessor = new(file);
            fileProcessor.Process();
        }
    }
}




WriteLine("Press enter to quit");
ReadLine();




