using CommonServiceLocator;
using Domain.Model;
using Domain.Service;
using MetadataExtractor.Formats.Exif;
using Directory = System.IO.Directory;

namespace Domain.UnitTest
{
  public class FileMoverServiceTest : PhotoMoverBaseTest
  {
    private static IFileMoverService Service => ServiceLocator.Current.GetInstance<IFileMoverService>();

    #region LoadFiles

    [TestCase("*", "*", Domain.TestData.FileCount)]
    [TestCase(" ", "*", Domain.TestData.FileCount)]
    [TestCase("*.JPG", "*.JPG", Domain.TestData.JpgCount)]
    [TestCase(".JPG", "*.JPG", Domain.TestData.JpgCount)]
    [Test]
    public void LoadFiles_MultipleFileEndings_CheckCorrectAmountOfFilesLoaded(
      string testFilter, string filter, int fileCount)
    {
      AppConfig.FilePattern = testFilter;
      Service.LoadFiles(AppConfig.FolderSource, TaskType.CreatedByUser);
      Assert.That(Database.Tasks.Count(), Is.EqualTo(fileCount));

      Assert.Multiple(
                      () =>
                      {
                        foreach (FileInfo file in Domain.SourceFolder.GetFiles(filter))
                        {
                          Assert.That(
                                      Database.Tasks.Count(taskModel => taskModel.SourceFile == file.FullName),
                                      Is.EqualTo(1));
                        }

                        foreach (TaskModel task in Database.Tasks.ToList())
                        {
                          Assert.That(task.State, Is.EqualTo(State.Created));
                          Assert.That(task.Type, Is.EqualTo(TaskType.CreatedByUser));
                        }
                      });
    }


    [Test]
    public void LoadFiles_ConfigurationDestinationFolderHasNoneExistingPath_DoesNotThrowAnException()
    {
      AppConfig.FolderTarget = Path.Combine(Path.GetTempPath(), "99999999999999999999999999999999999999");
      Thread.Sleep(TimeSpan.FromSeconds(2));
      Assert.That(Directory.Exists(AppConfig.FolderTarget), Is.False);
      Assert.DoesNotThrow(() => Service.LoadFiles(AppConfig.FolderSource, TaskType.CreatedByUser));
      Assert.That(Database.Tasks.Count(), Is.EqualTo(0));
    }

    [Test]
    public void LoadFiles_ConfigurationSourceFolderHasNoneExistingPath_DoesNotThrowAnException()
    {
      AppConfig.FolderSource = Path.Combine(Path.GetTempPath(), "99999999999999999999999999999999999999");
      Assert.That(Directory.Exists(AppConfig.FolderSource), Is.False);
      Assert.DoesNotThrow(() => Service.LoadFiles(AppConfig.FolderSource, TaskType.CreatedByUser));
      Assert.That(Database.Tasks.Count(), Is.EqualTo(0));
    }

    #endregion

    #region ProcessTasks

    [TestCase(ExifDirectoryBase.TagDateTime, "2019 04 24")]
    [TestCase(ExifDirectoryBase.TagModel, "ILCE-7M3")]
    [Test()]
    public void ProcessTasks_SingleFolderPattern_DestinationFileCorrect(int folderPattern, string folderPath)
    {
      AppConfig.FolderPattern = $"{folderPattern}";
      AppConfig.FilePattern = "*Sony ILCE-7M3 (A7M3).arw";
      Service.LoadFiles(AppConfig.FolderSource, TaskType.CreatedByUser);
      Service.ProcessFiles();
      TaskModel task = Database.Tasks.Single();
      Assert.Multiple(
                      () =>
                      {
                        Assert.That(
                                    task.DestinationFilePath,
                                    Is.EqualTo(
                                               $"{Path.Combine(Domain.TargetFolder.FullName, folderPath, "Sony ILCE-7M3 (A7M3).arw")}"));
                        Assert.That(task.State, Is.EqualTo(State.Processed));
                      });
    }


    [Test()]
    public void ProcessTasks_TwoFolderPattern_DestinationFileCorrect()
    {
      AppConfig.FolderPattern = $"{ExifDirectoryBase.TagModel}/{ExifDirectoryBase.TagDateTime}";
      AppConfig.FilePattern = "*Sony ILCE-7M3 (A7M3).arw";
      Service.LoadFiles(AppConfig.FolderSource, TaskType.CreatedByUser);
      Service.ProcessFiles();
      TaskModel task = Database.Tasks.Single();
      Assert.Multiple(
                      () =>
                      {
                        Assert.That(
                                    task.DestinationFilePath,
                                    Is.EqualTo(
                                               $"{Path.Combine(Domain.TargetFolder.FullName, Path.Combine("ILCE-7M3", "2019 04 24"), "Sony ILCE-7M3 (A7M3).arw")}"));
                        Assert.That(task.State, Is.EqualTo(State.Processed));
                      });
    }

    #endregion

    #region FinalizeTask

    [TestCase(ExifDirectoryBase.TagDateTime)]
    [TestCase(ExifDirectoryBase.TagModel)]
    [Test]
    public void FinalizeTask_SingleFolderPattern_MoveFileToCorrectFolder(int folderPattern)
    {
      AppConfig.FolderPattern = $"{folderPattern}";
      AppConfig.FilePattern = "*Sony ILCE-7M3 (A7M3).arw";
      Service.LoadFiles(AppConfig.FolderSource, TaskType.CreatedByUser);
      Service.ProcessFiles();
      Service.MoveFiles();
      TaskModel task = Database.Tasks.Single();
      Assert.Multiple(
                      () =>
                      {
                        Assert.That(File.Exists(task.DestinationFilePath));
                        Assert.That(File.Exists(task.SourceFilePath));
                        Assert.That(task.State, Is.EqualTo(State.Moved));
                      });
    }

    #endregion
  }
}