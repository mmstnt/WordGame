using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/DevelopMapDataSO")]
public class DevelopMapDataSO : ScriptableObject
{
    public Sprite background;
    public List<MapSite> mapSiteList = new List<MapSite>();

    [System.Serializable]
    public struct MapSite
    {
        public string name;
        public Vector2 site;
        public Sprite image;
    }

}
