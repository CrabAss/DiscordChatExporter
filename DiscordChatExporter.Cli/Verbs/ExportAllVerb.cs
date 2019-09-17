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

                const string bucketParam = "--bucket chatrank";
                var tokenParam = new StringBuilder($"--token \"{token.Value}\"");
                if (token.Type == AuthTokenType.Bot)
                    tokenParam.Append(" -b");

                foreach (var guild in guilds)
                {
                    Program.Main($"exportguild --after {Options.After} {tokenParam} {bucketParam} --guild {guild.Id} --bundled".Split(' '));
                    Program.Main($"exportguildmembers {tokenParam} {bucketParam} --guild {guild.Id}".Split(' '));
                }

                // guildListByToken.Add(token, guilds);
            }
        }
    }
}
