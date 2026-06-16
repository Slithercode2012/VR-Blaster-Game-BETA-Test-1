using System;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("=== TITAN RIVALS: GLOBAL NETWORKING SYSTEM ===");
        Console.WriteLine("Status: Fetching Regional Data Centers...\n");

        // 1. Create a worldwide network architecture
        ServerLobby asiaServer = new ServerLobby("Asia-Tokyo", 45);
        ServerLobby europeServer = new ServerLobby("Europe-Frankfurt", 120);
        ServerLobby usEastServer = new ServerLobby("US-East", 15);

        // 2. Create players located around the world
        Player localPlayer = new Player("Slithercode2012", 100, 15, 1250); // Local player
        Player proGamerJapan = new Player("Yuki_Tatsu", 100, 72, 1980);   // Player from Japan
        Player guestEuro = new Player("EuroRival_99", 100, 5, 950);       // Player from Europe

        // 3. Match players into their optimal regional server hubs
        Console.WriteLine("--- ROUTING TRAFFIC ---");
        usEastServer.ConnectPlayer(localPlayer);
        
        asiaServer.ConnectPlayer(proGamerJapan);
        
        europeServer.ConnectPlayer(guestEuro);

        // 4. Inspect the localized region lobbies
        asiaServer.ShowLobbyPlayers();
        usEastServer.ShowLobbyPlayers();
        
        Console.WriteLine("\n[Server Sync] Global lobbies operating smoothly. Ready for simulation match creation.");
    }
}
