using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class Role
    {
        public string Id { get; }

        public string Name { get; }

        public uint Color { get; }

        public bool IsHoisted { get; }

        public uint Position { get; }

        public Permissions PermissionSet { get; }
        
        public bool IsManaged { get; }

        public bool IsMentionable { get; }

        public Role(string id, string name, uint color, bool isHoisted, uint position, 
            ulong permissionBitSet, bool isManaged, bool isMentionable)
        {
            Id = id;
            Name = name;
            Color = color;
            IsHoisted = isHoisted;
            Position = position;
            IsManaged = isManaged;
            IsMentionable = isMentionable;

            PermissionSet = new Permissions(permissionBitSet);
        }

        public Role(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }

    public partial class Role
    {
        public static Role CreateDeletedRole(string id) => new Role(id, "deleted-role");
    }
}