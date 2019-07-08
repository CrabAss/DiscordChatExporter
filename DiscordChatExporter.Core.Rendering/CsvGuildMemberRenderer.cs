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
    public class CsvGuildMemberRenderer : IChatRenderer
    {

        private readonly IReadOnlyList<GuildMember> _guildMembers;
        private readonly string _dateFormat;

        public CsvGuildMemberRenderer(IReadOnlyList<GuildMember> guildMembers, string dateFormat)
        {
            _guildMembers = guildMembers;
            _dateFormat = dateFormat;
        }

        private string FormatDate(DateTimeOffset? date) =>
            date?.ToLocalTime().ToString(_dateFormat, CultureInfo.InvariantCulture);

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderBoolAsync(TextWriter writer, bool value)
        {
            await writer.WriteAsync(value == true ? "true;" : "false;");
        }

        private async Task RenderRolesAsync(TextWriter writer, IReadOnlyList<string> roles)
        {
            // TO BE IMPROVED

            await writer.WriteAsync("\"");
            foreach (string role in roles)
            {
                await writer.WriteAsync($"{role}, ");
            }
            await writer.WriteAsync("\";");
        }

        private async Task RenderGuildMemberAsync(TextWriter writer, GuildMember guildMember)
        {
            // User ID
            await RenderFieldAsync(writer, guildMember.User.Id);

            // User FullName
            await RenderFieldAsync(writer, guildMember.User.FullName);

            // User is bot?
            await RenderBoolAsync(writer, guildMember.User.IsBot);

            // User avatar URL
            await RenderFieldAsync(writer, guildMember.User.AvatarUrl);

            // User Nickname in guild
            await RenderFieldAsync(writer, guildMember.Nickname ?? "");

            // User Roles
            await RenderRolesAsync(writer, guildMember.Roles);

            // when did the user join this guild
            await RenderFieldAsync(writer, FormatDate(guildMember.JoinedAt));

            // premium since
            await RenderFieldAsync(writer, guildMember.PremiumSince == null ? "" : FormatDate(guildMember.PremiumSince));

            // User is deaf?
            await RenderBoolAsync(writer, guildMember.IsDeaf);

            // User is mute?
            await RenderBoolAsync(writer, guildMember.IsMute);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;FullName;IsBot;AvatarURL;NicknameInGuild;Roles;JoinedAt;PremiumSince;IsDeaf;IsMute;");

            // Log
            foreach (GuildMember guildMember in _guildMembers)
                await RenderGuildMemberAsync(writer, guildMember);
        }
    }
}
