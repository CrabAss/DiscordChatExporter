# DiscordChatExporter

DiscordChatExporter can be used to export message history from a [Discord](https://discordapp.com) channel to a file. It works for both direct message chats and guild chats, supports markdown, message grouping, embeds, attachments, mentions, reactions and other features. It works with both user and bot tokens, supports multiple output formats and allows you to trim messages by dates.

This is a modified version of [Tyrrrz's DiscordChatExporter](https://github.com/Tyrrrz/DiscordChatExporter) by CrabAss for some data analysis project. This modified version only focuses on CSV format and is able to export more kinds of data/metadata like guild members, roles, channels and guilds.

## Download

- [Stable releases](https://github.com/CrabAss/DiscordChatExporter/releases)

## Features

- Command line interface
- Supports both user tokens and bot tokens
- Allows retrieving messages in specified date range
- Renders all message features including: markdown, attachments, embeds, emojis, mentions, etc

## Libraries used

- [Stylet](https://github.com/canton7/Stylet)
- [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Newtonsoft.Json](http://www.newtonsoft.com/json)
- [Scriban](https://github.com/lunet-io/scriban)
- [CommandLineParser](https://github.com/commandlineparser/commandline)
- [Ookii.Dialogs](https://github.com/caioproiete/ookii-dialogs-wpf)
- [Failsafe](https://github.com/Tyrrrz/Failsafe)
- [Gress](https://github.com/Tyrrrz/Gress)
- [Onova](https://github.com/Tyrrrz/Onova)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
- [Tyrrrz.Settings](https://github.com/Tyrrrz/Settings)
- [AWSSDK.Core](https://github.com/aws/aws-sdk-net)
- [AWSSDK.S3](https://github.com/aws/aws-sdk-net)

## Donate

You may consider donating to the original author [Tyrrrz](https://github.com/Tyrrrz) on [Patreon](https://patreon.com/tyrrrz) or [BuyMeACoffee](https://buymeacoffee.com/tyrrrz) if you like.