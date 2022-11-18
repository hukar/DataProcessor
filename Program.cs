using System.Collections.Concurrent;
using System.Runtime.Caching;

WriteLine("Parsing command line options");

string directoryToWatch = args[0];

MemoryCache Files = MemoryCache.Default;

ProcessExistingFiles(directoryToWatch);

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
    AddToCache(e.FullPath);
}

void FileChanged(object sender, FileSystemEventArgs e)
{
    WriteLine($"* File changed: {e.Name} - type: {e.ChangeType}");
    AddToCache(e.FullPath);
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

void AddToCache(string fullPath)
{
    var item = new CacheItem(fullPath, fullPath);

    var policy = new CacheItemPolicy {
        RemovedCallback = ProcessFile,
        SlidingExpiration = TimeSpan.FromSeconds(2)
    };

    Files.Add(item, policy);
}

void ProcessFile(CacheEntryRemovedArguments args)
{
    WriteLine($"* Cache Item removed: {args.CacheItem.Key} because {args.RemovedReason}");

    if(args.RemovedReason == CacheEntryRemovedReason.Expired)
    {
        var fileProcessor = new FileProcessor(args.CacheItem.Key);
        fileProcessor.Process();
    }
    else
    {
        WriteLine($"WARNING {args.CacheItem.Key} was removed unexpectedly");
    }
}

void ProcessExistingFiles(string inputDirectory)
{
    WriteLine($"Checking {inputDirectory} for existing files\n");
    
    foreach(var filePath in Directory.EnumerateFiles(inputDirectory))
    {
        WriteLine($"  - Found {filePath}");
        AddToCache(filePath);
    }
}




WriteLine("Press enter to quit");
ReadLine();




