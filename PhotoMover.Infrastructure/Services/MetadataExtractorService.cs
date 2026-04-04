namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Globalization;

/// <summary>
/// Implementation of metadata extraction using the MetadataExtractor library.
/// </summary>
public sealed class MetadataExtractorService : IMetadataExtractor
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".heic", ".raw", ".arw"
    };

    public async Task<PhotoMetadata?> ExtractMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ExtractMetadata(filePath), cancellationToken);
    }

    private PhotoMetadata? ExtractMetadata(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var extension = Path.GetExtension(filePath);
            if (!SupportedExtensions.Contains(extension))
                return null;

            var directories = ImageMetadataReader.ReadMetadata(filePath);
            var allMetadata = new Dictionary<string, string>();
            var tagIdMetadata = new Dictionary<string, string>();

            // Extract common EXIF data
            // Start with file creation time as ultimate fallback (more reliable than modified time)
            var fileInfo = new FileInfo(filePath);
            DateTime dateTaken = fileInfo.CreationTime;
            string? cameraModel = null;
            string? lensModel = null;
            int? orientation = null;

            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    string tagValue = tag.Description ?? string.Empty;
                    allMetadata[tag.Name] = tagValue;

                    // Store by tag type ID (both hex and decimal)
                    string tagId = tag.Type.ToString();  // Decimal
                    string tagIdHex = $"0x{tag.Type:X4}";  // Hex (e.g., 0x0110)

                    tagIdMetadata[tagId] = tagValue;
                    tagIdMetadata[tagIdHex] = tagValue;

                    // Extract specific fields
                    if (directory is ExifIfd0Directory exif0)
                    {
                        if (tag.Name == "Model")
                            cameraModel = tag.Description;
                        if (tag.Name == "Orientation")
                        {
                            if (int.TryParse(tag.Description, out var orientValue))
                                orientation = orientValue;
                        }

                        // Some cameras store DateTime in IFD0
                        if (tag.Name == "DateTime" || tag.Name == "Date/Time")
                        {
                            if (DateTime.TryParseExact(tag.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                                dateTaken = dt;
                        }
                    }

                    if (directory is ExifSubIfdDirectory exifSubIfd)
                    {
                        // Try multiple possible field names for date taken
                        if (tag.Name == "Date/Time Original" || 
                            tag.Name == "DateTime Original" ||
                            tag.Name == "Date Time Original")
                        {
                            if (DateTime.TryParseExact(tag.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                            {
                                dateTaken = dt;
                            }
                        }

                        // Fallback to Digitized Date if original not found
                        if (dateTaken == fileInfo.CreationTime && 
                            (tag.Name == "Date/Time Digitized" || tag.Name == "DateTime Digitized"))
                        {
                            if (DateTime.TryParseExact(tag.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                                dateTaken = dt;
                        }

                        if (tag.Name == "Lens Model")
                            lensModel = tag.Description;
                    }
                }
            }

            return new PhotoMetadata
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                DateTaken = dateTaken,
                CameraModel = cameraModel ?? "Unknown",
                LensModel = lensModel,
                Orientation = orientation,
                AllMetadata = allMetadata,
                TagIdMetadata = tagIdMetadata
            };
        }
        catch
        {
            return null;
        }
    }

    public IReadOnlyCollection<string> GetAvailableMetadataFields()
    {
        return new[]
        {
            "DateTaken",
            "CameraModel",
            "LensModel",
            "Orientation",
            "ISO",
            "FocalLength",
            "FNumber",
            "ExposureTime"
        };
    }
}
