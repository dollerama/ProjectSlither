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
    public Vector2 localPos;
    public Vector2 prevPos;
    public bool localPosSeek;
    public GameObject localObjPref;
    private GameObject localDup;

    public Grid grid;
    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.FindObjectOfType<Grid>();
        if(photonView.IsMine) {
            localDup = Instantiate(localObjPref, transform.position, Quaternion.identity);
            localDup.GetComponent<SpriteRenderer>().enabled = false;
            prevPos = transform.position;
        }
        
        SetPos(new Vector2(transform.position.x, transform.position.y));

        // if(!photonView.IsMine) {
		// 	GetComponent<SpriteRenderer>().enabled = true;
		// }   
    }

    void OnDestroy() {
        Destroy(localDup);
    }

    public void SetPos(Vector2 newP) {
        GetComponent<PhotonView>().RPC("SetPosPriv", RpcTarget.All, newP);
    }

    public void SetLocalPos(Vector2 newP) {
        prevPos = newP;
    }

    [PunRPC]
    void SetPosPriv(Vector2 newP)
    {
        pos = newP;
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine) {
            localDup.transform.position = localPos;
        }
        transform.position = grid.trans(pos);
    }
}
