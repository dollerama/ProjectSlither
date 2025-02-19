using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Food : MonoBehaviourPun, ISnakeCollidable 
{
    public bool alive;
    // Start is called before the first frame update
    void Start()
    {
        alive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine) {
            if(!alive) {
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
            } 
        }
    }

    [PunRPC]
    void MarkForDel()
    {
        alive = false;
    }

    public void collide(Snake snek)
    {
        snek.AddTail();
        photonView.RPC("MarkForDel", RpcTarget.All);
    }
}
