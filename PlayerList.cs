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
            return filteredPlayerStrings(withBots, noColor);
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
            for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) {//used
                if (Weapons.getWeaponIDs().Contains(wCounter)) { //Only Weapons can be used, not World-Stuff
                    string weaponKill = "Kills with " + Weapons.weaponNames[wCounter];
                    cells.Add(weaponKill);
                }

            }
            for(uint wCounter = 0; wCounter < numberOfDamageDealers; wCounter++) {//used
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
    }
}
