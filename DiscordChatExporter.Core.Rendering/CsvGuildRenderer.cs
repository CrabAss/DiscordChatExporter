using DiscordChatExporter.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvGuildRenderer : IChatRenderer
    {
        private readonly Guild _guild;
        private readonly string _dateFormat;

        public CsvGuildRenderer(Guild guild, string dateFormat)
        {
            _guild = guild;
            _dateFormat = dateFormat;
        }

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderGuildAsync(TextWriter writer, Guild guild)
        {
            // Guild ID
            await RenderFieldAsync(writer, guild.Id);

            // Guild Name
            await RenderFieldAsync(writer, guild.Name);

            // Guild Owner ID
            await RenderFieldAsync(writer, guild.OwnerId);

            // Guild Verification Level
            await RenderFieldAsync(writer, guild.VerificationLevel.ToString());

            // Guild Description
            await RenderFieldAsync(writer, guild.Description);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;Name;OwnerId;VerificationLevel;Description;");

            // Log
            await RenderGuildAsync(writer, _guild);
        }
    }
}
