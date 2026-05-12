using UnityEngine;

[CreateAssetMenu(menuName = "Data/BattleMapDataSO")]
public class BattleMapDataSO : ScriptableObject
{
    public int siteCount;
    public Sprite image;
    public Vector2[] unitSite;

    private void OnValidate()
    {
        if (unitSite.Length != siteCount)
        {
            System.Array.Resize(ref unitSite, siteCount);
        }
    }
}
