using System;

public class Player {
    public string name;
    public int health;
    public int accountLevel;
    public int rankPoints;

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
        Console.WriteLine($"   [USER] {name} (Lvl {accountLevel}) HP: {health}/100 [{healthBar}]");
    }
}
