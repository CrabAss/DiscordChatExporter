using System;
using System.Drawing;
using System.Linq;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services.Internal;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService
    {
        private User ParseUser(JToken json)
        {
            string id = json["id"].Value<string>();
            int discriminator = json["discriminator"].Value<int>();
            string name = json["username"].Value<string>();
            string avatarHash = json["avatar"].Value<string>();
            bool isBot = json["bot"]?.Value<bool>() ?? false;

            return new User(id, discriminator, name, avatarHash, isBot);
        }

        private Guild ParseGuild(JToken json)
        {
            string id = json["id"].Value<string>();
            string name = json["name"].Value<string>();
            string iconHash = json["icon"].Value<string>();

            return new Guild(id, name, iconHash);
        }

        private Channel ParseChannel(JToken json)
        {
            // Get basic data
            string id = json["id"].Value<string>();
            string parentId = json["parent_id"]?.Value<string>();
            ChannelType type = (ChannelType) json["type"].Value<int>();
            string topic = json["topic"]?.Value<string>();

            // Try to extract guild ID
            string guildId = json["guild_id"]?.Value<string>();

            // If the guild ID is blank, it's direct messages
            if (guildId.IsNullOrWhiteSpace())
                guildId = Guild.DirectMessages.Id;

            // Try to extract name
            string name = json["name"]?.Value<string>();

            // If the name is blank, it's direct messages
            if (name.IsNullOrWhiteSpace())
                name = json["recipients"].Select(ParseUser).Select(u => u.Name).JoinToString(", ");

            return new Channel(id, parentId, guildId, name, topic, type);
        }

        private Role ParseRole(JToken json)
        {
            string id = json["id"].Value<string>();
            string name = json["name"].Value<string>();

            return new Role(id, name);
        }

        private Attachment ParseAttachment(JToken json)
        {
            string id = json["id"].Value<string>();
            string url = json["url"].Value<string>();
            int? width = json["width"]?.Value<int>();
            int? height = json["height"]?.Value<int>();
            string fileName = json["filename"].Value<string>();
            long fileSizeBytes = json["size"].Value<long>();

            FileSize fileSize = new FileSize(fileSizeBytes);

            return new Attachment(id, width, height, url, fileName, fileSize);
        }

        private EmbedAuthor ParseEmbedAuthor(JToken json)
        {
            string name = json["name"]?.Value<string>();
            string url = json["url"]?.Value<string>();
            string iconUrl = json["icon_url"]?.Value<string>();

            return new EmbedAuthor(name, url, iconUrl);
        }

        private EmbedField ParseEmbedField(JToken json)
        {
            string name = json["name"].Value<string>();
            string value = json["value"].Value<string>();
            bool isInline = json["inline"]?.Value<bool>() ?? false;

            return new EmbedField(name, value, isInline);
        }

        private EmbedImage ParseEmbedImage(JToken json)
        {
            string url = json["url"]?.Value<string>();
            int? width = json["width"]?.Value<int>();
            int? height = json["height"]?.Value<int>();

            return new EmbedImage(url, width, height);
        }

        private EmbedFooter ParseEmbedFooter(JToken json)
        {
            string text = json["text"].Value<string>();
            string iconUrl = json["icon_url"]?.Value<string>();

            return new EmbedFooter(text, iconUrl);
        }

        private Embed ParseEmbed(JToken json)
        {
            // Get basic data
            string title = json["title"]?.Value<string>();
            string description = json["description"]?.Value<string>();
            string url = json["url"]?.Value<string>();
            DateTimeOffset? timestamp = json["timestamp"]?.Value<DateTime>().ToDateTimeOffset();

            // Get color
            Color color = json["color"] != null
                ? Color.FromArgb(json["color"].Value<int>()).ResetAlpha()
                : Color.FromArgb(79, 84, 92); // default color

            // Get author
            EmbedAuthor author = json["author"] != null ? ParseEmbedAuthor(json["author"]) : null;

            // Get fields
            EmbedField[] fields = json["fields"].EmptyIfNull().Select(ParseEmbedField).ToArray();

            // Get thumbnail
            EmbedImage thumbnail = json["thumbnail"] != null ? ParseEmbedImage(json["thumbnail"]) : null;

            // Get image
            EmbedImage image = json["image"] != null ? ParseEmbedImage(json["image"]) : null;

            // Get footer
            EmbedFooter footer = json["footer"] != null ? ParseEmbedFooter(json["footer"]) : null;

            return new Embed(title, url, timestamp, color, author, description, fields, thumbnail, image, footer);
        }

        private Emoji ParseEmoji(JToken json)
        {
            string id = json["id"]?.Value<string>();
            string name = json["name"]?.Value<string>();
            bool isAnimated = json["animated"]?.Value<bool>() ?? false;

            return new Emoji(id, name, isAnimated);
        }

        private Reaction ParseReaction(JToken json)
        {
            int count = json["count"].Value<int>();
            Emoji emoji = ParseEmoji(json["emoji"]);

            return new Reaction(count, emoji);
        }

        private Message ParseMessage(JToken json)
        {
            // Get basic data
            string id = json["id"].Value<string>();
            string channelId = json["channel_id"].Value<string>();
            DateTimeOffset timestamp = json["timestamp"].Value<DateTime>().ToDateTimeOffset();
            DateTimeOffset? editedTimestamp = json["edited_timestamp"]?.Value<DateTime?>()?.ToDateTimeOffset();
            string content = json["content"].Value<string>();
            MessageType type = (MessageType) json["type"].Value<int>();

            // Workarounds for non-default types
            if (type == MessageType.RecipientAdd)
                content = "Added a recipient.";
            else if (type == MessageType.RecipientRemove)
                content = "Removed a recipient.";
            else if (type == MessageType.Call)
                content = "Started a call.";
            else if (type == MessageType.ChannelNameChange)
                content = "Changed the channel name.";
            else if (type == MessageType.ChannelIconChange)
                content = "Changed the channel icon.";
            else if (type == MessageType.ChannelPinnedMessage)
                content = "Pinned a message.";
            else if (type == MessageType.GuildMemberJoin)
                content = "Joined the server.";

            // Get author
            User author = ParseUser(json["author"]);

            // Get attachments
            Attachment[] attachments = json["attachments"].EmptyIfNull().Select(ParseAttachment).ToArray();

            // Get embeds
            Embed[] embeds = json["embeds"].EmptyIfNull().Select(ParseEmbed).ToArray();

            // Get reactions
            Reaction[] reactions = json["reactions"].EmptyIfNull().Select(ParseReaction).ToArray();

            // Get mentioned users
            User[] mentionedUsers = json["mentions"].EmptyIfNull().Select(ParseUser).ToArray();

            return new Message(id, channelId, type, author, timestamp, editedTimestamp, content, attachments, embeds,
                reactions, mentionedUsers);
        }

        private GuildMember ParseGuildMember(JToken json)
        {
            // https://discordapp.com/developers/docs/resources/guild#guild-member-object

            User user = ParseUser(json["user"]);
            string nick = json["nick"]?.Value<string>();
            string[] roles = json["roles"].ToObject<string[]>();
            DateTimeOffset joinedAt = json["joined_at"].Value<DateTime>().ToDateTimeOffset();
            DateTimeOffset? premiumSince = json["premium_since"]?.Value<DateTime?>()?.ToDateTimeOffset();
            bool isDeaf = json["deaf"]?.Value<bool>() ?? false;
            bool isMute = json["mute"]?.Value<bool>() ?? false;

            return new GuildMember(user, nick, roles, joinedAt, premiumSince, isDeaf, isMute);
        }
    }
}