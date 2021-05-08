using System; // Tupel
namespace EfStats {
    public class Incident {
        public string name {get; set;} = null;
        public uint counter  {get; set;} = 0;
        public Incident(string newName) {
            this.name = newName;
        }
        public void Add() {
            if (efstats.debug) Console.Write("Adding Incident for " + name + " to current value: " + counter);
            this.counter++;
            if (efstats.debug) Console.WriteLine(" new value: " + counter);
        }
        public override bool Equals(object otherGuy) {
            if (   (otherGuy == null)
                || !this.GetType().Equals(otherGuy.GetType())) return false;
            return (((Incident)otherGuy).name.Equals(this.name));
        }

        public override int GetHashCode() {// Just enough so that the compiler shuts up...
            return Tuple.Create(name, counter).GetHashCode();
        }
    }
}
