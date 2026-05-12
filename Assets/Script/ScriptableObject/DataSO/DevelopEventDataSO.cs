using UnityEngine;

[CreateAssetMenu(menuName = "Data/DevelopEventDataSO")]
public class DevelopEventDataSO : ScriptableObject
{
    [Header("事件描述")]
    public string developEventName;
    [TextArea(5, 10)]
    public string description;

    [Header("事件參數")]
    public int actionPoint;
    public Sprite image;
}
