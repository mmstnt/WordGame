using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProficiencyButton : MonoBehaviour
{
    [Header("參數")]
    public string proficiencyId;

    [Header("組件")]
    public Image proficiencyImage;
    public TMP_Text proficiencyText;

    public void initialize(string id)
    {
        proficiencyId = id;
        proficiencyImage.sprite = DataManager.instance.proficiencyDataList.getData(proficiencyId).image;
        proficiencyText.text = DataManager.instance.proficiencyDataList.getData(proficiencyId).proficiencyName;
    }

}
