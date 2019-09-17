using DiscordChatExporter.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvGuildRolesRenderer : IChatRenderer
    {

        private readonly IReadOnlyList<Role> _roles;
        private readonly string _dateFormat;

        public CsvGuildRolesRenderer(IReadOnlyList<Role> roles, string dateFormat)
        {
            _roles = roles;
            _dateFormat = dateFormat;
        }

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderBoolAsync(TextWriter writer, bool value) => await writer.WriteAsync($"{value.ToString().ToLower()};");

        private async Task RenderGuildRoleAsync(TextWriter writer, Role role)
        {
            // Role ID
            await RenderFieldAsync(writer, role.Id);

            // Role Name
            await RenderFieldAsync(writer, role.Name);

            // Role Color
            await RenderFieldAsync(writer, $"#{role.Color.ToString("X6")}");

            // Role Position
            await RenderFieldAsync(writer, role.Position.ToString());

            // Role PermissionSet
            await RenderFieldAsync(writer, role.PermissionSet.ToString());

            // Role is hoisted?
            await RenderBoolAsync(writer, role.IsHoisted);

            // Role is managed?
            await RenderBoolAsync(writer, role.IsManaged);

            // Role is mentionable?
            await RenderBoolAsync(writer, role.IsMentionable);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("ID;Name;Color;Position;Permissions;IsHoisted;IsManaged;IsMentionable;");

            // Log
            foreach (Role role in _roles)
                await RenderGuildRoleAsync(writer, role);
        }
    }
}
