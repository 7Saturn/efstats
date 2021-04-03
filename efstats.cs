using System; // Simple WriteLine
using System.IO; // File
using System.Collections.Generic; // Lists
using System.Text.RegularExpressions; // Split
// uses https://github.com/JamesNK/Newtonsoft.Json

public static class efstats {
    public static List<Encounter> encounters = new List<Encounter>();
    public static void Main(string[] args) {
        string inputFileName = "classic.log";
        string outputFileName = "classicStats.json";
        uint playerNumber = 1022; //World...
        bool isBot = false;
        bool withBots = false;
        bool withUnnamed = false;
        bool reportParseErrors = false;
        bool toSTDOUT = false;
        uint counter = 0;
        uint minEncounters = 30;
        string nickName = "";
        List<string> parseErrorLines = new List<string>();
        PlayerList list = new PlayerList();
        foreach (string line in File.ReadLines(inputFileName)) {
            counter++;
            if (line.Length > 6) {// At least the time stamp *must* be present, but honestly, that's still too nice.
                string payload = line.Substring(7);
                string[] elements = Regex.Split(payload, "\\s+");
                string prefix = elements[0];
                bool success;
                if (prefix.Equals("ClientUserinfoChanged:")) {// This actually only keeps track of the player mapping. EF logs player's IDs /and/ names, but as names are fucky to parse, we will only use IDs and track the mapping of IDs to names here ourselves.
                    if (UInt32.TryParse(elements[1], // Player's ID
                                        out playerNumber)) {
                        string[] userInfoAll = Regex.Split(payload, "\\\\");
                        Player tempGuy  = null;
                        if (userInfoAll.Length == 16) { //Very likely a human
                            nickName = userInfoAll[1];
                            isBot = false;
                            tempGuy = new Player(nickName, isBot);
                            PlayerMapping.setPlayer(playerNumber, tempGuy);
                        }
                        else {
                            if (userInfoAll.Length == 18) { //Very likely a bot
                                nickName = userInfoAll[1];
                                if (userInfoAll[16].Equals("skill")) { // But let's actually recheck that
                                    isBot = true;
                                }
                                tempGuy = new Player(nickName, isBot);
                                PlayerMapping.setPlayer(playerNumber, tempGuy); // Always add, regardles of being a Bot or not
                            }
                            else {// There is that strange behavior, that there is just a line of the form »ClientUserinfoChanged: <number> <number>« that actually cannot come from the server, according to the code. Strange shit is going on here.
                                parseErrorLines.Add(counter + " (not enough minerals^wparameters, " + userInfoAll.Length + "): " + line);
                            }
                        }
                    }
                }
                else {
                    if (prefix.Equals("Kill:")){
                        string[] blocks = Regex.Split(payload, "\\s+");
                        if (blocks.Length > 3) {
                            string attackerString = blocks[1];
                            int attackerNumber;
                            success = Int32.TryParse(attackerString, out attackerNumber);
                            if (!success) attackerNumber = -1;
                            int victimNumber;
                            success = Int32.TryParse(blocks[2], out victimNumber);
                            if (!success) victimNumber = -1;
                            string weapon = blocks[3];
                            weapon = Regex.Replace(weapon, ":", String.Empty);
                            int weaponNumber;
                            success = Int32.TryParse(weapon, out weaponNumber);
                            if (!success) weaponNumber = -1;
                            if (   victimNumber   > -1
                                && attackerNumber > -1
                                && weaponNumber   > -1
                                && weaponNumber < Weapons.weaponNames.Length) {
                                Player victim = PlayerMapping.getPlayer((uint) victimNumber);
                                if (victim == null) { //Should not be the case even if bots are excluded as they are kept in the Mapping but are not used in the score calculations if excluded.
                                    parseErrorLines.Add(counter + " (player ID unknown): " + line);
                                }
                                else {
                                    if (attackerString.Equals("1022")) { // The map killed the idiot. This is not to be included in ELO. (This would let ELOs go to hell as a result, do not do it! Believe me, I tried...). But still counts as death!
                                        victim.addDeath();
                                        victim.addWeaponsEndured((uint) weaponNumber);
                                    }
                                    else {
                                        Player attacker = PlayerMapping.getPlayer((uint) attackerNumber);
                                        if (attacker == null) {
                                            parseErrorLines.Add(counter + " (player ID unknown): " + line);
                                        }
                                        else {
                                            if (!(   (   attacker.isBot
                                                      || victim.isBot)
                                                  && !withBots)) {
                                                string attackerName = attacker.getName();
                                                string   victimName =   victim.getName();
                                                if (!(   (   Player.unnamedPlayerNicks.Contains(attackerName)
                                                          || Player.unnamedPlayerNicks.Contains(victimName))
                                                      && !withUnnamed)) {// Those might mess up things so better make sure they are actually wanted.
                                                    list.AddMissing(attacker); //This also implies (as ConnectionInfoChange does not add players when it finds new ones), that players not getting killed and not killing anything will never be listed!
                                                    list.AddMissing(victim);
                                                    attacker = list.getPlayer(attacker);// Looks strange, but what if adding did not actually add a new player?
                                                    victim   = list.getPlayer(victim);// Then the existing player has the actual values and we want those, nothing else.
                                                    Incident newVictimIncident = new Incident(victimName);
                                                    if (attackerName.Equals(victimName)) {// Yes, this could happen. Think for example of splash damage or the Ultritium Mine.
                                                        victim.addDeath();
                                                        victim.addWeaponsEndured((uint) weaponNumber); //Don't give the fool credit for killing himself by counting it in attacks, but count his own stupidity by counting in enduring..
                                                    }
                                                    else {
                                                        newVictimIncident.Add();
                                                        attacker.addVictim(newVictimIncident);
                                                        attacker.addKill();
                                                        attacker.addWeaponsUsage((uint) weaponNumber);
                                                        victim.addDeath();
                                                        victim.addWeaponsEndured((uint) weaponNumber);
                                                        Incident newAttackIncident = new Incident(attackerName);
                                                        newAttackIncident.Add();
                                                        victim.addAttack(newAttackIncident);
                                                        Elo.updateEloScores(attacker, victim);
                                                        Encounter newEncounter = new Encounter(attackerName, victimName);
                                                        addEncounter(newEncounter);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else {
                                parseErrorLines.Add("Kill-line has strange values: " + line);
                            }
                        }
                        else {
                            parseErrorLines.Add("Kill-line too few values: " + line);
                        }
                    }
                }
            }
            else {
                parseErrorLines.Add("Line to short: " + line);
            }
        }
        if (minEncounters > 0) list.removeBeginners(minEncounters);
        list.sort(false);
        if (toSTDOUT) {
            list.dump(withBots, true);
        }
        else {
            list.dumpToJSON(outputFileName, withBots);
            list.dumpToFile(withBots, true, "dump.txt");
            Console.WriteLine(Elo.EloReport(list.getList(), encounters, true));
        }
        if (reportParseErrors) {
            Console.WriteLine("Parse-Errors:");
            dumpList(parseErrorLines, "parseErrorLines");
        }
    }

    public static void dumpArray(string[] array, string name) {
        Console.WriteLine("---- Dumping Array " + name + ", Länge: " + array.Length);
        foreach (string s in array) Console.WriteLine(s);
        Console.WriteLine("----");
    }

    public static void dumpList(List<string> list, string name) {
        Console.WriteLine("---- Dumping List " + name + ", Länge: " + list.Count);
        foreach (string s in list) Console.WriteLine(s);
        Console.WriteLine("----");
    }

    public static void addEncounter(Encounter e) {
        if (encounters.Contains(e)) {
            foreach (Encounter e2 in encounters) if (e.Equals(e2)) e2.addOccurence();
        }
        else {
            e.addOccurence();
            encounters.Add(e);
        }
    }
}
