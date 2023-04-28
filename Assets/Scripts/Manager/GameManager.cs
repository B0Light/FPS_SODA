using Cinemachine;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }
            return m_instance;
        }
    }

    private static GameManager m_instance; 

    public GameObject playerPrefab;
    [SerializeField] Transform playerSpawnTransform;
    public CinemachineVirtualCamera PlayerSightCam;

    [Header("UIManager")]
    [SerializeField] CrosshairManager crosshairManager;
    [SerializeField] Compass compass;
    [SerializeField] PlayerHealthBar PlayerHealthBar;
    public bool isGameover { get; private set; }

    [Header("ScoreBoard")]
    public int score = 0;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }


    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, playerSpawnTransform.position, playerSpawnTransform.rotation);
        player.GetComponent<PlayerController>().VirtualCamera = PlayerSightCam;
        compass.playerController = player.GetComponent<PlayerController>();
        compass.setPlayer();
        crosshairManager.m_WeaponsManager = player.GetComponent<PlayerWeaponsManager>();
        crosshairManager.setPlayer();
        PlayerHealthBar.m_playerController = player.GetComponent<PlayerController>();
        PlayerHealthBar.m_PlayerHealth = player.GetComponent<Health>();
    }

    public void EndGame()
    {
        isGameover = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("LobbyScene");
    }

    public void JoinPlayer()
    {

    }
}