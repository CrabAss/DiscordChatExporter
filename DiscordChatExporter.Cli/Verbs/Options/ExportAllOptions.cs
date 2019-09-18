using CommandLine;
using DiscordChatExporter.Core.Models;
using System.IO;

namespace DiscordChatExporter.Cli.Verbs.Options
{

    [Verb("exportall", HelpText = "Export all guilds the user joined.")]
    public class ExportAllOptions : ExportOptions
    {

        [Option('t', "token", Required = true, SetName = "token", HelpText = "Directory of the file which contains authorization tokens.")]
        public new string TokenValue { get; set; }

        public AuthToken[] GetTokens()
        {
            string[] lines = File.ReadAllLines(TokenValue);
            AuthToken[] output = new AuthToken[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                output[i] = new AuthToken(IsBotToken ? AuthTokenType.Bot : AuthTokenType.User, lines[i]);
            }
            return output;
        }

    }

}