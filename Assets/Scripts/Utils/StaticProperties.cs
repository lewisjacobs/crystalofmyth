using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum GameTypes
{
    ADVENTURE,
    ARENA,
    ARENATEAM
}

public enum ChosenCharacter
{
    MYSTIC,
    KNIGHT,
    BARBARIAN,
    HUNTRESS,
    DM,
    Default
}

public struct TeamColours
{
    public const int NOTEAM = 0;
    public const int BLUE = 1;
    public const int RED = 2;
    public const int YELLOW = 3;
    public const int GREEN = 4;
}

public class StaticProperties : Singleton<StaticProperties>
{
    public StaticProperties()
    {
        PlayerID = "-1";
        Lives = 1;
        NumberOfServers = 0;
    }

    public GameTypes GameType { get; set; }

    public bool MultiPlayer { get; set; }

    public string ArenaLobbyText { get; set; }

    public string HostText { get; set; }

    public bool IsHost { get; set; }

    public int NumberOfServers { get; set; }

    public int ServerIndex { get; set; }

	public string ServerName { get; set; }

    public ChosenCharacter SelectedCharacter { get; set; }

    public string PlayerID { get; set; }

    public int Lives { get; set; }

    public int PlayerTeam { get; set; }
}