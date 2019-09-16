using CommandLine;
using DiscordChatExporter.Core.Models;
using System.IO;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    public abstract class TokenOptions
    {
        [Option('t', "token", Required = true, SetName = "token", HelpText = "Authorization token.")]
        public string TokenValue { get; set; }

        [Option("tokenfile", Required = true, SetName = "token", HelpText = "Directory of the file which contains authorization tokens.")]
        public string TokenFileDir { get; set; }

        [Option('b', "bot", Default = false, HelpText = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }

        public AuthToken GetToken() => new AuthToken(IsBotToken ? AuthTokenType.Bot : AuthTokenType.User, TokenValue);

        public AuthToken[] GetTokens()
        {
            string[] lines = File.ReadAllLines(TokenFileDir);
            AuthToken[] output = new AuthToken[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                output[i] = new AuthToken(IsBotToken ? AuthTokenType.Bot : AuthTokenType.User, lines[i]);
            }
            return output;
        }
    }
}