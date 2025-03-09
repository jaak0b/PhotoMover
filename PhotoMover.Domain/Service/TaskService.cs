using System.Collections.Immutable;
using System.Globalization;
using Domain.Model;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using Directory = MetadataExtractor.Directory;

namespace Domain.Service;

public interface ITaskService
{
    public void LoadFiles(ConfigurationModel configuration, TaskType type);

    public void ProcessTasks();
}

public class TaskService(Database db) : ITaskService
{
    private const string ExifDateFormat = "yyyy:MM:dd HH:mm:ss";

    public void LoadFiles(ConfigurationModel configuration, TaskType type)
    {
        if (configuration.GetDestinationFolder() is null)
        {
            Log.Error(
                "Destination Folder {0} does not exist. Files will not be loaded!", configuration.DestinationFolder);
            return;
        }

        if (configuration.GetSourceFolder() is null)
            return;
        string filePattern = configuration.FilePattern.Trim();
        if (!filePattern.StartsWith("*"))
            filePattern = "*" + filePattern;
        List<TaskModel> files = configuration.GetSourceFolder()!.GetFiles(filePattern, SearchOption.AllDirectories)
            .Select(
                e => new TaskModel()
                {
                    Configuration = configuration,
                    State = State.Created,
                    SourceFile = e.FullName,
                    Type = type
                }).ToList();
        db.AddRange(files);
        Log.Information("Loaded {0} files for configuration {1}", files.Count, configuration.Name);
    }

    public void ProcessTasks()
    {
        List<TaskModel> tasks = db.Tasks.Include(taskModel => taskModel.Configuration)
            .Where(e => e.State == State.Created && e.Configuration.DestinationFolder != null)
            .ToList();
        foreach (TaskModel task in tasks)
        {
            IReadOnlyList<Directory> metadata = ImageMetadataReader.ReadMetadata(task.SourceFile);
            ImmutableSortedSet<int> tags =
                task.Configuration.FolderPattern?.Split("/")?.Select(int.Parse)?.ToImmutableSortedSet() ?? [];

            string destinationFolder = task.Configuration.DestinationFolder!;
            foreach (int tagType in tags)
            {
                string value = metadata.Where(e => e.HasTagName(tagType))
                    .Select(e => e.GetString(tagType))
                    .Where(e => e != null)
                    .Cast<string>()
                    .Distinct()
                    .Single();
                if (string.IsNullOrWhiteSpace(value))
                    continue;
                if (DateTime.TryParseExact(
                        value,
                        ExifDateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime dateTime))
                {
                    destinationFolder = Path.Combine(destinationFolder, $"{dateTime:yyyy MM dd}");
                }
                else
                {
                    destinationFolder = Path.Combine(destinationFolder, value);
                }
            }

            task.DestinationFile = Path.Combine(destinationFolder, Path.GetFileName(task.SourceFile));
            task.State = State.Processed;
            db.Update(task);
            Log.Information(
                "{0} processed successfully. Destination: {1}", Path.GetFileName(task.SourceFile),
                task.DestinationFile);
        }
    }
}