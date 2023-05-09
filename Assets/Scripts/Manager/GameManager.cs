using Cinemachine;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private GameObject player;
    [SerializeField] Transform playerSpawnTransform;
    public CinemachineVirtualCamera PlayerSightCam;

    [Header("UIManager")]
    [SerializeField] CrosshairManager crosshairManager;
    [SerializeField] Compass compass;
    [SerializeField] PlayerHealthBar PlayerHealthBar;
    public bool isGameover { get; private set; }

    [Header("Respawn")]
    public Transform[] respawnPos;

    [Header("ScoreBoard")]
    public int score = 0;
    private int idx = 0;
    public Dictionary<int, Tuple<string, int>> killScore = new Dictionary<int, Tuple<string, int>>();
    public Image[] ranking_Health;
    public TMP_Text[] ranking_Text;

    class PlayerScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(killScore.Count); // Dictionary의 크기를 먼저 전송합니다.

            foreach (var kvp in killScore)
            {
                stream.SendNext(kvp.Key); // key값 전송
                stream.SendNext(kvp.Value.Item1); // Tuple의 string 값 전송
                stream.SendNext(kvp.Value.Item2); // Tuple의 int 값 전송
            }
        }
        else // 수신
        {
            int count = (int)stream.ReceiveNext(); // Dictionary의 크기를 먼저 수신합니다.

            for (int i = 0; i < count; i++)
            {
                int key = (int)stream.ReceiveNext(); // key값 수신
                string strValue = (string)stream.ReceiveNext(); // Tuple의 string 값 수신
                int intValue = (int)stream.ReceiveNext(); // Tuple의 int 값 수신

                if (killScore.ContainsKey(key))
                {
                    var score = killScore[key];
                    killScore[key] = new Tuple<string, int>(strValue, intValue);
                }
                else
                {
                    killScore.Add(key, new Tuple<string, int>(strValue, intValue));
                }
            }
        }
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

        CreatPlayer();

        foreach (Player photonPlayer in PhotonNetwork.PlayerList)
        {
            killScore.Add(photonPlayer.ActorNumber, Tuple.Create(photonPlayer.NickName, 20));
        }

    }

    public void EndGame()
    {
        isGameover = true;
    }

    private void Update()
    {
        idx = 0;
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PhotonNetwork.LeaveRoom();
        }
        
        var sortedByIntDescending = killScore.OrderByDescending(x => x.Value.Item2);
        foreach (var kvp in sortedByIntDescending)
        {
            ranking_Health[idx].fillAmount = (float)kvp.Value.Item2 / 20;
            ranking_Text[idx++].text = kvp.Value.Item1 + " : " + kvp.Value.Item2.ToString();
        }
        
    }

    public void RespawnPlayer()
    {
        Invoke("CreatPlayer", 5f);
    }

    public void CreatPlayer()
    {
        player = PhotonNetwork.Instantiate(playerPrefab.name, respawnPos[UnityEngine.Random.Range(0, respawnPos.Length)].position, respawnPos[UnityEngine.Random.Range(0, respawnPos.Length)].rotation);
        player.GetComponent<PlayerController>().VirtualCamera = PlayerSightCam;
        compass.playerController = player.GetComponent<PlayerController>();
        compass.setPlayer();
        crosshairManager.m_WeaponsManager = player.GetComponent<PlayerWeaponsManager>();
        crosshairManager.setPlayer();
        PlayerHealthBar.m_playerController = player.GetComponent<PlayerController>();
        PlayerHealthBar.m_PlayerHealth = player.GetComponent<Health>();
    }

    public override void OnLeftRoom()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("LobbyScene");
    }
}