using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using CommonServiceLocator;
using Domain.Model;
using MetadataExtractor;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Directory = MetadataExtractor.Directory;

namespace Domain.Service
{
  public interface IFileMoverService
  {
    public void LoadFiles(string sourceFolder, TaskType type);

    public void ProcessFiles();

    public void MoveFiles();
  }

  public class FileMoverService(Database db, IAppConfig appConfig) : IFileMoverService
  {
    private const string ExifDateFormat = "yyyy:MM:dd HH:mm:ss";

    public void LoadFiles(string sourceFolder, TaskType type)
    {
      try
      {
        if (!System.IO.Directory.Exists(appConfig.FolderTarget))
        {
          Log.Error(
                    "Destination Folder {0} does not exist. Files will not be loaded!",
                    appConfig.FolderTarget);
          return;
        }

        if (!System.IO.Directory.Exists(sourceFolder))
        {
          return;
        }

        string filePattern = appConfig.FilePattern.Trim();
        if (!filePattern.StartsWith("*"))
        {
          filePattern = "*" + filePattern;
        }


        List<TaskModel> files = new DirectoryInfo(sourceFolder)
                               .GetFiles(filePattern, SearchOption.AllDirectories)
                               .Select(
                                       fileInfo => new TaskModel()
                                                   {
                                                     State = State.Created,
                                                     SourceFile = fileInfo.FullName,
                                                     Type = type
                                                   }).ToList();
        db.AddRange(files);
        Log.Information("Loaded {0} files.", files.Count);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    public void ProcessFiles()
    {
      List<TaskModel> tasks = db.Tasks
                                .Where(taskModel => taskModel.State == State.Created)
                                .ToList();
      foreach (TaskModel task in tasks)
      {
        try
        {
          IReadOnlyList<Directory> metadata = ImageMetadataReader.ReadMetadata(task.SourceFile);
          ImmutableSortedSet<int> tags =
            appConfig.FolderPattern?.Split("/")?.Select(int.Parse)?.ToImmutableSortedSet() ?? [];

          string destinationFolder = appConfig.FolderTarget;
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
        }
        catch (Exception e)
        {
          Log.Error(e, $"Failed to process task {task}");

          task.State = State.Error;
          db.Update(task);
        }
      }
    }

    public void MoveFiles()
    {
      ConcurrentQueue<TaskModel> queue = new(
                                             db.Tasks
                                               .Where(taskModel => taskModel.State == State.Processed)
                                               .ToList());

      List<Thread> tasks = Enumerable.Repeat(0, Math.Abs(Environment.ProcessorCount / 2))
                                     .Select(_ => new Thread(() => FinalizeTaskInternal(queue)))
                                     .ToList();

      foreach (Thread task in tasks)
      {
        task.Start();
      }

      foreach (Thread task in tasks)
      {
        task.Join();
      }
    }

    private void FinalizeTaskInternal(ConcurrentQueue<TaskModel> queue)
    {
      Database? localDb = ServiceLocator.Current.GetInstance<Database>();
      while (queue.TryDequeue(out TaskModel task))
      {
        try
        {
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

          localDb.Update(task);
          Log.Information(
                          "{0} moved from '{1}' to '{2}'", Path.GetFileName(task.SourceFile),
                          task.SourceFile, task.DestinationFile);
        }
        catch (Exception e)
        {
          Log.Error(e, $"Failed to finalize task {task}");

          task.State = State.Error;
          localDb.Update(task);
        }
      }
    }
  }
}