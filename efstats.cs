using System; // Simple WriteLine
using System.IO; // File
using System.Collections.Generic; // Lists
using System.Text.RegularExpressions; // Split
// uses https://github.com/JamesNK/Newtonsoft.Json

namespace EfStats {
    public static class efstats {
        public static readonly string Version = "0.1.1";
        public static List<Encounter> encounters = new List<Encounter>();
        public static void Main(string[] args) {
            ConsoleParameters.InitializeParameters("--",
                                                   new ParameterDefinition[] {
                                                       new ParameterDefinition("inname",
                                                                               ParameterType.String,
                                                                               true,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "This defines the name of the input file containing the log data to be analyzed. This parameter is required.",
                                                                               delegate(Parameter p){
                                                                                   string filename = p.getStringValues()[0];
                                                                                   if (!File.Exists(filename)) {
                                                                                       return "A file with the name '" + filename + "' does not seem to exist.";
                                                                                   }
                                                                                   return null;
                                                                               }),
                                                       new ParameterDefinition("outname",
                                                                               ParameterType.String,
                                                                               false,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "This defines the name of the output file that will contain the analysis results. This parameter is optional. If it is not set, the data is written to STDOUT. Important: Existing files will be overwritten without asking!",
                                                                               delegate(Parameter p){
                                                                                   string filename = p.getStringValues()[0];
                                                                                   if (filename.Equals("")) {
                                                                                       return "Please provide a filename if you use this option.";
                                                                                   }
                                                                                   return null;
                                                                               }),
                                                       new ParameterDefinition("withbots",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "By default no bot players are included in the analysis. Setting this switch includes them."),
                                                       new ParameterDefinition("withunnamed",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "By default no players with the nick names 'Redshirt', 'RedShirt' or 'UnnamedPlayer' are included in the analysis. Those are the standard nick names that the game uses when the player did not change his name. This means, various players use this nick, which will distort the results. Setting this switch includes them anyways."),
                                                       /* new ParameterDefinition("statssave",
                                                                               ParameterType.String,
                                                                               false,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "Use this parameter to provide a save file name. By default, this program analyses the data from the entire log file and writes the results to the output file. If you require an analysis of a log file being continuosly extended (e. g. the live log file of a running EF server) this filename is used to load the analysis results from the last analysis run, and continue where the last analysis left off. After the analysis the save file will be updated with the now added new analysis data. If there exists no analysis file under this name it will be created after analysis and the log analysis starts from the beginning of the file."),*/
                                                       new ParameterDefinition("outformat",
                                                                               ParameterType.String,
                                                                               false,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "This optional parameter defines the output format. You can choose from 'text', 'csv', 'json' and 'html'. Default is 'html'.",
                                                                               delegate (Parameter p) {
                                                                                   List<string> allowedOutFormats = new List<string>() {"text", "csv", "html", "json"};
                                                                                   string outformat = p.getStringValues()[0];
                                                                                   if (!allowedOutFormats.Contains(outformat)) {
                                                                                       return "The output format type '" + outformat + "' is unknown.";
                                                                                   }
                                                                                   return null;
                                                                               }),
                                                       new ParameterDefinition("sortorder",
                                                                               ParameterType.String,
                                                                               false,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "This optional parameter defines the value that is used for the ranking. You can choose from 'elo', 'ratio' and 'eff' (efficiency). Default is 'elo'.",
                                                                               delegate(Parameter p) {
                                                                                   List<string> allowedSortOrders = new List<string>() {"elo", "eff", "ratio"};
                                                                                   string sortorder = p.getStringValues()[0];
                                                                                   if (!allowedSortOrders.Contains(sortorder)) {
                                                                                       return "The sort order type '" + sortorder + "' is unknown.";
                                                                                   }
                                                                                   return null;
                                                                               }),
                                                       new ParameterDefinition("minenc",
                                                                               ParameterType.Uinteger,
                                                                               false,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "This optional parameter defines the minimum number of encounters a player has to be part of (sum of fragging or being fragged) in order to be listed. Default is 30. This prevents players who just droped by for a few frags and were never heard from again from distorting results. Below a certain number of frags the scoring actually cannot be accurate enough for a reasonable ranking."),
                                                       new ParameterDefinition("rpe",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "The optional switch Report Parse Errors (rpe) makes the analyzer report problems with the input log file. By default such problems are not reported."),
                                                       new ParameterDefinition("eloreport",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "Setting this switch makes efstats additionally print an ELO score probability analysis to STDOUT."),
                                                       new ParameterDefinition("help",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "Prints this help and stops, regardless of the presence of other parameters or values."),
                                                   },
                                                   args,
                                                   "EFStats " + Version + ": This program takes a server log file from the game 'Star Trek: Voyager Elite Force', analyses the occuring frags of players and calculates a ranking based on ELO score/K:D-ratio/efficiency of the players, derived by the frag occurences of all involved players. You can set various output formats (text, HTML, JSON, CSV) and if needed, save the results for continuation of the analysis at a later time with an extended log file.",
                                                   true);

            if (ConsoleParameters.getParameterByName("help").getBoolValue()) {
                ConsoleParameters.printParameterHelp();
                Environment.Exit(0);
            }
            string inputFileName = ConsoleParameters.getParameterByName("inname").getStringValues()[0];
            string outputFileName = null;
            if (ConsoleParameters.getParameterByName("outname").getNumberOfValues() == 1) {
                outputFileName = ConsoleParameters.getParameterByName("outname").getStringValues()[0];
            }

            string outFormat = "html";
            if (ConsoleParameters.getParameterByName("outformat").getNumberOfValues() == 1)
                outFormat = ConsoleParameters.getParameterByName("outformat").getStringValues()[0];
            string sortOrder = "elo";
            if (ConsoleParameters.getParameterByName("sortorder").getNumberOfValues() == 1)
                sortOrder = ConsoleParameters.getParameterByName("sortorder").getStringValues()[0];
            uint minEncounters = 30;
            if (ConsoleParameters.getParameterByName("minenc").getNumberOfValues() == 1)
                minEncounters = ConsoleParameters.getParameterByName("minenc").getUintegerValues()[0];



            bool withBots = ConsoleParameters.getParameterByName("withbots").getBoolValue();
            bool withUnnamed = ConsoleParameters.getParameterByName("withunnamed").getBoolValue();
            bool reportParseErrors = ConsoleParameters.getParameterByName("rpe").getBoolValue();
            bool eloreport = ConsoleParameters.getParameterByName("eloreport").getBoolValue();

            uint counter = 0;
            uint playerNumber = 1022; //World...
            bool isBot = false;
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
                    parseErrorLines.Add("Line too short: " + line);
                }
            }
            if (minEncounters > 0) list.removeBeginners(minEncounters);
            list.sort(sortOrder, false);
            if (outputFileName == null) {
                List<string> outdata = new List<string>();
                switch (outFormat) {
                    case "json":
                        outdata.Add(list.getJSON(withBots));
                        break;
                    case "text":
                        outdata = list.getTextDump(withBots,
                                                   false);
                        break;
                    case "html":
                        outdata = list.getHTMLDump(withBots);
                        break;
                    case "csv":
                        outdata = list.getCsvDump(withBots,
                                                  false);
                        break;
                    default:
                        break;
                }
                dumpList(outdata);
            }
            else {
                switch (outFormat) {
                    case "json":
                        list.dumpToJSON(outputFileName,
                                        withBots);
                        break;
                    case "text":
                        list.dumpToText(withBots,
                                        true,
                                        outputFileName);
                        break;
                    case "html":
                        list.dumpToHTML(withBots,
                                        outputFileName);
                        break;
                    case "csv":
                        list.dumpToCsv(withBots,
                                       true,
                                       outputFileName);
                        break;
                    default:
                        break;
                }
                if (eloreport) Console.WriteLine(Elo.EloReport(list.getList(), encounters, true));
            }
            if (reportParseErrors) {
                Console.WriteLine("Parse-Errors:");
                dumpList(parseErrorLines);
            }
        }

        public static void dumpList(List<string> list) {
            foreach (string s in list) Console.WriteLine(s);
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
}
