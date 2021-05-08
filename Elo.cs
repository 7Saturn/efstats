using System; // for Math...
using System.Collections.Generic;
namespace EfStats {
    public static class Elo {
        /*These starting values are a result of empiric research.
          They just seem to work rather well. ELO scores of different players seem
          to be far enough from each other while not spreading the values too much. */
        public static uint startValue = 15000;
        public static uint skillDifferenceScale = 4000;
        public static uint valueChangeScale = 300;
        public static uint getEloDiff(Player attacker,
                                      Player victim) {
            double difference;
            /*
              The following formular has the following properties:
              * ELO scores are conservative. They cannot vanish or appear out of nothing.
                Exception: A new player adds 15000 points to the overall pool.
                Aside from that, points are only exchanged, never destroyed or created.
                So the average of all players is always 15000. (If you are missing some points, then that's due to players not being reported for not having the least number of encounters.
              * The scores converges for equally strong players (which on the everage kill each other equally often)
                --> Both players will alternate round about +- 76 Points around 15000 (the starting value) if they are playing alone.
              * The higher the difference of scores, the higher the pentalty for a favoured player (higher ELO score) for loosing and the higher gain for the winning unfavoured player.
              * Maximum change would be theoretically for a score difference of infinity and the favoured player loosing, resp. zero for the other way around, the favoured player winning.
                The uint will see to that earlier...
                So it will converge eventually (e.g. after approximately 1500 rounds of consecutive loosing for 15000 start value of both players) between 9000 and 10000.
            */
            difference = 1 / (1 + Math.Pow(10, (Math.Abs(victim.getElo() - attacker.getElo()) / skillDifferenceScale)));
            if (attacker.getElo() >= victim.getElo()) {
                if (efstats.debug) Console.WriteLine("Attacker was already favoured.");
                difference = valueChangeScale * difference;
            }
            else {
                if (efstats.debug) Console.WriteLine("Victim was favoured.");
                difference = valueChangeScale * (1 - difference);
            }
            return (uint) rounded(difference);
        }

        public static void updateEloScores(Player attacker,
                                           Player victim) {
            if (   efstats.debug
                || efstats.elodetails) Console.Write("" + attacker.getName() + " (" + attacker.getElo()+ ") x " + victim.getName() + " (" + victim.getElo() + ") --> ");

            uint difference = getEloDiff(attacker,
                                         victim);
            attacker.setElo(attacker.getElo() + difference);
            victim.setElo(victim.getElo() - difference);
            if (   efstats.debug
                || efstats.elodetails) Console.WriteLine(attacker.getName() + " (" + attacker.getElo() + ") / " + victim.getName() + " (" + victim.getElo() + ")");
        }

        public static string EloReport(List<Player> allPlayers,
                                       List<Encounter> allEncounters,
                                       bool onlyNumbers) {
            string report = "";
            if (onlyNumbers) report += "attacker\tvictim\tprobability\tencounters\n";
            foreach(Player attacker in allPlayers) {
                foreach(Player victim in allPlayers) {
                    if (attacker != victim) {
                        double eloDifference = Math.Abs(attacker.getElo() - victim.getElo());
                        double eloRatio = attacker.getElo() / victim.getElo();
                        Encounter thisEncounter = new Encounter(attacker.getName(), victim.getName());
                        Encounter reversedEncounter = new Encounter(victim.getName(), attacker.getName());
                        uint thisEncounterOccurence = 0;
                        uint reversedEncounterOccurence = 0;
                        if (allEncounters.Contains(thisEncounter)) {
                            foreach(Encounter e in allEncounters) {
                                if (e.Equals(thisEncounter)) thisEncounter = e;
                            }
                            thisEncounterOccurence = thisEncounter.occurence;
                        }
                        if (allEncounters.Contains(reversedEncounter)) {
                            foreach(Encounter e in allEncounters) {
                                if (e.Equals(reversedEncounter)) reversedEncounter = e;
                            }
                            reversedEncounterOccurence = reversedEncounter.occurence;
                        }
                        uint allEncounterOccurence = reversedEncounterOccurence + thisEncounterOccurence;
                        if (allEncounterOccurence > 0) {
                            int probability = rounded((double) thisEncounterOccurence / (double) allEncounterOccurence * 100);
                            if (onlyNumbers) {
                                report +=
                                    rounded(attacker.getElo())
                                    + "\t" + rounded(victim.getElo())
                                    + "\t" + probability
                                    + "\t" + allEncounterOccurence
                                    + "\n";
                            }
                            else {
                                report += attacker.getName()
                                    + "(" + rounded(attacker.getElo()) + ") vs. "
                                    + victim.getName()
                                    + "(" + rounded(victim.getElo()) + "): "
                                    + "Diff " + rounded(eloDifference)
                                    + " Ratio " + rounded(eloRatio * 100)
                                    + " Probability " + probability
                                    + " Encounters " + allEncounterOccurence
                                    + "\n";
                            }
                        }
                    }
                }
            }
            return report;
        }

        public static int rounded(double d) {
            return (int) (d + 0.5);
        }
    }
}
