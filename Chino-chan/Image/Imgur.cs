using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Chino_chan.Image
{
    public class Imgur
    {
        ImgurClient Client;
        ImageEndpoint Endpoint;
        
        public Imgur()
        {
            Global.Logger.Log(ConsoleColor.Green, Modules.LogType.Imgur, "Login", "Logging in...");

            try
            {
                Client = new ImgurClient(Global.Settings.Credentials.Imgur.ClientId, Global.Settings.Credentials.Imgur.ClientSecret);
                Endpoint = new ImageEndpoint(Client);

                Global.Logger.Log(ConsoleColor.Green, Modules.LogType.Imgur, "Login", "Logged in!");
            }
            catch (Exception Exception)
            {
                Global.Logger.Log(ConsoleColor.Red, Modules.LogType.Imgur, "Login", "Couldn't login due: " + Exception.Message);
            }
        }

        public async Task<string> UploadImage(string Path)
        {
            if (!File.Exists(Path))
            {
                Global.Logger.Log(ConsoleColor.Red, Modules.LogType.Imgur, "Upload", "File doesn't exists: " + Path);
                return "";
            }
            using (var Stream = new FileStream(Path, FileMode.Open))
            {
                return await UploadImage(Stream);
            }
        }
        public async Task<string> UploadImage(Stream Stream)
        {
            try
            {
                var Image = await Endpoint.UploadImageStreamAsync(Stream);
                return Image.Link;
            }
            catch (ImgurException Exception)
            {
                Global.Logger.Log(ConsoleColor.Red, Modules.LogType.Imgur, "Upload", "An error occured while uploading an image:"
                    + Environment.NewLine
                    + Exception.Message);
            }
            return "";
        }
    }
}
