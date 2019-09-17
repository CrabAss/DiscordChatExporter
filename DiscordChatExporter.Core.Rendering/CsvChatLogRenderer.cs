using DiscordChatExporter.Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvChatLogRenderer : IChatRenderer
    {
        private readonly ChatLog _chatLog;
        private readonly string _dateFormat;
        private readonly CsvChatLogRenderUtil renderUtil;

        public CsvChatLogRenderer(ChatLog chatLog, string dateFormat)
        {
            _chatLog = chatLog;
            _dateFormat = dateFormat;
            renderUtil = new CsvChatLogRenderUtil(chatLog, _dateFormat);
        }


        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("MessageID;ChannelID;AuthorID;Date;Content;Attachments;Reactions;");

            // Log
            foreach (Message message in _chatLog.Messages)
                await renderUtil.RenderMessageAsync(writer, message);
        }
    }
}