using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class VotePlugin
    {
        static Dictionary<uint, Vote<object>> RunningVotes = new Dictionary<uint, Vote<object>>();

        static uint count;
        
        public static uint Count { get { return count; } }

        public static void CreateVote<T>(params T[] Objects)
        {
            Vote<T> vote = new Vote<T>();
            var list = new List<VoteObject<T>>();
            foreach (var obj in Objects)
            {
                VoteObject<T> voteobj = new VoteObject<T>();
                
            }
        }

        public static void KillVote(uint id)
        {
            if (!RunningVotes.ContainsKey(id))
                return;


        }
    }

    public class Vote<T>
    {
        Dictionary<uint, VoteObject<T>> Entries;

        uint tag = 0;

        /// <summary>
        /// Create new list
        /// </summary>
        /// <param name="objects">Entries to add</param>
        public Vote<T> Create(params VoteObject<T>[] objects)
        {
            Entries = new Dictionary<uint, VoteObject<T>>();
            foreach (var x in objects)
            {
                Add(x);
            }
            return this;
        }
        
        /// <summary>
        /// Adds the object to the list.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(VoteObject<T> obj)
        {
            Entries.Add(tag, obj);
            tag++;
        }

        /// <summary>
        /// Removes the object from the list
        /// </summary>
        /// <param name="id">ID of the </param>
        public void Remove(uint id)
        {
            if (Entries.ContainsKey(id))
                Entries.Remove(id);
        }

        /// <summary>
        /// Add a vote to teh votes and stuff
        /// TODO: If the user has voted, change vote instead.
        /// </summary>
        /// <param name="id">ID of the vote.</param>
        /// <param name="u">User that voted.</param>
        public void CreateVote(uint id, Discord.User u)
        {
            //If the TAG IS TOO FUCKING HIGH DUDE, and if entries does not exist, and if 
            if (id > tag || Entries == null || !Entries.ContainsKey(id))
                return;

            if (UserHasVoted(u))
            {
                //Change vote
                return;
            }

            var vote = Entries[id];

            vote.voteAmount++;
            vote.UsersVoted.Add(u.Id);
        }

        bool UserHasVoted(Discord.User u)
        {
            //Just iterate through all entries to see if the user has already voted.
            foreach (var entry in Entries.Values)
            {
                if (entry.UsersVoted.Contains(u.Id))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Get all entries
        /// </summary>
        /// <returns>fuck all</returns>
        public string[] GetEntries()
        {
            if (Entries.Count == 0)
                return null;

            List<string> entries = new List<string>() ;

            for (uint i = 0; i < Entries.Count; i++)
            {
                entries.Add($"#{i + 1} {Entries[i].VoteType.ToString()}");
            }

            return entries.ToArray();
        }
    }

    public class VoteObject<T>
    {
        public object VoteType;
        public uint voteAmount;
        public List<ulong> UsersVoted = new List<ulong>();
    }
}
