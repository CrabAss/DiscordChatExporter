using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Nodes;
using DiscordChatExporter.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering
{
    class CsvChatLogRenderUtil
    {
        private readonly ChatLog _chatLog;
        private readonly string _dateFormat;

        public CsvChatLogRenderUtil(ChatLog chatLog, string dateFormat)
        {
            _chatLog = chatLog;
            _dateFormat = dateFormat;
        }

        private string FormatDate(DateTimeOffset date) =>
            date.ToLocalTime().ToString(_dateFormat, CultureInfo.InvariantCulture);

        private string FormatMarkdown(Node node)
        {
            // Formatted node
            if (node is FormattedNode formattedNode)
            {
                // Recursively get inner text
                string innerText = FormatMarkdown(formattedNode.Children);

                return $"{formattedNode.Token}{innerText}{formattedNode.Token}";
            }

            // Non-meta mention node
            if (node is MentionNode mentionNode && mentionNode.Type != MentionType.Meta)
            {
                // User mention node
                if (mentionNode.Type == MentionType.User)
                {
                    User user = _chatLog.Mentionables.GetUser(mentionNode.Id);
                    return $"@{user.Name}";
                }

                // Channel mention node
                if (mentionNode.Type == MentionType.Channel)
                {
                    Channel channel = _chatLog.Mentionables.GetChannel(mentionNode.Id);
                    return $"#{channel.Name}";
                }

                // Role mention node
                if (mentionNode.Type == MentionType.Role)
                {
                    Role role = _chatLog.Mentionables.GetRole(mentionNode.Id);
                    return $"@{role.Name}";
                }
            }

            // Custom emoji node
            if (node is EmojiNode emojiNode && emojiNode.IsCustomEmoji)
            {
                return $":{emojiNode.Name}:";
            }

            // All other nodes - simply return source
            return node.Source;
        }

        private string FormatMarkdown(IEnumerable<Node> nodes) => nodes.Select(FormatMarkdown).JoinToString("");

        private string FormatMarkdown(string markdown) => FormatMarkdown(MarkdownParser.Parse(markdown));

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            string encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderStringListAsync(TextWriter writer, string value)
        {
            await writer.WriteAsync($"[{value}];");
        }

        private async Task RenderReactionsAsync(TextWriter writer, IReadOnlyList<Reaction> reactions)
        {
            string value = reactions.Select(r => $"\"{r.Emoji.Name}\": {r.Count}").JoinToString(", ");
            await writer.WriteAsync($"{{{value}}};");
        }

        public async Task RenderMessageAsync(TextWriter writer, Message message)
        {
            // Message ID
            await RenderFieldAsync(writer, message.Id);

            // Channel ID
            await RenderFieldAsync(writer, message.ChannelId);

            // Author ID
            await RenderFieldAsync(writer, message.Author.Id);

            // Timestamp
            await RenderFieldAsync(writer, FormatDate(message.Timestamp));

            // Content
            await RenderFieldAsync(writer, FormatMarkdown(message.Content));

            // Attachments
            string formattedAttachments = message.Attachments.Select(a => $"\"{a.Url}\"").JoinToString(", ");
            await RenderStringListAsync(writer, formattedAttachments);

            // Reactions
            await RenderReactionsAsync(writer, message.Reactions);

            

            // Line break
            await writer.WriteLineAsync();
        }

    }
}
