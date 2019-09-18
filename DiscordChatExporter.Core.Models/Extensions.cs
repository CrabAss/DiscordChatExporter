using System;

namespace DiscordChatExporter.Core.Models
{
    public static class Extensions
    {
        public static string GetFileExtension(this ExportFormat format)
        {
            if (format == ExportFormat.Csv)
                return "csv";

            throw new ArgumentOutOfRangeException(nameof(format));
        }
    }
}