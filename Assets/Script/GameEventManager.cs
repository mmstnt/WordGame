using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;
    [Header("¼s¼½")]
    public VoidEventSO dialogConfirmEvent;
    public VoidEventSO selectBackEvent;
    public StringEventSO selectConfirmEvent;
    public Vector2EventSO keyboardSelectEvent;
    public Vector2EventSO mouseMoveEvent;

    [Header("²Õ¥ó")]
    public GameObject dialogManager;
    public GameObject battleManager;

    public PlayerControl playerControl; 

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        playerControl = new PlayerControl();
        dialogManager.GetComponent<DialogManager>().initialize();
    }

    private void OnEnable()
    {
        playerControl.Enable();
        playerControl.Input.DialogConfirm.started += onDialogConfirm;
        playerControl.Input.SelectConfirm.started += onSelectConfirmEvent;
        playerControl.Input.SelectBack.started += onSelectBackEvent;
        playerControl.Input.KeyboardSelect.started += onKeyboardSelectEvent;
        playerControl.Input.MouseMove.performed += onMouseMoveEvent;
    }

    private void OnDisable()
    {
        playerControl.Disable();
        playerControl.Input.DialogConfirm.started -= onDialogConfirm;
        playerControl.Input.SelectConfirm.started -= onSelectConfirmEvent;
        playerControl.Input.SelectBack.started -= onSelectBackEvent;
        playerControl.Input.KeyboardSelect.started -= onKeyboardSelectEvent;
        playerControl.Input.MouseMove.performed -= onMouseMoveEvent;
    }

    private void onDialogConfirm(InputAction.CallbackContext context)
    {
        dialogConfirmEvent.raiseEvent();
    }

    private void onSelectConfirmEvent(InputAction.CallbackContext context)
    {
        string inputMode = context.control.device is Mouse ? "Mouse" : "Keyboard";
        selectConfirmEvent.raiseEvent(inputMode);
    }

    private void onSelectBackEvent(InputAction.CallbackContext context)
    {
        selectBackEvent.raiseEvent();
    }

    private void onKeyboardSelectEvent(InputAction.CallbackContext context) 
    {
        Vector2 dir = context.ReadValue<Vector2>();
        keyboardSelectEvent.raiseEvent(dir);
    }

    private void onMouseMoveEvent(InputAction.CallbackContext context)
    {
        Vector2 vector = context.ReadValue<Vector2>();
        mouseMoveEvent.raiseEvent(vector);
    }

    public void enterBattle(string battleID) 
    {
        dialogManager.SetActive(false);
        battleManager.SetActive(true);
        battleManager.GetComponent<BattleManager>().battleInitialize(battleID, DataManager.instance.playerData);
    }

    public void endBattle(bool isVictory) 
    {

        battleManager.SetActive(false);
        dialogManager.SetActive(true);
        dialogManager.GetComponent<DialogManager>().jumpToStory("B");
        Debug.Log(isVictory);
    }
}
