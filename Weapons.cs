using System.Collections.Generic; // Lists
public class Weapons {
    public static readonly string[] weaponNames = {
        "Unknown",                              //  0 MOD_UNKNOWN              = WP_NONE
        "Water",                                //  1 MOD_WATER                = WP_NONE = Drowned
        "Slime",                                //  2 MOD_SLIME                = WP_NONE = Acid
        "Lava",                                 //  3 MOD_LAVA                 = WP_NONE
        "Crushed",                              //  4 MOD_CRUSH                = WP_NONE = Map parts moving, that can crush you
        "Tele Frag",                            //  5 MOD_TELEFRAG             = WP_NONE = Teleporter spawn
        "Falling",                              //  6 MOD_FALLING              = WP_NONE = falling damage
        "Suicide",                              //  7 MOD_SUICIDE              = WP_NONE = event »kill« a player can invoke on himself
        "Laser",                                //  8 MOD_TARGET_LASER         = WP_NONE = never seen...
        "Map Item",                             //  9 MOD_TRIGGER_HURT         = WP_NONE = forcefields and other stuff, that damages on touch.
        "Phaser",                               // 10 MOD_PHASER               = WP_PHASER
        "Alt. Phaser",                          // 11 MOD_PHASER_ALT           = WP_PHASER
        "Compression Rifle",                    // 12 MOD_CRIFLE               = WP_COMPRESSION_RIFLE
        "Compression Rifle Splash Damage",      // 13 MOD_CRIFLE_SPLASH        = WP_COMPRESSION_RIFLE
        "Alt. Compression Rifle",               // 14 MOD_CRIFLE_ALT           = WP_COMPRESSION_RIFLE
        "Alt. Compression Rifle Splash Damage", // 15 MOD_CRIFLE_ALT_SPLASH    = WP_COMPRESSION_RIFLE
        "I-Mod",                                // 16 MOD_IMOD                 = WP_IMOD
        "Alt. I-Mod",                           // 17 MOD_IMOD_ALT             = WP_IMOD
        "Scavanger Rifle",                      // 18 MOD_SCAVENGER            = WP_SCAVENGER_RIFLE
        "Alt. Scavanger Rifle",                 // 19 MOD_SCAVENGER_ALT        = WP_SCAVENGER_RIFLE
        "Scavanger Rifle Splash Damage",        // 20 MOD_SCAVENGER_ALT_SPLASH = WP_SCAVENGER_RIFLE
        "Stasis Weapon",                        // 21 MOD_STASIS               = WP_STASIS
        "Alt. Stasis Weapon",                   // 22 MOD_STASIS_ALT           = WP_STASIS
        "Grenade Launcher",                     // 23 MOD_GRENADE              = WP_GRENADE_LAUNCHER
        "Alt. Grenade Launcher",                // 24 MOD_GRENADE_ALT          = WP_GRENADE_LAUNCHER
        "Grenade Launcher Splash Damage",       // 25 MOD_GRENADE_SPLASH       = WP_GRENADE_LAUNCHER
        "Alt. Grenade Launcher Splash Damage",  // 26 MOD_GRENADE_ALT_SPLASH   = WP_GRENADE_LAUNCHER
        "Tetrion Disruptor",                    // 27 MOD_TETRYON              = WP_TETRION_DISRUPTOR
        "Alt. Tetrion Disruptor",               // 28 MOD_TETRYON_ALT          = WP_TETRION_DISRUPTOR
        "Arc Welder",                           // 29 MOD_DREADNOUGHT          = WP_DREADNOUGHT
        "Alt. Arc Welder",                      // 30 MOD_DREADNOUGHT_ALT      = WP_DREADNOUGHT
        "Photon Burst",                         // 31 MOD_QUANTUM              = WP_QUANTUM_BURST
        "Photon Burst Splash Damage",           // 32 MOD_QUANTUM_SPLASH       = WP_QUANTUM_BURST
        "Alt. Photon Burst",                    // 33 MOD_QUANTUM_ALT          = WP_QUANTUM_BURST
        "Alt. Photon Burst Splash Damage",      // 34 MOD_QUANTUM_ALT_SPLASH   = WP_QUANTUM_BURST
        "Ultritium Mine",                       // 35 MOD_DETPACK              = HI_DETPACK = not to be confused with Explosions of other origin, that can happen on maps!
        "Seeker Drone",                         // 36 MOD_SEEKER               = PW_SEEKER
        "Tranquilizer",                         // 37 MOD_KNOCKOUT             = WP_VOYAGER_HYPO = not used normally on maps, but custom maps may have it, the red TOS hypo.
        "Borg Assimilator",                     // 38 MOD_ASSIMILATE           = WP_BORG_ASSIMILATOR
        "Borg Weapon",                          // 39 MOD_BORG                 = WP_BORG_WEAPON
        "Alt. Borg Weapon",                     // 40 MOD_BORG_ALT             = WP_BORG_WEAPON
        "Spawn",                                // 41 MOD_RESPAWN              = WP_NONE = Similar to MOD_TELEFRAG but not achieved by teleporting but by spawning (e. g. entering the match or reentering the match after being fragged.
        "Map Explosion",                        // 42 MOD_EXPLOSION            = WP_NONE
    };

    private static readonly List<uint> WorldDamageIDs = new List<uint> {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 41, 42 //WP_NONE values
    };

    private static List<uint> WeaponIDs = null;

    public static List<uint> getWeaponIDs() {
        if (WeaponIDs == null) {
            WeaponIDs = new List<uint>();
            for (uint counter = 0; counter < weaponNames.Length; counter++) {
                if (!WorldDamageIDs.Contains(counter)) WeaponIDs.Add(counter);
            }
        }
        return WeaponIDs;
    }

    public static List<uint> getWorldDamageIDs() {
        return WorldDamageIDs;
    }
}
