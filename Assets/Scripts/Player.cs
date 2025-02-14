using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

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
    }

	[PunRPC]
	void ReadyUpInit() {
		Ready = true;
		PlayerList.transform.parent.gameObject.SetActive(false);
	}

	public override void OnConnectedToMaster(){
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}
	
	public override void OnJoinedRoom(){
		Debug.Log("Connected to Room");
		var p = PhotonNetwork.Instantiate("SnakeHead", new Vector3(0, 0, 0), Quaternion.identity);
		var tmp = Instantiate(PlayerTagPref, Vector3.zero, Quaternion.identity);
		tmp.transform.SetParent(PlayerList);
		tmp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Player{PhotonNetwork.PlayerList.Length}";
	}
}