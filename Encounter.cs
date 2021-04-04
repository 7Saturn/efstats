using System; //Tuple
namespace EfStats {
    public class Encounter {
        public string attacker {get; set;} = null;
        public string victim {get; set;} = null;
        public uint occurence {get; set;} = 0;
        public Encounter (string newAttacker, string newVictim) {
            this.attacker = newAttacker;
            this.victim = newVictim;
        }

        public override bool Equals(object otherCase) {
            if ((otherCase == null) || !this.GetType().Equals(otherCase.GetType())) return false;
            return (   this.attacker.Equals(((Encounter)otherCase).attacker)
                    && this.victim.Equals(((Encounter)otherCase).victim));
        }

        public override int GetHashCode() {// Just enough so that the compiler shuts up...
            return Tuple.Create(attacker, victim).GetHashCode();
        }

        public void addOccurence(){
            this.occurence++;
        }
    }
}
