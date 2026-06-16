using System;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("=== VR Battle Simulation Started ===");

        // 1. Setup our Player, Blaster, and an Enemy
        Player player = new Player("Hero", 100);
        VRBlaster blaster = new VRBlaster("Plasma Pistol", 5);
        Enemy targetDummy = new Enemy("Training Drone", 50);

        // Show starting stats
        player.ShowStatus();
        targetDummy.ShowStatus();
        blaster.GetInfo();

        // 2. Try to shoot without holding the blaster
        Console.WriteLine("\n[Action] You try to shoot the enemy before picking up the gun...");
        blaster.ShootEnemy(targetDummy);

        // 3. Grab the blaster
        Console.WriteLine("\n[Action] You grab the blaster...");
        blaster.OnGrab();

        // 4. Battle Begins! Shoot the enemy
        Console.WriteLine("\n[Action] You aim and pull the trigger at the Drone!");
        blaster.ShootEnemy(targetDummy);

        Console.WriteLine("\n[Action] You fire a second shot!");
        blaster.ShootEnemy(targetDummy);

        // 5. Enemy Attacks Back!
        Console.WriteLine("\n[Action] The Training Drone fires a laser back at you!");
        targetDummy.AttackPlayer(player, 30);
        player.ShowStatus(); // Look at your health bar now!

        // 6. Finish off the enemy
        Console.WriteLine("\n[Action] You open fire to finish the fight!");
        blaster.ShootEnemy(targetDummy);
        blaster.ShootEnemy(targetDummy);
        blaster.ShootEnemy(targetDummy); // This shot should defeat it!

        // 7. Try to shoot it again while it's down
        Console.WriteLine("\n[Action] You try to shoot the broken drone...");
        blaster.ShootEnemy(targetDummy);

        Console.WriteLine("\n=== Simulation Ended ===");
        Console.WriteLine($"Final Score: {blaster.score} Points!");
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
        // This generates a visual text health bar like [██████░░░░]
        int barLength = 10;
        int filledBars = health / 10;
        if (filledBars < 0) filledBars = 0;
        
        string healthBar = new string('█', filledBars) + new string('░', barLength - filledBars);
        Console.WriteLine($"[PLAYER] {name} Health: {health}/100 [{healthBar}]");
    }
}

// ------------------- ENEMY CLASS -------------------
class Enemy {
    public string name;
    public int health;
    public bool isDefeated = false;

    public Enemy(string enemyName, int startHealth) {
        name = enemyName;
        health = startHealth;
    }

    public void ShowStatus() {
        if (isDefeated) {
            Console.WriteLine($"[ENEMY] {name} is completely DESTROYED! [☠️]");
        } else {
            Console.WriteLine($"[ENEMY] {name} HP: {health}/50");
        }
    }

    public void AttackPlayer(Player targetPlayer, int damage) {
        if (isDefeated) return;
        Console.WriteLine($"*ZAP!* {name} hits {targetPlayer.name} for {damage} damage!");
        targetPlayer.health -= damage;
    }
}

// ------------------- BLASTER CLASS -------------------
class VRBlaster {
    public string objectName;
    public bool isGrabbed = false;
    public int ammoCount;
    public int maxAmmo;
    public int score = 0; // Score counter extra!

    public VRBlaster(string name, int startingAmmo) {
        objectName = name;
        maxAmmo = startingAmmo;
        ammoCount = startingAmmo;
    }

    public void GetInfo() {
        Console.WriteLine($"[WEAPON] {objectName} | Ammo: {ammoCount}/{maxAmmo}");
    }

    public void OnGrab() {
        isGrabbed = true;
        Console.WriteLine($"*Zzzt* {objectName} equipped to your VR hand.");
    }

    public void ShootEnemy(Enemy target) {
        if (!isGrabbed) {
            Console.WriteLine("❌ Error: You can't shoot! The blaster is on the floor.");
            return;
        }

        if (target.isDefeated) {
            Console.WriteLine($"*Click* No need to shoot. The {target.name} is already wrecked.");
            return;
        }

        if (ammoCount > 0) {
            ammoCount--;
            int damageDealt = 15; // Each shot does 15 damage
            score += 10;          // Get 10 score points per hit!
            
            Console.WriteLine($"*PEW!* Hit {target.name} for {damageDealt} damage! (Ammo: {ammoCount}/{maxAmmo})");
            target.health -= damageDealt;

            // Check if that shot destroyed it
            if (target.health <= 0) {
                target.health = 0;
                target.isDefeated = true;
                score += 50; // Bonus score for defeating the enemy!
                Console.WriteLine($"💥 BOOM! You destroyed the {target.name}!");
            }
            target.ShowStatus();
        } else {
            Console.WriteLine("*Click Click* Out of ammo! Shaking controller to reload...");
            ammoCount = maxAmmo;
            Console.WriteLine("*Ch-Chck!* Loaded back up.");
        }
    }
}
