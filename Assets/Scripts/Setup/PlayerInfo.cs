using System;
public class PlayerInfo
{
    private static PlayerInfo instance;

    public string PlayerName { get; set; }
    public string PlayerID { get; set; }
    public string ExperimentTag { get; set; }

    private PlayerInfo(string playerName, string playerID, string experimentTag)
    {
        PlayerName = playerName;
        PlayerID = playerID;
        ExperimentTag = experimentTag;
    }

    public static PlayerInfo Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PlayerInfo("DefaultPlayerName", "DefaultID", "DefaultExperimentTag");
            }
            return instance;
        }
    }

    public static void Initialize(string playerName, string playerID, string experimentTag)
    {
        instance = new PlayerInfo(playerName, playerID, experimentTag);
    }
}