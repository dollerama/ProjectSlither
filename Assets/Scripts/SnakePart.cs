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
            localPos = transform.position;
        }
        
        SetPos(new Vector2(transform.position.x, transform.position.y));   
    }

    void OnDestroy() {
        Destroy(localDup);
    }

    public void SetPos(Vector2 newP) {
        GetComponent<PhotonView>().RPC("SetPosPriv", RpcTarget.All, newP);
    }

    public void SetLocalPos(Vector2 newP) {
        localPos = newP;
        //if(localPosSeek) return;
        //localPosSeek = true;
        //DOTween.To(()=> localPos, x=> localPos = x, newP, Time.fixedDeltaTime*5).OnComplete(() => localPosSeek = false);
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
            var distx = (localPos.x - localDup.transform.position.x);
            distx *= distx;
            var disty = (localPos.y - localDup.transform.position.y);
            disty *= disty;
            if(distx > disty) {
                localDup.transform.position = Vector2.MoveTowards(localDup.transform.position, new Vector2(localPos.x, localDup.transform.position.y), 2f);
            } else if(distx < disty) {
                localDup.transform.position = Vector2.MoveTowards(localDup.transform.position, new Vector2(localDup.transform.position.x, localPos.y), 2f);
            } else {
                localDup.transform.position = Vector2.MoveTowards(localDup.transform.position, localPos, 2f);
            }
        }
        transform.position = grid.trans(pos);
    }
}
