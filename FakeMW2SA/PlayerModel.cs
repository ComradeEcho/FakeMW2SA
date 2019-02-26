using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeMW2SA
{
    public class PlayerModel
    {
        public PlayerModel(string ipaddress, string steamid, bool memberjoin)
        {
                this.steamid = steamid;
                this.memberjoin = memberjoin;
                ip = ipaddress;
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;
                lastseen = FakeMW2SA.Utils.GetEpochSeconds();
         }
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
        public string avatarfull { get; set; }
        public int personastate { get; set; }
        public int timecreated { get; set; }
        public int communitybanned { get; set; }
        public int vacbanned { get; set; }
        public int numberofvacbans { get; set; }
        public int dateoflastban { get; set; } 
        public int numberofgamebans { get; set; }
        public int mw2hours { get; set; }
        public int csgohours { get; set; }
        public int cssourcehours { get; set; }
        public int lastseen { get; set; }
        public string country { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string countrycode { get; set; }
        public int playerprimaryid { get; set; }
        public string level { get; set; }
        public string presteige { get; set; }
        public int unknown1 { get; set; }
        public int unknown2 { get; set; }
        public int missing { get; set; }
        public int deaths { get; set; }
        public int score { get; set; }
        public int partyID { get; set; }
        public bool host { get; set; }
        public bool memberjoin { get; set; }
        public string lobby { get; set; }
        public string ip { get; set; }
        public string banned { get; set; }
        public int lastupdated { get; set; }
        public bool updated { get; set; } = false;
        public int vacbypass { get; set; } = 2;
    }
    public class jsonoutput
    {
        public jsonoutput()
        {
            try
            {
                if (FakeMW2SA.Program.ipaddresses.Count > 0)
                {
                    host = FakeMW2SA.Program.ipaddresses.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    ipaddresses = Program.ipaddresses.Distinct().ToList();
                }
            } catch {
                
            }
        }
        
        public List<string> ipaddresses { get; set; }
        public List<FakeMW2SA.PlayerModel> players { get; set; } = FakeMW2SA.Program.players;
        public string host { get; set; } = "0.0.0.0";
        public int apicalls { get; set; } = FakeMW2SA.Program.apicalls;
        public int partystatecount { get; set; } = FakeMW2SA.Program.partystate;
        public int memberjoincount { get; set; } = FakeMW2SA.Program.memberjoin;

}
}

