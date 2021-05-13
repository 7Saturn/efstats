using System; // Simple WriteLine
using System.IO; // File
using System.Collections.Generic; // Lists
using System.Text.RegularExpressions; // Split
// uses https://github.com/JamesNK/Newtonsoft.Json

/*
Ideas for improvement/expansion:
 * MD5-sum for each line and the lines predecessor for the stats save. This way when saving, the current MD5-sum can be saved in the stats save and compared during the re-run, using the stats save file. This way you can ensure it was the same file that was used last time. Possible problem: Might defeat the purpose of the save file, to reduce system load during consecutive runs on logs with added lines.
 * Maybe an overall stats server might be possible, connecting multiple servers into one system.
*/


namespace EfStats {
    public static class efstats {
        public static readonly string Version = "0.1.2";
        public static readonly uint saveFileRevision = 1;
        public static List<Encounter> encounters = new List<Encounter>();
        public static bool debug = false;
        public static bool elodetails = false;
        public static uint minEncounters = 100;
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
                                                       new ParameterDefinition("statssave",
                                                                               ParameterType.String,
                                                                               false,
                                                                               1,
                                                                               1,
                                                                               true,
                                                                               "Use this parameter to provide a save file name. By default, this program analyses the data from the entire log file and writes the results to the output file (or STDOUT). If you require an analysis of a log file that is being continuosly extended (e. g. the live log file of a running EF server) this filename is used to load the analysis results from the last analysis run, and continue where the last analysis run left off. After the analysis the save file will be updated with the now added new analysis data. If there exists no analysis file under this name already it will be created after analysis and the log analysis starts from the beginning of the log file, first. If you use this feature, you really should consider setting g_logsync = 1 in your EF server's config."),
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
                                                                               "This optional parameter defines the minimum number of encounters a player has to be part of (sum of fragging or being fragged) in order to be listed. Default is " + minEncounters + ". This prevents players who just droped by for a few frags and were never heard from again from distorting results. Below a certain number of frags the scoring actually cannot be accurate enough for a reasonable ranking."),
                                                       new ParameterDefinition("rpe",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "The optional switch Report Parse Errors (rpe) makes the analyzer report problems with the input log file. By default such problems are not reported."),
                                                       new ParameterDefinition("verbose",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "The optional switch verbose makes EF Stats print some more information during the analysis run."),
                                                       new ParameterDefinition("debug",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "This prints additional information during stats analysis runs, useful for debugging. Warning: This can be quite extensive, depending on the size of the log file being analyzed. Do not use it in productive scenarios."),
                                                       new ParameterDefinition("eloreport",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "Setting this switch makes EF Stats additionally print an Elo score probability analysis to STDOUT."),
                                                       new ParameterDefinition("elodetails",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "Setting this switch makes EF Stats additionally print the Elo score dynamics (changes with every frag) to STDOUT."),
                                                       new ParameterDefinition("help",
                                                                               ParameterType.Boolean,
                                                                               false,
                                                                               0,
                                                                               0,
                                                                               false,
                                                                               "Prints this help and stops, regardless of the presence of other parameters or values."),
                                                   },
                                                   args,
                                                   "EFStats " + Version + ": This program takes a server log file from the game 'Star Trek: Voyager Elite Force', analyses the occuring frags of players and calculates a ranking based on Elo score/K:D-ratio/efficiency of the players, derived by the frag occurences of all involved players. You can set various output formats (text, HTML, JSON, CSV) and if needed, save the results for continuation of the analysis at a later time with an extended log file.",
                                                   true);

            bool verbose = ConsoleParameters.getParameterByName("verbose").getBoolValue();
            debug = ConsoleParameters.getParameterByName("debug").getBoolValue();
            if (ConsoleParameters.getParameterByName("help").getBoolValue()) {
                if (verbose) Console.WriteLine("--help switch found");
                ConsoleParameters.printParameterHelp();
                Environment.Exit(0);
            }
            string inputFileName = ConsoleParameters.getParameterByName("inname").getStringValues()[0];
            if (verbose) Console.WriteLine("File to be analysed: " + inputFileName);
            string outputFileName = null;
            if (ConsoleParameters.getParameterByName("outname").getNumberOfValues() == 1) {
                outputFileName = ConsoleParameters.getParameterByName("outname").getStringValues()[0];
                if (verbose) Console.WriteLine("Destination file for the results: " + outputFileName);
            }

            string saveFileName = null;
            if (ConsoleParameters.getParameterByName("statssave").getNumberOfValues() == 1) {
                saveFileName = ConsoleParameters.getParameterByName("statssave").getStringValues()[0];
                if (verbose) Console.WriteLine("Stats save file: " + saveFileName);
            }

            bool readSaveFile = false;
            if (saveFileName != null) readSaveFile = File.Exists(saveFileName);
            if (verbose) {
                if (readSaveFile){
                    Console.WriteLine("Stats save file exists and will be read first.");
                }
                else {
                    Console.WriteLine("Stats save file does not exist, yet.");
                }
            }

            string outFormat = "html";
            if (ConsoleParameters.getParameterByName("outformat").getNumberOfValues() == 1) {
                outFormat = ConsoleParameters.getParameterByName("outformat").getStringValues()[0];
                if (verbose) Console.WriteLine ("Requested format: " + outFormat);
            }
            else {
                if (verbose) Console.WriteLine ("Using format: " + outFormat);
            }

            string sortOrder = "elo";
            if (ConsoleParameters.getParameterByName("sortorder").getNumberOfValues() == 1) {
                sortOrder = ConsoleParameters.getParameterByName("sortorder").getStringValues()[0];
                if (verbose) Console.WriteLine ("Requested sort order: " + sortOrder);
            }
            else {
                if (verbose) Console.WriteLine ("Using sort order: " + sortOrder);
            }
            if (ConsoleParameters.getParameterByName("minenc").getNumberOfValues() == 1) {
                minEncounters = ConsoleParameters.getParameterByName("minenc").getUintegerValues()[0];
                if (verbose) Console.WriteLine ("Using requested number of encounters filter: " + minEncounters);
            }
            else {
                if (verbose) Console.WriteLine ("Using standard minimal number of encounters filter: " + minEncounters);
            }

            bool withBots = ConsoleParameters.getParameterByName("withbots").getBoolValue();
            bool withUnnamed = ConsoleParameters.getParameterByName("withunnamed").getBoolValue();
            bool reportParseErrors = ConsoleParameters.getParameterByName("rpe").getBoolValue();
            bool eloreport = ConsoleParameters.getParameterByName("eloreport").getBoolValue();
            elodetails = ConsoleParameters.getParameterByName("elodetails").getBoolValue();

            if (verbose) {
                if (withBots) Console.WriteLine ("Including bots into analysis.");
                if (withUnnamed) Console.WriteLine ("Allowing all player names.");
                if (reportParseErrors) Console.WriteLine ("Parsing errors will be reported.");
                if (eloreport) Console.WriteLine ("At the end there will be a report on the Elo score <-> encounters correlation.");
                if (elodetails) Console.WriteLine ("During analysis the Elo score changes will be printed to STDOUT.");
            }
            uint counter = 0;
            uint playerNumber = 1022; //World...
            bool isBot = false;
            string nickName = "";
            List<string> parseErrorLines = new List<string>();
            PlayerList list = new PlayerList();
            uint noOfLinesToSkip = 0;
            if (readSaveFile) {
                if (verbose) Console.WriteLine("Trying to read the save file...");
                SaveFile saveFile = new SaveFile(saveFileRevision,
                                                 saveFileName,
                                                 withUnnamed,
                                                 withUnnamed,
                                                 0);
                try {
                    saveFile.Load();
                }
                catch (WrongRevisionException e) {
                    string problem = e.Message;
                    Console.WriteLine(problem);
                    Environment.Exit(1);
                }
                catch (WrongSettingsException e) {
                    string problem = e.Message;
                    Console.WriteLine(problem);
                    Console.WriteLine("Re-run EF Stats with proper switches set or use another save file.");
                    Environment.Exit(2);
                }
                catch (SaveFileDefectException e) {
                    string problem = e.Message;
                    Console.WriteLine(problem);
                    Environment.Exit(3);
                }
                PlayerMapping.setMapping(saveFile.playerMapping) ;
                list = saveFile.playerList;
                encounters = saveFile.encounters;
                noOfLinesToSkip = saveFile.numberOfLines;
                if (verbose) Console.WriteLine("Skipping first " + noOfLinesToSkip + " lines of input log file.");
            }
            foreach (string line in File.ReadLines(inputFileName)) {
                counter++;
                if (counter > noOfLinesToSkip) {
                    if (line.Length > 6) {// At least the time stamp *must* be present, but honestly, that's still too nice.
                        string payload = line.Substring(7);
                        string[] elements = Regex.Split(payload, "\\s+");
                        string prefix = elements[0];
                        bool success;
                        if (prefix.Equals("ClientUserinfoChanged:")) {// This actually only keeps track of the player mapping. EF logs player's IDs /and/ names, but as names are fucky to parse, we will only use IDs and track the mapping of IDs to names here ourselves.
                            if (debug) Console.Write("User ID: " + elements[1]);
                            if (UInt32.TryParse(elements[1], // Player's ID
                                                out playerNumber)) {
                                string[] userInfoAll = Regex.Split(payload, "\\\\");
                                Player tempGuy  = null;
                                if (userInfoAll.Length == 16) { //Very likely a human
                                    if (debug) Console.Write(" = human ");
                                    nickName = userInfoAll[1];
                                    isBot = false;
                                    if (debug) Console.WriteLine(nickName);
                                    tempGuy = new Player(nickName, isBot);
                                    PlayerMapping.setPlayer(playerNumber, tempGuy);
                                }
                                else {
                                    if (userInfoAll.Length == 18) { //Very likely a bot
                                        if (debug) Console.Write(" = bot ");
                                        nickName = userInfoAll[1];
                                        if (userInfoAll[16].Equals("skill")) { // But let's actually recheck that
                                            isBot = true;
                                        }
                                        if (debug) Console.WriteLine(nickName);
                                        tempGuy = new Player(nickName, isBot);
                                        PlayerMapping.setPlayer(playerNumber, tempGuy); // Always add, regardles of being a Bot or not
                                    }
                                    else {// There is that strange behavior, that there is just a line of the form »ClientUserinfoChanged: <number> <number>« that actually cannot come from the server, according to the code. Strange shit is going on here.
                                        if (debug) Console.WriteLine(", parse error, split brought up to little parameters: line");
                                        parseErrorLines.Add(counter + " (not enough minerals^wparameters, " + userInfoAll.Length + "): " + line);
                                    }
                                }
                            }
                        }
                        else {
                            if (prefix.Equals("Kill:")){
                                if (debug) Console.Write("\nKill: ");
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
                                    if (debug) Console.WriteLine(attackerNumber + " kills " + victimNumber + " with " + weaponNumber);
                                    if (   victimNumber   > -1
                                        && attackerNumber > -1
                                        && weaponNumber   > -1
                                        && weaponNumber   < Weapons.weaponNames.Length) {
                                        Player victim = PlayerMapping.getPlayer((uint) victimNumber);
                                        if (victim == null) { //Should not be the case even if bots are excluded as they are kept in the Mapping but are not used in the score calculations if excluded.
                                            if (debug) Console.WriteLine(" player ID " + victimNumber + " unknown");
                                            parseErrorLines.Add(counter + " (player ID unknown): " + line);
                                        }
                                        else {
                                            if (attackerString.Equals("1022")) { // The map killed the idiot. This is not to be included in Elo. (This would let Elos go to hell as a result, do not do it! Believe me, I tried...). But still counts as death!
                                                if (debug) Console.WriteLine(" World-Kill (suicide with map item)");
                                                victim.addDeath();
                                                victim.addWeaponsEndured((uint) weaponNumber);
                                            }
                                            else {
                                                Player attacker = PlayerMapping.getPlayer((uint) attackerNumber);
                                                if (attacker == null) {
                                                    if (debug) Console.WriteLine(" player ID " + attackerNumber + " unknown");
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
                                                            if (attackerName.Equals(victimName)) {// Yes, this could happen. Think for example of splash damage or the Ultritium Mine.
                                                                if (debug) Console.WriteLine(" suicide...");
                                                                victim.addDeath();
                                                                victim.addWeaponsEndured((uint) weaponNumber); //Don't give the fool credit for killing himself by counting it in attacks, but count his own stupidity by counting in enduring..
                                                            }
                                                            else {
                                                                Incident newVictimIncident = new Incident(victimName);
                                                                attacker.addVictim(newVictimIncident);
                                                                attacker.addKill();
                                                                attacker.addWeaponsUsage((uint) weaponNumber);
                                                                victim.addDeath();
                                                                victim.addWeaponsEndured((uint) weaponNumber);

                                                                if (debug) Console.WriteLine(" " + attacker.getName() + " kills " + victim.getName() + ", adding incident");
                                                                Incident newAttackIncident = new Incident(attackerName);
                                                                victim.addAttack(newAttackIncident);
                                                                Elo.updateEloScores(attacker, victim);
                                                                Encounter newEncounter = new Encounter(attackerName, victimName);
                                                                addEncounter(newEncounter);
                                                            }
                                                        }
                                                        else {
                                                            if (debug) Console.WriteLine(" player is skipped due to either name filter or bot filter.");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else {
                                        parseErrorLines.Add("Kill-line has strange values: " + line);
                                        if (debug) Console.WriteLine("Kill-line has strange values: " + line);
                                    }
                                }
                                else {
                                    parseErrorLines.Add("Kill-line too few values: " + line);
                                    if (debug) Console.WriteLine("Kill-line too few values: " + line);
                                }
                            }
                        }
                    }
                    else {
                        parseErrorLines.Add("Line too short: " + line);
                        if (debug) Console.WriteLine("Line too short: " + line);
                    }
                }
            }
            if (minEncounters > 0) list.removeBeginners(minEncounters);
            list.sort(sortOrder, false);
            if (outputFileName == null) {
                if (debug) Console.WriteLine("Screen dump");
                if (verbose) Console.WriteLine("Exporting results as " + outFormat + " on screen.");
                List<string> outdata = new List<string>();
                switch (outFormat) {
                    case "json":
                        if (debug) Console.WriteLine("As JSON");
                        outdata.Add(list.getJSON(withBots));
                        break;
                    case "text":
                        if (debug) Console.WriteLine("As text");
                        outdata = list.getTextDump(withBots,
                                                   false);
                        break;
                    case "html":
                        if (debug) Console.WriteLine("As HTML");
                        outdata = list.getHTMLDump(withBots);
                        break;
                    case "csv":
                        if (debug) Console.WriteLine("As CSV");
                        outdata = list.getCsvDump(withBots,
                                                  false);
                        break;
                    default:
                        break;
                }
                if (debug) Console.WriteLine("Dumping...");
                dumpList(outdata);
            }
            else {
                if (debug) Console.WriteLine("File dump to " + outputFileName);
                if (verbose) Console.WriteLine("Exporting results as " + outFormat + " into file " + outputFileName + ".");
                switch (outFormat) {
                    case "json":
                        if (debug) Console.WriteLine("As JSON");
                        list.dumpToJSON(outputFileName,
                                        withBots);
                        break;
                    case "text":
                        if (debug) Console.WriteLine("As text");
                        list.dumpToText(withBots,
                                        true,
                                        outputFileName);
                        break;
                    case "html":
                        if (debug) Console.WriteLine("As HTML");
                        list.dumpToHTML(withBots,
                                        outputFileName);
                        break;
                    case "csv":
                        if (debug) Console.WriteLine("As CSV");
                        list.dumpToCsv(withBots,
                                       true,
                                       outputFileName);
                        break;
                    default:
                        break;
                }
                if (eloreport){
                    if (debug) Console.WriteLine("Doing Elo report...");
                    Console.WriteLine(Elo.EloReport(list.getList(), encounters, true));
                }
            }
            if (reportParseErrors) {
                if (debug) Console.WriteLine("Printing parse errors...");
                Console.WriteLine("Parse-Errors:");
                dumpList(parseErrorLines);
            }
            if (saveFileName != null) {
                if (debug) Console.WriteLine("Writing save file to file " + saveFileName + "...");
                if (verbose) Console.WriteLine("Writing save file to file " + saveFileName + "...");
                //Hier sollte das Save geschrieben werden.
                SaveFile saveFile = new SaveFile(saveFileRevision,
                                                 saveFileName,
                                                 withUnnamed,
                                                 withUnnamed,
                                                 counter,
                                                 PlayerMapping.getMapping(),
                                                 list,
                                                 encounters);
                try {
                    saveFile.Save();
                }
                catch (Exception e) {
                    Console.WriteLine("Could not save the save file data into " + saveFileName);
                    Console.WriteLine(e.Message);
                    Environment.Exit(4);
                }
                if (verbose) Console.WriteLine("Done!");
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
