using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("exportguildmembers", HelpText = "Export all members within a given guild.")]
    public class ExportGuildMembersOptions : ExportOptions
    {
        [Option('g', "guild", Required = true, HelpText = "Guild ID.")]
        public string GuildId { get; set; }
    }
}