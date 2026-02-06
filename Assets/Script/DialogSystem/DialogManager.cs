using Ink.Runtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("監聽")]
    public VoidEventSO gameConfirmEvent;

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
    public GameObject characterGameObject;

    private Dictionary<string, Transform> dialogCharacterDic;

    public Story story;

    private void Awake()
    {
        story = new Story(ink.text);
        dialogCharacterDic = new Dictionary<string, Transform>();
    }

    private void OnEnable()
    {
        gameConfirmEvent.onEventRaised += onGameConfirmEvent;
    }

    private void OnDisable()
    {
        gameConfirmEvent.onEventRaised -= onGameConfirmEvent;
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
    private void onGameConfirmEvent()
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
                readDialogTags(story.currentTags);
            }
        }
    }

    private void readDialogTags(List<string> dialogTags) 
    {
        foreach (var tags in dialogTags)
        {
            string[] tagsCmd = tags.Split(",");
            switch (tagsCmd[0])
            {
                case "name":
                    string name = tagsCmd[1];
                    getDialogName(name);
                    break;
                case "show":
                    string ch = tagsCmd[1];
                    string chImage = tagsCmd[2];
                    Vector2 site = new Vector2(float.Parse(tagsCmd[3]), float.Parse(tagsCmd[4]) - 3);
                    bool RL = (tagsCmd[5] == "R") ? true : false;
                    showCharacter(ch, chImage, site, RL);
                    break;
                case "exit":

                    break;
                case "battle":
                    string battleID = tagsCmd[1];
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

    private void showCharacter(string ch, string chImage, Vector2 site, bool RL) 
    {
        if (!dialogCharacterDic.ContainsKey(ch)) 
        {
            GameObject dialogCharacter = Instantiate(characterGameObject, characterGroup);
            dialogCharacterDic[ch] = dialogCharacter.transform;
            dialogCharacterDic[ch].GetComponent<Image>().sprite = DataManager.instance.characterImageDataList.getData(chImage);
            dialogCharacterDic[ch].GetComponent<Image>().SetNativeSize();
            dialogCharacterDic[ch].GetComponent<Image>().color = Color.white;
            dialogCharacterDic[ch].position = site;
            dialogCharacterDic[ch].rotation = Quaternion.Euler(0, RL ? 0 : 180, 0);
            dialogCharacterDic[ch].SetAsLastSibling();
        }
        else
        {
            dialogCharacterDic[ch].GetComponent<Image>().sprite = DataManager.instance.characterImageDataList.getData(chImage);
            dialogCharacterDic[ch].GetComponent<Image>().SetNativeSize();
            dialogCharacterDic[ch].GetComponent<Image>().color = Color.white;
            dialogCharacterDic[ch].GetComponent<DialogCharacter>().moveTo(site, 5);
            dialogCharacterDic[ch].rotation = Quaternion.Euler(0, RL ? 0 : 180, 0);
            dialogCharacterDic[ch].SetAsLastSibling();
        }
    }
}
