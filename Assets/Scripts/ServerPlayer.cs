using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class ServerPlayer : MonoBehaviourPunCallbacks
{
	public GameObject PlayerTagPref;
	public Transform PlayerList;
	public Button ReadyUpBtn;
	public bool Ready;

	public GameObject WinPanel;
	public TMPro.TextMeshProUGUI WinPanelText;
	private float winTimer;
	private int winner;

	public List<Base> teams;

	public Base GetTeam(int id) {
		return teams.Find(x => x.teamId == id);
	}
	
    // Start is called before the first frame update
    void Start()
    {
		if(Ready) {
			PlayerList.transform.parent.gameObject.SetActive(false);
		}
		winner = -1;
		
		ReadyUpBtn.onClick.AddListener(() => {
			photonView.RPC("ReadyUpInit", RpcTarget.All);
		});

		if(photonView.IsMine) {
			var b = GameObject.FindObjectsOfType<Base>();
			foreach(var ba in b) {
				teams.Add(ba);
			}
		}
    }

    void Update()
    {
        if(photonView.IsMine && Ready) {
			if(winner != -1) {
				Time.timeScale = Mathf.Lerp(Time.timeScale, 0.05f, Time.deltaTime);

				winTimer += Time.deltaTime;

				if(winTimer > 2) {
					Time.timeScale = 1;
					PhotonNetwork.LoadLevel(0);
					//PhotonNetwork.LeaveRoom();
				}
			} else if(teams.Count(x => x.alive) == 1) {
				winner = teams.Where(x => x.alive).First().teamId;
				WinPanel.SetActive(true);
				WinPanelText.text = $"Team {winner} Wins!";
			}
		}
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Ready);
			stream.SendNext(winner);
        }
        else
        {
            Ready = (bool)stream.ReceiveNext();
			winner = (int)stream.ReceiveNext();
        }
    }

	[PunRPC]
	void ReadyUpInit() {
		Ready = true;
		teams = teams.OrderBy(x => x.teamId).ToList();
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
	
	public override void OnJoinedRoom(){
		Debug.Log($"Connected to Room -> {PhotonNetwork.CurrentRoom}");
		var p = PhotonNetwork.Instantiate("SnakeHead", Vector3.zero, Quaternion.identity);
		p.GetComponent<Snake>().teamId = -1;

		photonView.RPC("RefreshPlayerList", RpcTarget.All);
	}

    public override void OnLeftRoom()
    {
		PhotonNetwork.LoadLevel(0);
    }
}
