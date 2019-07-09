using DiscordChatExporter.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private async Task RenderPermissionSetAsync(TextWriter writer, HashSet<PermissionType> permissionSet)
        {
            string[] permissions = permissionSet
                .Select(x => Enum.GetName(typeof(PermissionType), x))
                .OrderBy(x => x)
                .ToArray();
            string result = string.Join(", ", permissions);
            await RenderFieldAsync(writer, result);
        }

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderBoolAsync(TextWriter writer, bool value)
        {
            await writer.WriteAsync(value == true ? "true;" : "false;");
        }

        private async Task RenderGuildRoleAsync(TextWriter writer, Role role)
        {
            // Role ID
            await RenderFieldAsync(writer, role.Id);

            // Role Name
            await RenderFieldAsync(writer, role.Name);

            // Role Color
            await RenderFieldAsync(writer, $"#{role.Color.ToString("X6")}");

            // Role is hoisted?
            await RenderBoolAsync(writer, role.IsHoisted);

            // Role Position
            await RenderFieldAsync(writer, role.Position.ToString());

            // Role PermissionSet
            await RenderPermissionSetAsync(writer, role.PermissionSet);

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
            await writer.WriteLineAsync("ID;Name;Color;IsHoisted;Position;Permissions;IsManaged;IsMentionable;");

            // Log
            foreach (Role role in _roles)
                await RenderGuildRoleAsync(writer, role);
        }
    }
}
