using System;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("=== Advanced VR Battle Simulation Started ===");

        // 1. Setup our Player and an Enemy
        Player player = new Player("Hero", 100);
        Enemy bossDrone = new Enemy("Heavy Titan Drone", 120);

        // 2. Setup Arsenal (Different Weapons)
        // VRBlaster(Name, MaxAmmo, BodyDamage, HeadshotMultiplier)
        VRBlaster plasmaPistol = new VRBlaster("Plasma Pistol", 6, 15, 2.0f); // 2x headshot damage
        VRBlaster laserRifle = new VRBlaster("Heavy Laser Rifle", 3, 35, 1.5f);  // Huge body damage, 1.5x headshot

        player.ShowStatus();
        bossDrone.ShowStatus();

        Console.WriteLine("\n--- ROUND 1: Plasma Pistol Run ---");
        plasmaPistol.OnGrab();
        
        // Fire a few rounds with the pistol
        plasmaPistol.ShootEnemy(bossDrone);
        plasmaPistol.ShootEnemy(bossDrone);
        plasmaPistol.ShootEnemy(bossDrone);

        Console.WriteLine("\n--- ROUND 2: Switching Weapons to Heavy Rifle ---");
        laserRifle.OnGrab(); // Equip the bigger weapon

        // Fire the rifle to deal massive damage
        laserRifle.ShootEnemy(bossDrone);
        
        // Enemy retaliates!
        Console.WriteLine("\n[Action] The Heavy Titan Drone charges its lasers and blasts you!");
        bossDrone.AttackPlayer(player, 45);
        player.ShowStatus();

        // Finish the fight with the rifle
        Console.WriteLine("\n--- ROUND 3: Final Assault ---");
        laserRifle.ShootEnemy(bossDrone);
        laserRifle.ShootEnemy(bossDrone); 

        // Calculate total score combined from both weapons
        int totalScore = plasmaPistol.score + laserRifle.score;
        Console.WriteLine("\n=== Simulation Ended ===");
        Console.WriteLine($"Final Combined Score: {totalScore} Points!");
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
        Console.WriteLine($"[PLAYER] {name} Health: {health}/100 [{healthBar}]");
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
    }

    public void ShowStatus() {
        if (isDefeated) {
            Console.WriteLine($"[ENEMY] {name} is completely DESTROYED! [☠️]");
        } else {
            Console.WriteLine($"[ENEMY] {name} HP: {health}/{maxHealth}");
        }
    }

    public void AttackPlayer(Player targetPlayer, int damage) {
        if (isDefeated) return;
        Console.WriteLine($"*💥 BOOM!* {name} hits {targetPlayer.name} for {damage} damage!");
        targetPlayer.health -= damage;
    }
}

// ------------------- ADVANCED BLASTER CLASS -------------------
class VRBlaster {
    public string objectName;
    public bool isGrabbed = false;
    public int ammoCount;
    public int maxAmmo;
    public int baseDamage;
    public float headshotMultiplier;
    public int score = 0;
    
    private Random randomGenerator = new Random(); // Used to calculate hit locations

    public VRBlaster(string name, int startingAmmo, int damage, float multiplier) {
        objectName = name;
        maxAmmo = startingAmmo;
        ammoCount = startingAmmo;
        baseDamage = damage;
        headshotMultiplier = multiplier;
    }

    public void OnGrab() {
        isGrabbed = true;
        Console.WriteLine($"[Weapon Swap] Equipped: {objectName} (Base Dmg: {baseDamage}, Max Ammo: {maxAmmo})");
    }

    public void ShootEnemy(Enemy target) {
        if (!isGrabbed) {
            Console.WriteLine("❌ Error: Weapon not equipped.");
            return;
        }

        if (target.isDefeated) {
            Console.WriteLine($"*Click* {target.name} is already scraps.");
            return;
        }

        if (ammoCount > 0) {
            ammoCount--;
            
            // Determine hit location (70% chance body shot, 30% chance headshot)
            bool isHeadshot = randomGenerator.Next(0, 100) < 30;
            int finalDamage = baseDamage;
            
            if (isHeadshot) {
                finalDamage = (int)(baseDamage * headshotMultiplier);
                score += 30; // More points for a headshot!
                Console.WriteLine($"🎯 [HEADSHOT!] *CRITICAL HIT* with {objectName}! Dealt {finalDamage} damage to {target.name}!");
            } else {
                score += 10; // Standard points
                Console.WriteLine($"💥 [Body Shot] Hit {target.name} with {objectName} for {finalDamage} damage.");
            }

            Console.WriteLine($"Weapon Status -> Ammo remaining: {ammoCount}/{maxAmmo}");
            target.health -= finalDamage;

            if (target.health <= 0) {
                target.health = 0;
                target.isDefeated = true;
                score += 100; // Massive bonus for defeating the boss drone
                Console.WriteLine($"🏆 TARGET DESTROYED! You defeated the {target.name}!");
            }
            target.ShowStatus();
        } else {
            Console.WriteLine($"*Click Click* {objectName} is empty! Automatic reload initiated...");
            Reload();
            // Fire again after reloading
            ShootEnemy(target);
        }
    }

    public void Reload() {
        ammoCount = maxAmmo;
        Console.WriteLine($"*Ch-Chck!* {objectName} reloaded back to {maxAmmo}.");
    }
}
