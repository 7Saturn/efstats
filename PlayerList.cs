using System;
using System.Linq; // File.ReadLines().First()
using System.IO;//File
using System.Collections.Generic;
using Newtonsoft.Json; //From Newtonsoft.Json.dll

namespace EfStats {
    public class PlayerList{
        public List<Player> list {get; set;} = null;

        public PlayerList() {
            this.list = new List<Player>();
        }

        public List<Player> getList() {
            return list;
        }

        public Player getPlayerByNameAndType(string name,
                                             bool isBot) {
            return getPlayer(new Player(name, isBot));
        }

        public Player getPlayer(Player p) {
            foreach (Player guy in list) {
                if (p.Equals(guy)) return guy;
            }
            return null;
        }

        public bool Contains(Player p) {
            foreach (Player guy in list) {
                if (p.Equals(guy)) return true;
            }
            return false;
        }

        public void AddMissing(Player p) {
            if (!list.Contains(p)) list.Add(p);
        }

        public List<string> getTextDump(bool withBots,
                                        bool noColor) {
            string playerHeading = "Player";
            string isBotHeading = "Is a Bot";
            string eloHeading = "ELO";
            string efficiencyHeading = "Efficiency";
            string ratioHeading = "Ratio";
            string killsHeading = "Kills";
            string deathsHeading = "Deaths";
            string weaponsUsedHeading = "Weapons Used";
            string weaponsEnduredHeading = "Weapons Endured";
            string weaponsMaxUsedHeading = "Weapon Used Most";
            string weaponsMaxEnduredHeading = "Weapon Endured Most";
            string attackersHeading = "Attackers";
            string victimsHeading = "Victims";
            string worstEnemyHeading = "Worst Enemy";
            string easiestTargetHeading = "Easiest Target";

            uint maxNicks         = (uint)playerHeading.Length;
            uint maxIsBot         = (uint)isBotHeading.Length; // "yes" is the most broad text that can happen so won't need correction
            uint maxElos          = (uint)eloHeading.Length;
            uint maxEffs          = (uint)efficiencyHeading.Length;
            uint maxRatios        = (uint)ratioHeading.Length;
            uint maxKills         = (uint)killsHeading.Length;
            uint maxDeaths        = (uint)deathsHeading.Length;
            uint maxWeapons       = (uint)weaponsUsedHeading.Length;
            uint maxEndurances    = (uint)weaponsEnduredHeading.Length;
            uint maxMostUsed      = (uint)weaponsMaxUsedHeading.Length;
            uint maxMostEndured   = (uint)weaponsMaxEnduredHeading.Length;
            uint maxAttackers     = (uint)attackersHeading.Length;
            uint maxVictims       = (uint)victimsHeading.Length;
            uint maxWorstEnemy    = (uint)worstEnemyHeading.Length;
            uint maxEasiestTarget = (uint)easiestTargetHeading.Length;

            foreach(Player p in list) {
                maxNicks = increaseIfNeeded(maxNicks, (uint)p.getName().ToString().Length);
                maxElos = increaseIfNeeded(maxElos, (uint)(Elo.rounded(p.getElo()*10)/10.0).ToString().Length);
                maxEffs = increaseIfNeeded(maxEffs, (uint)(Elo.rounded(p.getEfficiency()*10)/10.0).ToString().Length);
                maxRatios = increaseIfNeeded(maxRatios, (uint)(Elo.rounded(p.getRatio()*10)/10.0).ToString().Length);
                maxKills = increaseIfNeeded(maxKills, (uint)p.getKills().ToString().Length);
                maxDeaths = increaseIfNeeded(maxDeaths, (uint)p.getDeaths().ToString().Length);
                uint numberOfDamageDealers = (uint) Weapons.weaponNames.Length;
                for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) { // used
                    if (Weapons.getWeaponIDs().Contains(wCounter)) { // Only Weapons can be used, not World-Stuff
                        string weaponKill = Weapons.weaponNames[wCounter] + " (" + p.getWeaponUsage(wCounter) + ")";
                        if (p.getWeaponUsage(wCounter) > 0) maxWeapons = increaseIfNeeded(maxWeapons, (uint)weaponKill.Length);
                    }
                }
                for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) { // used
                    string causeOfDeath = Weapons.weaponNames[wCounter] + " (" + p.getWeaponEndured(wCounter) + ")";
                    if (p.getWeaponEndured(wCounter) > 0) maxEndurances = increaseIfNeeded(maxEndurances, (uint)causeOfDeath.Length);
                }
                foreach (Incident attack in p.attacks) {
                    string attackString = attack.name + " (" + attack.counter + ")";
                    maxAttackers = increaseIfNeeded(maxAttackers, (uint)attackString.Length);
                }
                foreach (Incident victim in p.victims) {
                    string victimString = victim.name + " (" + victim.counter + ")";
                    maxVictims = increaseIfNeeded(maxVictims, (uint)victimString.Length);
                }
                string worstEnemy = p.getWorstOpponentName() + " (" + p.getWorstOpponentCount() + ")";
                maxWorstEnemy = increaseIfNeeded(maxWorstEnemy, (uint)worstEnemy.Length);
                string easiestTarget = p.getEasiestOpponentName() + " (" + p.getEasiestOpponentCount() + ")";
                maxEasiestTarget = increaseIfNeeded(maxEasiestTarget, (uint)easiestTarget.Length);
                string mostUsed = Weapons.weaponNames[p.getWeaponUsedMaxId()] + " (" + p.getWeaponUsedMaxCount() + ")";
                maxMostUsed = increaseIfNeeded(maxMostUsed, (uint)mostUsed.Length);
                string mostEndured = Weapons.weaponNames[p.getWeaponEnduredMaxId()] + " (" + p.getWeaponEnduredMaxCount() + ")";
                maxMostEndured = increaseIfNeeded(maxMostEndured, (uint)mostEndured.Length);
            }
            //Two spaces inbetween should separate the columns.
            maxNicks         += 2;
            maxIsBot         += 2;
            maxElos          += 2;
            maxEffs          += 2;
            maxRatios        += 2;
            maxKills         += 2;
            maxDeaths        += 2;
            maxWeapons       += 2;
            maxEndurances    += 2;
            maxMostUsed      += 2;
            maxMostEndured   += 2;
            maxAttackers     += 2;
            maxVictims       += 2;
            maxWorstEnemy    += 2;
            maxEasiestTarget += 2;

            List<string> lines = new List<string>();
            string line =
                playerHeading.PadRight((int)maxNicks)
                + isBotHeading.PadRight((int)maxIsBot)
                + eloHeading.PadRight((int)maxElos)
                + efficiencyHeading.PadRight((int)maxEffs)
                + ratioHeading.PadRight((int)maxRatios)
                + killsHeading.PadRight((int)maxKills)
                + deathsHeading.PadRight((int)maxDeaths)
                + weaponsUsedHeading.PadRight((int)maxWeapons)
                + weaponsEnduredHeading.PadRight((int)maxEndurances)
                + weaponsMaxUsedHeading.PadRight((int)maxMostUsed)
                + weaponsMaxEnduredHeading.PadRight((int)maxMostEndured)
                + attackersHeading.PadRight((int)maxAttackers)
                + victimsHeading.PadRight((int)maxVictims)
                + worstEnemyHeading.PadRight((int)maxWorstEnemy)
                + easiestTargetHeading.PadRight((int)maxEasiestTarget);

            lines.Add(line.Trim());
            foreach(Player p in list) {
                List <string> weaponsUsed = new List<string>();
                List <string> weaponsEndured = new List<string>();
                List <string> Attackers = new List<string>();
                List <string> Victims = new List<string>();

                for (uint counter = 0; counter < p.getWeaponsEndured().Length; counter++) {
                    string weaponsName = Weapons.weaponNames[counter];
                    if (p.getWeaponsUsage()[counter] > 0) weaponsUsed.Add(weaponsName + " (" + p.getWeaponsUsage()[counter] + ")");
                    if (p.getWeaponsEndured()[counter] > 0) weaponsEndured.Add(weaponsName + " (" + p.getWeaponsEndured()[counter] + ")");
                }

                foreach(Incident i in p.victims)   Victims.Add(i.name + " (" + i.counter + ")");
                foreach(Incident i in p.attacks) Attackers.Add(i.name + " (" + i.counter + ")");

                line = p.getName().PadRight((int) maxNicks);
                if (p.getIsBot()) {
                    line += ("yes").PadRight((int) maxIsBot);
                }
                else {
                    line += ("no").PadRight((int) maxIsBot);
                }
                line += (Elo.rounded(p.getElo()*10)/10.0).ToString().PadRight((int)maxElos) +
                    (Elo.rounded(p.getEfficiency()*10)/10.0).ToString().PadRight((int)maxEffs) +
                    (Elo.rounded(p.getRatio()*10)/10.0).ToString().PadRight((int)maxRatios) +
                    p.getKills().ToString().PadRight((int)maxKills) +
                    p.getDeaths().ToString().PadRight((int)maxDeaths);
                string weaponUsed = "";
                if (weaponsUsed.Count > 0) {
                    weaponUsed = weaponsUsed.ElementAt(0);
                    weaponsUsed.RemoveAt(0);
                }
                line += weaponUsed.PadRight((int) maxWeapons);
                string weaponEndured = "";
                if (weaponsEndured.Count > 0) {
                    weaponEndured = weaponsEndured.ElementAt(0);
                    weaponsEndured.RemoveAt(0);
                }
                line += weaponEndured.PadRight((int) maxEndurances);
                line += (Weapons.weaponNames[p.getWeaponUsedMaxId()] + " (" + p.getWeaponUsedMaxCount() + ")").PadRight((int) maxMostUsed) +
                    (Weapons.weaponNames[p.getWeaponEnduredMaxId()] + " (" + p.getWeaponEnduredMaxCount() + ")").PadRight((int) maxMostEndured);

                string attacker = "";
                if (Attackers.Count > 0) {
                    attacker = Attackers.ElementAt(0);
                    Attackers.RemoveAt(0);
                }
                line += attacker.PadRight((int) maxAttackers);

                string victim = "";
                if (Victims.Count > 0) {
                    victim = Victims.ElementAt(0);
                    Victims.RemoveAt(0);
                }
                line += victim.PadRight((int) maxVictims);

                line += (p.getWorstOpponentName() + " (" + p.getWorstOpponentCount() + ")").PadRight((int) maxWorstEnemy) +
                    (p.getEasiestOpponentName() + " (" + p.getEasiestOpponentCount() + ")").PadRight((int) maxEasiestTarget);
                lines.Add(line.Trim());
                while (   weaponsUsed.Count > 0
                       || weaponsEndured.Count > 0
                       || Attackers.Count > 0
                       || Victims.Count > 0) {
                    line = "".PadRight((int)(maxNicks + maxIsBot + maxElos + maxEffs + maxRatios + maxKills + maxDeaths));

                    weaponUsed = "";
                    if (weaponsUsed.Count > 0) {
                        weaponUsed = weaponsUsed.ElementAt(0);
                        weaponsUsed.RemoveAt(0);
                    }
                    line += weaponUsed.PadRight((int) maxWeapons);
                    weaponEndured = "";
                    if (weaponsEndured.Count > 0) {
                        weaponEndured = weaponsEndured.ElementAt(0);
                        weaponsEndured.RemoveAt(0);
                    }
                    line += weaponEndured.PadRight((int) maxWeapons);

                    line += "".PadRight((int)(maxMostUsed + maxMostEndured));

                    attacker = "";
                    if (Attackers.Count > 0) {
                        attacker = Attackers.ElementAt(0);
                        Attackers.RemoveAt(0);
                    }

                    line += attacker.PadRight((int)maxAttackers);

                    victim = "";
                    if (Victims.Count > 0) {
                        victim = Victims.ElementAt(0);
                        Victims.RemoveAt(0);
                    }

                    line += victim;

                    lines.Add(line);
                }
            }
            return lines;
        }

        public void dumpToText(bool withBots,
                               bool noColor,
                               string filename) {
            writeToFile(filename,
                        getTextDump(withBots,
                                    noColor));
        }

        public List<string> getCsvDump(bool withBots,
                                       bool noColor) {
            List<string> stringList = new List<string>();
            uint rankCounter = 0;
            string line;
            List<string> cells = new List<string>();
            cells.Add("Rank");
            cells.Add("Nick");
            if (withBots) cells.Add("Is Bot");
            cells.Add("Elo");
            cells.Add("Efficiency");
            cells.Add("Ratio");
            cells.Add("Kills");
            cells.Add("Deaths");
            uint numberOfDamageDealers = (uint) Weapons.weaponNames.Length;
            for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) { // used
                if (Weapons.getWeaponIDs().Contains(wCounter)) { // Only Weapons can be used, not World-Stuff
                    string weaponKill = "Kills with " + Weapons.weaponNames[wCounter];
                    cells.Add(weaponKill);
                }
            }
            for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) { // endured
                cells.Add("Killed by " + Weapons.weaponNames[wCounter]);
            }
            cells.Add("Most often Used Weapon");
            cells.Add("Most often Endured Weapon");
            cells.Add("Attackers");
            cells.Add("Victims");
            cells.Add("Worst Enemy");
            cells.Add("Easiest Target");
            line = Csv.CsvLine(cells);
            stringList.Add(line);
            foreach(Player p in list) {
                if (!(   p.getIsBot()
                      && !withBots)) {
                    rankCounter++;
                    cells = new List<string>(); // One for each row, even empty ones
                    cells.Add(rankCounter.ToString());
                    cells.Add(p.getName());
                    if (withBots) cells.Add(p.getIsBot().ToString());
                    cells.Add(Elo.rounded(p.getElo()).ToString());
                    cells.Add((Elo.rounded(p.getEfficiency() * 10)/10.0).ToString());
                    cells.Add((Elo.rounded(p.getRatio() * 10)/10.0).ToString());
                    cells.Add(p.getKills().ToString());
                    cells.Add(p.getDeaths().ToString());
                    for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) {// used on others, damage done by the map is not of interest, as it cannot come from the player.
                        if (Weapons.getWeaponIDs().Contains(wCounter)) { // Only Weapons can be used, not map damage stuff such as lava or force fields
                            string NumberOfKills =p.getWeaponsUsage()[wCounter].ToString();
                            cells.Add(NumberOfKills);
                        }
                    }
                    for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) {// endured, here we want all of them.
                        string NumberOfDeaths =p.getWeaponsEndured()[wCounter].ToString();
                        cells.Add(NumberOfDeaths);
                    }
                    uint maxUsedID = p.getWeaponUsedMaxId();
                    uint maxEnduredID = p.getWeaponEnduredMaxId();
                    string mostUsedWeapon = Weapons.weaponNames[maxUsedID];
                    mostUsedWeapon+= " (" + p.getWeaponUsedMaxCount() + ")";
                    string mostEnduredWeapon = Weapons.weaponNames[maxEnduredID];
                    mostEnduredWeapon += " (" + p.getWeaponEnduredMaxCount() + ")";
                    cells.Add(mostUsedWeapon);
                    cells.Add(mostEnduredWeapon);
                    List<Incident> attackersList = p.attacks;
                    string attackersString = "";
                    foreach(Incident i in attackersList) {
                        string attackerString = i.name + " (" + i.counter + ")";
                        if (attackersString.Equals("")) {
                            attackersString = attackerString;
                        }
                        else {
                            attackersString += ",\n" + attackerString;
                        }
                    }
                    cells.Add(attackersString);
                    List<Incident> victimsList = p.victims;
                    string victimsString = "";
                    foreach(Incident i in victimsList) {
                        string victimString = i.name + " (" + i.counter + ")";
                        if (victimsString.Equals("")) {
                            victimsString = victimString;
                        }
                        else {
                            victimsString += ",\n" + victimString;
                        }
                    }
                    cells.Add(victimsString);
                    string worstEnemyName = p.getWorstOpponentName();
                    worstEnemyName += " (" + p.getWorstOpponentCount() + ")";
                    cells.Add(worstEnemyName);
                    string easiestEnemyName = p.getEasiestOpponentName();
                    easiestEnemyName += " (" + p.getEasiestOpponentCount() + ")";
                    cells.Add(easiestEnemyName);
                    line = Csv.CsvLine(cells);
                    stringList.Add(line);
                }
            }
            return stringList;
        }
        public void dumpToCsv(bool withBots,
                              bool noColor,
                              string filename) {
            writeToFile(filename,
                        getCsvDump(withBots,
                                   noColor));
        }

        public string getJSON(bool withBots) {
            List<Player> players = filteredPlayers(withBots);
            return JsonConvert.SerializeObject(players, Formatting.Indented);
        }

        public void dumpToJSON(string filename,
                               bool withBots) {
            string[] dummyList = new string[] {getJSON(withBots)};
            File.WriteAllLines(filename, dummyList);
        }

        public void readFromJSON(string filename) {
            string lines = File.ReadLines(filename).First();
            List<Player> tempList = JsonConvert.DeserializeObject<List<Player>>(lines);
            this.list = tempList;
        }

        public static string printTextJson(Player p) {
            return JsonConvert.SerializeObject(p);
        }

        List<string> filteredPlayerStrings(bool withBots,
                                           bool noColor) {
            List<string> stringList = new List<string>();
            foreach(Player p in list) if (!(p.getIsBot() && !withBots)) stringList.Add(p.getDetails(noColor));
            return stringList;
        }

        List<Player> filteredPlayers(bool withBots) {
            if (withBots) return this.list; // This always means all of them, because there is no noPlayer or something...
            List<Player> filteredList = new List<Player>();
            foreach(Player p in list) if (!p.getIsBot()) filteredList.Add(p);
            return filteredList;
        }

        public void sort(string criterion,
                         bool ascending) {
            this.list.Sort(
                           delegate(Player a, Player b) {
                               if (ascending) return Player.Compare(a, b, criterion);
                               return -Player.Compare(a, b, criterion);
                           });
        }

        public void removeBeginners(uint minEncounters = 30) {
            List<Player> newList = new List<Player> ();
            foreach(Player p in list) if ((p.kills + p.deaths) >= minEncounters) newList.Add(p);
            list = newList;
        }

        void writeToFile(string filename,
                         List<string> stringList) {
            File.WriteAllLines(filename, stringList.ToArray());
        }

        public static uint getMaxWidth(List<string> strings) {
            uint maxLength = 0;
            foreach(string s in strings) if (maxLength < (uint) s.Length) maxLength = (uint) s.Length;
            return maxLength;
        }

        private static uint increaseIfNeeded(uint value, uint comparison) {
            if (value < comparison) return comparison;
            return value;
        }

    }
}
