using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PlayerLevelData", menuName = "Game/Player Level Data")]
public class PlayerLevelData : ScriptableObject
{
    [Header("Exp = 해당 레벨업에 필요한 경험치\nSp = 추가되는 스킬 포인트\nInt, Speed는 레벨업 적용X\n\nElement 1 = 레벨 1")]
    public P_Stats[] levels;

#if UNITY_EDITOR
    [ContextMenu("Fill Player Level Data")]
    private void FillPlayerLevelData()
    {
        levels = new P_Stats[31];
                             //   hp def str dex int spd wgh exp sp
        levels[0] = CreateStats(1000, 50, 100, 50, 0, 0, 500, 0, 0);

        levels[1] = CreateStats(1000, 50, 100, 50, 0, 0, 500, 100, 5);
        levels[2] = CreateStats(1010, 52, 105, 55, 0, 0, 500, 100, 1);
        levels[3] = CreateStats(1020, 54, 110, 60, 0, 0, 500, 100, 1);
        levels[4] = CreateStats(1030, 56, 115, 65, 0, 0, 500, 100, 1);
        levels[5] = CreateStats(1050, 58, 120, 70, 0, 0, 550, 150, 1);

        levels[6] = CreateStats(1060, 60, 125, 75, 0, 0, 550, 150, 1);
        levels[7] = CreateStats(1070, 62, 130, 80, 0, 0, 550, 150, 1);
        levels[8] = CreateStats(1080, 64, 135, 85, 0, 0, 550, 150, 1);
        levels[9] = CreateStats(1090, 66, 140, 90, 0, 0, 550, 150, 1);
        levels[10] = CreateStats(1110, 68, 145, 95, 0, 0, 600, 200, 3);

        levels[11] = CreateStats(1120, 70, 150, 100, 0, 0, 600, 200, 1);
        levels[12] = CreateStats(1130, 72, 155, 105, 0, 0, 600, 200, 1);
        levels[13] = CreateStats(1140, 74, 160, 110, 0, 0, 600, 200, 1);
        levels[14] = CreateStats(1150, 76, 165, 115, 0, 0, 600, 200, 1);
        levels[15] = CreateStats(1170, 78, 170, 120, 0, 0, 650, 250, 1);

        levels[16] = CreateStats(1180, 80, 175, 125, 0, 0, 650, 250, 1);
        levels[17] = CreateStats(1190, 82, 180, 130, 0, 0, 650, 250, 1);
        levels[18] = CreateStats(1200, 84, 185, 135, 0, 0, 650, 250, 1);
        levels[19] = CreateStats(1210, 86, 190, 140, 0, 0, 650, 250, 1);
        levels[20] = CreateStats(1230, 88, 195, 145, 0, 0, 700, 300, 3);

        levels[21] = CreateStats(1240, 90, 200, 150, 0, 0, 700, 300, 1);
        levels[22] = CreateStats(1250, 92, 205, 155, 0, 0, 700, 300, 1);
        levels[23] = CreateStats(1260, 94, 210, 160, 0, 0, 700, 300, 1);
        levels[24] = CreateStats(1270, 96, 215, 165, 0, 0, 700, 300, 1);
        levels[25] = CreateStats(1290, 98, 220, 170, 0, 0, 750, 350, 1);

        levels[26] = CreateStats(1300, 100, 225, 175, 0, 0, 750, 350, 1);
        levels[27] = CreateStats(1310, 102, 230, 180, 0, 0, 750, 350, 1);
        levels[28] = CreateStats(1320, 104, 235, 185, 0, 0, 750, 350, 1);
        levels[29] = CreateStats(1330, 106, 240, 190, 0, 0, 750, 350, 1);
        levels[30] = CreateStats(1400, 110, 250, 200, 0, 0, 800, 500, 3);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log("Player Level Data 입력 완료!");
    }

    private P_Stats CreateStats(
        int hp,
        int def,
        int str,
        int dex,
        int intelligence,
        float speed,
        int weight,
        int exp,
        int sp)
    {
        P_Stats stats = new P_Stats();

        stats.HP = hp;
        stats.DEF = def;
        stats.STR = str;
        stats.DEX = dex;
        stats.INT = intelligence;
        stats.Speed = speed;
        stats.Weight = weight;
        stats.EXP = exp;
        stats.SP = sp;

        return stats;
    }
#endif
}