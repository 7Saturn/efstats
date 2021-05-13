using System;
using System.Linq; // File.ReadLines().First()
using System.IO;//File
using System.Collections.Generic;
using Newtonsoft.Json; //From Newtonsoft.Json.dll
using System.Text.RegularExpressions; // Split

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
            string playerHeading = "Name";
            string isBotHeading = "Is a Bot";
            string eloHeading = "Elo";
            string efficiencyHeading = "Efficiency";
            string ratioHeading = "Ratio";
            string killsHeading = "Score";
            string deathsHeading = "Eliminated";
            string weaponsUsedHeading = "Weapons Used";
            string weaponsEnduredHeading = "Weapons Endured";
            string weaponsMaxUsedHeading = "Favorite Weapon";
            string weaponsMaxEnduredHeading = "Worst Weapon";
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
                maxElos = increaseIfNeeded(maxElos, (uint)(p.getEloString().Length));
                maxEffs = increaseIfNeeded(maxEffs, (uint)(p.getRatioString().Length));
                maxRatios = increaseIfNeeded(maxRatios, (uint)p.getEfficiencyString().Length);
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
            if (!withBots) maxIsBot = 0;

            List<string> lines = new List<string>();
            string line = playerHeading.PadRight((int)maxNicks);
            if (withBots) line += isBotHeading.PadRight((int)maxIsBot);
            line += eloHeading.PadRight((int)maxElos)
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

                string playerName = p.getName();
                if (noColor) playerName = Player.noColors(playerName);
                line = playerName.PadRight((int) maxNicks);
                if (withBots) {
                    if (p.getIsBot()) {
                        line += ("yes").PadRight((int) maxIsBot);
                    }
                    else {
                        line += ("no").PadRight((int) maxIsBot);
                    }
                }
                line += p.getEloString().PadRight((int)maxElos) +
                    p.getEfficiencyString().PadRight((int)maxEffs) +
                    p.getRatioString().PadRight((int)maxRatios) +
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

        public List<string> getHTMLDump(bool withBots) {
            //Here, we treat colors differently, as in HTML we /can/ set colors, ans so we should always!
            List<string> lines = new List<string>();
            lines.Add("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">");
            lines.Add("<html>");
            lines.Add("<head>");
            lines.Add("<meta http-equiv=\"CONTENT-TYPE\" content=\"text/html; charset=utf8\">");
            lines.Add("<title>Star Trek: Voyager Elite Force Stats dump</title>");
            lines.Add(@"    <script>
      function alter_arrows (element_id, replacement) {
          if (document.getElementById(element_id).innerHTML == '&lt;&gt;')
          {
              document.getElementById(element_id).innerHTML = '><';
              document.getElementById(element_id).title = 'Click to hide ' + replacement;
          }
          else
          {
              document.getElementById(element_id).innerHTML = '<>';
              document.getElementById(element_id).title = 'Click to show ' + replacement;
          }
      }
      function switch_elements(class_name) {
          var x = document.getElementsByClassName(class_name);
          for (i = 0; i < x.length; i++) {
              if (x[i].style.display == 'none')
              {
                  x[i].style.display = 'table-cell';
              }
              else
              {
                  x[i].style.display = 'none'
              }
          }
      }
    </script>");
            lines.Add(@"<style>
body
{
    background:#000000;
    color:#8E72BA;
    font-family: sans-serif;
    overflow: auto;
}
.white
{
    color:#FFFFFF;
}
.black
{
    color:#000000;
}
.red
{
    color:#FF0000;
}
.green
{
    color:#00FF00;
}
.yellow
{
    color:#FFFF00;
}
.blue
{
    color:#0000FF;
}
.cyan
{
    color:#00FFFF;
}
.magenta
{
    color:#FF00FF;
}
.ocher
{
    color: #FFEA94;
}
.button
{
    background: #8E72BA;
    color: #000000;
    font-weight: bold;
    font-family: sans;
    border-top-left-radius:15px;
    border-top-right-radius:15px;
    border-bottom-left-radius:15px;
    border-bottom-right-radius:15px;
    padding-left: 5px;
    padding-right: 5px;
    cursor: pointer;
}
table
{
    table-layout:fixed;
    border-collapse: collapse;
    border-color: #BC7200;
    border: none;
    color: #FFFFFF;
}
td
{
    border-width: 2px;
    padding: 3px;
    white-space: nowrap;
    vertical-align: baseline;
    border-width: 0px;
}
.numbercell
{
    text-align: right;
}
th
{
    border-width: 0px;
    padding: 3px;
    white-space: nowrap;
    background: #BC7200;
    color: #160C00;
    position: -webkit-sticky;
    position: sticky;
    top: 0;
    z-index: 2;
}
tr:nth-child(even)
{
    background-color: #400000;
}
tr:nth-child(odd)
{
    background-color: #000040;
}
.leftborder
{
    background: #BC7200;
}
.upperleftcorner
{
    background: #BC7200;
    display: inline-block;
    width: 1.5em;
    border-color: #BC7200;
}
.rightcorner
{
    border-top-right-radius:15px;
    border-bottom-right-radius:15px;
    background: #BC7200;
    display: inline-block;
    width: 1.5em;
    height: 30px;
    border-color: #BC7200;
}
.rightcornercontainer
{
    background: #000000;
    padding: 0px;
    border-left-width: 4px;
}
.lowerleftcornercontainer
{
    background: #000000;
    padding: 0px;
    border-width: 0px;
}
.lowerleftcorner
{
    border-bottom-left-radius:10px;
    background: #BC7200;
    display: inline-block;
    width: 100%;
    height: 30px;
    border-color: #BC7200;
}
.centered
{
    text-align: center;
}
</style>");
            lines.Add("<body>");
            lines.Add("<table border = 1>");
            //Table
            string line = "<tr><th><span class=\"upperleftcorner\"></span></th><th title=\"Ranking of Players\">Rank</th><th title=\"The Name the Player Gave Himself\">Name</th>";
            if (withBots) line += "<th title=\"Is this Player Actually a Bot?\">Is a Bot</th>";
            line += "<th title=\"Skill Value Considering the Different Skill Levels of Both Opponents When Counting a Frag\">Elo</th><th title=\"Equals Score Devided by the Sum of Score and Eliminated\">Efficiency</th><th title=\"Score Devided by Eliminated\">Ratio</th><th title=\"Achieved Frags\">Score</th><th title=\"Times the Player Suicided, Got Fragged or Had an Accident\">Eliminated</th><th title=\"The Player did the Highest Number of Frags With this Weapon\">Favorite Weapon</th><th title=\"The Player got Fragged the Highest Number of Times With This Weapon\">Worst Weapon <span class=\"button\" id=\"initial1\" onclick=\"switch_elements('switchable1'); alter_arrows('initial1', 'Weapons Report');\" title=\"Click to show Weapons Report\">&lt;&gt;</span></th><th title=\"How Often did the Player Frag with the Different Weapons?\" class=\"switchable1\">Weapons Used</th><th title=\"How Often did the Player get Fragged by the Different Weapons?\" class=\"switchable1\">Weapons Endured</th><th title=\"The Opponent that Fragged this Player Most\">Worst Enemy</th><th title=\"The Opponent this Player Fragged the Most\">Easiest Target <span class=\"button\" id=\"initial2\" onclick=\"switch_elements('switchable2'); alter_arrows('initial2', 'Opponent Report');\" title=\"Click to show Opponent Report\">&lt;&gt;</span></th><th title=\"How Often did Other Players Frag this Player?\" class=\"switchable2\">Attackers</th><th title=\"How Often did This Player Frag Other Players?\" class=\"switchable2\">Victims</th><th class=\"rightcornercontainer\"><span class=\"rightcorner\"></span></th></tr>";
            lines.Add(line);
            uint rankCounter = 0;
            foreach(Player p in list) {
                rankCounter++;
                line = "<tr><td class=\"leftborder\"></td><td class=\"numbercell ocher\">" + rankCounter + "</td>";
                line += "<td>" + nameAsHTML(p.getName()) + "</td>";
                if (withBots) {
                    line += "<td class=\"centered ocher\">";
                    if (p.getIsBot()) {
                        line += ("yes");
                    }
                    else {
                        line += ("no");
                    }
                    line += "</td>";
                }
                line += "<td class=\"numbercell ocher\">" + p.getEloString() + "</td>"
                    + "<td class=\"centered ocher\">" + p.getEfficiencyString() + "</td>"
                    + "<td class=\"numbercell ocher\">" + p.getRatioString() + "</td>"
                    + "<td class=\"numbercell yellow\">" + p.getKills() + "</td>"
                    + "<td class=\"numbercell red\">" + p.getDeaths() + "</td>";

                line += "<td class=\"ocher\">" + Weapons.weaponNames[p.getWeaponUsedMaxId()] + " (" + p.getWeaponUsedMaxCount() + ")</td>";
                line += "<td class=\"ocher\">" + Weapons.weaponNames[p.getWeaponEnduredMaxId()] + " (" + p.getWeaponEnduredMaxCount() + ")</td>";

                line += "<td class=\"switchable1\">";
                uint weaponUsedCounter = sumUintArray(p.weaponsUsage);
                if (weaponUsedCounter > 0) {
                    line += "<ul>";
                    for (uint counter = 0; counter < p.weaponsUsage.Length; counter++) {
                        uint weaponCount = p.weaponsUsage[counter];
                        if (weaponCount > 0) {
                            string weaponName = Weapons.weaponNames[counter];
                            line += "<li class=\"ocher\">" + weaponName + " (" + weaponCount + ")</li>";
                        }
                    }
                    line += "</ul>";
                }
                line += "</td>";
                line += "<td class=\"switchable1\">";
                uint weaponEnduredCounter = sumUintArray(p.weaponsEndured);
                if (weaponEnduredCounter > 0) {
                    line += "<ul>";
                    for (uint counter = 0; counter < p.weaponsEndured.Length; counter++) {
                        uint weaponCount = p.weaponsEndured[counter];
                        if (weaponCount > 0) {
                            string weaponName = Weapons.weaponNames[counter];
                            line += "<li class=\"ocher\">" + weaponName + " (" + weaponCount + ")</li>";
                        }
                    }
                    line += "</ul>";
                }
                line += "</td>";
                line += "<td class=\"red\">" + Player.noColors(p.getWorstOpponentName()) + " (" + p.getWorstOpponentCount() + ")</td>";
                line += "<td class=\"ocher\">" + Player.noColors(p.getEasiestOpponentName()) + " (" + p.getEasiestOpponentCount() + ")</td>";
                //Hier müssen noch die Nicks gefärbt werden!
                line += "<td class=\"switchable2\">";
                if (p.attacks.Count > 0) {
                    line += "<ul>";
                    foreach(Incident i in p.attacks) line += "<li class=\"ocher\">" + Player.noColors(i.name) + " (" + i.counter + ")</li>";
                    line += "</ul>";
                }
                line += "</td><td class=\"switchable2\">";
                if (p.victims.Count > 0) {
                    line += "<ul>";
                    foreach(Incident i in p.victims) line += "<li class=\"ocher\">" + Player.noColors(i.name) + " (" + i.counter + ")</li>";
                    line += "</ul>";
                }
                line += "</td></tr>";
                lines.Add(line);
            }
            line = "<tr><th class=\"lowerleftcornercontainer\"><span class=\"lowerleftcorner\"></span></th><th colspan=\"";
            if (withBots) {
                line += 10;
            }
            else {
                line +=9;
            }
            line += "\"></th><th class=\"switchable1\" colspan=\"2\"></th><th colspan=\"2\"><th class=\"switchable2\" colspan=\"2\"></th><th class=\"rightcornercontainer\"><span class=\"rightcorner\"></span></th></tr>";
            lines.Add(line);
            lines.Add("</table>");
            lines.Add("<script>switch_elements('switchable1');switch_elements('switchable2');</script>");
            lines.Add("</body>");
            lines.Add("</head>");
            lines.Add("</html>");
            return lines;
        }

        public void dumpToHTML(bool withBots,
                               string filename) {
            writeToFile(filename,
                        getHTMLDump(withBots));
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
                    cells.Add(p.getEloString());
                    cells.Add(p.getEfficiencyString());
                    cells.Add(p.getRatioString());
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

        private uint sumUintArray(uint[] uints) {
            uint uintSum = 0;
            for(uint counter = 0; counter < uints.Length; counter++) {
                uintSum += uints[counter];
            }
            return uintSum;
        }

        private string deHTMLized(string input) {
            input = Regex.Replace(input, "&", "&amp;");
            input = Regex.Replace(input, "<", "&lt;");
            input = Regex.Replace(input, ">", "&gt;");
            return input;
        }

        public string nameAsHTML(string nick) {
            string tempNick = "<span class=\"white\">" + deHTMLized(nick); // Standard color
            tempNick = Regex.Replace(tempNick, "\\^0", "</span><span class=\"black\">");
            tempNick = Regex.Replace(tempNick, "\\^1", "</span><span class=\"red\">");
            tempNick = Regex.Replace(tempNick, "\\^2", "</span><span class=\"green\">");
            tempNick = Regex.Replace(tempNick, "\\^3", "</span><span class=\"yellow\">");
            tempNick = Regex.Replace(tempNick, "\\^4", "</span><span class=\"blue\">");
            tempNick = Regex.Replace(tempNick, "\\^5", "</span><span class=\"cyan\">");
            tempNick = Regex.Replace(tempNick, "\\^6", "</span><span class=\"magenta\">");
            tempNick = Regex.Replace(tempNick, "\\^7", "</span><span class=\"white\">");
            tempNick = Regex.Replace(tempNick, "\\^8", "</span><span class=\"black\">");
            tempNick = Regex.Replace(tempNick, "\\^9", "</span><span class=\"red\">");
            if (!tempNick.EndsWith("</span>")) tempNick += "</span>";
            return tempNick;
        }
    }
}
