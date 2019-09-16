using CommandLine;
namespace DiscordChatExporter.Cli.Verbs.Options
{

    [Verb("exportall", HelpText = "Export all guilds the user joined.")]
    public class ExportAllOptions : ExportOptions { }

}
