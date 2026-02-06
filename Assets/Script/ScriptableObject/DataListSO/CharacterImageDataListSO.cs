using UnityEngine;
using static CharacterImageDataListSO;

[CreateAssetMenu(menuName = "DataList/CharacterImageDataListSO")]
public class CharacterImageDataListSO : DataListSO<CharacterImage, Sprite>
{
    [System.Serializable]
    public struct CharacterImage
    {
        public string id;
        public Sprite data;
    }

    protected override string getID(CharacterImage entry) => entry.id;

    protected override Sprite getValue(CharacterImage entry) => entry.data;
}
