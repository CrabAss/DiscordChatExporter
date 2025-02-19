﻿using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/guild#guild-object

    public partial class Guild
    {
        public string Id { get; }

        public string Name { get; }

        public string IconHash { get; }

        public string IconUrl { get; }

        public string OwnerId { get; }

        public uint? VerificationLevel { get; }

        public string Description { get; }

        public Guild(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guild(string id, string name, string iconHash, string ownerId, uint? verificationLevel, string description)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;
            OwnerId = ownerId;
            VerificationLevel = verificationLevel;
            Description = description ?? "";

            IconUrl = GetIconUrl(id, iconHash);
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static string GetIconUrl(string id, string iconHash)
        {
            return !iconHash.IsNullOrWhiteSpace()
                ? $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png"
                : "https://cdn.discordapp.com/embed/avatars/0.png";
        }

        public static Guild DirectMessages { get; } = new Guild("@me", "Direct Messages");
    }
}