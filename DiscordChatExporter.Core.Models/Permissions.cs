using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Models
{
    public partial class Permissions
    {
        public HashSet<PermissionType> PermissionSet { get; }
        public ulong RawBitSet { get; }

        public Permissions(ulong permissionBitSet)
        {
            RawBitSet = permissionBitSet;
            PermissionSet = ParsePermissionInt(RawBitSet);
        }

        public override string ToString()
        {
            return string.Join(", ", ToStringArray());
        }
    }

    public partial class Permissions
    {
        public string[] ToStringArray()
        {
            return PermissionSet
                .Select(x => Enum.GetName(typeof(PermissionType), x))
                .OrderBy(x => x)
                .ToArray();
        }

        public static HashSet<PermissionType> ParsePermissionInt(ulong permissions)
        {
            HashSet<PermissionType> permissionSet = new HashSet<PermissionType>();
            foreach (PermissionType permissionType in Enum.GetValues(typeof(PermissionType)))
            {
                ulong permissionTypeValue = Convert.ToUInt64(permissionType);
                if ((permissions & permissionTypeValue) == permissionTypeValue)
                    permissionSet.Add(permissionType);
            }
            return permissionSet;
        }
    }
}
