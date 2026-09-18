using System.Drawing.Imaging;

namespace Eikonelle;

/// <summary>The folder taken screenshots are saved into.</summary>
public static class ScreenshotFolder
{
    /// <summary>The save folder used when none is configured: Documents\Eikonelle.</summary>
    public static string Default { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eikonelle");

    /// <summary>Whether <paramref name="folder"/> is a full local or network folder path.</summary>
    public static bool IsValid(string? folder) =>
        !string.IsNullOrWhiteSpace(folder) &&
        folder.IndexOfAny(Path.GetInvalidPathChars()) < 0 &&
        Path.IsPathFullyQualified(folder);

    /// <summary>
    /// Save <paramref name="screenshot"/> as a PNG named after <paramref name="takenAt"/> into
    /// <paramref name="folder"/>, creating the folder if needed. Never overwrites an existing file.
    /// </summary>
    /// <returns>The path of the saved file.</returns>
    public static string Save(Screenshot screenshot, string folder, DateTime takenAt)
    {
        ArgumentNullException.ThrowIfNull(screenshot);
        if (!IsValid(folder))
        {
            throw new ArgumentException("The save folder must be a full folder path.", nameof(folder));
        }

        Directory.CreateDirectory(folder);
        string name = "Eikonelle " + takenAt.ToString("yyyy-MM-dd HH-mm-ss");
        for (int attempt = 1; ; attempt++)
        {
            string path = Path.Combine(folder, attempt == 1 ? $"{name}.png" : $"{name} ({attempt}).png");
            FileStream file;
            try
            {
                file = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            }
            catch (IOException) when (File.Exists(path))
            {
                continue;
            }

            try
            {
                using (file)
                {
                    screenshot.Image.Save(file, ImageFormat.Png);
                }

                return path;
            }
            catch
            {
                File.Delete(path);
                throw;
            }
        }
    }
}
