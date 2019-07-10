using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Models
{
    public partial class PermissionOverwrite
    {
        public string Id { get; }

        public PermissionOverwriteType Type { get; }

        public Permissions Allow { get; }

        public Permissions Deny { get; }

        public PermissionOverwrite(string id, string type, ulong allowBitSet, ulong denyBitSet)
        {
            Id = id;
            if (type == "role")
                Type = PermissionOverwriteType.Role;
            else
                Type = PermissionOverwriteType.User;
            Allow = new Permissions(allowBitSet);
            Deny = new Permissions(denyBitSet);
        }

        public override string ToString()
        {
            return $"{{\"{Id}\", \"{Enum.GetName(typeof(PermissionOverwriteType), Type)}\", \"{Allow.ToString()}\", \"{Deny.ToString()}\"}}";
        }
    }
}
