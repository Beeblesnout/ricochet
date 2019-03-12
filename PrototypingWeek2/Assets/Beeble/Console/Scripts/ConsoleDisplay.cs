using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ConsoleDisplay : ConsoleDisplayBase
{
    [SerializeField]
    private GameObject displayPanelPrefab;
    [SerializeField]
    private AnimationCurve moveSmoothing;
    [SerializeField]
    private RectTransform panel;
    public TMP_Text displayText { get; private set; }
    public TMP_InputField inputField { get; private set; }

    public bool isOpen;

    private int moving;
    private float moveTime;
    const float moveDuration = .25f;

    public UnityEvent consoleToggle;

    public override void Awake()
    {
        base.Awake();
        Player player = FindObjectOfType<Player>();
        consoleToggle.AddListener(player.ToggleInput);
        if (panel == null) 
            panel = Instantiate(displayPanelPrefab, Vector3.up * -500, Quaternion.identity, UIManager.Instance.canvas).GetComponent<RectTransform>();
        displayText = panel.GetChild(0).GetComponent<TMP_Text>();
        inputField = panel.GetChild(1).GetComponent<TMP_InputField>();
        inputField.onSubmit.AddListener(ProcessCommand);
        // inputField.resetOnDeActivation = true;
    }

    void OnGUI() {
        if (moving == 1)
        {
            panel.transform.position = Vector3.Lerp(Vector3.up * -500, Vector3.zero, (Time.time - moveTime) / moveDuration);
            if (Time.time - moveTime >= moveDuration) moving = 0;
        }
        else if (moving == -1)
        {
            panel.transform.position = Vector3.Lerp(Vector3.zero, Vector3.up * -500, (Time.time - moveTime) / moveDuration);
            if (Time.time - moveTime >= moveDuration) moving = 0;
        }
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Toggle();
        }
    }

    public void AddLine(string line)
    {
        displayText.text = displayText.text + "\n" + line;
    }

    public void Open()
    {
        moveTime = Time.time;
        moving = 1;
        isOpen = true;
        // Cursor.lockState = CursorLockMode.None;
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void Close()
    {
        moveTime = Time.time;
        moving = -1;
        isOpen = false;
        // Cursor.lockState = CursorLockMode.Locked;
        inputField.DeactivateInputField();
    }

    public void Toggle()
    {
        consoleToggle.Invoke();
        if (!isOpen) Open();
        else Close();
        Debug.Log(isOpen ? "Opened" : "Closed");
    }

    public void ProcessCommand(string command)
    {
        if (command.Equals("")) return;
        Console.WriteLine(">" + command);
        Console.RecieveCommand(command);
        inputField.text = "";
        inputField.Select();
    }
}

public abstract class ConsoleDisplayBase : MonoBehaviour
{
    private static object sm_lock = new object();
    private static bool sm_destroying;
    private static ConsoleDisplay sm_instance;
    public static ConsoleDisplay Display { 
        get 
        {
            if (sm_destroying)
            {
                Debug.LogWarning("[Singleton] Instance of console display is destroying. (returning null)");
                return null;
            }
            
            lock (sm_lock)
            {
                if (sm_instance == null)
                {
                    sm_instance = (ConsoleDisplay)FindObjectOfType(typeof(ConsoleDisplay));

                    if (sm_instance == null)
                    {
                        var spawnedManager = new GameObject();
                        sm_instance = spawnedManager.AddComponent<ConsoleDisplay>();
                        spawnedManager.name = "ConsoleDisplay";
                    }
                }
                return sm_instance;
            }
        }
    }

    public virtual void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public virtual void OnApplicationQuit()
    {
        SetDestroying();
    }

    public virtual void OnDestroy()
    {
        SetDestroying();
    }

    protected void SetDestroying()
    {
        sm_destroying = true;
    }
}
