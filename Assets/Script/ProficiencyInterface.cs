using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ProficiencyInterface : MonoBehaviour
{
    [Header("²Õ¥ó")]
    public TMP_Text pointText;
    public TMP_Text attributeText_1;
    public TMP_Text attributeText_2;
    public TMP_Text attributeText_3;
    public TMP_Text attributeText_4;
    public TMP_Text attributeText_5;
    public TMP_Text attributeText_6;
    public TMP_Text attributeText_7;
    public TMP_Text attributeText_8;
    public Transform proficiencyGroup;
    public GameObject proficiencyButtonGameObject;

    public void OnEnable()
    {
        initialize();
    }

    public void OnDisable()
    {
        
    }

    public void initialize() 
    {
        createProficiencyButton();
    }

    private void createProficiencyButton() 
    {
        clearUIGrounp(proficiencyGroup);

        List<string> proficiencyIDList = DataManager.instance.playerData.getProficiencyIDList(ProficiencyType.Exercise);
        foreach(string proficiencyID in proficiencyIDList) 
        {
            ProficiencyButton proficiencyButton = Instantiate(proficiencyButtonGameObject, proficiencyGroup).GetComponent<ProficiencyButton>();
            proficiencyButton.initialize(proficiencyID);
        }
    }
    private void clearUIGrounp(Transform UIGrounp)
    {
        for (int i = UIGrounp.childCount - 1; i >= 0; i--)
        {
            GameObject UIGameObject = UIGrounp.GetChild(i).gameObject;
            UIGameObject.transform.SetParent(null);
            Destroy(UIGameObject);
        }
    }
}
