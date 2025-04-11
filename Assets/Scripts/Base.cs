using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class Base : MonoBehaviourPunCallbacks, ISnakeCollidable
{
    private Tweener punchT;
    public Transform WorldCanvas;
    public int Resources;
    public TMPro.TextMeshProUGUI ResourcesT;
    public int teamId;
    public int playerCount;
    public int livingPlayerCount;
    public bool alive;

    void Start()
    {
        alive = true;
    }
    
    public void collide(Snake snek)
    {
        if(photonView.IsMine && snek.RemoveTail()) {
            photonView.RPC("SetResources", RpcTarget.All, snek.teamId == teamId ? 1 : -1);
            if(punchT is null || !punchT.IsPlaying()) {
                punchT = transform.DOPunchScale(Vector3.one*1.05f, 0.16f).Play();
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Resources);
            stream.SendNext(alive);
            stream.SendNext(playerCount);
            stream.SendNext(teamId);
            stream.SendNext(livingPlayerCount);
        }
        else
        {
            Resources = (int)stream.ReceiveNext();
            ResourcesT.text = Resources.ToString(); 
              
            alive = (bool)stream.ReceiveNext();
            playerCount = (int)stream.ReceiveNext();
            teamId = (int)stream.ReceiveNext();
            livingPlayerCount = (int)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if(photonView.IsMine) {
            if(Resources < 0 && alive) {
                photonView.RPC("killRPC", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void SetResources(int amt) {
        Resources += amt;
        
        ResourcesT.text = Resources.ToString();         
    }

    [PunRPC]
    void addPlayerRPC() {
        playerCount++;
        livingPlayerCount++;       
    }

    [PunRPC]
    void removePlayerRPC() {
        livingPlayerCount--;       
    }

    [PunRPC]
    void killRPC() {
        alive = false;
        transform.DOScale(Vector3.one*0.2f, 0.25f);    
    }

    public void addPlayer() {
        photonView.RPC("addPlayerRPC", RpcTarget.All);
    }

    public void removePlayer() {
        photonView.RPC("removePlayerRPC", RpcTarget.All);
    }
}
