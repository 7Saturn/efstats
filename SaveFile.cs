using System; // Simple WriteLine
using System.IO; // File
using System.Collections.Generic; // Lists
using Newtonsoft.Json; //From Newtonsoft.Json.dll
namespace EfStats {
    public class SaveFile {

        public uint       revision = 0; //Save file version
        public string     saveFileName = null;
        public bool       withUnnamed = false;
        public bool       withBots = false;

        public Player[]   playerMapping; //ID <-> Nick
        public PlayerList playerList;
        public List<Encounter> encounters;
        public uint numberOfLines = 0;

        public SaveFile (uint newRevision,
                         string newSaveFileName,
                         bool newWithUnnamed,
                         bool newWithBots,
                         uint newNumberOfLines,
                         Player[] newPlayerMapping = null,
                         PlayerList newPlayerList = null,
                         List<Encounter> newEncounters = null) {
            this.revision = newRevision;
            this.saveFileName = newSaveFileName;
            this.withUnnamed = newWithUnnamed;
            this.withBots = newWithBots;
            this.playerMapping = newPlayerMapping;
            this.playerList = newPlayerList;
            this.encounters = newEncounters;
            this.numberOfLines = newNumberOfLines;
        }

        public void Save() {
            if (efstats.debug){
                Console.WriteLine("Writing Save to " + saveFileName);
                Console.WriteLine("---- Dump start ---- ");
                foreach (Player p in playerList.list) {
                    if (p != null) Console.WriteLine(p.getDetails(false));
                }
                Console.WriteLine("---- Dump end ---- ");
            }
            string output = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});
            if (efstats.debug) Console.WriteLine(output);
            File.WriteAllLines(saveFileName, new string[] {output});
        }
        public void Load() {
            if (!File.Exists(saveFileName)) {
                throw new FileNotFoundException("File " + saveFileName + " cannot be loaded as it does not exist.");
            }
            string lines = File.ReadAllText(saveFileName);
            SaveFile tempSaveFile = new SaveFile(revision,
                                                 saveFileName,
                                                 withUnnamed,
                                                 withBots,
                                                 0);
            if (efstats.debug) Console.WriteLine("Reading Save from " + saveFileName);
            try {
                tempSaveFile = JsonConvert.DeserializeObject<SaveFile>(lines);
            }
            catch {
                throw new SaveFileDefectException("File " + saveFileName + " could not be parsed properly. Are you sure this is an EF Stats save file? If so, the file is most likely defective.");
            }
            if (!tempSaveFile.revision.Equals(revision)) {
                throw new WrongRevisionException("File " + saveFileName + " should have revision " + revision + ". It does have revision " + tempSaveFile.revision + ". This file cannot be loaded by this version of EF Stats.");
            }
            if (tempSaveFile.withUnnamed != withUnnamed) {
                if (tempSaveFile.withUnnamed) {
                    throw new WrongSettingsException("File " + saveFileName + " was created with option withunnamed. You did not provided this switch for this run. As this would give inconsistent data, it cannot be used with these settings.");
                }
                else {
                    throw new WrongSettingsException("File " + saveFileName + " was created without option withunnamed. You did provide this switch for this run. As this would give inconsistent data, it cannot be used with these settings.");
                }
            }
            if (tempSaveFile.withBots != withBots) {
                if (tempSaveFile.withBots) {
                    throw new WrongSettingsException("File " + saveFileName + " was created with option withbots. You did not provided this switch for this run. As this would give inconsistent data, it cannot be used with these settings.");
                }
                else {
                    throw new WrongSettingsException("File " + saveFileName + " was created without option withbots. You did provide this switch for this run. As this would give inconsistent data, it cannot be used with these settings.");
                }
            }
            this.playerMapping = tempSaveFile.playerMapping;
            this.playerList = tempSaveFile.playerList;
            this.encounters = tempSaveFile.encounters;
            this.numberOfLines = tempSaveFile.numberOfLines;
            if (efstats.debug){
                Console.WriteLine("---- Dump start ---- ");
                foreach (Player p in playerList.list) {
                    if (p != null) Console.WriteLine(p.getDetails(false));
                }
                Console.WriteLine("---- Dump end ---- ");
            }
        }
    }
    public class FileNotFoundException : System.Exception {
        public FileNotFoundException() : base() {
            throw new FileNotFoundException("File cannot be loaded as it does not exist.");
        }
        public FileNotFoundException(string message) : base(message) { }
        public FileNotFoundException(string message, System.Exception inner) : base(message, inner) { }

        protected FileNotFoundException(System.Runtime.Serialization.SerializationInfo info,
                                        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class WrongRevisionException : System.Exception {
        public WrongRevisionException() : base() {
            throw new WrongRevisionException("The files has the wrong revision!");
        }
        public WrongRevisionException(string message) : base(message) { }
        public WrongRevisionException(string message, System.Exception inner) : base(message, inner) { }

        protected WrongRevisionException(System.Runtime.Serialization.SerializationInfo info,
                                         System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class WrongSettingsException : System.Exception {
        public WrongSettingsException() : base() {
            throw new WrongSettingsException("The files has different settings than those provided for this run. As this would give inconsistent data, it cannot be used with these settings.");
        }
        public WrongSettingsException(string message) : base(message) { }
        public WrongSettingsException(string message, System.Exception inner) : base(message, inner) { }

        protected WrongSettingsException(System.Runtime.Serialization.SerializationInfo info,
                                         System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class SaveFileDefectException : System.Exception {
        public SaveFileDefectException() : base() {
            throw new SaveFileDefectException("The files has different settings than those provided for this run. As this would give inconsistent data, it cannot be used with these settings.");
        }
        public SaveFileDefectException(string message) : base(message) { }
        public SaveFileDefectException(string message, System.Exception inner) : base(message, inner) { }

        protected SaveFileDefectException(System.Runtime.Serialization.SerializationInfo info,
                                          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
