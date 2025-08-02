using System.Collections.Generic;

public interface IMiniGame
{
    bool IsGameOver(); // Mini oyun bitti mi?
    Dictionary<ulong, int> GetPlayerScores(); // Oyuncularýn puanlarý
}
