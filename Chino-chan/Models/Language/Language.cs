using System.Collections.Generic;

namespace Chino_chan.Models.Language
{
    public class Language
    {
        #region Base information
        public string Id { get; set; } = "en";
        public string Name { get; set; } = "English";

        public string GlobalAdmin { get; set; } = "Global Admin";
        public string Admin { get; set; } = "Admin";
        public string Owner { get; set; } = "Owner";

        public string NameSafety { get; set; } = "Be careful with the nicknames and the usernames, because if there're more than one people with the same name, the found person may not be the one you want!";

        public string InfoHeader { get; set; } = "Information about me";
        public string MemoryUsageHeader { get; set; } = "Memory Usage";
        public string DiscordLibraryHeader { get; set; } = "Discord Library";
        public string UsersHeader { get; set; } = "Users";
        public string CreatorHeader { get; set; } = "Creator";
        public string UptimeHeader { get; set; } = "Uptime";
        public string JoinedServersHeader { get; set; } = "Joined servers";
        public string ServerInformationHeader { get; set; } = "Server Information";
        public string OperatingSystemHeader { get; set; } = "Operating System";
        public string GraphicCardHeader { get; set; } = "Graphic Card";
        public string ProcessorHeader { get; set; } = "Processor";
        public string NameHeader { get; set; } = "Name";
        public string Socket { get; set; } = "Socket";
        public string Description { get; set; } = "Description";
        public string Memory { get; set; } = "Memory";
        public string Speed { get; set; } = "Speed";
        public string Usage { get; set; } = "Usage";
        public string Cache { get; set; } = "Cache";
        public string Cores { get; set; } = "Cores";
        public string Threads { get; set; } = "Threads";
        public string Version { get; set; } = "Version";
        public string Architecture { get; set; } = "Architecture";
        public string Used { get; set; } = "Used";
        public string Free { get; set; } = "Free";
        public string Drives { get; set; } = "Drives";
        public string DriveDefaultName { get; set; } = "Local Disk";
        public string VideoCard { get; set; } = "Video card";
        public string Days { get; set; } = "days";
        public string Hours { get; set; } = "hours";
        public string Minutes { get; set; } = "minutes";
        public string Seconds { get; set; } = "seconds";
        public string Nsfw { get; set; } = "nsfw";
        public string Sfw { get; set; } = "sfw";
        public string On { get; set; } = "on";
        public string Off { get; set; } = "off";
        public string Uploaded { get; set; } = "Uploaded";
        public string Title { get; set; } = "Title";
        public string Length { get; set; } = "Length";
        public string Queue { get; set; } = "Queue";
        public string Url { get; set; } = "Url";
        public string Thumbnail { get; set; } = "Thumbnail";

        public string OnlyNsfw { get; set; } = "This command can only be invoked in private and nsfw channels, sorry :<";
        public string OnlyServer { get; set; } = "Sorry, but this is only a server-side command :c";

        public string Updated { get; set; } = "Successfully updated! uwu Restarting...";
        public string UpdateError { get; set; } = "Couldn't update :( Please check the console for more information";
        #endregion
        #region Debug
        public string GCDone { get; set; } = "Garbage cleaned!";
        public string LanguagesReloaded { get; set; } = "Languages have been reloaded!";
        #endregion
        #region Management
        public string ErrorMessage { get; set; } = "```css\nAn error occured! Please report this to %OWNER%!\n\nType: %TYPE%\nReason: %REASON%```";

        public string BadArgs { get; set; } = "You gave wrong paramteres! Please read %P%%COMMAND%";

        public string CheckPrivate { get; set; } = "Check our private channel %MENTION% c:";
        public string NoOwnerPermission { get; set; } = "You're not %OWNER%, you can't use this command :<";
        public string NoPermission { get; set; } = "You don't have the right to use this command, sorry :<";
        public string HelpNotDefined { get; set; } = "Sorry, but this command has no help message!";
        public string HelpFootage { get; set; } = "Type %P%help [command name] for further help!";

        public string CurrentPrefix { get; set; } = "My current prefix is `%P%` uwu";
        public string PrefixChanged { get; set; } = "My prefix has been changed to `%P%` uwu";

        public string LanguageHelp { get; set; } = "```css\nMy current language is English c:\n\nAvailable languages: %LANGS%\n```";
        public string LanguageChanged { get; set; } = "My language has been changed to `English` c:";

        public string CannotPurgeDM { get; set; } = "You can't purge DM channels!";
        public string ChannelPurged { get; set; } = "Channel purged!";

        public string HasNoAdmin { get; set; } = "I don't have any admins :c";
        public string UserNotFoundAdmin { get; set; } = "I didn't find any users with this identifier: %IDENTIFIER% :c";
        public string AlreadyAdminsTag { get; set; } = "Already Admins";
        public string NewAdminsTag { get; set; } = "New Admins";
        public string RemovedAdminsTag { get; set; } = "Removed Users";
        public string NotAdminsTag { get; set; } = "Not Admins";
        public string GlobalAdminHelp { get; set; } = "Use %P%admin add [UserId/Username/Nickname] to add an admin\nUse %P%admin remove [UserId/Username/Nickname] to remove an admin";
        public string OwnerAdminHelp { get; set; } = "Use %P%admin addglobal [UserId/Username/Nickname] to add a Global Admin\nUse %P%admin removeglobal [UserId/Username/Nickname] to remove a Global Admin";

        public string AlreadyBlocked { get; set; } = "%NAME% is already blocked :c";
        public string NewBlocked { get; set; } = "%NAME% is blocked so this person can't use any of my commands *hmpf*";
        public string NotBlocked { get; set; } = "%NAME% is not blocked *yet?*";
        public string FreeBlock { get; set; } = "%NAME% can use my commands again c:";
        public string GloballyBlocked { get; set; } = "Globally Blocked";
        public string BlockedAtGuild { get; set; } = "Blocked Here";
        public string NooneBlocked { get; set; } = "Currently no one is blocked c:";
        public string BlockHelp { get; set; } = "Use %P%block add [UserId/Username/Nickname] to add someone to the blocked list of the current guild\nUse %P%block remove [UserId/Username/Nickname] to remove someone from the blocked list of the current guild";
        public string GlobalBlockHelp { get; set; } = "Use %P%block addglobal [UserId/Username/Nickname] to add someone to the globally blocked list\nUse %P%block removeglobal [UserId/Username/Nickname] to remove someone from the globally blocked list";
        public string BlockedInfo { get; set; } = "Blocked user: %NAME% Id: %ID%\nBlocked by: %BLOCKER% Id: %BLOCKERID%\nReason: %REASON%";

        public string MessageDeleteHelp { get; set; } = "```css\n[Delete messages by id]\n%P%delete ids:[id1],[id2]\n\n[Delete x messages]\n%P%delete count:[count]\nCount has to be more than 0!\n\n[Other optional paramteres]\nnotify:[false/no/off] - turns off the response after deleting the messages (turned on by default)\n\nselfRemove:[true/yes/on] - removes the invoking message (turned off by deafult)```";
        public string MessageCountDeleted { get; set; } = "I've deleted [%COUNT%] messages!";
        public string MessageIdsDeleted { get; set; } = "I've deleted the messages which had these ids: %IDS%";
        public string MessageIdsNotDeleted { get; set; } = "I couldn't delete the messages which had these ids: %IDS%";

        public string HighlightProvideArgs { get; set; } = "```css\nHighlighting a simple message\n%P%highlight [Message Id]\n\nHighlighting last messages of a user\n%P%highlight [User Id or User name / nickname]\n\n[Only working at Guilds, also in the specific channel!]\n```";
        public string HighlightNotFound { get; set; } = "I couldn't find any messages from %USER% here, sorry :<";
        public string HighlightUserNotFound { get; set; } = "I couldn't find any users with %PROPERTY% name or nickname, sorry :<";
        public string HighlightUserOrMessageNotFound { get; set; } = "I couldn't find any users or messages with %ID% id, sorry :<";
        public string HighlightTitle { get; set; } = "Title";
        public string HighlightDescription { get; set; } = "Description";
        public string HighlightFooter { get; set; } = "Footer";
        public string HighlightEmbed { get; set; } = "Embed";

        public string DmIsNsfw { get; set; } = "You can't change the state of the direct messge channel, it'll be marked as nsfw, sorry :<";
        public string NsfwStateChanged { get; set; } = "This channel is marked as %STATE% channel~";
        public string NsfwState { get; set; } = "This channel is an %STATE% channel~";

        public string SayHelp { get; set; } = "```css\n" +
            "Crash-course for say\n\n" +
            "[With listening]\n" +
            "%P%say listen [Channel Id]\n" +
            "I start listening to your messages in the channel, and I send all the new messages there~\n" +
            "I stop listening with `%P%say listen stop`~\n\n" +
            "[With providing Id every time from another server]\n" +
            "%P%say [Channel Id] [Message]\n" +
            "I send the message to the channel you've provided~\n\n" +
            "[With just saying]\n" +
            "%P%say [Message]\n\n" +
            "[Other commands]" +
            "%P%say autodel [yes: true, yes, ya, 1 / no: false, no, nai, 0]\n" +
            "You can turn off the auto deletion of the message which is being said" +
            "If you'd like to send a message starting with an id or any with the previously set properties, you should type a \\ first~\n" +
            "```";
        public string SayServerNotFound { get; set; } = "I couldn't find the server, sorry :c";
        public string SayListening { get; set; } = "I'm listening now~ Every message will be sent to `%CHANNEL%`, so be careful~ To stop it, type `%P%say listen stop`~";
        public string SayNotListening { get; set; } = "I'm not listening from now~";
        public string SayAutoDelChanged { get; set; } = "You turned %STATE% the auto deleting of messages~";
        public string SayListeningNotSelectedServer { get; set; } = "I can't listen to you, because you didn't select any servers and channels :c";
        #endregion
        #region Fun
        public string ImageUnavailable { get; set; } = "Sorry, but this command's source path is not defined so this command is unavailable :c";
        public string ImageDirectoryEmpty { get; set; } = "Sorry, but I have 0 images from this category :c";
        public string ImageCount { get; set; } = "I have **%COUNT%** %IMAGE% images c:";
        public string Images { get; set; } = "These are the available images: `%IMAGES%`";
        public string NoAvailableImageFolder { get; set; } = "Sorry, but there're no folders I can send images from :c";
        public string CantUploadImage { get; set; } = "Sorry, but I can't upload any of the images, because it's empty, or all the files are over 8 megabytes, also there's no Imgur host provided :c";

        public string UserNotFound { get; set; } = "Sorry, but I couldn't find any users with this identifier: %IDENTIFIER%";

        public string NoImages { get; set; } = "Sorry, but I couldn't find any images with these tags: `%TAGS%` :c";

        public string MusicHelp { get; set; } = "```css\n" +
                                                "[Connection]\n" +
                                                "%P%music connect / c - connects to the voice channel where you are if not connected\n" +
                                                "%P%music disconnect / dc - disconnect from the voice channel - only admins\n\n" +
                                                "[Management]\n" +
                                                "%P%music play (youtube link or id) - adds a YouTube video, or resumes the current if it's written without parameters~\n" +
                                                "%P%music play search [keyword] - searches for a YouTube video and adds to the queue [Currently the first match is selected]\n" +
                                                "%P%music stop - Stops the current music playing\n" +
                                                "%P%music pause - Pauses the current music playing\n" +
                                                "%P%music volume - Resumes the current music playing\n" +
                                                "%P%music skip - Skips the current music\n" +
                                                "%P%music list - Lists the first 5 item of the Queue\n" +
                                                "%P%music current - Displays info about the current played song" +
                                                "```";
        public string MusicConnected { get; set; } = "Connected to %VOICE%~";
        public string MusicCannotConnect { get; set; } = "I can't connect to the voice channel, please join into one!";
        public string MusicDisconnect { get; set; } = "I've disconnected from the voice channel~";
        public string MusicNotConnected { get; set; } = "I'm not connected to any of the voice channels here :(";
        public string MusicNotPlaying { get; set; } = "There's nothing played currently, sorry :c";
        public string MusicStopped { get; set; } = "Playing stopped";
        public string MusicFinishedQueue { get; set; } = "Playing finished the queue~";
        public string MusicEmptyQueue { get; set; } = "The queue is empty :(";
        public string MusicPaused { get; set; } = "Paused";
        public string MusicResumed { get; set; } = "Resumed";
        public string MusicVolume { get; set; } = "The current volume is %VOLUME%%~";
        public string MusicNoKeywordMatch { get; set; } = "Sorry, but I couldn't find anything with these keywords!";
        public string MusicPlaying { get; set; } = "Now playing";
        public string MusicRemoveMissingId { get; set; } = "Please provide the number of queue item to remove it~";
        public string MusicStillDownloading { get; set; } = "The next music is still downloading, please wait~";
        public string MusicYouTubeNotValidUrl { get; set; } = "Sorry, but it's not a valid url to provide :c I only accept available YouTube videos to play~";
        public string MusicCouldnotDownload { get; set; } = "Coudln't download the music file, sorry!";

        public string GelbooruImagesDownloading { get; set; } = "Downloading images with the tags~";
        public string GelbooruImagesUploaded { get; set; } = "Images are uploaded, this is the link: %LINK%";
        public string AnonUploadStarted { get; set; } = "Images downloaded, uploading to Anonfile~";
        public string AnonNotAvailable { get; set; } = "Couldn't upload the package to the file host, sorry!";
        #endregion
        #region Information
        public string HasNoGit { get; set; } = "Sorry, but I can't give it to you :c";
        public string GitDescription { get; set; } = "Click on my name to go to the site~";

        public string HasNoInvitationLink { get; set; } = "Sorry, but I can't send any invite links :c";
        public string InvitationDescription { get; set; } = "Click on my name to invite me~";
        public string WMINotSupported { get; set; } = "Sorry, but I can't do this in the current server :<";
        #endregion
        #region Owner
        public string Game { get; set; } = "My game is %GAME% owo";
        public string NewGame { get; set; } = "My game has been changed to %GAME% uwu";
        public string NoGame { get; set; } = "I'm not playing qwq";

        public string Restarting { get; set; } = "Restarting..";
        public string Reloaded { get; set; } = "Reloaded owo";

        public string Shutdown { get; set; } = "I'm going offline :< Bye~";
        public string Reloading { get; set; } = "Reloading, please wait~";
        #endregion

        public Dictionary<string, string> Help { get; set; } = new Dictionary<string, string>()
        {
            { "avatar", "I send specific users' avatars" },
            { "waifu", "no." },
            { "sankaku", "I send an image from https://chan.sankakucomplex.com/ owo [on non-nsfw channels, rating:safe tag will be automatically added!]" },
            { "danbooru", "I send an image from https://danbooru.donmai.us/ owo [on non-nsfw channels, rating:safe tag will be automatically added!]" },
            { "gelbooru", "I send an image from https://gelbooru.com/ owo [on non-nsfw channels, rating:safe tag will be automatically added!]" },
            { "yandere", "I send an image from https://yande.re/ owo [on non-nsfw channels, rating:s tag will be automatically added!]" },
            { "images", "Locally added images~" },
            { "dl", "Downloads all images with the given tags, then uploads it owo" },

            { "help", "Casual help system uwu" },
            { "purgedm", "I remove everything from our DM channel, of course only my messages" },
            { "prefix", "I change my prefix owo" },
            { "lang", "I change my language owo" },
            { "purge", "I purge a channel, be careful where you use it~" },
            { "rlang", "I refresh my knowledge of languages c:" },
            { "admin", "My dear admins~" },
            { "ping", "I send a message about the Discord message sending latency" },
            { "block", "Simple blocking system D:" },
            { "highlight", "Highlighting message owo" },
            { "delete", "Simple delete sytem owo" },
            { "setnsfw", "Add or remove the channel from nsfw zone *doesn't override the nsfw channel mark of Discord*" },
            { "say", "Basic say command~" },
            { "music", "Music part of me uwu" },

            { "info", "Some info about me o//o" },
            { "serverinfo", "Some information of the environment where I am owo" },
            { "git", "I send my Github page owo (if available)" },
            { "invite", "Invitation owo" },

            { "game", "Changing of my game :3" },
            { "restart", "Restarts me :c" },
            { "shutdown", "Shuts me down qwq" },
            { "update", "Update system :3" },
            { "set", "Set global variable" },
            { "get", "Get global variable" }
        };
    }
}
