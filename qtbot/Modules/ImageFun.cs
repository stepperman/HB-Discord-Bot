using System;
using System.Collections.Generic;
using System.Text;
using ImageMagick;
using System.Threading.Tasks;
using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using System.Linq;
using QtNetHelper;
using System.IO;

namespace qtbot.Modules
{
    class ImageFun
    {

        [Command("magik", CommandType.User, "magic"),
            Description("Perform some magic on your image!")]
        public static async Task CmdMagick(CommandArgs e)
        {
            if (e.Message.Attachments.Count == 0 && e.Args.Length == 0)
                return;

            var attachmentlist = e.Message.Attachments.ToList();
            Discord.Attachment attachment = attachmentlist.Count > 0 ? attachmentlist[0] : null;

            var image = await GetFile(attachment);
            if (image == null || image.Length == 0)
            {
                if(e.Args.Count() != 0)
                {
                    QtNet netHelper = new QtNet(e.Args[0]);
                    image = await netHelper.GetByteArrayAsync();

                    if (image == null || image.Length == 0)
                        return;
                }
            }



            using (var stream = new MemoryStream())
            {
                var msg = await e.ReplyAsync("Processing, this might take a while.");
                string filetype = "";
                using (MagickImage img = new MagickImage(image))
                {
                    if(img.Width > 3000 || img.Height > 3000)
                    {
                        await e.ReplyAsync("Image is too big, (over 3000px)");
                        return;
                    }

                    
                    img.LiquidRescale(new Percentage(50));
                    img.LiquidRescale(new Percentage(200));

                    img.Write(stream);
                    filetype = img.Format.ToString();
                }
                
                await msg?.DeleteAsync();
                stream.Position = 0;
                await e.Channel.SendFileAsync(stream, "magic." + filetype);
            }
        }
        
        public static async Task<byte[]> GetFile(Discord.Attachment attachment)
        {
            if (attachment == null)
                return null;

            //Check if the filetypes are correct.
            string[] filetypes = { "png", "jpg", "bmp", "jpeg" };
            var filetype = attachment.Filename.Split('.')[1].ToLower();

            if (filetypes.Any(x => x == filetype) == false || attachment.Height > 3000 || attachment.Width > 3000)
                return null;

            QtNet qtNet = new QtNet(attachment.Url);
            return await qtNet.GetByteArrayAsync();
        }
    }
}
