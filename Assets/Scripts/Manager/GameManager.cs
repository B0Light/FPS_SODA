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
using static UnityEngine.Rendering.DebugUI;

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
    public GameObject player;
    [SerializeField] Transform playerSpawnTransform;
    public CinemachineVirtualCamera PlayerSightCam;
    public bool isGameover { get; private set; }
    private KeyValuePair<int, Tuple<string, int>> winner;

    [Header("UIManager")]
    [SerializeField] CrosshairManager crosshairManager;
    [SerializeField] Compass compass;
    [SerializeField] PlayerHealthBar PlayerHealthBar;
    [SerializeField] TMP_Text coin;
    [SerializeField] GameObject EndGameObj;
    [SerializeField] GameObject WinScene;
    [SerializeField] GameObject LoseScene;


    [Header("Respawn")]
    public Transform[] respawnPos;

    [Header("ScoreBoard")]
    public int score = 0;
    private int idx = 0;
    public Dictionary<int, Tuple<string, int>> killScore = new Dictionary<int, Tuple<string, int>>();
    public Image[] ranking_Health;
    public TMP_Text[] ranking_Text;
    public TMP_Text[] End_RankingText;

    [Header("WeaponIcon")]
    public Image[] weaponIcon;
    private PlayerWeaponsManager pwm;
    private PlayerInventory inventory;

    class PlayerScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // ????
        {
            stream.SendNext(killScore.Count); // Dictionary?? ?????? ???? ??????????.

            foreach (var kvp in killScore)
            {
                stream.SendNext(kvp.Key); // key?? ????
                stream.SendNext(kvp.Value.Item1); // Tuple?? string ?? ????
                stream.SendNext(kvp.Value.Item2); // Tuple?? int ?? ????
            }
        }
        else // ????
        {
            int count = (int)stream.ReceiveNext(); // Dictionary?? ?????? ???? ??????????.

            for (int i = 0; i < count; i++)
            {
                int key = (int)stream.ReceiveNext(); // key?? ????
                string strValue = (string)stream.ReceiveNext(); // Tuple?? string ?? ????
                int intValue = (int)stream.ReceiveNext(); // Tuple?? int ?? ????

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
        WinScene.SetActive(false);
        LoseScene.SetActive(false);
        EndGameObj.SetActive(false);
        CreatPlayer();

        foreach (Player photonPlayer in PhotonNetwork.PlayerList)
        {
            killScore.Add(photonPlayer.ActorNumber, Tuple.Create(photonPlayer.NickName, 20));
        }
        isGameover = false;
    }

    public void EndGame()
    {
        isGameover = true;
        EndGameObj.SetActive(true); 
        EndRanking();
        if(PhotonNetwork.LocalPlayer.ActorNumber == winner.Key)
        {
            Win();
        }
        else
        {
            Lose();
        }
        Time.timeScale = 0;
    }

    private void Update()
    {
        idx = 0;
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PhotonNetwork.LeaveRoom();
        }

        var sortedByIntDescending = killScore.OrderByDescending(x => x.Value.Item2);
        winner = sortedByIntDescending.First();
        foreach (var kvp in sortedByIntDescending)
        {
            ranking_Health[idx].fillAmount = (float)kvp.Value.Item2 / 20;
            ranking_Text[idx++].text = kvp.Value.Item1 + " : " + kvp.Value.Item2.ToString();
        }

        coin.text = inventory.Coin.ToString();

        for (int i = 0; i < weaponIcon.Length; i++)
        {
            Color color = weaponIcon[i].color;
            if (pwm.ActiveWeaponLV[i] == 0)
            {
                color.a = 0;
                weaponIcon[i].color = color;
                continue;

            }

            if (i == pwm.ActiveWeaponIndex)
            {
                color.a = 1;
                weaponIcon[i].color = color;
            }
            else
            {
                color.a = 0.5f;
                weaponIcon[i].color = color;
            }
        }
    }

    public void RespawnPlayer()
    {
        CreatPlayer();
    }

    public void CreatPlayer()
    {
        player = PhotonNetwork.Instantiate(playerPrefab.name, respawnPos[UnityEngine.Random.Range(0, respawnPos.Length)].position, respawnPos[UnityEngine.Random.Range(0, respawnPos.Length)].rotation);
        player.GetComponent<PlayerController>().VirtualCamera = PlayerSightCam;
        player.GetComponent<PlayerController>().GM = this;
        compass.playerController = player.GetComponent<PlayerController>();
        compass.setPlayer();
        pwm = player.GetComponent<PlayerWeaponsManager>();
        inventory = player.GetComponent<PlayerInventory>();
        crosshairManager.m_WeaponsManager = player.GetComponent<PlayerWeaponsManager>();
        crosshairManager.setPlayer();
        PlayerHealthBar.m_playerController = player.GetComponent<PlayerController>();
        PlayerHealthBar.m_PlayerHealth = player.GetComponent<Health>();
    }

    public override void OnLeftRoom()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("0.TiltleSecene");
    }


    void Win()
    {
        Debug.Log("win");
        WinScene.SetActive(true);
    }

    void Lose()
    {
        Debug.Log("Lose");
        LoseScene.SetActive(true);
    }

    void EndRanking()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        var sortedByIntDescending = killScore.OrderByDescending(x => x.Value.Item2);
        idx = 0;
        foreach (var kvp in sortedByIntDescending)
        {
            End_RankingText[idx++].text = kvp.Value.Item1 + " : " + kvp.Value.Item2.ToString();
        }
    }

    public void ExitGame_End()
    {
        PhotonNetwork.LoadLevel("0.TiltleSecene");
    }
}