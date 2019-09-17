using DiscordChatExporter.Cli.Internal;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Exceptions;
using DiscordChatExporter.Core.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                Guild guild = await dataService.GetGuildAsync(Options.GetToken(), Options.GuildId);

                Console.Write($"Exporting members of guild [{Options.GuildId}]... ");
                using (var progress = new InlineProgress())
                {

                    // Get guild members
                    IReadOnlyList<GuildMember> guildMembers = await dataService.GetGuildMembersAsync(Options.GetToken(), guild.Id);

                    // Order guild members by date of join in descending order
                    guildMembers = guildMembers.OrderByDescending(c => c.JoinedAt).ToArray();

                    // Generate default file name
                    var fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, guild, "GuildMember");

                    // Generate file path
                    var filePath = Path.Combine(Options.OutputPath ?? "", fileName);

                    // Export
                    await exportService.ExportGuildMembersAsync(guildMembers, filePath, Options.ExportFormat, Options.BucketName);

                    progress.ReportCompletion();
                }

                Console.Write($"Exporting roles of guild [{Options.GuildId}]... ");
                using (var progress = new InlineProgress())
                {
                    // Get guild roles
                    IReadOnlyList<Role> roles = await dataService.GetGuildRolesAsync(Options.GetToken(), guild.Id);

                    // Order guild roles by position in descending order (Highest position goes first)
                    roles = roles.OrderByDescending(c => c.Position).ToArray();

                    var fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, guild, "Role");
                    var filePath = Path.Combine(Options.OutputPath ?? "", fileName);
                    await exportService.ExportGuildRolesAsync(roles, filePath, Options.ExportFormat, Options.BucketName);

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
}