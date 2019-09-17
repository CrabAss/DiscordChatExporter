using DiscordChatExporter.Core.Models;
using Stylet;
using System.Collections.Generic;

namespace DiscordChatExporter.Gui.ViewModels.Components
{
    public partial class GuildViewModel : PropertyChangedBase
    {
        public Guild Model { get; set; }

        public IReadOnlyList<ChannelViewModel> Channels { get; set; }
    }

    public partial class GuildViewModel
    {
        public static implicit operator Guild(GuildViewModel viewModel) => viewModel.Model;
    }
}