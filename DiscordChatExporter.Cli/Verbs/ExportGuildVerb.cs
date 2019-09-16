using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Internal;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Exceptions;
using DiscordChatExporter.Core.Services.Helpers;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Verbs
{
    public class ExportGuildVerb : Verb<ExportGuildOptions>
    {

        // Get services
        SettingsService settingsService = Container.Instance.Get<SettingsService>();
        DataService dataService = Container.Instance.Get<DataService>();
        ExportService exportService = Container.Instance.Get<ExportService>();

        Guild guild;
        IReadOnlyList<Channel> channels;


        public ExportGuildVerb(ExportGuildOptions options)
            : base(options)
        {
        }

        private async Task ExportGuildMetadata()
        {
            Console.Write("Exporting guild metadata... ");

            using (InlineProgress progress = new InlineProgress())
            {

                // Generate default file name
                string fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, guild, "Guild");

                // Generate file path
                string filePath = Path.Combine(Options.OutputPath ?? "", fileName);

                // Export
                await exportService.ExportGuildAsync(guild, filePath, Options.ExportFormat, Options.BucketName);

                progress.ReportCompletion();
            }
        }

        private async Task ExportChannelMetadata()
        {
            Console.Write("Exporting channel metadata... ");

            using (InlineProgress progress = new InlineProgress())
            {
                // Get channels

                // Filter and order channels
                channels = channels
                    .OrderBy(c => Enum.GetName(typeof(ChannelType), c.Type))
                    .OrderBy(c => c.ParentId)
                    .ToArray();

                // Generate default file name
                string fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, guild, "Channel");

                // Generate file path
                string filePath = Path.Combine(Options.OutputPath ?? "", fileName);

                // Export
                await exportService.ExportGuildChannelsAsync(channels, filePath, Options.ExportFormat, Options.BucketName);

                progress.ReportCompletion();
            }
        }

        private async Task ExportGuildChatLogs()
        {
            // Filter and order channels
            channels = channels.Where(c => c.Type == ChannelType.GuildTextChat || c.Type == ChannelType.News).OrderBy(c => c.Name).ToArray();

            ChatLog[] chatLogs = new ChatLog[channels.Count];

            // Loop through channels
            for (int i = 0; i < channels.Count; i++)
            {
                try
                {
                    // Track progress
                    Console.Write($"Exporting channel [{channels[i].Name}]... ");
                    using (InlineProgress progress = new InlineProgress())
                    {
                        // Get chat log
                        chatLogs[i] = await dataService.GetChatLogAsync(Options.GetToken(), channels[i],
                            Options.After, Options.Before, progress);

                        // Generate default file name
                        string fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, guild,
                            chatLogs[i].Channel, Options.After, Options.Before);

                        // Generate file path
                        string filePath = Path.Combine(Options.OutputPath ?? "", fileName);

                        // Export
                        await exportService.ExportChatLogAsync(chatLogs[i], filePath, Options.ExportFormat,
                            Options.PartitionLimit, Options.BucketName);

                        // Report successful completion
                        progress.ReportCompletion();
                    }
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    Console.Error.WriteLine("You don't have access to this channel");
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.Error.WriteLine("This channel doesn't exist");
                }
            }
        }

        private async Task ExportBundledGuildChatLog()
        {
            // Filter and order channels
            channels = channels.Where(c => c.Type == ChannelType.GuildTextChat || c.Type == ChannelType.News).OrderBy(c => c.Name).ToArray();

            ChatLog[] chatLogs = new ChatLog[channels.Count];

            // Loop through channels
            for (int i = 0; i < channels.Count; i++)
            {
                try
                {
                    // Track progress
                    Console.Write($"Exporting channel [{channels[i].Name}]... ");
                    using (InlineProgress progress = new InlineProgress())
                    {
                        // Get chat log
                        chatLogs[i] = await dataService.GetChatLogAsync(Options.GetToken(), channels[i],
                            Options.After, Options.Before, progress);

                        // Report successful completion
                        progress.ReportCompletion();
                    }
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    Console.Error.WriteLine("You don't have access to this channel");
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.Error.WriteLine("This channel doesn't exist");
                }
            }

            // Generate default file name
            string fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, guild,
                null, Options.After, Options.Before);

            // Generate file path
            string filePath = Path.Combine(Options.OutputPath ?? "", fileName);

            // Export
            await exportService.ExportChatLogAsync(chatLogs, filePath, Options.ExportFormat, Options.BucketName);


        }

        public override async Task ExecuteAsync()
        {

            // Configure settings
            if (!Options.DateFormat.IsNullOrWhiteSpace())
                settingsService.DateFormat = Options.DateFormat;

            guild = await dataService.GetGuildAsync(Options.GetToken(), Options.GuildId);
            channels = await dataService.GetGuildChannelsAsync(Options.GetToken(), Options.GuildId);

            await ExportGuildMetadata();
            await ExportChannelMetadata();
            if (Options.IsBundled)
                await ExportBundledGuildChatLog();
            else
                await ExportGuildChatLogs();
            
        }
    }
}