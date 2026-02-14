using UnityEngine;
using UnityEngine.InputSystem;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;
    [Header("¼s¼½")]
    public VoidEventSO dialogConfirmEvent;
    public VoidEventSO selectConfirmEvent;
    public VoidEventSO selectBackEvent;
    public Vector2EventSO selectEvent;

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
    }

    private void OnEnable()
    {
        playerControl.Enable();
        playerControl.Input.DialogConfirm.started += onDialogConfirm;
        playerControl.Input.SelectConfirm.started += onSelectConfirmEvent;
        playerControl.Input.SelectBack.started += onSelectBackEvent;
        playerControl.Input.Select.started += onSelect;
    }

    private void OnDisable()
    {
        playerControl.Disable();
        playerControl.Input.DialogConfirm.started -= onDialogConfirm;
        playerControl.Input.SelectConfirm.started -= onSelectConfirmEvent;
        playerControl.Input.SelectBack.started -= onSelectBackEvent;
        playerControl.Input.Select.started -= onSelect;
    }

    private void onDialogConfirm(InputAction.CallbackContext context)
    {
        dialogConfirmEvent.raiseEvent();
    }

    private void onSelectConfirmEvent(InputAction.CallbackContext context)
    {
        selectConfirmEvent.raiseEvent();
    }

    private void onSelectBackEvent(InputAction.CallbackContext context)
    {
        selectBackEvent.raiseEvent();
    }

    private void onSelect(InputAction.CallbackContext context) 
    {
        Vector2 dir = context.ReadValue<Vector2>();
        selectEvent.raiseEvent(dir);
    }

    public void enterBattle(string battleID) 
    {
        dialogManager.SetActive(false);
        battleManager.SetActive(true);
        battleManager.GetComponent<BattleManager>().battleInitialize(battleID, DataManager.instance.playerData);
    }
}
