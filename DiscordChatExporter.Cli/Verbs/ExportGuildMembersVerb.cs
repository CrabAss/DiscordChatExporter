using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Exceptions;
using DiscordChatExporter.Core.Services.Helpers;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Verbs
{
    public class ExportGuildMembersVerb : Verb<ExportGuildMembersOptions>
    {
        public ExportGuildMembersVerb(ExportGuildMembersOptions options)
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get services
            SettingsService settingsService = Container.Instance.Get<SettingsService>();
            DataService dataService = Container.Instance.Get<DataService>();
            ExportService exportService = Container.Instance.Get<ExportService>();

            // Configure settings
            if (!Options.DateFormat.IsNullOrWhiteSpace())
                settingsService.DateFormat = Options.DateFormat;


            try
            {
                Console.Write($"Exporting guild [{Options.GuildId}]... ");

                // Get guild members
                IReadOnlyList<GuildMember> guildMembers = await dataService.GetGuildMembersAsync(Options.GetToken(), Options.GuildId);

                // Filter and order channels
                guildMembers = guildMembers.OrderBy(c => c.User.Id).ToArray();

                // Generate default file name
                var fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, Options.GuildId);

                // Generate file path
                var filePath = Path.Combine(Options.OutputPath ?? "", fileName);

                // Export
                await exportService.ExportGuildMembersAsync(guildMembers, filePath, Options.ExportFormat);
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
}