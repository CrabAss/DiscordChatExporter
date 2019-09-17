using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public class ExportService
    {
        private readonly SettingsService _settingsService;

        // AWS S3
        // private const string bucketName = "chatrank";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest2;
        private static IAmazonS3 s3Client;

        public ExportService(SettingsService settingsService)
        {
            _settingsService = settingsService;

            // AWS S3
            s3Client = new AmazonS3Client(bucketRegion);
        }

        private IChatRenderer CreateRenderer(ChatLog chatLog, ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return new PlainTextChatLogRenderer(chatLog, _settingsService.DateFormat);

            if (format == ExportFormat.HtmlDark)
                return new HtmlChatLogRenderer(chatLog, "Dark", _settingsService.DateFormat);

            if (format == ExportFormat.HtmlLight)
                return new HtmlChatLogRenderer(chatLog, "Light", _settingsService.DateFormat);

            if (format == ExportFormat.Csv)
                return new CsvChatLogRenderer(chatLog, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        private IChatRenderer CreateRenderer(IReadOnlyList<ChatLog> chatLogs, ExportFormat format)
        {
            if (format == ExportFormat.Csv)
                return new CsvBundledChatLogRenderer(chatLogs, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        private IChatRenderer CreateRenderer(IReadOnlyList<GuildMember> guildMembers, ExportFormat format)
        {
            if (format == ExportFormat.Csv)
                return new CsvGuildMembersRenderer(guildMembers, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        private IChatRenderer CreateRenderer(IReadOnlyList<Role> roles, ExportFormat format)
        {
            if (format == ExportFormat.Csv)
                return new CsvGuildRolesRenderer(roles, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        private IChatRenderer CreateRenderer(IReadOnlyList<Channel> channels, ExportFormat format)
        {
            if (format == ExportFormat.Csv)
                return new CsvGuildChannelsRenderer(channels, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        private IChatRenderer CreateRenderer(Guild guild, ExportFormat format)
        {
            if (format == ExportFormat.Csv)
                return new CsvGuildRenderer(guild, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        public async Task ExportGuildMembersAsync(IReadOnlyList<GuildMember> guildMembers, string filePath, ExportFormat format, string S3BucketName = null)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!dirPath.IsNullOrWhiteSpace())
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            using (var writer = File.CreateText(filePath))
                await CreateRenderer(guildMembers, format).RenderAsync(writer);

            if (S3BucketName != null)
                await UploadToS3(S3BucketName, filePath);
        }

        public async Task ExportGuildRolesAsync(IReadOnlyList<Role> roles, string filePath, ExportFormat format, string S3BucketName = null)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!dirPath.IsNullOrWhiteSpace())
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            using (var writer = File.CreateText(filePath))
                await CreateRenderer(roles, format).RenderAsync(writer);

            if (S3BucketName != null)
                await UploadToS3(S3BucketName, filePath);
        }

        public async Task ExportGuildChannelsAsync(IReadOnlyList<Channel> channels, string filePath, ExportFormat format, string S3BucketName = null)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!dirPath.IsNullOrWhiteSpace())
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            using (var writer = File.CreateText(filePath))
                await CreateRenderer(channels, format).RenderAsync(writer);

            if (S3BucketName != null)
                await UploadToS3(S3BucketName, filePath);
        }

        public async Task ExportGuildAsync(Guild guild, string filePath, ExportFormat format, string S3BucketName = null)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!dirPath.IsNullOrWhiteSpace())
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            using (var writer = File.CreateText(filePath))
                await CreateRenderer(guild, format).RenderAsync(writer);

            if (S3BucketName != null)
                await UploadToS3(S3BucketName, filePath);
        }

        /* Bundled ChatLog Export */
        public async Task ExportChatLogAsync(IReadOnlyList<ChatLog> chatLogs, string filePath, ExportFormat format, string S3BucketName = null)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!dirPath.IsNullOrWhiteSpace())
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            File.Delete(filePath);
            using (var writer = File.AppendText(filePath))
                await CreateRenderer(chatLogs, format).RenderAsync(writer);

            if (S3BucketName != null)
                await UploadToS3(S3BucketName, filePath);
        }

        private async Task ExportChatLogAsync(ChatLog chatLog, string filePath, ExportFormat format, string S3BucketName = null)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!dirPath.IsNullOrWhiteSpace())
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            using (var writer = File.CreateText(filePath))
                await CreateRenderer(chatLog, format).RenderAsync(writer);

            if (S3BucketName != null)
                await UploadToS3(S3BucketName, filePath);
        }

        public async Task ExportChatLogAsync(ChatLog chatLog, string filePath, ExportFormat format, int? partitionLimit, string S3BucketName = null)
        {
            // If partitioning is disabled or there are fewer messages in chat log than the limit - process it without partitioning
            if (partitionLimit == null || partitionLimit <= 0 || chatLog.Messages.Count <= partitionLimit)
            {
                await ExportChatLogAsync(chatLog, filePath, format);
            }
            // Otherwise split into partitions and export separately
            else
            {
                // Create partitions by grouping up to X contiguous messages into separate chat logs
                var partitions = chatLog.Messages.GroupContiguous(g => g.Count < partitionLimit.Value)
                    .Select(g => new ChatLog(chatLog.Guild, chatLog.Channel, chatLog.After, chatLog.Before, g, chatLog.Mentionables))
                    .ToArray();

                // Split file path into components
                var dirPath = Path.GetDirectoryName(filePath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                var fileExt = Path.GetExtension(filePath);

                // Export each partition separately
                var partitionNumber = 1;
                foreach (ChatLog partition in partitions)
                {
                    // Compose new file name
                    var partitionFilePath = $"{fileNameWithoutExt} [{partitionNumber} of {partitions.Length}]{fileExt}";

                    // Compose full file path
                    if (!dirPath.IsNullOrWhiteSpace())
                        partitionFilePath = Path.Combine(dirPath, partitionFilePath);

                    // Export
                    await ExportChatLogAsync(partition, partitionFilePath, format);

                    if (S3BucketName != null)
                        await UploadToS3(S3BucketName, partitionFilePath);

                    // Increment partition number
                    partitionNumber++;
                }
            }
        }

        public async Task UploadToS3(string bucketName, string filePath, bool deleteLocalCopyAfterUpload = true)
        {
            string today = DateTime.Today.ToString("yyyyMMdd");
            string keyName = $"discord/raw_archive/{today}/{filePath}";
            try
            {
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(filePath, bucketName, keyName);
                if (deleteLocalCopyAfterUpload) File.Delete(filePath);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }
    }
}