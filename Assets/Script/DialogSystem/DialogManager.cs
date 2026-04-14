using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("監聽")]
    public VoidEventSO dialogConfirmEvent;

    [Header("故事")]
    public TextAsset ink;

    [Header("文字物件")]
    public TMP_Text nameTextGameObject;
    public TMP_Text dialogTextGameObject;
    public float textSpeed;
    public string nameText;
    public string dialogText;

    private float textTime;
    private string curNameText = "";
    private string curDialogText = "";
    //[Header("對話索引")]
    //public string dialogEvent;
    //public int dialogIndex;

    [Header("組件")]
    public Transform characterGroup;
    public Transform dialogChoiceGroup;
    public GameObject characterGameObject;
    public GameObject backgroundGameObject;
    public GameObject dialogChoiceButtonGameObject;

    private Dictionary<string, DialogCharacter> dialogCharacterDic;
    private Dictionary<string, int> tagPriority = new Dictionary<string, int>
    {
        {"backgroung", 0},  //更換背景
        {"name", 1},        //更換名字
        {"show", 2},        //創建和移動角色
        {"high", 3},        //角色高亮
        {"exit",4},         //移除角色
        {"battle",5},       //進入戰鬥
    };

    public Story story;

    private void OnEnable()
    {
        dialogConfirmEvent.onEventRaised += onDialogConfirmEvent;
    }

    private void OnDisable()
    {
        dialogConfirmEvent.onEventRaised -= onDialogConfirmEvent;
    }
    
    public void initialize() 
    {
        story = new Story(ink.text);
        clearCharacter();
    }

    private void Update()
    {
        textTime += Time.deltaTime;
        if (textTime > textSpeed)
        {
            textTime = 0;
            if (curDialogText.Length < dialogText.Length)
                curDialogText += dialogText.Substring(curDialogText.Length, 1);
            updataText(curNameText, curDialogText);
        }
    }

    private void onDialogConfirmEvent()
    {
        if (curDialogText != dialogText && dialogText != "")
        {
            curDialogText = dialogText;
        }
        else
        {
            nextDialog();
        }
    }

    public void updataText(string name, string dialog)
    {
        nameTextGameObject.text = name;
        dialogTextGameObject.text = dialog;
    }

    private void nextDialog()
    {
        if (dialogChoiceGroup.childCount > 0) 
        return; 

        if (story.canContinue) 
        {
            story.Continue();
            curDialogText = "";
            curNameText = "";
            dialogText = story.currentText;
            foreach (var dialogCharacter in dialogCharacterDic)
            {
                dialogCharacter.Value.GetComponent<Image>().color = Color.gray;
            }
            if (story.currentTags.Count > 0) 
            {
                List<string> sortedTags = story.currentTags.OrderBy(t => {
                    string key = t.Split(':')[0].Trim();
                    return tagPriority.ContainsKey(key) ? tagPriority[key] : 99; // 沒定義的排最後
                }).ToList();

                readDialogTags(sortedTags);
            }
        }
        else if (story.currentChoices.Count > 0) 
        {
            for (int i = 0; i < story.currentChoices.Count; i++) 
            {
                Choice choice = story.currentChoices[i];
                int index = choice.index;
                GameObject dialogChoiceButton = Instantiate(dialogChoiceButtonGameObject, dialogChoiceGroup);
                dialogChoiceButton.GetComponentInChildren<TMP_Text>().text = choice.text;
                dialogChoiceButton.GetComponent<Button>().onClick.AddListener
                    (
                    delegate
                    {
                        OnClickChoiceButton(index);
                    }
                    );
            }
            if (dialogChoiceGroup.childCount > 0)
            {
                StartCoroutine(choiceFirstButton());
            }
        }
    }

    public void jumpToStory(string storyID) 
    {
        initialize();
        story.ChoosePathString(storyID);
        nextDialog();
    }

    public IEnumerator choiceFirstButton()
    {
        dialogChoiceGroup.GetComponent<CanvasGroup>().interactable = false;
        dialogChoiceGroup.gameObject.SetActive(true);

        yield return new WaitForEndOfFrame();

        dialogChoiceGroup.GetComponent<CanvasGroup>().interactable = true;
        EventSystem.current.SetSelectedGameObject(dialogChoiceGroup.transform.GetChild(0).gameObject);
    }

    private void clearCharacter()
    {
        if (dialogCharacterDic != null)
        {
            foreach (var character in dialogCharacterDic)
            {
                Destroy(character.Value.gameObject);
            }
        }
        dialogCharacterDic = new Dictionary<string, DialogCharacter>();
    }

    private void readDialogTags(List<string> dialogTags) 
    {
        foreach (var tags in dialogTags)
        {
            string tagsCmd = tags.Contains(":") ? tags.Split(':')[0] : tags;
            string[] args = tags.Contains(":") ? tags.Split(':')[1].Split(',') : new string[0];
            switch (tagsCmd)
            {
                case "background":
                    string backgroundID = args[0];
                    showBackground(backgroundID);
                    break;
                case "name":
                    string name = args[0];
                    getDialogName(name);
                    break;
                case "show":
                    string ch = args[0];
                    string chImage = args[1];
                    Vector2 site = new Vector2(float.Parse(args[2]), float.Parse(args[3]) - 3);
                    bool RL = (args[4] == "L") ? true : false;
                    showCharacter(ch, chImage, site, RL);
                    break;
                case "high":
                    for(int i = 0; i < args.Length; i++) 
                    {
                        dialogCharacterDic[args[i]].image.color = Color.white;
                    }
                    break;
                case "exit":
                    string exitch = args[0];
                    StartCoroutine(exitCharacter(exitch));
                    break;
                case "battle":
                    string battleID = args[0];
                    GameEventManager.instance.enterBattle(battleID);
                    break;
            }
        }
    }

    private void getDialogName(string name) 
    {
        nameText = name;
        curNameText = nameText;
    }

    private void showCharacter(string ch, string chImageID, Vector2 site, bool dire) 
    {
        if (!dialogCharacterDic.ContainsKey(ch))
        {
            //生成角色物件
            GameObject dialogCharacter = Instantiate(characterGameObject, characterGroup);

            dialogCharacterDic[ch] = dialogCharacter.GetComponent<DialogCharacter>();
            dialogCharacterDic[ch].initialize(chImageID, site, dire);
            StartCoroutine(dialogCharacterDic[ch].characterFade(2.0f, false));
            
            //將角色排到最上層
            dialogCharacterDic[ch].transform.SetAsLastSibling();
        }
        else
        {
            dialogCharacterDic[ch].moveTo(chImageID, site, dire, 5);

            //將角色排到最上層
            dialogCharacterDic[ch].transform.SetAsLastSibling();
        }
    }

    private IEnumerator exitCharacter(string exitch)
    {
        //等待執行完
        yield return dialogCharacterDic[exitch].characterFade(2.0f, true);

        Destroy(dialogCharacterDic[exitch].gameObject);
        dialogCharacterDic.Remove(exitch);
    }

    private void showBackground(string backgroundID) 
    {
        Sprite backgroundImage = DataManager.instance.backgroundImageDataList.getData(backgroundID);
        if (backgroundImage != null) 
        {
            backgroundGameObject.GetComponent<Image>().sprite = backgroundImage;
            backgroundGameObject.GetComponent<Image>().color = Color.white;
        }
        else 
        {
            backgroundGameObject.GetComponent<Image>().sprite = null;
            backgroundGameObject.GetComponent<Image>().color = Color.clear;
        }
    }

    public void OnClickChoiceButton(int index) 
    {
        story.ChooseChoiceIndex(index);
        for (int i = 0; i < dialogChoiceGroup.childCount; i++)
        {
            Destroy(dialogChoiceGroup.GetChild(i).gameObject);
        }

        StartCoroutine(continueStory());
    }

    public IEnumerator continueStory()
    {
        dialogChoiceGroup.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        nextDialog();
    }
}
