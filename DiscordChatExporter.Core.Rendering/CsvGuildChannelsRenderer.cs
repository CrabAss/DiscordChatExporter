using DiscordChatExporter.Core.Models;
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
            await writer.WriteAsync($"[{string.Join(", ", overwriteStrings)}];");
        }

        private async Task RenderGuildChannelAsync(TextWriter writer, Channel channel)
        {
            // Channel ID
            await RenderFieldAsync(writer, channel.Id);

            // Channel's Parent ID
            await RenderFieldAsync(writer, channel.ParentId ?? "");

            // Channel Type
            await RenderFieldAsync(writer, ((int) channel.Type).ToString());

            // Channel Name
            await RenderFieldAsync(writer, channel.Name);

            // Channel Topics
            await RenderFieldAsync(writer, channel.Topic ?? "");

            // Channel Overwrites
            await RenderOverwriteAsync(writer, channel.Overwrites);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;ParentID;Type;Name;Topic;Overwrites;");

            // Log
            foreach (Channel channel in _channels)
                await RenderGuildChannelAsync(writer, channel);
        }
    }
}
