using System;
using System.Linq; // File.ReadLines().First()
using System.IO;//File
using System.Collections.Generic;
using Newtonsoft.Json; //From Newtonsoft.Json.dll

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

    public void dump(bool withBots,
                     bool noColor) {
        foreach(Player p in list) if (!(p.getIsBot() && !withBots)) Console.WriteLine(p.getDetails(noColor));
    }

    public void dumpToFile(bool withBots,
                           bool noColor,
                           string filename) {
        List<string> stringList = filteredPlayerStrings(withBots, noColor);
        writeToFile(filename, stringList);
    }

    public string getJSON(bool withBots) {
        List<Player> players = filteredPlayers(withBots);
        return JsonConvert.SerializeObject(players, Formatting.Indented);
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
        if (withBots) return this.list; //This always means all of them, becaus there is no noPlayer or something...
        List<Player> filteredList = new List<Player>();
        foreach(Player p in list) if (!p.getIsBot()) filteredList.Add(p);
        return filteredList;
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

    public void sort(bool ascending) {
        this.list.Sort(
                       delegate(Player a, Player b) {
                           if (ascending) return Player.Compare(a, b);
                           return -Player.Compare(a, b);
                       });
    }

    public void removeBeginners(uint minEncounters = 30) {
        List<Player> newList = new List<Player> ();
        foreach(Player p in list) if ((p.kills + p.deaths) >= minEncounters) newList.Add(p);
        list = newList;
    }

    void writeToFile(string filename, List<string> stringList) {
        File.WriteAllLines(filename, stringList.ToArray());
    }
}
