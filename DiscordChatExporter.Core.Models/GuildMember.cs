using System;
using System.Collections.Generic;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // ...

    public partial class GuildMember
    {
        public User User { get; }

        public string Nickname { get; }

        public IReadOnlyList<Role> Roles { get; }

        public DateTimeOffset JoinedAt { get; }

        public DateTimeOffset? PremiumSince { get; }

        public bool IsDeaf { get; }

        public bool IsMute { get; }

        public GuildMember(User user, string nickname, IReadOnlyList<Role> roles, DateTimeOffset joinedAt,
            DateTimeOffset? premiumSince, bool isDeaf, bool isMute)
        {
            User = user;
            Nickname = nickname;
            Roles = roles;
            JoinedAt = joinedAt;
            PremiumSince = premiumSince;
            IsDeaf = isDeaf;
            IsMute = isMute;

            // ...
        }

        public override string ToString() => User.Id;
    }

    public partial class GuildMember
    {
        // ...
    }
}