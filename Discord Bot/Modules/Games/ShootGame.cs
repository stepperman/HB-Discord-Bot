using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Games
{
    public static class ShootGame
    {
        /// <summary>
        /// Alright so how this game works is that you can shoot a user, or multiple users.
        /// If you shoot someone, you have chance to shoot yourself in a certain body part. (<seealso cref="BodyPart"/>)
        /// If that happens, you get timed out for the minutes it has. I think it's one of the funniest commands I've made yet for some reason.
        /// But well you can tag multiple! For every person you tag it increases the chance of hitting yourself by 5 procent.
        /// Or well, it decreases the chance of hitting them by 5 percent to better associate it as it is in the code.
        /// There's 2 commands with this as well, /shoot top and /shoot stats
        /// with /shoot stats you can see your own score. The amount of people you've brutally murdered you sick fuck.
        /// with /shoot top you can see the top 5 scores of other people. They're the most brutal murderers on this server!
        /// 
        /// </summary>
        public static Func<Discord_Bot.CommandPlugin.CommandArgs, Task> ShootUser = async e =>
        {
            //Prematurely check if the user exists in the dictionary, if not, create the fuck.
            Storage.CheckUser(e.User.Id);

            //check if the mentioned users do not exists, if not, create the fuck(s)
            if (e.Message.MentionedUsers.Count() > 0)
            {
                foreach (var user in e.Message.MentionedUsers)
                {
                    Storage.CheckUser(user.Id);
                }
            }

            var arg = e.Args[0];

            //Get your own score
            //TODO: Tag users to get their score. (1 user or more?)
            if (arg == "stats")
            {
                if (e.Message.MentionedUsers.Count() > 0)
                {
                    var user = e.Message.MentionedUsers.ToArray()[0];
                    var sc = Storage.GetUser(user.Id).kills;
                    var de = Storage.GetUser(user.Id).deaths;
                    var k = Storage.GetUser(user.Id).kdRatio;

                    await Tools.Reply(e, $"{user.Name} has killed {sc} people, died {de} times. Their k/d ratio is {k}");

                    return;
                }

                var score = Storage.GetUser(e.User.Id).kills;
                var deaths = Storage.GetUser(e.User.Id).deaths;
                var kd = Storage.GetUser(e.User.Id).kdRatio;

                await Tools.Reply(e, $"You've killed {score} people, and you've died {deaths} times. Your k/d ratio is {kd}");
                return;
            }

            //Get top players
            else if (arg == "top")
            {

                string orderType = "";
                if (e.Args.Count() > 1)
                    orderType = e.Args[1];

                var list = ShootTopPlayers(5, orderType);

                string players = "Top 5 murderers:\n";
                players += String.Format("{0,-25}{1,-18}{2,-20}{3,-12}\n", "Name", "Kills", "Deaths", "K/D ratio");

                int i = 1;
                foreach (var element in list)
                {
                    var username = e.Server.GetUser(element.Key).Name;
                    var userKills = element.Value.kills;
                    var userDeaths = element.Value.deaths;
                    var kd = element.Value.kdRatio;

                    players += String.Format("{0,-25}{1,-18}{2,-20}{3,-12}\n", $"#{i} {username}", userKills, userDeaths, kd);
                    //players += $"#{i}: **{username,8}** Kills: {userKills,8}. Deaths: {userDeaths,8}. k/d ratio: {kd}\n";
                    i++;
                }

                Regex regex = new Regex(@"[^\u0000-\u007F]");
                players = regex.Replace(players, "?");
                await Tools.Reply(e, $"```{players}```", false);
                return;

            }

            //Get count of all the mentioned users. Can be multiple. Count starts at one, an array starts at 0. So if you'd want to access
            //the first occurence in an array. You'd use e.Message.MentionedUsers[0].
            //Actually e.Message.MentionedUsers.ToArray()[0] because it's an IEnumerable but that's fuck.
            int mentionedUserCount = e.Message.MentionedUsers.Count();

            //All the responses. {0} is the shooter, {1} the victim
            string[] responses =
            {
                "{0} just killed {1} to death.",
                "{0} just fucking murdered {1}.",
                "{0} exploded {1}.",
                "{1} is now dead. {0} didn't do it, I swear.",
                "{0} test fired his gun. The bullet ricochet to {1}. Woops.",
                "{0} assassinated {1}.",
                "{0} sprayed {1}.",
                "{0} no-scoped {1} to another dimension.",
                "{0} shot {1} to a fucking pulp.",
                "{0} didn't shoot {1} to death, but fucked them to death!",
                "I don't even want to say what {0} did to {1}.",
                "{0} quickscoped {1}.",
                "{0} invited {1} to 1v1 Rust. He won.",
                "{1} killed himself after years of bullying by {0}.",
                "{0} locked up {1} in a cell and forgot to feed him.",
                "{0} ripped {1}.",
                "{0} talked to {1}, and {1} died.",
                "{0} had a nice bath with {1}.",
                "{0} installed Windows 10 on {1}'s computer",
                "{1} has no idea what this has to do with the /shoot command that {0} initiated.", //courtesy to Will :* // and Will's also a gay cunt!!!
                "{1} proposed to {0}. They said no.", // TURNIP EDIT: ADDED IN SOME DEATHS
                "{0} sold bad batch of meth to {1}.",
                "{0} killed {1} in a cocaine rage.",
                "{0} raped {1} to death.",
                "{0} molested {1} until he committed suicide.",
                "{0} force fed cyanide to {1}.",
                "{0} gave anime to {1}, they later wasted their whole life on a computer screen.",
                "{0} gave a body pillow to {1}, he died 2 weeks later of starvation after {1}'s parent's kicked them out.",
                "{0} stomped {1}'s face into the gutter.",
                "{0} told {1} the truth about his face. {1} then committed suicide.",
                "{0} shanked {1} in the bathrooms.",
                "{0} eyefucked {1} to death.",
                "{0} shot {1} in the head.",
                "{0} brutally bashed {1} in maccas carpark.",
                "{0} told {1} to do a mono.",
                "{0} teabagged {1} to death."
            };

            //Bodyparts
            Dictionary<string, double> BodyParts = new Dictionary<string, double>()
            {
                { "Foot", 0.5 },
                { "Stomach", 1 },
                { "Heart", 1.5 },
                { "Head", 2 },
                { "Dick", 3 }
            };

            //Chance needed
            double SUICIDE_CHANCE = 12.5;
            double MISS_CHANCE = 25;
            double SUICIDE_CHANCE_TOP = 20; // TURNIP EDIT: LOWERED THIS SHIT
            double MISS_CHANCE_TOP = 40; // TURNIP EDIT: LOWERED THIS SHIT
            int MAX_RAND = 100;

            double suicideChance = SUICIDE_CHANCE;
            double missChance = MISS_CHANCE;

            var chance = Tools.random.Next(0, MAX_RAND + 1);
            var hitChance = chance - (int)Math.Floor((3.5 * mentionedUserCount));

            //If player is in top 5, set the hit chance to be a harder difficulty.
            if (ShootTopPlayers(5).Any(u => u.Key == e.User.Id))
            {
                suicideChance = SUICIDE_CHANCE_TOP;
                missChance = MISS_CHANCE_TOP;
            }

            if (mentionedUserCount == 0)
                return;

            bool shotHimself = e.Message.MentionedUsers.Contains(e.User);

            //Already create the premade response
            string names = $"";

            if (mentionedUserCount != 1)
            {
                for (int i = 0; i < mentionedUserCount; i++)
                {
                    //Add the name to response.
                    names += e.Message.MentionedUsers.ToArray()[i].Mention;

                    //If this is the one to last mentioned user, add a " , ".
                    if (i == mentionedUserCount - 2)
                        names += " and ";
                    //Otherwise if it's less than the one to last mentioned user, add an " and ".
                    else if (i < mentionedUserCount - 2)
                        names += ", ";
                }
            }
            else
                names = e.Message.MentionedUsers.ToArray()[0].Mention;

            //Suicide
            if (mentionedUserCount != 0 && (hitChance < suicideChance || shotHimself))
            {
                var bodypart = BodyParts.ElementAt(Tools.random.Next(BodyParts.Count));

                string s = bodypart.Value == 1.0 ? "s" : "";

                if (shotHimself)
                    await Tools.Reply(e, $"Dude! You just fucking shot yourself in the {bodypart.Key.ToLower()}! Why would you do that? You've been timed out for {bodypart.Value} minute{s}!");
                else
                    await Tools.Reply(e, $"Woops~! You just shot yourself in the {bodypart.Key.ToLower()}! You've been timed out for {bodypart.Value} minute{s}! Your chance was {hitChance}. (need > {missChance}/100)");

                //Save the suicide to the deaths
                // TURNIP EDIT: FIXED THE GAY FOOT DEATH NOW IT DOESN'T KILL YOU :D
                if (bodypart.Key.ToLower() != "foot" && bodypart.Key.ToLower() != "stomach")
                        Storage.GetUser(e.User.Id).deaths += 1;

                await Program.timeout.TimeoutUser(e, bodypart.Value, e.User);
                return;
            }
            //Missed shot.
            else if (Tools.InRange(hitChance, suicideChance, missChance))
            {
                if (shotHimself)
                    await Tools.Reply(e, $"Wow! You almost shot yourself to death! For some reason, you missed. (need > {missChance}/100)");
                else
                    await Tools.Reply(e, $"{e.User.Mention} missed {names}. Your chance was {hitChance}. (need > {missChance}/100)", false);

                return;
            }
            //If not any of that, it's a hit!
            else
            {
                string response = "";

                //This is to cheat the system so that Aowashi always has a bath. Always.
                if (e.Message.MentionedUsers.Any(u => u.Id == 99511799421861888))
                    response = "{0} had a nice bath with {1}.".Replace("{0}", e.User.Mention).Replace("{1}", names);
                else
                    response = responses[Tools.random.Next(responses.Length)].Replace("{0}", e.User.Mention).Replace("{1}", names);

                //response.
                await Tools.Reply(e, $"{response} Your chance was {chance} (need > {missChance}/100)", false);

                //aaand save the kills he has.
                Storage.UserInfo[e.User.Id].kills += (uint)mentionedUserCount;

                //Save the deaths of the people that the user has killed.
                foreach (var user in e.Message.MentionedUsers)
                {
                    Storage.UserInfo[user.Id].deaths += 1;
                }

                //Serialize it so that it exists even after the bot is down.
                string json = JsonConvert.SerializeObject(Storage.UserInfo);
                Tools.SaveFile(json, Storage.UserSettingsPath, false); //Save it to disk.
            }
        };

        private static Dictionary<ulong, Models.UserSetting> ShootTopPlayers(int amount, string type = "kills")
        {
            var list = Storage.UserInfo.ToList();

            switch (type)
            {
                case "death":
                case "deaths":
                case "d":
                case "noobs":
                case "deadest":
                    list.Sort((pair1, pair2) => pair1.Value.deaths.CompareTo(pair2.Value.deaths));
                    break;
                case "killdeath":
                case "kd":
                    list.Sort((pair1, pair2) => pair1.Value.kdRatio.CompareTo(pair2.Value.kdRatio));
                    break;
                default:
                    list.Sort((pair1, pair2) => pair1.Value.kills.CompareTo(pair2.Value.kills));
                    break;
            }

            list.Reverse();
            return list.Take(amount).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}