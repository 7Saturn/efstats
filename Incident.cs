using System; // Tupel
public class Incident {
    public string name {get; set;} = null;
    public uint counter  {get; set;} = 0;
    public Incident(string newName) {
        this.name = newName;
    }
    public void Add() {
        this.counter++;
    }
    public override bool Equals(object otherGuy) {
        if ((otherGuy == null) || !this.GetType().Equals(otherGuy.GetType())) return false;
        return (((Incident)otherGuy).name.Equals(this.name));
    }

    public override int GetHashCode() {// Just enough so that the compiler shuts up...
        return Tuple.Create(name, counter).GetHashCode();
    }
}
