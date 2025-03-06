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
        StartCoroutine(Efx());
    }

    public void SetPos(Vector2 newP) {
        pos = newP;
    }

    IEnumerator Efx() {
        var tmp = PhotonNetwork.Instantiate("Snare", transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        PhotonNetwork.Destroy(tmp.GetComponent<PhotonView>());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(pos);
        }
        else
        {
            pos = (Vector2)stream.ReceiveNext();
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = grid.trans(pos);
    }
}
