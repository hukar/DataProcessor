namespace DataProcessor;

public class FileProcessor
{
    private const string BackupDirectoryName = "backup";
    private const string InProgressDirectoryName = "processing";
    private const string CompletedDirectoryName = "complete";

    public string InputPathFile { get; }

    public FileProcessor(string inputPathFile)
    {
        InputPathFile = inputPathFile;
    }

    public void Process()
    {
        WriteLine($"Begin process of {Path.GetFileNameWithoutExtension(InputPathFile)}\n");

        if (Path.IsPathFullyQualified(InputPathFile) == false)
        {
            WriteLine($"HUK-ERROR: path {Path.GetFileNameWithoutExtension(InputPathFile)} must be fully qualified.\n");
            ReadLine();
            return;
        }

        if (File.Exists(InputPathFile) == false)
        {
            WriteLine($"HUK-ERROR: file {Path.GetFileNameWithoutExtension(InputPathFile)} does not exist.\n");
            return;
        }

        string? rootDirectoryPath = new DirectoryInfo(InputPathFile).Parent?.Parent?.FullName;
        WriteLine($"Root data path is {rootDirectoryPath}");

        if (rootDirectoryPath is null)
        {
            throw new InvalidOperationException($"Cannot determine root directory path {rootDirectoryPath}");
        }

        string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);

        if (Directory.Exists(backupDirectoryPath) == false)
        {
            WriteLine($"Creating {Path.GetDirectoryName(backupDirectoryPath)}\n");
            Directory.CreateDirectory(backupDirectoryPath);
        }

        string inputFileName = Path.GetFileName(InputPathFile);
        string backupFilePath = Path.Combine(backupDirectoryPath, inputFileName);

        WriteLine($"Copying {Path.GetFileNameWithoutExtension(InputPathFile)} to {Path.GetFileNameWithoutExtension(backupFilePath)}\n");
        File.Copy(InputPathFile, backupFilePath, overwrite: true);

        // Ensure the directory exists
        Directory.CreateDirectory(Path.Combine(rootDirectoryPath, InProgressDirectoryName));
        string inProgressFilePath
            = Path.Combine(rootDirectoryPath, InProgressDirectoryName, inputFileName);

        if (File.Exists(inProgressFilePath))
        {
            WriteLine($"ERROR: a file with the name {Path.GetFileNameWithoutExtension(inProgressFilePath)} is already in the directory\n");
        }

        WriteLine($"Moving {Path.GetFileNameWithoutExtension(InputPathFile)} to {Path.GetFileNameWithoutExtension(inProgressFilePath)}\n");
        File.Move(InputPathFile, inProgressFilePath);

        // Determine type of file
        string extension = Path.GetExtension(InputPathFile);

        switch (extension)
        {
            case ".txt":
                ProcessTextFile(inProgressFilePath);
                break;
            default:
                WriteLine($"{extension} is not supported\n");
                break;
        }

        // Moving file after processing is complete
        string completedDirectoryPath
            = Path.Combine(rootDirectoryPath, CompletedDirectoryName);
        Directory.CreateDirectory(completedDirectoryPath);

        string fileNameWithCompletedExtension =
            Path.ChangeExtension(inputFileName, ".complete");
        string completedFileName =
            $"{Guid.NewGuid()}_{Path.GetFileName(fileNameWithCompletedExtension)}";

        string completedFilePath = Path.Combine(completedDirectoryPath, completedFileName);

        WriteLine($"Moving {Path.GetFileNameWithoutExtension(inProgressFilePath)} to {Path.GetFileNameWithoutExtension(completedFilePath)}\n");
        File.Copy(inProgressFilePath, completedFilePath);

        // Deleting processing directory
        string? inProgressDirectoryPath = Path.GetDirectoryName(inProgressFilePath);

        Directory.Delete(inProgressDirectoryPath!, recursive:true);
    }

    private void ProcessTextFile(string inProgressFilePath)
    {
        WriteLine($"Processing text file {Path.GetFileNameWithoutExtension(inProgressFilePath)}\n");
    }
}
