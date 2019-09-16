using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvChatLogRenderer : IChatRenderer
    {
        private readonly ChatLog _chatLog;
        private readonly string _dateFormat;
        private CsvChatLogRenderUtil renderUtil;

        public CsvChatLogRenderer(ChatLog chatLog, string dateFormat)
        {
            _chatLog = chatLog;
            _dateFormat = dateFormat;
            renderUtil = new CsvChatLogRenderUtil(chatLog, _dateFormat);
        }


        public async Task RenderAsync(StreamWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("MessageID;ChannelID;AuthorID;Date;Content;Attachments;Reactions;");

            // Log
            foreach (Message message in _chatLog.Messages)
                await renderUtil.RenderMessageAsync(writer, message);
        }
    }
}