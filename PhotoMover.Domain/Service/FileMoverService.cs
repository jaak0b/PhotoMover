using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using Domain.Model;
using MetadataExtractor;
using Serilog;
using Directory = MetadataExtractor.Directory;

namespace Domain.Service
{
  public interface IFileMoverService
  {
    public IEnumerable<TaskModel> LoadFiles();

    public void ProcessFiles(IEnumerable<TaskModel> taskModels);

    public void MoveFiles(IEnumerable<TaskModel> taskModels);
  }

  public class FileMoverService(ISettingsProvider provider) : IFileMoverService
  {
    private const string ExifDateFormat = "yyyy:MM:dd HH:mm:ss";

    public IEnumerable<TaskModel> LoadFiles()
    {
      try
      {
        if (!System.IO.Directory.Exists(provider.Settings.Value.DestinationFolder))
        {
          Log.Error("Destination Folder {0} does not exist. Files will not be loaded!",
                    provider.Settings.Value.DestinationFolder);
          return new List<TaskModel>();
        }

        string filePattern = provider.Settings.Value.FilePattern.Trim();
        if (!filePattern.StartsWith("*"))
        {
          filePattern = "*" + filePattern;
        }

        List<TaskModel> files = new DirectoryInfo(provider.Settings.Value.SourceFolder)
                               .GetFiles(filePattern, SearchOption.AllDirectories)
                               .Select(fileInfo => new TaskModel() { SourceFilePath = fileInfo.FullName, })
                               .ToList();
        return files;
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    public void ProcessFiles(IEnumerable<TaskModel> taskModels)
    {
      foreach (TaskModel task in taskModels.Where(taskModel => taskModel.ErrorMessage is null))
      {
        try
        {
          IReadOnlyList<Directory> metadata = ImageMetadataReader.ReadMetadata(task.SourceFilePath);
          ImmutableSortedSet<int> tags = provider.Settings.Value.Groups.Select(groupOptions => (int)groupOptions)?.ToImmutableSortedSet() ?? [];

          string destinationFolder = provider.Settings.Value.DestinationFolder;
          foreach (int tagType in tags)
          {
            string value = metadata.Where(directory => directory.HasTagName(tagType))
                                   .Select(directory => directory.GetString(tagType))
                                   .Where(s => s != null)
                                   .Cast<string>()
                                   .Distinct()
                                   .Single();
            if (string.IsNullOrWhiteSpace(value))
            {
              continue;
            }

            if (DateTime.TryParseExact(value,
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

          task.DestinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(task.SourceFilePath));
          Log.Information("{0} processed successfully. Destination: {1}",
                          Path.GetFileName(task.SourceFilePath),
                          task.DestinationFilePath);
        }
        catch (Exception exception)
        {
          Log.Error(exception, $"Failed to process task {task}");
          task.ErrorMessage = exception.Message;
        }
      }
    }

    public void MoveFiles(IEnumerable<TaskModel> taskModels)
    {
      ConcurrentQueue<TaskModel> queue = new(taskModels.ToList());

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
      while (queue.TryDequeue(out TaskModel task))
      {
        try
        {
          string folder = Path.GetDirectoryName(task.DestinationFilePath!) ?? throw new ApplicationException();
          if (!System.IO.Directory.Exists(folder))
          {
            System.IO.Directory.CreateDirectory(folder);
          }

          if (!File.Exists(task.DestinationFilePath))
          {
            File.Copy(task.SourceFilePath, task.DestinationFilePath!);
            task.FileAlreadyExists = false;
          }
          else
          {
            task.FileAlreadyExists = true;
          }

          Log.Information("{0} moved from '{1}' to '{2}'",
                          Path.GetFileName(task.SourceFilePath),
                          task.SourceFilePath,
                          task.DestinationFilePath);
        }
        catch (Exception e)
        {
          Log.Error(e, $"Failed to finalize task {task}");
          task.ErrorMessage += e.Message;
        }
      }
    }
  }
}