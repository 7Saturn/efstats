using System;
public static class PlayerMapping {
    public static Player[] players = new Player[64]; //The highest the game will actually allow you to go in sv_maxclients, although I doubt anyone ever used that many slots...

    public static void setPlayer(uint slotNumber, Player p) {
        players[slotNumber] = p;
    }

    public static Player getPlayer(uint slotNumber) {
        return players[slotNumber];
    }

    public static Player[] getMapping() {
        return players;
    }

    public static void dumpMapping() {
        Console.WriteLine("---- Dumping Mapping");
        uint counter = 0;
        foreach (Player p in players){
            Console.Write((counter++) + ": ");
            if (p != null){
                Console.WriteLine(p.ToString());
            }
            else {
                Console.WriteLine();
            }
        }
        Console.WriteLine("----");
    }
}
