using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProficiencyInterface : MonoBehaviour
{
    [Header("監聽")]
    public AttributeImageEventSO attributeSourceUIEvent;
    public VoidEventSO closeAttributeSourceUIEvent;

    [Header("參數")]
    public int[] attributeValue;
    public string[] attributeImage;
    public string[] attributeName;

    [Header("組件")]
    public TMP_Text pointText;
    public RadarChart radarChart;
    public Transform attributeGroup;
    public Transform proficiencyGroup;
    public Transform attributeSourceInterface;
    public GameObject attributeImageGameObject;
    public GameObject proficiencyButtonGameObject;

    private AttributeImage curAttributeImage;

    public void OnEnable()
    {
        attributeSourceUIEvent.onEventRaised += onAttributeSourceUIEvent;
        closeAttributeSourceUIEvent.onEventRaised += onCloseAttributeSourceUIEvent;
        initialize(ProficiencyType.Exercise);
    }

    public void OnDisable()
    {
        attributeSourceUIEvent.onEventRaised -= onAttributeSourceUIEvent;
        closeAttributeSourceUIEvent.onEventRaised -= onCloseAttributeSourceUIEvent;
    }

    public void initialize(ProficiencyType proficiencyType) 
    {
        PlayerDataSO playerData = DataManager.instance.playerData;
        playerData.getAllProficiency();
        switch (proficiencyType) 
        {
            case ProficiencyType.Exercise:
                attributeValue = new int[] 
                {   
                    playerData.hp,
                    playerData.mp,
                    playerData.strength,
                    playerData.dexterity,
                    playerData.constitution,
                    playerData.intelligence,
                    playerData.wisdom,
                    playerData.charisma
                };
                attributeImage = new string[]
                {
                    "UI00014",
                    "UI00015",
                    "UI00016",
                    "UI00017",
                    "UI00018",
                    "UI00019",
                    "UI00020",
                    "UI00021"
                };
                attributeName = new string[]
                {
                    "生命",
                    "能量",
                    "力量",
                    "敏捷",
                    "體質",
                    "智力",
                    "感知",
                    "魅力",
                };
                break;

            case ProficiencyType.Martial:
                break;

            case ProficiencyType.Research:
                break;
        }

        
        updataAttributeGroup(attributeGroup, attributeValue, attributeImage);
        createProficiencyButton(proficiencyType);
    }

    private void updataAttributeGroup(Transform UIGroup, int[] newValue, string[] newValueImage)
    {
        clearUIGrounp(UIGroup);
        //繪製雷達圖
        radarChart.setValue(newValue, newValueImage);

        //更新屬性值
        for (int i = 0; i < attributeValue.Length; i++) 
        {
            AttributeImage attributeImage = Instantiate(attributeImageGameObject, UIGroup).GetComponent<AttributeImage>();
            TMP_Text attributeText = attributeImage.transform.GetChild(0).GetComponent<TMP_Text>();

            attributeImage.attributeType = (Attribute)i;
            attributeImage.GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData(newValueImage[i]);
            attributeText.text = $" {attributeName[i]}：{newValue[i]}";
            //Debug.Log($"attributeText.text\n屬性來源\n{DataManager.instance.playerData.getAttributeSource((Attribute)i)}");
        }
    }

    private void createProficiencyButton(ProficiencyType proficiencyType) 
    {
        clearUIGrounp(proficiencyGroup);

        List<string> proficiencyIDList = DataManager.instance.playerData.getProficiencyIDList(proficiencyType);
        foreach(string proficiencyID in proficiencyIDList) 
        {
            ProficiencyButton proficiencyButton = Instantiate(proficiencyButtonGameObject, proficiencyGroup).GetComponent<ProficiencyButton>();
            proficiencyButton.initialize(proficiencyID);
        }
    }

    private void clearUIGrounp(Transform UIGroup)
    {
        for (int i = UIGroup.childCount - 1; i >= 0; i--)
        {
            GameObject UIGameObject = UIGroup.GetChild(i).gameObject;
            UIGameObject.transform.SetParent(null);
            Destroy(UIGameObject);
        }
    }

    private void onAttributeSourceUIEvent(AttributeImage attributeImage)
    {
        if (curAttributeImage == attributeImage)
            return;

        curAttributeImage = attributeImage;
        updateAttribureImageInterface();
    }

    private void updateAttribureImageInterface()
    {
        if (curAttributeImage == null)
            return;

        TMP_Text attributeSourceText = attributeSourceInterface.GetComponentInChildren<TMP_Text>();
        attributeSourceText.text = DataManager.instance.playerData.getAttributeSource(curAttributeImage.attributeType);
        attributeSourceInterface.gameObject.SetActive(true);
    }

    private void onCloseAttributeSourceUIEvent()
    {
        if (curAttributeImage == null)
            return;

        curAttributeImage = null;
        attributeSourceInterface.gameObject.SetActive(false);
    }
}
