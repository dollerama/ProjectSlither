using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

public class Food : MonoBehaviourPun, ISnakeCollidable 
{
    public bool alive;
    public bool dead;
    // Start is called before the first frame update
    void Start()
    {
        alive = true;
        dead = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine) {
            if(!alive && !dead) {
                dead = true;
                StartCoroutine(DestroyPart());
            } 
        }
    }

    IEnumerator DestroyPart() {
        
        var tmp = PhotonNetwork.Instantiate("Bloom", transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        PhotonNetwork.Destroy(tmp.GetComponent<PhotonView>());
        PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }

    [PunRPC]
    void MarkForDel()
    {
        alive = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void collide(Snake snek)
    {
        snek.AddTail();
        photonView.RPC("MarkForDel", RpcTarget.All);
    }
}
