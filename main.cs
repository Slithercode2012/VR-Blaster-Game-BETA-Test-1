using System;
using System.Collections.Generic;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("=== TITAN RIVALS: ULTIMATE EDITION ===");
        Console.WriteLine("Status: Connected to Live Servers");

        // 1. Create our Players with Levels and Ranks
        Player player1 = new Player("Slithercode2012", 100, 15, 1250); // lvl 15, 1250 Rank Points
        Player player2 = new Player("Guest_Player", 100, 3, 900);

        // 2. Setup Upgradable Weapons (Name, MaxAmmo, BaseDamage, Level)
        UpgradableBlaster pistol = new UpgradableBlaster("Plasma Pistol", 6, 15, 1);
        UpgradableBlaster rifle = new UpgradableBlaster("Vortex Rifle", 4, 30, 4); // Already Level 4!

        // Upgrade a gun on the spot to show how it works
        Console.WriteLine("\n[Weapon Bench] Upgrading your arsenal...");
        pistol.UpgradeWeapon(); // Levels up to 2, increasing its damage!

        // 3. SELECT GAME MODE (Options: "Normal", "Ranked", "Zombies")
        // Change this string to test different modes!
        string chosenMode = "Ranked"; 

        Console.WriteLine($"\n[MATCHMAKING] Loading Mode: {chosenMode.ToUpper()}...");

        // 4. Run the Game Mode Logic
        if (chosenMode == "Zombies") {
            RunZombiesMode(player1, pistol);
        } else if (chosenMode == "Ranked") {
            RunCompetitiveMatch(player1, player2, rifle, true);
        } else {
            RunCompetitiveMatch(player1, player2, rifle, false);
        }
    }

    // --- ZOMBIES MODE SYSTEM ---
    static void RunZombiesMode(Player player, UpgradableBlaster weapon) {
        Console.WriteLine("\n🧟!!! ZOMBIES MODE: WAVE 1 !!!🧟");
        Enemy zombie1 = new Enemy("Walker Zombie", 40);
        Enemy zombie2 = new Enemy("Runner Zombie", 30);

        weapon.OnGrab(player.name);
        
        // Blast through the horde
        Console.WriteLine("\n[Action] Clearing the first line of defense!");
        weapon.ShootEnemy(zombie1);
        weapon.ShootEnemy(zombie2);
        weapon.ShootEnemy(zombie1); // Should defeat the first one

        Console.WriteLine("\n[Action] The undead crawl closer...");
        zombie2.AttackPlayer(player, 15);
        player.ShowStatus();
    }

    // --- NORMAL / RANKED MULTIPLAYER SYSTEM ---
    static void RunCompetitiveMatch(Player p1, Player p2, UpgradableBlaster weapon, bool isRanked) {
        Enemy raidBoss = new Enemy("Rival Apex Drone", 140);
        
        if (isRanked) {
            Console.WriteLine($"🏆 RANKED MATCH Active. Lobby Average Tier: {p1.GetRankTier()}");
        }

        weapon.OnGrab(p1.name);
        weapon.ShootEnemy(raidBoss);
        weapon.ShootEnemy(raidBoss);

        Console.WriteLine("\n[Action] Enemy retaliation strike incoming!");
        raidBoss.AttackPlayer(p1, 35);
        p1.ShowStatus();

        // Finish the match
        weapon.ShootEnemy(raidBoss);
        weapon.ShootEnemy(raidBoss); // Finishes it off

        Console.WriteLine("\n=== Match Finished ===");
        if (isRanked && raidBoss.isDefeated) {
            int rpGained = 25;
            p1.rankPoints += rpGained;
            Console.WriteLine($"📈 Victory! {p1.name} gained +{rpGained} Rank Points! New Tier: {p1.GetRankTier()} ({p1.rankPoints} RP)");
        }
    }
}

// ------------------- UPGRADABLE BLASTER CLASS -------------------
class UpgradableBlaster {
    public string weaponName;
    public string currentHolder = "None";
    public int ammoCount;
    public int maxAmmo;
    public int baseDamage;
    public int weaponLevel;
    private Random rand = new Random();

    public UpgradableBlaster(string name, int ammo, int dmg, int lvl) {
        weaponName = name;
        maxAmmo = ammo;
        ammoCount = ammo;
        baseDamage = dmg;
        weaponLevel = lvl;
    }

    public void UpgradeWeapon() {
        weaponLevel++;
        baseDamage += 5; // Adds +5 damage per level upgrade
        Console.WriteLine($"⭐ LEVEL UP! {weaponName} is now Level {weaponLevel}! (Damage boosted to: {baseDamage})");
    }

    public void OnGrab(string playerName) {
        currentHolder = playerName;
        Console.WriteLine($"[Sync] {playerName} equipped Level {weaponLevel} {weaponName}.");
    }

    public void ShootEnemy(Enemy target) {
        if (target.isDefeated) return;

        if (ammoCount > 0) {
            ammoCount--;
            bool isHeadshot = rand.Next(0, 100) < 35;
            int finalDamage = baseDamage;

            if (isHeadshot) {
                finalDamage = (int)(baseDamage * 2.0f);
                Console.WriteLine($"🎯 [HEADSHOT!] {currentHolder} hit a CRIT with {weaponName}! Dealt {finalDamage} damage to {target.name}!");
            } else {
                Console.WriteLine($"💥 [Body Shot] {currentHolder} shot {target.name} for {finalDamage} damage.");
            }

            target.health -= finalDamage;

            if (target.health <= 0) {
                target.health = 0;
                target.isDefeated = true;
                Console.WriteLine($"🏆 TRIPLE KILL! {target.name} was wiped out!");
            }
            target.ShowStatus();
        } else {
            Console.WriteLine($"*Click Click* {weaponName} empty! Auto reloading...");
            ammoCount = maxAmmo;
            ShootEnemy(target);
        }
    }
}

// ------------------- PLAYER CLASS WITH RANKS -------------------
class Player {
    public string name;
    public int health;
    public int accountLevel;
    public int rankPoints; // Tracked for Ranked mode

    public Player(string playerName, int startHealth, int level, int rp) {
        name = playerName;
        health = startHealth;
        accountLevel = level;
        rankPoints = rp;
    }

    public string GetRankTier() {
        if (rankPoints < 1000) return "Bronze III";
        if (rankPoints < 1200) return "Silver I";
        if (rankPoints < 1500) return "Gold II";
        return "Diamond Apex";
    }

    public void ShowStatus() {
        int barLength = 10;
        int filledBars = health / 10;
        if (filledBars < 0) filledBars = 0;
        if (filledBars > 10) filledBars = 10;
        
        string healthBar = new string('█', filledBars) + new string('░', barLength - filledBars);
        Console.WriteLine($"[USER] {name} (Lvl {accountLevel}) HP: {health}/100 [{healthBar}]");
    }
}

// ------------------- ENEMY CLASS -------------------
class Enemy {
    public string name;
    public int health;
    public int maxHealth;
    public bool isDefeated = false;

    public Enemy(string enemyName, int startHealth) {
        maxHealth = startHealth;
        health = startHealth;
        name = enemyName;
    }

    public void ShowStatus() {
        if (isDefeated) {
            Console.WriteLine($"[Target] {name} is CLEAR.");
        } else {
            Console.WriteLine($"[Target] {name} HP: {health}/{maxHealth}");
        }
    }

    public void AttackPlayer(Player targetPlayer, int damage) {
        if (isDefeated) return;
        Console.WriteLine($"*💥 ATTACK!* {name} dealt {damage} damage to {targetPlayer.name}!");
        targetPlayer.health -= damage;
    }
}
