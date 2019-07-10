using DiscordChatExporter.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvGuildChannelsRenderer : IChatRenderer
    {
        private readonly IReadOnlyList<Channel> _channels;
        private readonly string _dateFormat;

        public CsvGuildChannelsRenderer(IReadOnlyList<Channel> channels, string dateFormat)
        {
            _channels = channels;
            _dateFormat = dateFormat;
        }

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderOverwriteAsync(TextWriter writer, PermissionOverwrite[] overwrites)
        {
            string[] overwriteStrings = overwrites.Select(x => x.ToString()).ToArray();
            await writer.WriteAsync($"\"{string.Join(", ", overwriteStrings)}\";");
        }

        private async Task RenderGuildChannelAsync(TextWriter writer, Channel channel)
        {
            // User ID
            await RenderFieldAsync(writer, channel.Id);

            // User FullName
            await RenderFieldAsync(writer, channel.ParentId ?? "");

            // User is bot?
            await RenderFieldAsync(writer, channel.GuildId);

            // User avatar URL
            await RenderFieldAsync(writer, Enum.GetName(typeof(ChannelType), channel.Type));

            // User Nickname in guild
            await RenderFieldAsync(writer, channel.Name);

            // User Roles
            await RenderFieldAsync(writer, channel.Topic ?? "");

            // when did the user join this guild
            await RenderOverwriteAsync(writer, channel.Overwrites);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;ParentId;GuildId;Type;Name;Topic;PermissionOverwrites;");

            // Log
            foreach (Channel channel in _channels)
                await RenderGuildChannelAsync(writer, channel);
        }
    }
}
