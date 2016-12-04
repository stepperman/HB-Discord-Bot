using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Discord_Bot
{
    class Setup
    {
        public static void GetProgramInfo()
        {
            if (File.Exists("./../LocalFiles/ProgramInfo.json"))
            {
                using (StreamReader sr = new StreamReader("./../LocalFiles/ProgramInfo.json"))
                {
                    var jsonfile = sr.ReadToEnd();
                    Storage.programInfo = JsonConvert.DeserializeObject(jsonfile);
                }
            }
        }
    }
}
