using System;
using System.Collections.Generic;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("=== VR MULTIPLAYER CO-OP LOBBY ===");
        Console.WriteLine("Status: Connected to Server [US-East]");

        // 1. Create our Multiplayer Lobby
        List<Player> lobbyPlayers = new List<Player>();
        lobbyPlayers.Add(new Player("Player1_Slither", 100));
        lobbyPlayers.Add(new Player("Player2_Guest", 100));

        // 2. Select a Random Map (CoD Style)
        GameMap currentMap = MapManager.GetRandomMap();
        Console.WriteLine($"\n[MATCHMAKING] Map Voted: {currentMap.mapName.ToUpper()}!");
        Console.WriteLine($"[MAP MODIFIER] {currentMap.description}");

        // 3. Spawn the Boss Drone
        Enemy raidBoss = new Enemy("MEGA TITAN RAID BOSS", 250);

        // 4. Setup Arsenal (Name, MaxAmmo, BodyDamage, HeadshotMultiplier)
        VRBlaster plasmaPistol = new VRBlaster("Plasma Pistol", 6, 15, 2.0f);
        VRBlaster railGun = new VRBlaster("Heavy Railgun", 2, 45, 1.5f);

        Console.WriteLine($"\n[Server] Match Started on {currentMap.mapName}!");
        raidBoss.ShowStatus();

        // --- ROUND 1: Player 1 Attacks ---
        Console.WriteLine($"\n--- [ROUND 1] {lobbyPlayers[0].name}'s Turn ---");
        plasmaPistol.OnGrab(lobbyPlayers[0].name);
        // Map damage modifiers apply here!
        plasmaPistol.ShootEnemy(raidBoss, currentMap);

        // --- ROUND 2: Player 2 Attacks ---
        Console.WriteLine($"\n--- [ROUND 2] {lobbyPlayers[1].name}'s Turn ---");
        railGun.OnGrab(lobbyPlayers[1].name);
        railGun.ShootEnemy(raidBoss, currentMap);

        // --- ROUND 3: Boss Attacks Back ---
        Console.WriteLine("\n--- [BOSS PHASE] Raid Boss Retaliates! ---");
        // Tight maps increase enemy damage!
        int bossDamage = 30 + currentMap.enemyDamageModifier; 
        raidBoss.AttackRandomPlayer(lobbyPlayers, bossDamage);

        // Show updated server status
        Console.WriteLine("\n[Server Sync] Updating Team Status:");
        foreach (var p in lobbyPlayers) {
            p.ShowStatus();
        }

        // --- ROUND 4: Final Assault ---
        Console.WriteLine("\n--- [FINAL ROUND] Combined Fire! ---");
        plasmaPistol.OnGrab(lobbyPlayers[0].name);
        plasmaPistol.ShootEnemy(raidBoss, currentMap);

        railGun.OnGrab(lobbyPlayers[1].name);
        railGun.ShootEnemy(raidBoss, currentMap);

        Console.WriteLine("\n=== Match Ended ===");
        Console.WriteLine($"Team Total Score: {plasmaPistol.score + railGun.score} Points!");
    }
}

// ------------------- MAP SYSTEM -------------------
class GameMap {
    public string mapName;
    public string description;
    public int playerDamageModifier; // Extra damage players deal on this map
    public int enemyDamageModifier;  // Extra damage enemies deal on this map

    public GameMap(string name, string desc, int playerMod, int enemyMod) {
        mapName = name;
        description = desc;
        playerDamageModifier = playerMod;
        enemyDamageModifier = enemyMod;
    }
}

class MapManager {
    public static GameMap GetRandomMap() {
        List<GameMap> maps = new List<GameMap>();
        // Add classic style maps
        maps.Add(new GameMap("Shipment (CQC)", "Ultra tight spaces! Enemy attacks deal +15 more damage!", 0, 15));
        maps.Add(new GameMap("Rust (Desert)", "High ground advantages! All players deal +10 bonus damage!", 10, 0));
        maps.Add(new GameMap("Cyber Neon (Rivals Arena)", "Perfectly balanced test arena. No modifiers.", 0, 0));

        Random rand = new Random();
        return maps[rand.Next(0, maps.Count)];
    }
}

// ------------------- PLAYER CLASS -------------------
class Player {
    public string name;
    public int health;

    public Player(string playerName, int startHealth) {
        name = playerName;
        health = startHealth;
    }

    public void ShowStatus() {
        int barLength = 10;
        int filledBars = health / 10;
        if (filledBars < 0) filledBars = 0;
        if (filledBars > 10) filledBars = 10;
        
        string healthBar = new string('█', filledBars) + new string('░', barLength - filledBars);
        Console.WriteLine($"[🌐 SERVER] {name} HP: {health}/100 [{healthBar}]");
    }
}

// ------------------- ENEMY CLASS -------------------
class Enemy {
    public string name;
    public int health;
    public int maxHealth;
    public bool isDefeated = false;
    private Random rand = new Random();

    public Enemy(string enemyName, int startHealth) {
        maxHealth = startHealth;
        health = startHealth;
        name = enemyName;
    }

    public void ShowStatus() {
        if (isDefeated) {
            Console.WriteLine($"[BOSS] {name} is DEFEATED! 🏆");
        } else {
            Console.WriteLine($"[BOSS] {name} HP: {health}/{maxHealth}");
        }
    }

    public void AttackRandomPlayer(List<Player> players, int damage) {
        if (isDefeated || players.Count == 0) return;

        int targetIndex = rand.Next(0, players.Count);
        Player targetPlayer = players[targetIndex];

        Console.WriteLine($"*🚨 LOCK ON* On this map, {name} hits {targetPlayer.name} for {damage} damage!");
        targetPlayer.health -= damage;
    }
}

// ------------------- BLASTER CLASS -------------------
class VRBlaster {
    public string objectName;
    public string currentHolder = "None";
    public int ammoCount;
    public int maxAmmo;
    public int baseDamage;
    public float headshotMultiplier;
    public int score = 0;
    
    private Random randomGenerator = new Random();

    public VRBlaster(string name, int startingAmmo, int damage, float multiplier) {
        objectName = name;
        maxAmmo = startingAmmo;
        ammoCount = startingAmmo;
        baseDamage = damage;
        headshotMultiplier = multiplier;
    }

    public void OnGrab(string playerName) {
        currentHolder = playerName;
        Console.WriteLine($"[Network Sync] {playerName} equipped the {objectName}.");
    }

    // Now accepts the active map to check for damage bonuses
    public void ShootEnemy(Enemy target, GameMap activeMap) {
        if (currentHolder == "None") return;
        if (target.isDefeated) return;

        if (ammoCount > 0) {
            ammoCount--;
            
            bool isHeadshot = randomGenerator.Next(0, 100) < 35;
            // Add map damage bonus to base damage
            int mapBaseDamage = baseDamage + activeMap.playerDamageModifier;
            int finalDamage = mapBaseDamage;
            
            if (isHeadshot) {
                finalDamage = (int)(mapBaseDamage * headshotMultiplier);
                score += 30;
                Console.WriteLine($"🎯 [HEADSHOT!] {currentHolder} scored a CRIT on {activeMap.mapName}! Dealt {finalDamage} damage to {target.name}!");
            } else {
                score += 10;
                Console.WriteLine($"💥 [Body Shot] {currentHolder} hit {target.name} for {finalDamage} damage.");
            }

            target.health -= finalDamage;

            if (target.health <= 0) {
                target.health = 0;
                target.isDefeated = true;
                score += 150;
                Console.WriteLine($"🏆 MATCH WINNER! {currentHolder} secured the final kill on {activeMap.mapName}!");
            }
            target.ShowStatus();
        } else {
            Console.WriteLine($"*Click Click* {objectName} is empty! Automatic reload...");
            Reload();
            ShootEnemy(target, activeMap);
        }
    }

    public void Reload() {
        ammoCount = maxAmmo;
        Console.WriteLine($"*Ch-Chck!* {objectName} reloaded.");
    }
}
