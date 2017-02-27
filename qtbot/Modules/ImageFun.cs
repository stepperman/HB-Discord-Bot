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
            [In, Out] ref ImageBlob imageBlob, [In] byte[] data, [In] UInt64 size, [In] double delta_x);

        [DllImport("MagickQT", EntryPoint = "liquidresizeimage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int linuxliquidresizeimage(
            [In, Out] ref ImageBlob imageBlob, [In] byte[] data, [In] UInt64 size, [In] double delta_x);

        [Command("magick"),
            Description("Perform some magick with the associated image!"),
            Cooldown(30, Cooldowns.Seconds)]
        public static async Task CmdMagickImg(CommandArgs e)
        {
            IMessage msg = null;
            string[] Args = e.Args;
            double delta_x = GetRigidity(ref Args);
            bool silent = IsSilent(Args);

            if (!silent)
                msg = await e.ReplyAsync("Processing.. this might take a while.");
            else
                await e.Message.DeleteAsync();

            string link = await GetImageData(e.Message, Args, e.Channel);

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
                result = liquidresizeimage(ref blob, image, (UInt64)image.Length, delta_x);
            else // If not Windows, we assume Linux.
                result = linuxliquidresizeimage(ref blob, image, (UInt64)image.Length, delta_x);


            byte[] imageBuffer = new byte[blob.size];
            Marshal.Copy(blob.buffer, imageBuffer, 0, (int)blob.size);

            if (blob.buffer == null || imageBuffer.Length == 0 || result != 0)
            {
                if(result == 3000)
                {
                    await e.ReplyAsync("Jesus, that image is too fucking big! Keep it under 3000 pixels in both width and height!");
                    if(!silent) await msg.DeleteAsync();
                    return;
                }

                await e.ReplyAsync("Something went wrong with image processing! Error code: " + result);
                if(!silent) await msg.DeleteAsync();
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

        public static double GetRigidity(ref string[] Args)
        {
            double rigidity = 0;
            List<string> args = new List<string>(Args);
            int?[] deletPos = new int?[2];

            for(int i = 0; i<args.Count; i++)
            {
                if(args[i] == "--delta" || args[i] == "-d")
                {
                    deletPos[0] = i;
                    if (i + 1 != args.Count && double.TryParse(args[i+1], out rigidity))
                        deletPos[1] = i;
                    else
                        rigidity = 0;
                }
            }

            if(deletPos[0] != null)
                args.RemoveAt((int)deletPos[0]);

            if(deletPos[1] != null)
                args.RemoveAt((int)deletPos[0]);

            Args = args.ToArray();


            return rigidity<0?0:rigidity>100?100:rigidity;
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

        public static bool IsSilent(string[] Args)
        {
            if (Args.Length == 0)
                return false;

            if (Args[Args.Length - 1] == "-s")
                return true;
            return false;
        }

        public static async Task<string> GetImageData(IMessage msgcontext, string[] Args, ITextChannel channel)
        {
            if(msgcontext.Attachments.Count != 0)
            {
                var attachments = msgcontext.Attachments.ToList();
                var net = new QtNet(attachments[0].Url);
                foreach(var at in attachments)
                {
                    net.BaseUrl = at.Url;
                    if (await net.IsImage())
                        return at.Url; 
                }
            }
            else if (Args.Length != 0 && Args[0] != "-s")
                return Args[0];
            else
            {
                var messages = channel.GetMessagesAsync();
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
