using Discord;
using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using QtNetHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace qtbot.Modules
{
    class ImageFun
    {
        [DllImport("LocalFiles/libs/MagickQT", EntryPoint = "liquidresizeimage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int liquidresizeimage(
            [In, Out] ref ImageBlob imageBlob, [In] byte[] data, [In] UInt64 size);

        [DllImport("MagickQT", EntryPoint = "liquidresizeimage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int linuxliquidresizeimage(
            [In, Out] ref ImageBlob imageBlob, [In] byte[] data, [In] UInt64 size);

        [Command("magick"),
            Description("Perform some magick with the associated image!")]
        public static async Task CmdMagickImg(CommandArgs e)
        {
            IMessage msg = null;
            bool silent = IsSilent(e);

            if (!silent)
                msg = await e.ReplyAsync("Processing.. this might take a while.");
            else
                await e.Message.DeleteAsync();
            string link = await GetImageData(e);

            if (string.IsNullOrEmpty(link))
            {
                await e.ReplyAsync("Couldn't find any images.");
                return;
            }
            QtNet net = new QtNet(link);
            var image = await net.GetByteArrayAsync(); 
            ImageBlob blob = new ImageBlob();

            bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            int result = -1;
            if (windows)
                result = liquidresizeimage(ref blob, image, (UInt64)image.Length);
            else // If not Windows, we assume Linux.
                result = linuxliquidresizeimage(ref blob, image, (UInt64)image.Length);


            byte[] imageBuffer = new byte[blob.size];
            Marshal.Copy(blob.buffer, imageBuffer, 0, (int)blob.size);

            if (blob.buffer == null || imageBuffer.Length == 0 || result != 0)
            {
                await e.ReplyAsync("Something went wrong with image processing! Error code: " + result);
                await msg.DeleteAsync();
                return;
            }

            using (var memStream = new MemoryStream(imageBuffer))
            {
                memStream.Position = 0;
                if (memStream.Length == 0)
                    return;

                if (msg != null)
                    await msg.DeleteAsync();
                await e.Channel.SendFileAsync(memStream, "magick.png");
            }
        }

        [Command("bigemoji", CommandType.User, "e"),
            Description("Get the emoji, but BIG!")]
        public static async Task CmdBigEmoji(CommandArgs e)
        {
            Emoji em;
            var tags = e.Message.Tags.ToList();
            if (tags.Count == 0)
                return;

            var emoji = tags.FirstOrDefault(x => x.Type == TagType.Emoji);
            if (emoji == null)
                return;
            em = (Emoji)emoji.Value;

            var net = new QtNet(em.Url);
            var b = await net.GetByteArrayAsync();
            using (Stream m = new MemoryStream(b))
                await e.Channel.SendFileAsync(m, "emoji.png");
        }

        public static bool IsSilent(CommandArgs e)
        {
            if (e.Args.Length == 0)
                return false;

            if (e.Args[e.Args.Length - 1] == "-s")
                return true;
            return false;
        }

        public static async Task<string> GetImageData(CommandArgs e)
        {
            if(e.Message.Attachments.Count != 0)
            {
                var attachments = e.Message.Attachments.ToList();
                var net = new QtNet(attachments[0].Url);
                foreach(var at in attachments)
                {
                    net.BaseUrl = at.Url;
                    if (await net.IsImage())
                        return at.Url; 
                }
            }
            else if (e.Args.Length != 0 && e.Args[0] != "-s")
                return e.Args[0];
            else
            {
                var messages = e.Channel.GetMessagesAsync();
                var msg = await messages.Flatten();
                
                foreach(var message in msg)
                {
                    if (message.Attachments.Count != 0)
                    {
                        var attachments = message.Attachments.ToList();
                        var net = new QtNet(attachments[0].Url);
                        foreach (var at in attachments)
                        {
                            net.BaseUrl = at.Url;
                            if (await net.IsImage())
                                return at.Url;
                        }
                    }
                    else
                    {
                        var net = new QtNet("temp");
                        var parts = message.Content.Split(' ');
                        string[] webstarts = { "http", "https", "www" };
                        foreach (var part in parts)
                        {
                            if (webstarts.Any(x => part.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
                            {
                                net.BaseUrl = part;
                                if (await net.IsImage())
                                    return part;
                            }
                        }
                    }
                }
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ImageBlob
        {
            public IntPtr buffer;
            public UInt64 size;
        }
    }
}
