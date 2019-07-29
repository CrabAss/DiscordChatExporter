using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvBundledChatLogRenderer : IChatRenderer
    {
        private readonly IReadOnlyList<ChatLog> _chatLogs;
        private readonly string _dateFormat;
        private CsvChatLogRenderUtil renderUtil;

        public CsvBundledChatLogRenderer(IReadOnlyList<ChatLog> chatLogs, string dateFormat)
        {
            _chatLogs = chatLogs;
            _dateFormat = dateFormat;
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("MessageID;ChannelID;AuthorID;Date;Content;Attachments;Reactions;");

            // Log
            foreach (ChatLog chatLog in _chatLogs)
            {
                if (chatLog == null || chatLog.Messages.Count == 0) continue;
                renderUtil = new CsvChatLogRenderUtil(chatLog, _dateFormat);
                foreach (Message message in chatLog.Messages)
                    await renderUtil.RenderMessageAsync(writer, message);
            }
        }
    }
}