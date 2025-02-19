﻿using DiscordChatExporter.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvGuildMembersRenderer
    {

        private readonly IReadOnlyList<GuildMember> _guildMembers;
        private readonly string _dateFormat;

        public CsvGuildMembersRenderer(IReadOnlyList<GuildMember> guildMembers, string dateFormat)
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

        private async Task RenderBoolAsync(TextWriter writer, bool value) => await writer.WriteAsync($"{value.ToString().ToLower()};");

        private async Task RenderRolesAsync(TextWriter writer, IReadOnlyList<string> roles)
        {
            await writer.WriteAsync($"[{roles.Select(a => $"\"{a}\"").JoinToString(", ")}];");
        }

        private async Task RenderGuildMemberAsync(TextWriter writer, GuildMember guildMember)
        {
            // User ID
            await RenderFieldAsync(writer, guildMember.User.Id);

            // Full Username
            await RenderFieldAsync(writer, guildMember.User.FullName);

            // User avatar URL
            await RenderFieldAsync(writer, guildMember.User.AvatarUrl);

            // User is bot?
            await RenderBoolAsync(writer, guildMember.User.IsBot);

            // User Nickname in guild
            await RenderFieldAsync(writer, guildMember.Nickname ?? "");

            // when did the user join this guild
            await RenderFieldAsync(writer, FormatDate(guildMember.JoinedAt));

            // premium since
            await RenderFieldAsync(writer, guildMember.PremiumSince == null ? "" : FormatDate(guildMember.PremiumSince));

            // User Roles
            await RenderRolesAsync(writer, guildMember.Roles);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;FullUsername;AvatarURL;IsBot;NicknameInGuild;JoinedAt;PremiumSince;Roles;");

            // Log
            foreach (GuildMember guildMember in _guildMembers)
                await RenderGuildMemberAsync(writer, guildMember);
        }
    }
}
