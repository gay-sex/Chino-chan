using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.WebServer
{
    public enum ProcessType
    {
        ServerRequest = 0x01,
        ClientRequest = 0x02,
        Unknown
    }
    public enum MessageType
    {
        Discord = 0x01,
        Clients = 0x02,
        Auth = 0x03,
        Unknown
    }

    public enum AuthType
    {
        RequestUserId = 0x01,
        UserIdReceived = 0x02,
        RequestPassword = 0x03,
        PasswordReceived = 0x04,
        Accepted = 0x05,
        Declined = 0x06,
        Unknown
    }
    public enum ClientType
    {
        GetClients = 0x01,
        SendAll = 0x02,
        SendSpecific = 0x03
    }
    public enum DiscordType
    {
        SendDm = 0x01,
        SendChannel = 0x02,
        GetUser = 0x03,
        GetChannel = 0x04,
        GetGuild = 0x05,
        GetGuilds = 0x06,
        GetGuildsNames = 0x07,
        GetRoles = 0x08,
        CreateChannel = 0x09,
        GetDmChannel = 0x10
    }
    public enum DiscordChannelType
    {
        Private = 0x01,
        GuildChannel = 0x02
    }
    public enum DiscordMessageType
    {
        Simple = 0x01,
        Embed = 0x02
    }

    public class ReceivedMessageWarning
    {
        public string Message { get; private set; }

        public ReceivedMessageWarning(string Message)
        {
            this.Message = Message;
        }
    }
    public class DiscordInformationRequest
    {
        public DiscordType Type { get; private set; }
        public ulong Id { get; private set; }

        public DiscordInformationRequest(DiscordType Type, ulong Id)
        {
            this.Type = Type;
            this.Id = Id;
        }
    }
    public class DiscordMessageRequest
    {
        public DiscordChannelType ChannelType { get; private set; }
        public DiscordMessageType MessageType { get; private set; }
        public ulong Id { get; private set; }
        public string Content { get; private set; }

        public DiscordMessageRequest(DiscordChannelType ChannelType, DiscordMessageType MessageType, ulong Id, string Content)
        {
            this.ChannelType = ChannelType;
            this.MessageType = MessageType;
            this.Id = Id;
            this.Content = Content;
        }

        public Embed ParseEmbed()
        {
            var Builder = new EmbedBuilder();
            // TODO
            return Builder.Build();
        }
    }

    public class ReceivedMessage
    {
        public byte[] RawData { get; private set; }
        public MessageType Type { get; private set; }

        public ReceivedMessage(byte[] Data)
        {
            Type = (MessageType)Data[0];
            Data.CopyTo(RawData, 1);
        }

        public object Convert()
        {
            if (Type == MessageType.Auth)
            {
                var ParsedType = (AuthType)RawData[0];
                if (ParsedType == AuthType.UserIdReceived || ParsedType == AuthType.PasswordReceived)
                {
                    return Encoding.UTF8.GetString(RawData, 1, RawData.Length - 1);
                }
                else
                {
                    return new ReceivedMessageWarning($"Unknown auth type Received: { ParsedType }!");
                }
            }
            else if (Type == MessageType.Clients)
            {
                return (ClientType)RawData[0];
            }
            else if (Type == MessageType.Discord)
            {
                var ParsedType = (DiscordType)RawData[0];
                switch (ParsedType)
                {
                    case DiscordType.GetChannel:
                    case DiscordType.GetDmChannel:
                    case DiscordType.GetGuild:
                    case DiscordType.GetRoles:
                    case DiscordType.GetUser:
                        return new DiscordInformationRequest(ParsedType, BitConverter.ToUInt64(RawData, 1));
                    case DiscordType.SendChannel:
                    case DiscordType.SendDm:
                        var ChannelType = (DiscordChannelType)RawData[1];
                        var MessageType = (DiscordMessageType)RawData[2];
                        var ChannelId = BitConverter.ToUInt64(RawData, 3);
                        var Content = Encoding.UTF8.GetString(RawData, 11, RawData.Length - 11);
                        return new DiscordMessageRequest(ChannelType, MessageType, ChannelId, Content);
                }
            }

            return new ReceivedMessageWarning($"Unknown Type: { Type }!");
        }
    }
    public static class CommunicationHelper
    {
        public static string GetString(byte[] Bytes)
        {
            return GetString(Bytes, 0, Bytes.Length);
        }
        public static string GetString(byte[] Bytes, int StartIndex, int Length)
        {
            return Encoding.UTF8.GetString(Bytes, StartIndex, Length);
        }

        public static ulong GetUInt32(byte[] Bytes)
        {
            return GetUInt32(Bytes, 0);
        }
        public static ulong GetUInt32(byte[] Bytes, int Index)
        {
            return BitConverter.ToUInt64(Bytes, Index);
        }

        public static byte[] ConcatEnums(params Enum[] Enums)
        {
            var Bytes = new byte[Enums.Length];

            if (Enums.Length == 0)
                return Bytes;

            for (int i = 0; i < Enums.Length; i++)
            {
                var CurrentEnum = Enums[i];
                
                Bytes[i] = (byte)Convert.ChangeType(CurrentEnum, Enums[i].GetTypeCode());
            }

            return Bytes;
        }
    }
}
