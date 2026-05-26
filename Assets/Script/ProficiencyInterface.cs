using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class ProficiencyInterface : MonoBehaviour
{
    [Header("監聽")]
    public VoidEventSO closeAttributeSourceUIEvent;
    public AttributeImageEventSO attributeSourceUIEvent;

    [Header("參數")]
    public int[] attributeValue;
    public string[] attributeImage;
    public string[] attributeName;

    [Header("組件")]
    public TMP_Text pointText;
    public TMP_Text proficiencyNameText;
    public TMP_Text proficiencyText;
    public TMP_Text proficiencyExpText;
    public TMP_Text curEffectText;
    public TMP_Text nextEffectText;
    public TMP_Text upgradeEffectText;
    public Image proficiencyImage;
    public Image proficiencyExpImage;
    public Slider proficiencyExp;
    public Button upgradeButton;
    public RadarChart radarChart;
    public Transform attributeGroup;
    public Transform proficiencyGroup;
    public Transform attributeSourceInterface;
    public Transform informationGroup;
    public GameObject attributeImageGameObject;
    public GameObject proficiencyButtonGameObject;

    private int point;
    private int curPoint;
    private ProficiencyType proficiencyType;
    private AttributeImage curAttributeImage;
    private PlayerDataSO playerData;

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

    public void initialize(ProficiencyType type) 
    {
        this.proficiencyType = type;
        this.playerData = DataManager.instance.playerData;
        this.point = 1 + (playerData.getAllAttribute() / 20);
        this.curPoint = point;

        updataProiciencyInterface();
        createProficiencyButton(proficiencyType);
    }

    private void updataProiciencyInterface() 
    {
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

        updataAttributeGroup();
    }

    private void updataAttributeGroup()
    {
        clearUIGrounp(attributeGroup);
        //繪製雷達圖
        radarChart.setValue(attributeValue, attributeImage);

        //更新屬性值
        for (int i = 0; i < attributeValue.Length; i++) 
        {
            AttributeImage attribute = Instantiate(attributeImageGameObject, attributeGroup).GetComponent<AttributeImage>();
            TMP_Text attributeText = attribute.transform.GetChild(0).GetComponent<TMP_Text>();

            attribute.attributeType = (Attribute)i;
            attribute.GetComponent<Image>().sprite = DataManager.instance.uiImageDataList.getData(attributeImage[i]);
            attributeText.text = $" {attributeName[i]}：{attributeValue[i]}";
        }
    }

    private void createProficiencyButton(ProficiencyType proficiencyType) 
    {
        clearUIGrounp(proficiencyGroup);

        //創建所有修練項按鈕
        List<string> proficiencyIDList = playerData.getProficiencyIDList(proficiencyType);
        foreach(string proficiencyID in proficiencyIDList) 
        {
            string curProficiencyID = proficiencyID;

            GameObject proficiencyButton = Instantiate(proficiencyButtonGameObject, proficiencyGroup);
            proficiencyButton.GetComponent<ProficiencyButton>().initialize(curProficiencyID);

            proficiencyButton.GetComponent<Button>().onClick.AddListener
            (
                delegate
                {
                    proficiencySelectEvent(curProficiencyID);
                }
            );
        }

        if (proficiencyIDList.Count > 0) 
        {
            proficiencySelectEvent(proficiencyIDList[0]);
        }
    }

    private void proficiencySelectEvent(string curProficiencyID) 
    {
        ProficiencyDataSO curProficiency = DataManager.instance.proficiencyDataList.getData(curProficiencyID);
        if (curProficiency == null) 
        {
            informationGroup.gameObject.SetActive(false);
            return;
        }

        informationGroup.gameObject.SetActive(true);
        
        int curProficiencyLevel = playerData.proficiencyIndexDic[curProficiencyID].curLevel;
        float curProficiencyExp = playerData.proficiencyIndexDic[curProficiencyID].curExp;
        float curProficiencyNeedExp = curProficiency.getNeedExp(curProficiencyLevel + 1, curProficiencyLevel + 1);
        
        int addMinExp = (int)(18 * (1 + (playerData.getAllAttribute() / 100f)));
        int addMaxExp = (int)(36 * (1 + (playerData.getAllAttribute() / 100f)));

        bool isLevelMax = (curProficiencyLevel >= curProficiency.levelSettings.Count);

        proficiencyImage.sprite = curProficiency.image;
        proficiencyExp.value = isLevelMax ? 1 : curProficiencyExp / curProficiencyNeedExp;
        pointText.text = $"鍛鍊次數：{curPoint} / {point}";
        proficiencyNameText.text = $"{curProficiency.proficiencyName} {curProficiencyLevel} / {curProficiency.levelSettings.Count}";
        proficiencyText.text = $"{curProficiency.description}";
        proficiencyExpText.text = $"{curProficiency.getFullEffectExp(curProficiencyLevel + 1, curProficiencyLevel + 1, (int)curProficiencyExp)}";
        curEffectText.text = $"{curProficiency.getFullEffectDescription(1, curProficiencyLevel)}";
        nextEffectText.text = $"{curProficiency.getFullEffectDescription(1, curProficiencyLevel + 1)}";
        upgradeEffectText.text = $"增加經驗：\n{addMinExp}～{addMaxExp}";
        //upgradeEffectText.text = $"{curProficiency.getFullEffectDescription(curProficiencyLevel + 1, curProficiencyLevel + 1)}";

        //重製修練按鈕
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener
        (
            delegate
            {
                if (curPoint <= 0 || isLevelMax) return;

                curPoint -= 1;
                upgradeButton.interactable = false;
                int addExp = Random.Range(addMinExp, addMaxExp);
                proficiencyExp.value = ((curProficiencyExp + addExp) >= curProficiencyNeedExp) ? 1 : (curProficiencyExp + addExp) / curProficiencyNeedExp;
        

                Sequence mySequence = DOTween.Sequence();
                //閃白接續淡出
                mySequence.Insert(0, proficiencyExpImage.DOFade(0.25f, 0.05f).OnComplete(() =>
                {
                    
                    proficiencyExpImage.DOFade(0f, 0.15f);
                }));
                //震動
                mySequence.Insert(0, proficiencyExpImage.transform.DOPunchScale(new Vector3(0.03f, 0.42f, 0), 0.2f, 5, 0.5f));

                mySequence.OnComplete(() =>
                {
                    //添加經驗與數據更新
                    playerData.addProficiencyExp(curProficiencyID, addExp);
                    updataProiciencyInterface();
                    proficiencySelectEvent(curProficiencyID);

                    // 恢復按鈕點擊
                    upgradeButton.interactable = true;
                });
            }
        );
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
        attributeSourceText.text = playerData.getAttributeSource(curAttributeImage.attributeType);
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
