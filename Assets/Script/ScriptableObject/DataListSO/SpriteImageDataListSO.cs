using UnityEngine;
using static SpriteImageDataListSO;

[CreateAssetMenu(menuName = "DataList/SpriteImageDataListSO")]
public class SpriteImageDataListSO : DataListSO<SpriteImage, Sprite>
{
    [System.Serializable]
    public struct SpriteImage
    {
        public string id;
        public Sprite data;
    }

    protected override string getID(SpriteImage entry) => entry.id;

    protected override Sprite getValue(SpriteImage entry) => entry.data;
}
