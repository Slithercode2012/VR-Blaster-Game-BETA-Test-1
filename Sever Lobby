using System;
using System.Collections.Generic;

public class ServerLobby {
    public string regionName;
    public int currentPing; 
    private List<Player> connectedPlayers = new List<Player>();

    public ServerLobby(string region, int ping) {
        regionName = region;
        currentPing = ping;
    }

    public void ConnectPlayer(Player player) {
        connectedPlayers.Add(player);
        Console.WriteLine($"🌐 [Network] Player '{player.name}' joined the [{regionName}] gateway. (Ping: {currentPing}ms)");
    }

    public void ShowLobbyPlayers() {
        Console.WriteLine($"\n=== Regional Lobby: {regionName.ToUpper()} ===");
        foreach (var player in connectedPlayers) {
            player.ShowStatus();
            Console.WriteLine($"   Rank Tier: {player.GetRankTier()}");
        }
        Console.WriteLine("=============================================");
    }
}
