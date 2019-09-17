using CommandLine;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Cli.Verbs
{
    class ExportAllVerb : Verb<ExportAllOptions>
    {
        // SettingsService settingsService = Container.Instance.Get<SettingsService>();
        readonly DataService dataService = Container.Instance.Get<DataService>();
        // ExportService exportService = Container.Instance.Get<ExportService>();

        public ExportAllVerb(ExportAllOptions options) : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // AuthToken[] tokens = Options.GetTokens();
            // var guildListByToken = new Dictionary<AuthToken, IReadOnlyList<Guild>>();

            foreach (var token in Options.GetTokens())
            {
                IReadOnlyList<Guild> guilds = await dataService.GetUserGuildsAsync(token);

                foreach (var guild in guilds)
                {
                    string exportGuildArg = Parser.Default.FormatCommandLine(new ExportGuildOptions
                    {
                        BucketName = Options.BucketName,
                        After = Options.After,
                        Before = Options.Before,
                        TokenValue = token.Value,
                        IsBotToken = token.Type == AuthTokenType.Bot,
                        GuildId = guild.Id,
                        IsBundled = true,
                        PartitionLimit = Options.PartitionLimit,
                        DateFormat = Options.DateFormat,
                    });
                    var exportGuildMemberArg = Parser.Default.FormatCommandLine(new ExportGuildMembersOptions
                    {
                        BucketName = Options.BucketName,
                        TokenValue = token.Value,
                        IsBotToken = token.Type == AuthTokenType.Bot,
                        GuildId = guild.Id,
                        DateFormat = Options.DateFormat,
                    });

                    Program.Main($"exportguild {exportGuildArg}".Split(' '));
                    Program.Main($"exportguildmembers {exportGuildMemberArg}".Split(' '));
                }

                // guildListByToken.Add(token, guilds);
            }
        }
    }
}
