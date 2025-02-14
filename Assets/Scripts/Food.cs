using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Food : MonoBehaviourPun
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
}
