using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using qtbot.CommandPlugin;
using qtbot.CommandPlugin.Attributes;
using qtbot.BotTools;
using HtmlAgilityPack;

namespace qtbot.Modules.SearchCommands
{
    class BingImage
    {
        [Command("image", CommandType.User, "img"),
            Description("Get an image from Bing.")]
        public static async Task ImageFromBing(CommandArgs e)
        {
            QtNetHelper.QtNet net = new QtNetHelper.QtNet("http://www.bing.com/images/search");

            string safeSearchMode = "moderate";

            switch (Tools.GetServerInfo(e.Guild.Id).safesearch)
            {
                case "high":
                    safeSearchMode = "strict"; break;
                case "off":
                    safeSearchMode = "off"; break;
            }


            net.Query = new Dictionary<string, string>()
            {
                { "q", e.ArgText },
                { "adlt",  safeSearchMode}
            };

            var results = GetResults(await net.GetStringAsync());
            string randomResult = results[RandomNumber.Next(results.Length)];
            
            await Tools.ReplyAsync(e, randomResult);
        }

        private static string[] GetResults(string html)
        {
            List<string> results = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var docnode = doc.DocumentNode;

            //Get array of images
            var imageNodes = docnode.SelectNodes("//a[@class='thumb']");

            for (int i = 0; i < imageNodes.Count; i++)
            {

                var x = imageNodes[i].Attributes["href"].Value;
                results.Add(x);
            }

            return results.ToArray();
        }
    }
}
