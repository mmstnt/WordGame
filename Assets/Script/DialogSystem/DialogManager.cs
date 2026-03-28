using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private Dictionary<string, Transform> dialogCharacterDic;

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
                readDialogTags(story.currentTags);
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
        dialogCharacterDic = new Dictionary<string, Transform>();
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
                case "background":
                    string backgroundID = tagsCmd[1];
                    showBackground(backgroundID);
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
