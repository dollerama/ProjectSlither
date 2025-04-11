using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ServerMenu : MonoBehaviourPunCallbacks
{
    public Button connect;
    public Button CreateRoom;
    public Button Refresh;
    public RectTransform connectPanel;
    public TMPro.TextMeshProUGUI connectPanelText;
    public RectTransform RoomList;
    public RectTransform RoomListContent;
    public GameObject ServerRoomTile;
    private bool autoLeave;
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Disconnect();

        if(photonView.IsMine) {
            connectPanel.transform.localScale = Vector3.zero;
 
            connect.onClick.AddListener(() => {
                PhotonNetwork.ConnectUsingSettings();
                connect.transform.DOScale(0, 0.1f).OnComplete(() => {
                    connectPanel.gameObject.SetActive(true);
                    connectPanel.transform.localScale = Vector3.zero;
                    connectPanel.DOScale(1, 0.1f);
                    connectPanelText.text = "connecting...";
                });
            });
        }
    }

    public override void OnConnectedToMaster()
    {
        connectPanelText.text = "Success";
        connectPanel.DOScale(0, 0.1f).SetDelay(1).OnComplete(() => {
            connectPanel.gameObject.SetActive(false);
            RoomList.gameObject.SetActive(true);
        }).Play();

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        CreateRoom.onClick.AddListener(() => {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;

            PhotonNetwork.LoadLevel(1);
            PhotonNetwork.CreateRoom($"Room{PhotonNetwork.CountOfRooms}", roomOptions, null);
        });
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log(roomList.Count);
        var ch = RoomListContent.GetComponentsInChildren<Transform>();
		foreach(var c in ch) {
            if(c.parent == RoomListContent.transform) {
                Destroy(c.gameObject);
            }
		}

        foreach(var rl in roomList) {
            var tmp = Instantiate(ServerRoomTile, Vector3.zero, Quaternion.identity);
		    tmp.transform.SetParent(RoomListContent);
            tmp.GetComponent<ServerRoomTile>().Init(rl.Name);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectPanelText.text = cause.ToString();
        connectPanel.DOScale(0, 0.1f).SetDelay(1).OnComplete(() => {
            connectPanel.gameObject.SetActive(false);
            connect.transform.DOScale(1, 0.15f).Play();
        }).Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
