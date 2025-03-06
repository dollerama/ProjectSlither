using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ServerRoomTile : MonoBehaviourPunCallbacks
{
    public TMPro.TextMeshProUGUI Text;
    public Button ConnectBtn;
    // Start is called before the first frame update
    public void Init(string n)
    {
        Text.text = n;
        ConnectBtn.onClick.AddListener(() => {
            Debug.Log("Hi");
            PhotonNetwork.LoadLevel(1);
            PhotonNetwork.JoinRoom(Text.text);
        });
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
