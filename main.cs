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

        // 2. Spawn a massive Raid Boss Drone
        Enemy raidBoss = new Enemy("MEGA TITAN RAID BOSS", 250);

        // 3. Give players their weapons
        VRBlaster plasmaPistol = new VRBlaster("Plasma Pistol", 6, 15, 2.0f);
        VRBlaster railGun = new VRBlaster("Heavy Railgun", 2, 45, 1.5f);

        Console.WriteLine($"\n[Server] Match Started! {lobbyPlayers.Count} players vs 1 Boss.");
        foreach (var p in lobbyPlayers) {
            p.ShowStatus();
        }
        raidBoss.ShowStatus();

        // --- TURN 1: Player 1 Attacks ---
        Console.WriteLine($"\n--- [ROUND 1] {lobbyPlayers[0].name}'s Turn ---");
        plasmaPistol.OnGrab(lobbyPlayers[0].name);
        plasmaPistol.ShootEnemy(raidBoss);
        plasmaPistol.ShootEnemy(raidBoss);

        // --- TURN 2: Player 2 Attacks ---
        Console.WriteLine($"\n--- [ROUND 2] {lobbyPlayers[1].name}'s Turn ---");
        railGun.OnGrab(lobbyPlayers[1].name);
        railGun.ShootEnemy(raidBoss);

        // --- TURN 3: Boss Target Selection (Multiplayer Mechanic) ---
        Console.WriteLine("\n--- [BOSS PHASE] Raid Boss Retaliates! ---");
        // The boss randomly chooses a player in the lobby to attack
        raidBoss.AttackRandomPlayer(lobbyPlayers, 40);

        // Show updated server status for both players
        Console.WriteLine("\n[Server Sync] Updating Player Status:");
        foreach (var p in lobbyPlayers) {
            p.ShowStatus();
        }

        // --- TURN 4: Combined Firepower ---
        Console.WriteLine("\n--- [FINAL ROUND] Combined Fire! ---");
        plasmaPistol.OnGrab(lobbyPlayers[0].name);
        plasmaPistol.ShootEnemy(raidBoss); // Player 1 shoots

        railGun.OnGrab(lobbyPlayers[1].name);
        railGun.ShootEnemy(raidBoss); // Player 2 shoots
        railGun.ShootEnemy(raidBoss); // Player 2 finishes it!

        Console.WriteLine("\n=== Match Ended ===");
        Console.WriteLine($"Team Total Score: {plasmaPistol.score + railGun.score} Points!");
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

    // Multiplayer Attack Logic: Picks a random player from the list
    public void AttackRandomPlayer(List<Player> players, int damage) {
        if (isDefeated || players.Count == 0) return;

        int targetIndex = rand.Next(0, players.Count);
        Player targetPlayer = players[targetIndex];

        Console.WriteLine($"*🚨 LOCK ON!* {name} targets and blasts {targetPlayer.name} for {damage} damage!");
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

    // Pass the player's name so the server knows who picked it up
    public void OnGrab(string playerName) {
        currentHolder = playerName;
        Console.WriteLine($"[Network Sync] {playerName} equipped the {objectName}.");
    }

    public void ShootEnemy(Enemy target) {
        if (currentHolder == "None") {
            Console.WriteLine("❌ Error: Gun is not held by any network player.");
            return;
        }

        if (target.isDefeated) {
            Console.WriteLine($"*Click* {target.name} is already down.");
            return;
        }

        if (ammoCount > 0) {
            ammoCount--;
            
            bool isHeadshot = randomGenerator.Next(0, 100) < 35; // 35% headshot chance
            int finalDamage = baseDamage;
            
            if (isHeadshot) {
                finalDamage = (int)(baseDamage * headshotMultiplier);
                score += 30;
                Console.WriteLine($"🎯 [HEADSHOT!] {currentHolder} hit a CRITICAL shot with {objectName}! Dealt {finalDamage} damage to {target.name}!");
            } else {
                score += 10;
                Console.WriteLine($"💥 [Body Shot] {currentHolder} shot {target.name} with {objectName} for {finalDamage} damage.");
            }

            target.health -= finalDamage;

            if (target.health <= 0) {
                target.health = 0;
                target.isDefeated = true;
                score += 150; // Big multiplayer boss kill bonus!
                Console.WriteLine($"🏆 BOSS DESTROYED! {currentHolder} landed the final blow!");
            }
            target.ShowStatus();
        } else {
            Console.WriteLine($"*Click Click* {objectName} is empty! {currentHolder} is reloading...");
            Reload();
            ShootEnemy(target);
        }
    }

    public void Reload() {
        ammoCount = maxAmmo;
        Console.WriteLine($"*Ch-Chck!* {objectName} reloaded back to {maxAmmo} shots.");
    }
}
