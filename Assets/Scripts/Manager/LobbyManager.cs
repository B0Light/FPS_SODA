using Photon.Pun; // ����Ƽ�� ���� ������Ʈ��
using Photon.Realtime; // ���� ���� ���� ���̺귯��
using System;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.UI;

// ������(��ġ ����ŷ) ������ �� ������ ���
public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1"; // ���� ����

    private string userID = "jack";

    public Text connectionInfoText; // ��Ʈ��ũ ������ ǥ���� �ؽ�Ʈ
    public Button joinButton; // �� ���� ��ư

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;        // ���ӿ� �ʿ��� ����(���� ����) ����
        PhotonNetwork.NickName = userID;

        Debug.Log(PhotonNetwork.SendRate);

        PhotonNetwork.ConnectUsingSettings(); // ������ ������ ������ ������ ���� ���� �õ�

        // �� ���� ��ư�� ��� ��Ȱ��ȭ
        joinButton.interactable = false;
        // ������ �õ� ������ �ؽ�Ʈ�� ǥ��
        connectionInfoText.text = "������ ������ ������...";
    }

    // ������ ���� ���� ������ �ڵ� ����
    public override void OnConnectedToMaster()
    {
        // �� ���� ��ư�� Ȱ��ȭ
        joinButton.interactable = true;
        // ���� ���� ǥ��
        connectionInfoText.text = "�¶��� : ������ ������ �����";
    }

    // ������ ���� ���� ���н� �ڵ� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        // �� ���� ��ư�� ��Ȱ��ȭ
        joinButton.interactable = false;
        // ���� ���� ǥ��
        connectionInfoText.text = "�������� : ������ ������ ������� ����\n���� ��õ� ��...";

        // ������ �������� ������ �õ�
        PhotonNetwork.ConnectUsingSettings();
    }

    // �� ���� �õ�
    public void Connect()
    {
        // �ߺ� ���� �õ��� ���� ����, ���� ��ư ��� ��Ȱ��ȭ
        joinButton.interactable = false;

        // ������ ������ �������̶��
        if (PhotonNetwork.IsConnected)
        {
            // �� ���� ����
            connectionInfoText.text = "�뿡 ����...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // ������ ������ �������� �ƴ϶��, ������ ������ ���� �õ�
            connectionInfoText.text = "�������� : ������ ������ ������� ����\n���� ��õ� ��...";
            // ������ �������� ������ �õ�
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // (�� ���� ����)���� �� ������ ������ ��� �ڵ� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRandFail {returnCode} : {message}");
        // ���� ���� ǥ��
        connectionInfoText.text = "�� ���� ����, ���ο� �� ����...";
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom("NEW GAME", roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("CreatRoom");
        Debug.Log($"RoomName : {PhotonNetwork.CurrentRoom.Name}");
    }

    // �뿡 ���� �Ϸ�� ��� �ڵ� ����
    public override void OnJoinedRoom()
    {
        Debug.Log($"PhotonNetwork.InRoom : {PhotonNetwork.InRoom}");
        Debug.Log($"PlayerCount : {PhotonNetwork.CurrentRoom.PlayerCount}");
        // ���� ���� ǥ��
        connectionInfoText.text = "�� ���� ����";
        // ��� �� �����ڵ��� Main ���� �ε��ϰ� ��
        PhotonNetwork.LoadLevel("NewProject");
    }
}