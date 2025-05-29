using System.Collections.Immutable;
using System.Globalization;
using Domain.Model;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using Directory = MetadataExtractor.Directory;

namespace Domain.Service
{
  public interface ITaskService
  {
    public void LoadFiles(PresetModel preset, TaskType type);

    public void ProcessTasks();

    public void FinalizeTask();
  }

  public class TaskService(Database db) : ITaskService
  {
    private const string ExifDateFormat = "yyyy:MM:dd HH:mm:ss";

    public void LoadFiles(PresetModel preset, TaskType type)
    {
      try
      {
        if (preset.GetDestinationFolder() is null)
        {
          Log.Error(
                    "Destination Folder {0} does not exist. Files will not be loaded!",
                    preset.DestinationFolder);
          return;
        }

        if (preset.GetSourceFolder() is null)
        {
          return;
        }

        string filePattern = preset.FilePattern.Trim();
        if (!filePattern.StartsWith("*"))
        {
          filePattern = "*" + filePattern;
        }

        List<TaskModel> files = preset.GetSourceFolder()!.GetFiles(filePattern, SearchOption.AllDirectories)
                                      .Select(
                                              e => new TaskModel()
                                                   {
                                                     Preset = preset,
                                                     State = State.Created,
                                                     SourceFile = e.FullName,
                                                     Type = type
                                                   }).ToList();
        db.AddRange(files);
        Log.Information("Loaded {0} files for configuration {1}", files.Count, preset.Name);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    public void ProcessTasks()
    {
      List<TaskModel> tasks = db.Tasks.Include(taskModel => taskModel.Preset)
                                .Where(e => e.State == State.Created && e.Preset.DestinationFolder != null)
                                .ToList();
      foreach (TaskModel task in tasks)
      {
        try
        {
          db.Database.BeginTransaction();
          IReadOnlyList<Directory> metadata = ImageMetadataReader.ReadMetadata(task.SourceFile);
          ImmutableSortedSet<int> tags =
            task.Preset.FolderPattern?.Split("/")?.Select(int.Parse)?.ToImmutableSortedSet() ?? [];

          string destinationFolder = task.Preset.DestinationFolder!;
          foreach (int tagType in tags)
          {
            string value = metadata.Where(e => e.HasTagName(tagType))
                                   .Select(e => e.GetString(tagType))
                                   .Where(e => e != null)
                                   .Cast<string>()
                                   .Distinct()
                                   .Single();
            if (string.IsNullOrWhiteSpace(value))
            {
              continue;
            }

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
          db.Database.CommitTransaction();
        }
        catch (Exception e)
        {
          Log.Error(e, $"Failed to process task {task}");
          db.Database.RollbackTransaction();

          task.State = State.Error;
          db.Update(task);
        }
      }
    }

    public void FinalizeTask()
    {
      List<TaskModel> tasks = db.Tasks
                                .Include(taskModel => taskModel.Preset)
                                .Where(e => e.State == State.Processed && e.DestinationFile != null)
                                .ToList();
      foreach (TaskModel task in tasks)
      {
        try
        {
          db.Database.BeginTransaction();
          string folder = Path.GetDirectoryName(task.DestinationFile!) ?? throw new ApplicationException();
          if (!System.IO.Directory.Exists(folder))
          {
            System.IO.Directory.CreateDirectory(folder);
          }

          if (!File.Exists(task.DestinationFile))
          {
            File.Copy(task.SourceFile, task.DestinationFile!);
            task.State = State.Moved;
          }
          else
          {
            task.State = State.Skipped;
          }

          db.Update(task);
          Log.Information(
                          "{0} moved from '{1}' to '{2}'", Path.GetFileName(task.SourceFile),
                          task.SourceFile, task.DestinationFile);
          db.Database.CommitTransaction();
        }
        catch (Exception e)
        {
          Log.Error(e, $"Failed to finalize task {task}");
          db.Database.RollbackTransaction();

          task.State = State.Error;
          db.Update(task);
        }
      }
    }
  }
}