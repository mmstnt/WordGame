using UnityEngine;
using UnityEngine.UI;

public class CharacterSpeedBox : MonoBehaviour
{
    public Image box;
    public Image mask;
    public Image headImage;

    public void initialize(Sprite boxSprite, Sprite maskSprite, Sprite headImageSprite, Color color)
    {
        box.sprite = boxSprite;
        mask.sprite = maskSprite;
        headImage.sprite = headImageSprite;

        box.color = color;
        mask.color = color;
        headImage.color = color;
    }
}
