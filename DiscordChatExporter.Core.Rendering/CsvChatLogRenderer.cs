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
        private CsvChatLogRenderUtil renderUtil;

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