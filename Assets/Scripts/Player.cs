using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class Player : MonoBehaviourPunCallbacks
{
	public GameObject PlayerTagPref;
	public Transform PlayerList;
	public Button ReadyUpBtn;
	public bool Ready;
	
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

		if(Ready) {
			PlayerList.transform.parent.gameObject.SetActive(false);
		}

		ReadyUpBtn.onClick.AddListener(() => {
			photonView.RPC("ReadyUpInit", RpcTarget.All);
		});

		if(!photonView.IsMine) {
			GetComponent<SpriteRenderer>().enabled = true;
		}
    }

	[PunRPC]
	void ReadyUpInit() {
		Ready = true;
		PlayerList.transform.parent.gameObject.SetActive(false);
	}

	[PunRPC]
	void RefreshPlayerList() {
		var ch = PlayerList.GetComponentsInChildren<Transform>();
		foreach(var c in ch) {
			if(c != PlayerList) {
				Destroy(c.gameObject);
			}
		}
		for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
			var tmp = Instantiate(PlayerTagPref, Vector3.zero, Quaternion.identity);
			tmp.transform.SetParent(PlayerList);
			tmp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Player{i+1}";
			if(i == PhotonNetwork.LocalPlayer.GetPlayerNumber()) {
				tmp.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = Color.blue;
			}
		}
	}

	public override void OnConnectedToMaster(){
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)  
    {  
        Debug.Log(roomList.Count);  
    }
	
	public override void OnJoinedRoom(){
		Debug.Log($"Connected to Room -> {PhotonNetwork.CurrentRoom}");
		var p = PhotonNetwork.Instantiate("SnakeHead", new Vector3(0, 0, 0), Quaternion.identity);

		photonView.RPC("RefreshPlayerList", RpcTarget.All);
	}

    public override void OnLeftRoom()
    {
        photonView.RPC("RefreshPlayerList", RpcTarget.All);
    }
}
