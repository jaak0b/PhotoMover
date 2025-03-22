using CommonServiceLocator;
using Domain.Model;
using Domain.Service;
using MetadataExtractor.Formats.Exif;
using Microsoft.EntityFrameworkCore;
using Directory = System.IO.Directory;

namespace Domain.UnitTest;

public class TaskServiceTest : PhotoMoverBaseTest
{
    private static ITaskService Service => ServiceLocator.Current.GetInstance<ITaskService>();

    #region LoadFiles

    [TestCase("*", "*", Domain.TestData.FileCount)]
    [TestCase(" ", "*", Domain.TestData.FileCount)]
    [TestCase("*.JPG", "*.JPG", Domain.TestData.JpgCount)]
    [TestCase(".JPG", "*.JPG", Domain.TestData.JpgCount)]
    [Test]
    public void LoadFiles_MultipleFileEndings_CheckCorrectAmountOfFilesLoaded(string testFilter, string filter, int fileCount)
    {
        TestPreset.FilePattern = testFilter;
        Service.LoadFiles(TestPreset, TaskType.CreatedByUser);
        Assert.That(Database.Tasks.Count(), Is.EqualTo(fileCount));
        foreach (FileInfo file in Domain.SourceFolder.GetFiles(filter))
        {
            Assert.That(Database.Tasks.Count(e => e.SourceFile == file.FullName), Is.EqualTo(1));
        }

        foreach (TaskModel task in Database.Tasks.Include(e => e.Preset).ToList())
        {
            Assert.That(task.Preset, Is.EqualTo(TestPreset));
            Assert.That(task.State, Is.EqualTo(State.Created));
            Assert.That(task.Type, Is.EqualTo(TaskType.CreatedByUser));
        }
    }


    [Test]
    public void LoadFiles_ConfigurationDestinationFolderHasNoneExistingPath_DoesNotThrowAnException()
    {
        TestPreset.DestinationFolder = Path.Combine(
            Path.GetTempPath(), "99999999999999999999999999999999999999");
        Assert.That(Directory.Exists(TestPreset.DestinationFolder), Is.False);
        Assert.DoesNotThrow(() => Service.LoadFiles(TestPreset, TaskType.CreatedByUser));
        Assert.That(Database.Tasks.Count(), Is.EqualTo(0));
    }

    [Test]
    public void LoadFiles_ConfigurationSourceFolderHasNoneExistingPath_DoesNotThrowAnException()
    {
        TestPreset.SourceFolder = Path.Combine(
            Path.GetTempPath(), "99999999999999999999999999999999999999");
        Assert.That(Directory.Exists(TestPreset.SourceFolder), Is.False);
        Assert.DoesNotThrow(() => Service.LoadFiles(TestPreset, TaskType.CreatedByUser));
        Assert.That(Database.Tasks.Count(), Is.EqualTo(0));
    }

    #endregion

    #region ProcessTasks

    [TestCase(ExifDirectoryBase.TagDateTime, "2019 04 24")]
    [TestCase(ExifDirectoryBase.TagModel, "ILCE-7M3")]
    [Test()]
    public void ProcessTasks_SingleFolderPattern_DestinationFileCorrect(int folderPattern, string folderPath)
    {
        TestPreset.FolderPattern = $"{folderPattern}";
        TestPreset.FilePattern = "*Sony ILCE-7M3 (A7M3).arw";
        Service.LoadFiles(TestPreset, TaskType.CreatedByUser);
        Service.ProcessTasks();
        TaskModel task = Database.Tasks.Single();
        Assert.Multiple(
            () =>
            {
                Assert.That(
                    task.DestinationFile,
                    Is.EqualTo(
                        $"{Path.Combine(Domain.TargetFolder.FullName, folderPath, "Sony ILCE-7M3 (A7M3).arw")}"));
                Assert.That(task.State, Is.EqualTo(State.Processed));
            });
    }

    
    [Test()]
    public void ProcessTasks_TwoFolderPattern_DestinationFileCorrect()
    {
        TestPreset.FolderPattern = $"{ExifDirectoryBase.TagModel}/{ExifDirectoryBase.TagDateTime}";
        TestPreset.FilePattern = "*Sony ILCE-7M3 (A7M3).arw";
        Service.LoadFiles(TestPreset, TaskType.CreatedByUser);
        Service.ProcessTasks();
        TaskModel task = Database.Tasks.Single();
        Assert.Multiple(
            () =>
            {
                Assert.That(
                    task.DestinationFile,
                    Is.EqualTo(
                        $"{Path.Combine(Domain.TargetFolder.FullName, Path.Combine("ILCE-7M3","2019 04 24"), "Sony ILCE-7M3 (A7M3).arw")}"));
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
        TestPreset.FolderPattern = $"{folderPattern}";
        TestPreset.FilePattern = "*Sony ILCE-7M3 (A7M3).arw";
        Service.LoadFiles(TestPreset, TaskType.CreatedByUser);
        Service.ProcessTasks();
        Service.FinalizeTask();
        TaskModel task = Database.Tasks.Single();
        Assert.Multiple(
            () =>
            {
                Assert.That(File.Exists(task.DestinationFile));
                Assert.That(File.Exists(task.SourceFile));
                Assert.That(task.State, Is.EqualTo(State.Moved));
            });
    }

    #endregion
}