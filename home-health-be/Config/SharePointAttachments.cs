using home_health_be.Models.Responses;
using home_health_be.Models.Third_Party;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;

namespace home_health_be.Config
{
    public sealed class SharePointAttachments(GraphServiceClient graphClient, IOptions<SharePointConfig> sharePointOptions)
    {
        private const int UploadSliceSize = 320 * 1024;
        private readonly SharePointConfig? _sharePointConfig = sharePointOptions.Value;

        public async Task<string?> CreateFolderAsync(int evaluationId)
        {
            if (string.IsNullOrEmpty(_sharePointConfig?.DriveId))
            {
                throw new InvalidOperationException("SharePoint DriveId is not configured");
            }

            var folder = new DriveItem
            {
                Name = GetEvaluationFolderName(evaluationId),
                Folder = new Folder(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["@microsoft.graph.conflictBehavior"] = "rename"
                }
            };

            var result = await graphClient
                .Drives[_sharePointConfig.DriveId]
                .Items
                .PostAsync(folder);

            return result?.Id;
        }

        public async Task<DefaultResponse> CreateAttachmentAsync(
            int evaluationId,
            AttachmentParameters attachment)
        {
            var folder = await GetFolderAsync(attachment.FolderID);
            if (folder == null)
            {
                return Failure("Folder not found.");
            }

            if (await FileExistsAsync(folder.Id!, attachment.FileName))
            {
                return Failure("File with the same name already exists.");
            }

            try
            {
                using var fileStream = CreateFileStreamFromBase64(attachment.FileBase64);
                await UploadFileAsync(folder.Name!, attachment.FileName, fileStream);

                return Success("File uploaded successfully.");
            }
            catch (ServiceException ex)
            {
                return Failure($"Error uploading file: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Failure($"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<SharePointFile> DownloadAttachmentAsync(string fileId)
        {
            try
            {
                var driveItem = await graphClient
                    .Drives[_sharePointConfig.DriveId]
                    .Items[fileId]
                    .GetAsync();

                if (driveItem?.AdditionalData == null)
                {
                    return new SharePointFile();
                }

                var downloadUrl = driveItem.AdditionalData["@microsoft.graph.downloadUrl"]?.ToString();
                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    return new SharePointFile();
                }

                using var httpClient = new HttpClient();
                var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

                return new SharePointFile
                {
                    FileName = driveItem.Name,
                    FileType = driveItem.File?.MimeType,
                    FileBase64 = Convert.ToBase64String(fileBytes)
                };
            }
            catch (Exception)
            {
                // File download failed, return empty file
                return new SharePointFile();
            }
        }

        public async Task<DefaultResponse> DeleteAttachmentAsync(string fileId)
        {
            try
            {
                await graphClient
                    .Drives[_sharePointConfig.DriveId]
                    .Items[fileId]
                    .DeleteAsync();

                return Success("File deleted successfully.");
            }
            catch (Exception ex)
            {
                return Failure($"An error occurred: {ex.Message}");
            }
        }

        public async Task<List<Models.Third_Party.AttachmentInfo>> GetAttachmentsByFolderIdAsync(string folderId)
        {
            try
            {
                var children = await graphClient
                    .Drives[_sharePointConfig.DriveId]
                    .Items[folderId]
                    .Children
                    .GetAsync();

                if (children?.Value == null)
                {
                    return new List<Models.Third_Party.AttachmentInfo>();
                }

                return children.Value
                    .Where(item => item.File != null)
                    .Select(item => new Models.Third_Party.AttachmentInfo
                    {
                        FileId = item.Id ?? string.Empty,
                        FileName = item.Name ?? string.Empty,
                        FileType = item.File?.MimeType,
                        Size = item.Size,
                        ModifiedDateTime = item.LastModifiedDateTime
                    })
                    .ToList();
            }
            catch (Exception)
            {
                // Failed to retrieve attachments, return empty list
                return new List<Models.Third_Party.AttachmentInfo>();
            }
        }

        // ---------- Private Helpers ----------

        private static string GetEvaluationFolderName(int evaluationId) =>
            $"evaluation-{evaluationId:D2}";

        private async Task<DriveItem?> GetFolderAsync(string folderId) =>
            await graphClient
                .Drives[_sharePointConfig.DriveId]
                .Items[folderId]
                .GetAsync();

        private async Task<bool> FileExistsAsync(string folderId, string fileName)
        {
            var children = await graphClient
                .Drives[_sharePointConfig.DriveId]
                .Items[folderId]
                .Children
                .GetAsync();

            return children?.Value?.Any(f => f.Name == fileName) == true;
        }

        public async Task<string?> UploadFileAndGetIdAsync(
            string folderId,
            string fileName,
            string fileBase64)
        {
            var folder = await GetFolderAsync(folderId);
            if (folder == null)
            {
                return null;
            }

            try
            {
                using var fileStream = CreateFileStreamFromBase64(fileBase64);
                var filePath = $"{folder.Name!}/{fileName}";

                var uploadSession = await graphClient
                    .Drives[_sharePointConfig.DriveId]
                    .Root
                    .ItemWithPath(filePath)
                    .CreateUploadSession
                    .PostAsync(new CreateUploadSessionPostRequestBody
                    {
                        Item = new DriveItemUploadableProperties
                        {
                            AdditionalData = new Dictionary<string, object>
                            {
                                ["@microsoft.graph.conflictBehavior"] = "replace"
                            }
                        }
                    });

                var uploadTask = new LargeFileUploadTask<DriveItem>(
                    uploadSession,
                    fileStream,
                    UploadSliceSize);

                var uploadResult = await uploadTask.UploadAsync();
                return uploadResult?.ItemResponse?.Id;
            }
            catch (Exception)
            {
                // File upload failed, return null
                return null;
            }
        }

        private async Task UploadFileAsync(
            string folderName,
            string fileName,
            Stream fileStream)
        {
            var filePath = $"{folderName}/{fileName}";

            var uploadSession = await graphClient
                .Drives[_sharePointConfig.DriveId]
                .Root
                .ItemWithPath(filePath)
                .CreateUploadSession
                .PostAsync(new CreateUploadSessionPostRequestBody
                {
                    Item = new DriveItemUploadableProperties
                    {
                        AdditionalData = new Dictionary<string, object>
                        {
                            ["@microsoft.graph.conflictBehavior"] = "replace"
                        }
                    }
                });

            var uploadTask = new LargeFileUploadTask<DriveItem>(
                uploadSession,
                fileStream,
                UploadSliceSize);

            await uploadTask.UploadAsync();
        }

        private static MemoryStream CreateFileStreamFromBase64(string base64)
        {
            var cleanedBase64 = base64.Contains(',')
                ? base64.Split(',')[1]
                : base64;

            return new MemoryStream(Convert.FromBase64String(cleanedBase64));
        }

        private static DefaultResponse Success(string message) =>
            new() { Success = true, Message = message };

        private static DefaultResponse Failure(string message) =>
            new() { Success = false, Message = message };
    }

    public sealed class AttachmentParameters
    {
        public string FolderID { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileID { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileBase64 { get; set; } = string.Empty;
    }
}
