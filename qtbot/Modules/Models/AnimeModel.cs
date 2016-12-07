using System;
using System.Collections.Generic;
using System.Text;

namespace qtbot.Modules.Models
{
    class AnimeModel
    {
        public string Title;
        public string Genre;
        public string Url;
        public string ImageUrl;
        public string Episodes;
        public string Duration;
        public string Score;
        public string Type;
        public string Description;

        public override string ToString()
        {
            return Title;
        }
    }
}
