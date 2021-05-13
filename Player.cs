using System;
using System.Collections.Generic; // Lists
using System.Text.RegularExpressions; // Regex.Replace
using Newtonsoft.Json; //From Newtonsoft.Json.dll
using System.Globalization;

namespace EfStats {
    public class Player {
        public string nickName {get; set;} = "";
        public bool isBot {get; set;} = false;
        public double elo {get; set;} = Elo.startValue;
        public uint kills {get; set;} = 0;
        public uint deaths {get; set;} = 0;
        public uint[] weaponsUsage   {get; set;} = new uint[Weapons.weaponNames.Length]; // See efstats.weaponNames on the meaning of the indizes.
        public uint[] weaponsEndured {get; set;} = new uint[Weapons.weaponNames.Length]; // See efstats.weaponNames on the meaning of the indizes.
        public List<Incident> attacks {get; set;} = new List<Incident>();
        public List<Incident> victims {get; set;} = new List<Incident>();
        public Player(string newNickName,
                      bool newIsBot) {
            this.nickName = newNickName;
            this.isBot = newIsBot;
        }

        public string getName() {
            return this.nickName;
        }

        public bool getIsBot() {
            return this.isBot;
        }

        public void addKill() {
            this.kills++;
        }

        public void addDeath() {
            this.deaths++;
        }

        public void addWeaponsUsage(uint weaponNumber) {
            weaponsUsage[weaponNumber]++;
        }

        public uint getWeaponUsedMaxId() {
            uint id = 0;
            for(uint counter = 0; counter < weaponsUsage.Length; counter++) {
                if (weaponsUsage[counter] > weaponsUsage[id]) id = counter;
            }
            return id;
        }

        public uint getWeaponEnduredMaxId() {
            uint id = 0;
            for(uint counter = 0; counter < weaponsEndured.Length; counter++) {
                if (weaponsEndured[counter] > weaponsEndured[id]) id = counter;
            }
            return id;
        }

        public uint getWeaponUsedMaxCount() {
            return weaponsUsage[getWeaponUsedMaxId()];
        }

        public uint getWeaponEnduredMaxCount() {
            return weaponsEndured[getWeaponEnduredMaxId()];
        }

        public void addWeaponsEndured(uint weaponNumber) {
            weaponsEndured[weaponNumber]++;
        }

        public void setElo(double newElo) {
            this.elo = newElo;
        }

        public uint getKills() {
            return kills;
        }

        public uint getDeaths() {
            return deaths;
        }

        public uint getWeaponUsage(uint weaponNumber) {
            return weaponsUsage[weaponNumber];
        }

        public uint getWeaponEndured(uint weaponNumber) {
            return weaponsEndured[weaponNumber];
        }

        public uint[] getWeaponsEndured() {
            return weaponsEndured;
        }

        public uint[] getWeaponsUsage() {
            return weaponsUsage;
        }

        public double getElo() {
            return elo;
        }

        public string getEloString() {
            return Elo.rounded(getElo()).ToString();
        }

        public double getRatio() {
            if (deaths == 0) return kills;
            return (double)kills / (double)deaths;
        }

        public string getRatioString() {
            return (Elo.rounded(getRatio()*100)/100.0).ToString("F2", CultureInfo.CreateSpecificCulture("en-GB"));
        }

        public double getEfficiency() {
            if ((kills + deaths) == 0) return 0;
            return (double)kills / ((double)kills + (double)deaths);
        }

        public string getEfficiencyString() {
            return (Elo.rounded(getEfficiency()*100)/100.0).ToString("F2", CultureInfo.CreateSpecificCulture("en-GB"));
        }

        public override string ToString() {
            if (isBot) {
                return "Bot\\" + nickName;
            }
            else {
                return nickName;
            }
        }

        public override int GetHashCode() {// Just enough so that the compiler shuts up...
            return Tuple.Create(isBot, nickName).GetHashCode();
        }

        public override bool Equals(object otherGuy) {
            if ((otherGuy == null) || !this.GetType().Equals(otherGuy.GetType())) return false;
            return (   ((Player)otherGuy).getIsBot() == this.isBot
                    && ((Player)otherGuy).getName().Equals(this.nickName));
        }

        public static int Compare(Player a,
                                  Player b,
                                  string criterion) {
            double aval = 0;
            double bval = 0;
            switch (criterion) {
                case "elo":
                    aval = a.getElo();
                    bval = b.getElo();
                    break;
                case "eff":
                    aval = a.getEfficiency();
                    bval = b.getEfficiency();
                    break;
                case "ratio":
                    aval = a.getRatio();
                    bval = b.getRatio();
                    break;
                default:
                    break;
            }

            if (aval.Equals(bval)) return 0;
            if (aval > bval) return 1;
            return -1;
        }

        public string getDetails(bool noColors) {
            string outNick;
            if (noColors) {
                outNick = Player.noColors(this.nickName);
            }
            else {
                outNick = this.nickName;
            }
            string used = ":used{";
            string endured = ":endured{";
            for(uint counter = 0; counter < 43; counter++) {//used
                if (Weapons.getWeaponIDs().Contains(counter)) { //Only Weapons can be used, not World-Stuff
                    if (!used.Equals(":used{")) used += ",";
                    used += Weapons.weaponNames[counter] + "=" + weaponsUsage[counter];
                }
            }
            for(uint counter = 0; counter < 43; counter++) {//endured
                if (counter > 0) {
                    endured += ",";
                }
                endured += Weapons.weaponNames[counter] + "=" + weaponsEndured[counter];
            }
            used += "}";
            endured += "}";
            string mostUsed = Weapons.weaponNames[getWeaponUsedMaxId()];
            string mostEndured = Weapons.weaponNames[getWeaponEnduredMaxId()];
            string victims = "{";
            foreach(Incident i in this.victims) {
                if (!victims.Equals("")) victims += "\\";
                victims += i.name + "(" + i.counter + ")";
            }
            victims += "}";

            string attackers = "{";
            foreach(Incident i in attacks) {
                if (!attackers.Equals("")) attackers += "\\";
                attackers += i.name + "(" + i.counter + ")";
            }
            attackers += "}";

            Incident worstIncidence = getWorstIncident();
            uint worstOpponentCounter = worstIncidence.counter;
            string worstOpponentString = worstIncidence.name + "(" + worstOpponentCounter + ")";

            Incident easiestIncidence = getEasiestIncident();
            uint easiestOpponentCounter = easiestIncidence.counter;
            string easiestOpponentString = easiestIncidence.name + "(" + easiestOpponentCounter + ")";
            return outNick
                + ":Bot=" + isBot
                + ":K=" + kills
                + ":D=" + deaths
                + ":Elo=" + elo
                + used
                + endured
                + ":mostUsed=" + mostUsed + "(" + getWeaponUsedMaxCount() + ")"
                + ":mostEndured=" + mostEndured + "(" + getWeaponEnduredMaxCount() + ")"
                + ":attackers=" + attackers
                + ":victims=" + victims
                + ":WorstOpponent=" + worstOpponentString
                + ":EasiestTarget=" + easiestOpponentString;
        }

        public static string noColors(string nick) {
            if (nick == null) return null;
            return Regex.Replace(nick, "\\^\\d", String.Empty);
        }

        public static List<string> unnamedPlayerNicks = new List<string> { // Those are the standard names for the various flavours of EF. This can be anyone and it is almost certain, that there are multiple players playing with the very same nick, when it is one of these:
            "Redshirt",
            "RedShirt",
            "UnnamedPlayer"
        };

        public string getJSON() {
            return JsonConvert.SerializeObject(this);
        }

        public void setFromJSON(string line) {
            Player tempPlayer = JsonConvert.DeserializeObject<Player>(line);
            this.kills = tempPlayer.kills;
            this.deaths = tempPlayer.deaths;
            this.elo = tempPlayer.elo;
            this.nickName = tempPlayer.nickName;
            this.isBot = tempPlayer.isBot;
            this.weaponsUsage = tempPlayer.weaponsUsage;
            this.weaponsEndured = tempPlayer.weaponsEndured;
        }

        public void addAttack(Incident a) {
            if (efstats.debug) Console.WriteLine("Adding Attack");
            if (this.attacks.Contains(a)) {
                if (efstats.debug) Console.WriteLine("Got that one already.");
                this.attacks.Find(x => x.Equals(a)).Add();
            }
            else {
                if (efstats.debug) Console.WriteLine("This one was new.");
                attacks.Add(a);
                a.Add();
            }
            if (efstats.debug) Console.WriteLine("New attack value for " + a.name + ": " + a.counter);
        }

        public void addVictim(Incident v) {
            if (efstats.debug) Console.WriteLine("Adding Victim");
            if (this.victims.Contains(v)) {
                if (efstats.debug) Console.WriteLine("Got that one already.");
                this.victims.Find(x => x.Equals(v)).Add();
            }
            else {
                if (efstats.debug) Console.WriteLine("This one was new.");
                victims.Add(v);
                v.Add();
            }
            if (efstats.debug) Console.WriteLine("New victim value for " + v.name + ": " + v.counter);
        }

        private Incident getWorstIncident() {
            Incident worstIncident = new Incident("Nobody");
            foreach(Incident i in attacks) {
                if (worstIncident.counter < i.counter) worstIncident = i;
            }
            return worstIncident;
        }

        public string getWorstOpponentName() {
            Incident i = getWorstIncident();
            return i.name;
        }

        public uint getWorstOpponentCount() {
            Incident i = getWorstIncident();
            return i.counter;
        }

        private Incident getEasiestIncident() {
            Incident easiestIncident = new Incident("Nobody");
            foreach(Incident i in this.victims) {
                if (easiestIncident.counter < i.counter) easiestIncident = i;
            }
            return easiestIncident;
        }

        public string getEasiestOpponentName() {
            Incident i = getEasiestIncident();
            return i.name;
        }

        public uint getEasiestOpponentCount() {
            Incident i = getEasiestIncident();
            return i.counter;
        }
    }
}
