﻿using CommandLine;
using DiscordChatExporter.Cli.Verbs;
using DiscordChatExporter.Cli.Verbs.Options;
using System;
using System.Linq;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static void PrintTokenHelp()
        {
            Console.WriteLine("# To get user token:");
            Console.WriteLine(" 1. Open Discord");
            Console.WriteLine(" 2. Press Ctrl+Shift+I to show developer tools");
            Console.WriteLine(" 3. Navigate to the Application tab");
            Console.WriteLine(" 4. Select \"Local Storage\" > \"https://discordapp.com\" on the left");
            Console.WriteLine(" 5. Press Ctrl+R to reload");
            Console.WriteLine(" 6. Find \"token\" at the bottom and copy the value");
            Console.WriteLine();
            Console.WriteLine("# To get bot token:");
            Console.WriteLine(" 1. Go to Discord developer portal");
            Console.WriteLine(" 2. Open your application's settings");
            Console.WriteLine(" 3. Navigate to the Bot section on the left");
            Console.WriteLine(" 4. Under Token click Copy");
            Console.WriteLine();
            Console.WriteLine("# To get guild ID or guild channel ID:");
            Console.WriteLine(" 1. Open Discord");
            Console.WriteLine(" 2. Open Settings");
            Console.WriteLine(" 3. Go to Appearance section");
            Console.WriteLine(" 4. Enable Developer Mode");
            Console.WriteLine(" 5. Right click on the desired guild or channel and click Copy ID");
            Console.WriteLine();
            Console.WriteLine("# To get direct message channel ID:");
            Console.WriteLine(" 1. Open Discord");
            Console.WriteLine(" 2. Open the desired direct message channel");
            Console.WriteLine(" 3. Press Ctrl+Shift+I to show developer tools");
            Console.WriteLine(" 4. Navigate to the Console tab");
            Console.WriteLine(" 5. Type \"window.location.href\" and press Enter");
            Console.WriteLine(" 6. Copy the first long sequence of numbers inside the URL");
        }

        public static void Main(string[] args)
        {
            // Get all verb types
            var verbTypes = new[]
            {
                typeof(ExportChannelOptions),
                typeof(ExportDirectMessagesOptions),
                typeof(ExportAllOptions),
                typeof(ExportGuildOptions),
                typeof(ExportGuildMembersOptions),
                typeof(GetChannelsOptions),
                typeof(GetDirectMessageChannelsOptions),
                typeof(GetGuildsOptions)
            };

            // Parse command line arguments
            var parsedArgs = Parser.Default.ParseArguments(args, verbTypes);

            // Execute commands
            parsedArgs.WithParsed<ExportChannelOptions>(o => new ExportChannelVerb(o).Execute());
            parsedArgs.WithParsed<ExportDirectMessagesOptions>(o => new ExportDirectMessagesVerb(o).Execute());
            parsedArgs.WithParsed<ExportAllOptions>(o => new ExportAllVerb(o).Execute());
            parsedArgs.WithParsed<ExportGuildOptions>(o => new ExportGuildVerb(o).Execute());
            parsedArgs.WithParsed<ExportGuildMembersOptions>(o => new ExportGuildMembersVerb(o).Execute());
            parsedArgs.WithParsed<GetChannelsOptions>(o => new GetChannelsVerb(o).Execute());
            parsedArgs.WithParsed<GetDirectMessageChannelsOptions>(o => new GetDirectMessageChannelsVerb(o).Execute());
            parsedArgs.WithParsed<GetGuildsOptions>(o => new GetGuildsVerb(o).Execute());

            // Show token help if help requested or no verb specified
            parsedArgs.WithNotParsed(errs =>
            {
                var err = errs.First();

                if (err.Tag == ErrorType.NoVerbSelectedError)
                    PrintTokenHelp();

                if (err.Tag == ErrorType.HelpVerbRequestedError && args.Length == 1)
                    PrintTokenHelp();
            });
        }
    }
}