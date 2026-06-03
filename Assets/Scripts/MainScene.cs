using UnityEngine;

public class MainScene : MonoBehaviour
{

    public static MainScene Instance { get; private set; }

    [SerializeField] private FirstPersonController m_firstPersonController = default;
    [SerializeField] private PlayerInputHandler m_playerInputHandler = default;
    [SerializeField] private MainCanvas m_mainCanvas = default;
    public static FirstPersonController Player => Instance.m_firstPersonController;
    public static PlayerInputHandler InputHandler => Instance.m_playerInputHandler;
    public static MainCanvas MainCanvas => Instance.m_mainCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        MainCanvas.Init();
        MainCanvas.Open();
    }

}
