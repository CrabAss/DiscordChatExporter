using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services.Exceptions;
using DiscordChatExporter.Core.Services.Internal;
using Failsafe;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private async Task<JToken> GetApiResponseAsync(AuthToken token, string resource, string endpoint,
            params string[] parameters)
        {
            // Create retry policy
            IRetry retry = Retry.Create()
                .Catch<HttpErrorStatusCodeException>(false, e => (int)e.StatusCode >= 500)
                .Catch<HttpErrorStatusCodeException>(false, e => (int)e.StatusCode == 429)
                .WithMaxTryCount(10)
                .WithDelay(TimeSpan.FromSeconds(0.4));

            // Send request
            return await retry.ExecuteAsync(async () =>
            {
                // Create request
                const string apiRoot = "https://discordapp.com/api/v6";
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{apiRoot}/{resource}/{endpoint}"))
                {
                    // Set authorization header
                    request.Headers.Authorization = token.Type == AuthTokenType.Bot
                        ? new AuthenticationHeaderValue("Bot", token.Value)
                        : new AuthenticationHeaderValue(token.Value);

                    // Add parameters
                    foreach (string parameter in parameters)
                    {
                        string key = parameter.SubstringUntil("=");
                        string value = parameter.SubstringAfter("=");

                        // Skip empty values
                        if (value.IsNullOrWhiteSpace())
                            continue;

                        request.RequestUri = request.RequestUri.SetQueryParameter(key, value);
                    }

                    // Get response
                    using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                    {
                        // Check status code
                        // We throw our own exception here because default one doesn't have status code
                        if (!response.IsSuccessStatusCode)
                            throw new HttpErrorStatusCodeException(response.StatusCode, response.ReasonPhrase);

                        // Get content
                        string raw = await response.Content.ReadAsStringAsync();

                        // Parse
                        return JToken.Parse(raw);
                    }
                }
            });
        }

        public async Task<Guild> GetGuildAsync(AuthToken token, string guildId)
        {
            // Special case for direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Guild.DirectMessages;

            JToken response = await GetApiResponseAsync(token, "guilds", guildId);
            Guild guild = ParseGuild(response);

            return guild;
        }

        public async Task<IReadOnlyList<GuildMember>> GetGuildMembersAsync(AuthToken token, string guildId)
        {
            // Special case for direct messages pseudo-guild ...

            List<GuildMember> result = new List<GuildMember>();

            string offsetId = "0";
            while (true)
            {
                // Get message batch
                JToken response = await GetApiResponseAsync(token, "guilds", $"{guildId}/members",
                    "limit=1000", $"after={offsetId}");

                // Parse
                GuildMember[] guildMembers = response
                    .Select(ParseGuildMember)
                    .ToArray();

                // Break if there are no messages (can happen if messages are deleted during execution)
                if (!guildMembers.Any())
                    break;

                // Add to result
                result.AddRange(guildMembers);

                // Break if messages were trimmed (which means the last message was encountered)
                if (guildMembers.Length != 1000)
                    break;

                // Move offset
                offsetId = result.Last().User.Id;
            }

            return result;

        }

        public async Task<Channel> GetChannelAsync(AuthToken token, string channelId)
        {
            JToken response = await GetApiResponseAsync(token, "channels", channelId);
            Channel channel = ParseChannel(response);

            return channel;
        }

        public async Task<IReadOnlyList<Guild>> GetUserGuildsAsync(AuthToken token)
        {
            JToken response = await GetApiResponseAsync(token, "users", "@me/guilds");
            Guild[] guilds = response.Select(ParseGuild).ToArray();

            return guilds;
        }

        public async Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(AuthToken token)
        {
            JToken response = await GetApiResponseAsync(token, "users", "@me/channels");
            Channel[] channels = response.Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(AuthToken token, string guildId)
        {
            JToken response = await GetApiResponseAsync(token, "guilds", $"{guildId}/channels");
            Channel[] channels = response.Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Role>> GetGuildRolesAsync(AuthToken token, string guildId)
        {
            JToken response = await GetApiResponseAsync(token, "guilds", $"{guildId}/roles");
            Role[] roles = response.Select(ParseRole).ToArray();

            return roles;
        }

        public async Task<IReadOnlyList<Message>> GetChannelMessagesAsync(AuthToken token, string channelId,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double> progress = null)
        {
            List<Message> result = new List<Message>();

            // Get the last message
            JToken response = await GetApiResponseAsync(token, "channels", $"{channelId}/messages",
                "limit=1", $"before={before?.ToSnowflake()}");
            Message lastMessage = response.Select(ParseMessage).FirstOrDefault();

            // If the last message doesn't exist or it's outside of range - return
            if (lastMessage == null || lastMessage.Timestamp < after)
            {
                progress?.Report(1);
                return result;
            }

            // Get other messages
            string offsetId = after?.ToSnowflake() ?? "0";
            while (true)
            {
                // Get message batch
                response = await GetApiResponseAsync(token, "channels", $"{channelId}/messages",
                    "limit=100", $"after={offsetId}");

                // Parse
                Message[] messages = response
                    .Select(ParseMessage)
                    .Reverse() // reverse because messages appear newest first
                    .ToArray();

                // Break if there are no messages (can happen if messages are deleted during execution)
                if (!messages.Any())
                    break;

                // Trim messages to range (until last message)
                Message[] messagesInRange = messages
                    .TakeWhile(m => m.Id != lastMessage.Id && m.Timestamp < lastMessage.Timestamp)
                    .ToArray();

                // Add to result
                result.AddRange(messagesInRange);

                // Break if messages were trimmed (which means the last message was encountered)
                if (messagesInRange.Length != messages.Length)
                    break;

                // Report progress (based on the time range of parsed messages compared to total)
                progress?.Report((result.Last().Timestamp - result.First().Timestamp).TotalSeconds /
                                 (lastMessage.Timestamp - result.First().Timestamp).TotalSeconds);

                // Move offset
                offsetId = result.Last().Id;
            }

            // Add last message
            result.Add(lastMessage);
            //result.RemoveAll(x => x == null);   // remove null messages (BUG?)

            // Report progress
            progress?.Report(1);

            return result;
        }

        public async Task<Mentionables> GetMentionablesAsync(AuthToken token, string guildId,
            IEnumerable<Message> messages)
        {
            // Get channels and roles
            IReadOnlyList<Channel> channels = guildId != Guild.DirectMessages.Id
                ? await GetGuildChannelsAsync(token, guildId)
                : Array.Empty<Channel>();
            IReadOnlyList<Role> roles = guildId != Guild.DirectMessages.Id
                ? await GetGuildRolesAsync(token, guildId)
                : Array.Empty<Role>();

            // Get users
            Dictionary<string, User> userMap = new Dictionary<string, User>();
            foreach (Message message in messages)
            {
                // Author
                userMap[message.Author.Id] = message.Author;

                // Mentioned users
                foreach (User mentionedUser in message.MentionedUsers)
                    userMap[mentionedUser.Id] = mentionedUser;
            }

            User[] users = userMap.Values.ToArray();

            return new Mentionables(users, channels, roles);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, Guild guild, Channel channel,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double> progress = null)
        {
            // Get messages
            IReadOnlyList<Message> messages = await GetChannelMessagesAsync(token, channel.Id, after, before, progress);

            // Get mentionables
            Mentionables mentionables = await GetMentionablesAsync(token, guild.Id, messages);

            return new ChatLog(guild, channel, after, before, messages, mentionables);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, Channel channel,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double> progress = null)
        {
            // Get guild
            Guild guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : await GetGuildAsync(token, channel.GuildId);

            // Get the chat log
            return await GetChatLogAsync(token, guild, channel, after, before, progress);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, string channelId,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double> progress = null)
        {
            // Get channel
            Channel channel = await GetChannelAsync(token, channelId);

            // Get the chat log
            return await GetChatLogAsync(token, channel, after, before, progress);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}