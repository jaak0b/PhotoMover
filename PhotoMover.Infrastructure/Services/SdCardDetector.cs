namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using System.Runtime.InteropServices;

/// <summary>
/// Implementation of SD/microSD card detection and scanning.
/// </summary>
public sealed class SdCardDetector : ISdCardDetector
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common Image Formats
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp", ".heic", ".ico",

        // RAW Formats - Canon
        ".cr2", ".cr3", ".crw", ".raw",

        // RAW Formats - Nikon
        ".nef", ".nrw",

        // RAW Formats - Sony/Alpha
        ".arw", ".srf", ".sr2",

        // RAW Formats - Fujifilm
        ".raf",

        // RAW Formats - Panasonic/Lumix
        ".rw2",

        // RAW Formats - Pentax
        ".pef", ".dng",

        // RAW Formats - Olympus
        ".orf",

        // RAW Formats - Samsung
        ".srw",

        // RAW Formats - Leica
        ".rwl",

        // RAW Formats - Hasselblad
        ".h5",

        // RAW Formats - Phase One
        ".iiq", ".eip",

        // Video Formats
        ".mp4", ".mov", ".avi", ".mkv", ".webm", ".flv", ".wmv", ".3gp", ".m4v", ".ts", ".mts", ".m2ts", ".mpg", ".mpeg", ".mxf", ".mod", ".asf", ".vob", ".f4v"
    };

    public async Task<IReadOnlyCollection<ImportSource>> DetectSdCardsAsync()
    {
        return await Task.Run(DetectSdCards);
    }

    private IReadOnlyCollection<ImportSource> DetectSdCards()
    {
        var sdCards = new List<ImportSource>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return sdCards;

        try
        {
            var driveInfo = DriveInfo.GetDrives();

            System.Diagnostics.Debug.WriteLine($"[SdCardDetector] Found {driveInfo.Length} drives total");

            foreach (var drive in driveInfo)
            {
                System.Diagnostics.Debug.WriteLine($"[SdCardDetector] Checking drive {drive.Name} - Type: {drive.DriveType}, IsReady: {drive.IsReady}");

                // Detect all removable/external storage media (USB drives, SD cards, external hard drives, etc.)
                if (drive.DriveType != DriveType.Removable)
                {
                    System.Diagnostics.Debug.WriteLine($"[SdCardDetector]   Skipped: Not removable (type: {drive.DriveType})");
                    continue;
                }

                if (!drive.IsReady)
                {
                    System.Diagnostics.Debug.WriteLine($"[SdCardDetector]   Skipped: Not ready");
                    continue;
                }

                var source = new ImportSource
                {
                    Type = ImportSourceType.SdCard,
                    Name = $"{drive.Name.TrimEnd(Path.DirectorySeparatorChar)} - {FormatBytes(drive.TotalFreeSpace)} free ({FormatBytes(drive.TotalSize)} total)",
                    RootPath = drive.RootDirectory.FullName,
                    DetectedAt = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"[SdCardDetector]   Added: {source.Name} at {source.RootPath}");
                sdCards.Add(source);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SdCardDetector] ERROR: {ex}");
        }

        System.Diagnostics.Debug.WriteLine($"[SdCardDetector] Detection complete: Found {sdCards.Count} removable drive(s)");
        return sdCards;
    }

    public async Task<IReadOnlyCollection<string>> ScanSdCardAsync(string sdCardPath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ScanSdCard(sdCardPath), cancellationToken);
    }

    private IReadOnlyCollection<string> ScanSdCard(string sdCardPath)
    {
        var imageFiles = new List<string>();

        try
        {
            if (!Directory.Exists(sdCardPath))
                return imageFiles;

            var allFiles = Directory.GetFiles(sdCardPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                var extension = Path.GetExtension(file);
                if (SupportedImageExtensions.Contains(extension))
                {
                    imageFiles.Add(file);
                }
            }
        }
        catch
        {
            // Silently handle scanning errors
        }

        return imageFiles;
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
