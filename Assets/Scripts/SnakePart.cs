using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Unity.VisualScripting;
using UnityEngine;

public class SnakePart : MonoBehaviourPunCallbacks
{
    public Vector2 pos;

    public Grid grid;
    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.FindObjectOfType<Grid>();
        
        SetPos(new Vector2(transform.position.x, transform.position.y));
    }

    public void SetPos(Vector2 newP) {
        GetComponent<PhotonView>().RPC("SetPosPriv", RpcTarget.All, newP);
    }

    [PunRPC]
    void SetPosPriv(Vector2 newP)
    {
        pos = newP;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = grid.trans(pos);
    }
}
