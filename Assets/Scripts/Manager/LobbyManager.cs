using Photon.Pun; // 유니티용 포톤 컴포넌트들
using Photon.Realtime; // 포톤 서비스 관련 라이브러리
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 마스터(매치 메이킹) 서버와 룸 접속을 담당
public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1"; // 게임 버전

    public Text connectionInfoText; // 네트워크 정보를 표시할 텍스트
    public TMP_Text idText;
    public Button joinButton; // 룸 접속 버튼
    public GameObject joinObj;
    public Button startButton;
    public GameObject startObj;
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;        // 접속에 필요한 정보(게임 버전) 설정

        Debug.Log(PhotonNetwork.SendRate);

        PhotonNetwork.ConnectUsingSettings(); // 설정한 정보를 가지고 마스터 서버 접속 시도

        // 룸 접속 버튼을 비활성화
        joinObj.SetActive(false);
        startObj.SetActive(false);
        joinButton.interactable = false;
        startButton.interactable = false;
        // 접속을 시도 중임을 텍스트로 표시
        connectionInfoText.text = "Connecting to master server...";
    }

    // 룸 접속 시도
    public void Connect()
    {
        if (idText.text.Length <= 1)
        {
            connectionInfoText.text = "input your name more than 1";
            return;
        }
        else
        {
            joinButton.interactable = false;
            joinObj.SetActive(false);
            startObj.SetActive(false);
            PhotonNetwork.LocalPlayer.NickName = idText.text;
            // 마스터 서버에 접속중이라면
            if (PhotonNetwork.IsConnected)
            {
                // 룸 접속 실행
                connectionInfoText.text = "Connecting...";
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // 마스터 서버에 접속중이 아니라면, 마스터 서버에 접속 시도
                connectionInfoText.text = "Offline: No connection with master server\nRetrying connection...";
                // 마스터 서버로의 재접속 시도
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    // 마스터 서버 접속 성공시 자동 실행
    public override void OnConnectedToMaster()
    {
        // 룸 접속 버튼을 활성화
        joinButton.interactable = true;
        joinObj.SetActive(true);
        startObj.SetActive(false);
        // 접속 정보 표시
        connectionInfoText.text = "Online: Connected with Master Server";
    }

    // 마스터 서버 접속 실패시 자동 실행
    public override void OnDisconnected(DisconnectCause cause)
    {
        // 룸 접속 버튼을 비활성화
        joinButton.interactable = false;
        // 접속 정보 표시
        connectionInfoText.text = "Offline: Unable to connect to master server\nRetrying connection...";

        // 마스터 서버로의 재접속 시도
        PhotonNetwork.ConnectUsingSettings();
    }

    // (빈 방이 없어)랜덤 룸 참가에 실패한 경우 자동 실행
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRandFail {returnCode} : {message}");
        // 접속 상태 표시
        connectionInfoText.text = "No empty rooms, create new rooms...";
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 6;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom("Tutorial", roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("CreatRoom");
        Debug.Log($"RoomName : {PhotonNetwork.CurrentRoom.Name}");
    }

    // 룸에 참가 완료된 경우 자동 실행
    public override void OnJoinedRoom()
    {
        Debug.Log($"PhotonNetwork.InRoom : {PhotonNetwork.InRoom}");
        Debug.Log($"PlayerCount : {PhotonNetwork.CurrentRoom.PlayerCount}");
        // 접속 상태 표시
        connectionInfoText.text = "room join success";
        
        if(PhotonNetwork.IsMasterClient)
        {
            connectionInfoText.text = ($"PlayerCount : {PhotonNetwork.CurrentRoom.PlayerCount}");
            joinObj.SetActive(false);
            startObj.SetActive(true);
            joinButton.interactable = false;
            startButton.interactable = true;
        }
        else
        {
            photonView.RPC("UpdateInfoText", RpcTarget.MasterClient);
        }
        
    }

    public void StartGame()
    {
        // 모든 룸 참가자들이 Main 씬을 로드하게 함
        PhotonNetwork.LoadLevel("NewProject");
    }

    [PunRPC]
    public void UpdateInfoText()
    {
        connectionInfoText.text = ($"PlayerCount : {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
}