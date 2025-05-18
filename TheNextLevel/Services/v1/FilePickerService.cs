using Microsoft.Maui.Storage;

namespace TheNextLevel.Services.v1
{
    public class FilePickerService
    {
        public async Task<string> PickImageAsync()
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Select a background image",
                    FileTypes = FilePickerFileType.Images
                };

                var result = await FilePicker.PickAsync(options);

                if (result != null)
                {
                    // Copy the file to the app's data directory for persistence
                    return await CopyFileToAppStorage(result);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"File picking failed: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> CopyFileToAppStorage(FileResult file)
        {
            // Create backgrounds directory if it doesn't exist
            var backgroundsDir = Path.Combine(FileSystem.AppDataDirectory, "Backgrounds");
            if (!Directory.Exists(backgroundsDir))
            {
                Directory.CreateDirectory(backgroundsDir);
            }

            // Generate a unique filename
            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var targetPath = Path.Combine(backgroundsDir, uniqueFileName);

            // Copy the file
            using var sourceStream = await file.OpenReadAsync();
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);

            // Return the file path that can be used in the app
            return $"file://{targetPath}";
        }
    }
}