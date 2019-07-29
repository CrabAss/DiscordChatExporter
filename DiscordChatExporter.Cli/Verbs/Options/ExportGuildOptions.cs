using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("exportguild", HelpText = "Export all channels within a given guild.")]
    public class ExportGuildOptions : ExportOptions
    {
        [Option('g', "guild", Required = true, HelpText = "Guild ID.")]
        public string GuildId { get; set; }

        [Option("bundled", HelpText = "If you want all chatlogs to be in a single file.")]
        public bool IsBundled { get; set; }
    }
}