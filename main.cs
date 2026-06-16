using System;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("--- VR Blaster Simulation Started ---");

        // Create a VR blaster with 3 shots max capacity
        VRBlaster plasmaPistol = new VRBlaster("Plasma Pistol", 3);

        plasmaPistol.GetInfo();
        
        // 1. Try to shoot without picking it up first
        Console.WriteLine("\n[Action] Player tries to press the trigger while it's on the ground...");
        plasmaPistol.OnTriggerUse();

        // 2. Pick up the blaster
        Console.WriteLine("\n[Action] Player grabs the blaster...");
        plasmaPistol.OnGrab();

        // 3. Shoot it until it runs out of ammo
        Console.WriteLine("\n[Action] Player pulls trigger (Shot 1)...");
        plasmaPistol.OnTriggerUse();

        Console.WriteLine("\n[Action] Player pulls trigger (Shot 2)...");
        plasmaPistol.OnTriggerUse();

        Console.WriteLine("\n[Action] Player pulls trigger (Shot 3)...");
        plasmaPistol.OnTriggerUse();

        // 4. Try to fire when completely empty
        Console.WriteLine("\n[Action] Player pulls trigger (Shot 4)...");
        plasmaPistol.OnTriggerUse(); 

        // 5. Reload the blaster
        Console.WriteLine("\n[Action] Player shakes controller to reload...");
        plasmaPistol.Reload();

        // 6. Shoot one more time after reloading
        Console.WriteLine("\n[Action] Player pulls trigger after reloading...");
        plasmaPistol.OnTriggerUse();
    }
}

class VRBlaster {
    public string objectName;
    public bool isGrabbed = false;
    public int ammoCount;
    public int maxAmmo;

    // Constructor to setup our blaster properties
    public VRBlaster(string name, int startingAmmo) {
        objectName = name;
        maxAmmo = startingAmmo;
        ammoCount = startingAmmo;
    }

    public void GetInfo() {
        Console.WriteLine($"Found a {objectName}. Ammo: {ammoCount}/{maxAmmo}");
    }

    public void OnGrab() {
        isGrabbed = true;
        Console.WriteLine($"{objectName} equipped! Controller rumble: *Zzzt*");
    }

    public void OnTriggerUse() {
        // Condition 1: You must be holding the item to use it
        if (!isGrabbed) {
            Console.WriteLine("Error: Can't fire. Blaster is not in player's hand.");
            return;
        }

        // Condition 2: You must have ammo left to shoot
        if (ammoCount > 0) {
            ammoCount--; // subtracts 1 from ammoCount
            Console.WriteLine($"*PEW!* Blue plasma bolt fired! Ammo left: {ammoCount}/{maxAmmo}");
        } else {
            Console.WriteLine("*Click... Click...* Out of ammo! Need to reload.");
        }
    }

    public void Reload() {
        ammoCount = maxAmmo; // Resets ammo back to max capacity
        Console.WriteLine($"*Ch-Chck!* {objectName} reloaded to {maxAmmo} shots.");
    }
}
