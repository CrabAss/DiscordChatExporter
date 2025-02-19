﻿using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordChatExporter.Cli.Verbs
{
    public class GetChannelsVerb : Verb<GetChannelsOptions>
    {
        public GetChannelsVerb(GetChannelsOptions options)
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get data service
            var dataService = Container.Instance.Get<DataService>();

            // Get channels
            var channels = await dataService.GetGuildChannelsAsync(Options.GetToken(), Options.GuildId);

            // Filter and order channels
            channels = channels.Where(c => c.Type == ChannelType.GuildTextChat).OrderBy(c => c.Name).ToArray();

            // Print result
            foreach (var channel in channels)
                Console.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}