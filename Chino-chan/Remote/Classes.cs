using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Remote
{
    public enum MessageType
    {
        Auth = 0x00,
        Ping = 0x01,
        ServerMessage = 0x02,
        RemoteUserGlobalMessage = 0x03,
        RemoteUserDirectMessage = 0x04,
        DiscordGuildMessage = 0x05,
        DiscordDirectMessage = 0x06,
        DiscordChannelUpdate = 0x07,
        DiscordChannelDelete = 0x08,
        DiscordGuildDisconnect = 0x09,
        DiscordRoleDelete = 0x10,
        DiscordRoleCreate = 0x11,
        DiscordGuildConnect = 0x12,
        DiscordChannelCreate = 0x13,
        DiscordMessageEdit = 0x14,
        DiscordRoleUpdate = 0x15,
        DiscordDeleteMessage = 0x16,
        DiscordUpdateMessage = 0x17,
        DiscordDeleteChannel = 0x18,
        DiscordUpdateChannel = 0x19,
        DiscordSendMessage = 0x20,
        Credentials = 0x21
    }
    public enum UserType
    {
        Administrator = 0x00,
        GlobalAdministrator = 0x01,
        Owner = 0x02
    }
    public enum DiscordStatus
    {
        Online,
        DND,
        AFK,
        Offline
    }

    public class AuthResponse
    {
        public bool IsId { get; set; }
        public ulong UserId { get; set; }
        public string Password { get; set; }
    }
    
    public class DiscordGuild
    {
        public ulong Id { get; set; }
        public ulong Owner { get; set; }

        public string Name { get; set; }
        public string Icon { get; set; }
        public string Region { get; set; }

        public List<DiscordTextChannel> TextChannels { get; set; }
        public List<DiscordVoiceChannel> VoiceChannels { get; set; }

        public List<DiscordRole> Roles { get; set; }
        public List<DiscordUser> Users { get; set; }

        public List<DiscordEmotes> Emotes { get; set; }
    }
    public class DiscordTextChannel
    {
        public string Name { get; set; }
        public List<DiscordChannelPermission> Permissions { get; set; }
        public List<DiscordGuildMessage> Messages { get; set; }
    }
    public class DiscordVoiceChannel
    {
        public string Name { get; set; }
        public List<DiscordChannelPermission> Permissions { get; set; }
    }
    public class DiscordUser
    {
        public ulong Id { get; set; }

        public string Username { get; set; }
        public string Nickname { get; set; }

        public List<DiscordRole> Roles { get; set; }

        public string AvatarUrl { get; set; }

        public DiscordStatus Status { get; set; }
        public string Game { get; set; }
    }
    public class DiscordRole
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public int Color { get; set; }

        public ulong RawPermission { get; set; }
    }
    public class DiscordChannelPermission
    {
        public bool IsRole { get; set; }
        public ulong RoleId { get; set; }
        public ulong UserId { get; set; }

        public ulong RawPermission { get; set; }
    }
    public class DiscordEmotes
    {
        public string Name { get; set; }
        public ulong Id { get; set; }

        public bool Animated { get; set; }
    }

    public class ServerMessage
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }
    public class RemoteUserMessage
    {
        public string User { get; set; }
        public UserType UserType { get; set; }

        public string Message { get; set; }

        public DateTime Time { get; set; }
    }
    public class DiscordGuildMessage
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong From { get; set; }

        public bool Edited { get; set; }

        public DateTime Time { get; set; }

        public string Message { get; set; }
    }
    public class DiscordDirectMessage
    {
        public ulong From { get; set; }
        public ulong MessageId { get; set; }

        public bool Edited { get; set; }

        public DateTime Time { get; set; }

        public string Message { get; set; }
    }
}
