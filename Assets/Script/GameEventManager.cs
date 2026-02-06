using UnityEngine;
using UnityEngine.InputSystem;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;
    [Header("¼s¼½")]
    public VoidEventSO gameConfirmEvent;

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
        playerControl.Input.Confirm.started += onConfirm;
    }

    private void OnDisable()
    {
        playerControl.Disable();
        playerControl.Input.Confirm.started -= onConfirm;
    }

    private void onConfirm(InputAction.CallbackContext context)
    {
        gameConfirmEvent.raiseEvent();
    }

    public void enterBattle(string battleID) 
    {
        dialogManager.SetActive(false);
        battleManager.SetActive(true);
        battleManager.GetComponent<BattleManager>().battleInitialize(battleID);
    }
}
