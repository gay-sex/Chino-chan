using Chino_chan.Modules;
using Chino_chan.Models.File;
using System;
using System.Collections.Generic;

namespace Chino_chan.Image
{
    public class ImageHandler
    {
        public Dictionary<string, FileCollection> Images;

        public int Count
        {
            get
            {
                return Images.Count;
            }
        }
        
        public ImageHandler()
        {
            Images = new Dictionary<string, FileCollection>();

            Global.Logger.Log(ConsoleColor.Cyan, LogType.Images, null, "Loading images...");
            
            foreach (var Item in Global.Settings.ImagePaths)
            {
                if (System.IO.Directory.Exists(Item.Path))
                {
                    var Collection = new FileCollection(Item.Name, Item.Path, Item.TitleIncludeName, Item.IsNsfw, Item.SearchSubDirs);
                    Images.Add(Item.Name.ToLower(), Collection);
                    Global.Logger.Log(ConsoleColor.Green, LogType.Images, null, Item.Name + " images loaded!");
                }
                else
                {
                    Global.Logger.Log(ConsoleColor.Red, LogType.Images, null, Item.Name + ": " + Item.Path + " is not found!");
                }
            }

            Global.Logger.Log(ConsoleColor.Cyan, LogType.Images, null, "Loaded " + Images.Count + " folder" + (Images.Count > 1 ? "s" : ""));
        }
    }
}
