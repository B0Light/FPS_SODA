using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

// ??? ?? ?? ??, ?? UI? ???? ?? ???
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // ???? ??? ????? ???? ??? ????
    public static GameManager instance
    {
        get
        {
            // ?? ??? ??? ?? ????? ???? ????
            if (m_instance == null)
            {
                // ??? GameManager ????? ?? ??
                m_instance = FindObjectOfType<GameManager>();
            }

            // ??? ????? ??
            return m_instance;
        }
    }

    private static GameManager m_instance; // ???? ??? static ??

    public GameObject playerPrefab; // ??? ???? ??? ???

    public bool isGameover { get; private set; } // ?? ?? ??

    // ????? ?? ????, ??? ???
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }


    private void Awake()
    {
        // ?? ??? ????? ? ?? GameManager ????? ???
        if (instance != this)
        {
            // ??? ??
            Destroy(gameObject);
        }
    }

    // ?? ??? ??? ????? ? ?? ????? ??
    private void Start()
    {
        // ??? ?? ?? ??
        Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
        // ?? y?? 0?? ??
        randomSpawnPos.y = 0f;

        // ???? ?? ?? ???????? ?? ??
        // ?, ?? ?? ????? ????, ?? ???? ?? ??? ??????? ??
        PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
    }

    // ?? ?? ??
    public void EndGame()
    {
        // ?? ?? ??? ??? ??
        isGameover = true;
    }

    // ??? ??? ???? ?? ??? ?
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
}