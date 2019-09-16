using System.IO;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Rendering
{
    public interface IChatRenderer
    {
        Task RenderAsync(StreamWriter writer);
    }
}