using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Nodes;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvChatLogRenderer : IChatRenderer
    {
        private readonly ChatLog _chatLog;
        private readonly string _dateFormat;

        public CsvChatLogRenderer(ChatLog chatLog, string dateFormat)
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

        private async Task RenderMessageAsync(TextWriter writer, Message message)
        {
            // Message ID
            await RenderFieldAsync(writer, message.Id);

            // Author ID
            await RenderFieldAsync(writer, message.Author.Id);

            // Author Name
            await RenderFieldAsync(writer, message.Author.FullName);

            // Timestamp
            await RenderFieldAsync(writer, FormatDate(message.Timestamp));

            // Content
            await RenderFieldAsync(writer, FormatMarkdown(message.Content));

            // Attachments
            string formattedAttachments = message.Attachments.Select(a => a.Url).JoinToString(",");
            await RenderFieldAsync(writer, formattedAttachments);

            // Reactions
            string formattedReactions = message.Reactions.Select(r => r.Emoji.Name + $"({r.Count})").JoinToString(",");
            await RenderFieldAsync(writer, formattedReactions);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("MessageID;AuthorID;AuthorName;Date;Content;Attachments;Reactions;");

            // Log
            foreach (Message message in _chatLog.Messages)
                await RenderMessageAsync(writer, message);
        }
    }
}