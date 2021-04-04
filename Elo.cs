using System; // for Math...
using System.Collections.Generic;
namespace EfStats {
    public static class Elo {
        /*These starting values are a result of empiric research.
          They just seem to work rather well. ELO scores of different players seem
          to be far enough from each other while not spreading the values too much.
        */
        public static uint startValue = 15000;
        public static uint skillDifferenceScale = 4000;
        public static uint valueChangeScale = 300;
        public static double getEloDiff(Player attacker, Player victim) {
            double difference;
            if (attacker.getElo() >= victim.getElo()) {
                difference = valueChangeScale * (1 / (1 + Math.Pow(10, ((attacker.getElo() - victim.getElo()) / skillDifferenceScale))));
            }
            else {
                difference = valueChangeScale * (1 - (1 / (1 + Math.Pow(10, ((attacker.getElo() - victim.getElo()) / skillDifferenceScale)))));
            }
            return difference;
        }

        public static void updateEloScores(Player attacker, Player victim) {
            double difference = getEloDiff(attacker, victim);
            attacker.setElo(attacker.getElo() + difference);
            victim.setElo(victim.getElo() - difference);
        }

        public static string EloReport(List<Player> allPlayers, List<Encounter> allEncounters, bool onlyNumbers) {
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
