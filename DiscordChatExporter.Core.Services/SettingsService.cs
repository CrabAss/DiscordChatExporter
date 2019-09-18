using DiscordChatExporter.Core.Models;
using System;
using Tyrrrz.Settings;

namespace DiscordChatExporter.Core.Services
{
    public class SettingsService : SettingsManager
    {
        public bool IsAutoUpdateEnabled { get; set; } = true;

        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm tt";

        public AuthToken LastToken { get; set; }
        public ExportFormat LastExportFormat { get; set; } = ExportFormat.Csv;
        public int? LastPartitionLimit { get; set; }

        // The date when the program starts running: used to generate the object key when uploading to S3
        public string startDate = DateTime.Today.ToString("yyyyMMdd");

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}