using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace qtbot.BotTools
{
    class Setup
    {
        public static void GetProgramInfo()
        {
            var a = typeof(Program).GetTypeInfo().Assembly;
            var l = a.Location;
            l = Path.GetDirectoryName(l) + "/LocalFiles/ProgramInfo.json";

            if (File.Exists(l))
            {
                using (StreamReader sr = new StreamReader(File.OpenRead(l)))
                {
                    var jsonfile = sr.ReadToEnd();
                    Storage.programInfo = JsonConvert.DeserializeObject(jsonfile);
                }
            }
        }
    }
}
