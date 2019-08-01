using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Models
{
    public partial class PermissionOverwrite
    {
        public string EntityId { get; }

        public bool EntityIsMember { get; }

        public Permissions Allow { get; }

        public Permissions Deny { get; }

        public PermissionOverwrite(string id, string type, ulong allowBitSet, ulong denyBitSet)
        {
            EntityId = id;
            EntityIsMember = type == "member";
            Allow = new Permissions(allowBitSet);
            Deny = new Permissions(denyBitSet);
        }

        public override string ToString()
        {
            return $"{{\"EntityID\": \"{EntityId}\", \"EntityIsMember\": {EntityIsMember.ToString().ToLower()}, \"Allow\": \"{Allow.ToString()}\", \"Deny\": \"{Deny.ToString()}\"}}";
        }
    }
}
