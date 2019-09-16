using DiscordChatExporter.Core.Models;
using System.IO;
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

        private async Task RenderFieldAsync(StreamWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderGuildAsync(StreamWriter writer, Guild guild)
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

        public async Task RenderAsync(StreamWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;Name;OwnerID;VerificationLevel;Description;");

            // Log
            await RenderGuildAsync(writer, _guild);
        }
    }
}
