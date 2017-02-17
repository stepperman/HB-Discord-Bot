using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using QtNetHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace qtbot.Modules
{
    class ImageFun
    {
        [DllImport("LocalFiles/libs/MagickQT", EntryPoint = "liquidresizeimage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int liquidresizeimage(
            [In, Out] ref ImageBlob imageBlob, [In] byte[] data, [In] UInt64 size);

        [DllImport("LocalFiles/libs/libMagickQt", EntryPoint = "liquidresizeimage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int linuxliquidresizeimage(
            [In, Out] ref ImageBlob imageBlob, [In] byte[] data, [In] UInt64 size);

        [Command("magick")]
        public static async Task CmdMagickImg(CommandArgs e)
        {
            await e.Channel.TriggerTypingAsync();
            QtNet net = new QtNet(e.ArgText);
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
                return;
            }

            using (var memStream = new MemoryStream(imageBuffer))
            {
                memStream.Position = 0;
                if (memStream.Length == 0)
                    return;
                
                await e.Channel.SendFileAsync(memStream, "magick.png");
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ImageBlob
        {
            public IntPtr buffer;
            public UInt64 size;
        }
    }
}
