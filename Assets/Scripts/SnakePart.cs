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

    public SpriteRenderer spr;
    public Sprite[] sprites;
    public bool a_up;
    public bool a_down;
    public bool a_left;
    public bool a_right;
    public bool b_up;
    public bool b_down;
    public bool b_left;
    public bool b_right;
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

    public void regularPiece(bool vertical) {
        if(vertical) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            spr.sprite = sprites[0];
        } else {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            spr.sprite = sprites[0];
        }
    }

    public void tailPiece(int i) {
        transform.rotation = Quaternion.Euler(0, 0, (i+1)*-90);
        spr.sprite = sprites[2];
    }

    public void cornerPiece(int i) {
        spr.sprite = sprites[1];
        transform.rotation = Quaternion.Euler(0, 0, i*-90);
    }

    public void UpdateSprite() {
        if(a_right && b_up) {
            cornerPiece(0);
        }
        else if(a_right && b_down) {
            cornerPiece(1);
        }
        else if(a_down && b_left) {
            cornerPiece(2);
        }
        else if(a_down && b_right) {
            cornerPiece(1);
        }
        else if(a_left && b_down) {
            cornerPiece(2);
        }
        else if(a_left && b_up) {
            cornerPiece(3);
        }
        else if(a_up && b_right) {
            cornerPiece(0);
        }
        else if(a_up && b_left) {
            cornerPiece(3);
        }

        if((a_up && b_down || a_down && b_up) && (!a_left && !b_left && !a_right && !b_right)) {
            regularPiece(true);
        }
        if((a_left && b_right || a_right && b_left) && (!a_up && !b_up && !a_down && !b_down)) {
            regularPiece(false);
        }

        if(!a_up && !a_down && !a_left && !a_right) {
            if(b_up) 
                tailPiece(0);
            else if(b_right) 
                tailPiece(1);
            else if(b_down) 
                tailPiece(2);
            else if(b_left) 
                tailPiece(3);
        }

    }

    // Update is called once per frame
    void Update()
    {
        UpdateSprite();
        transform.position = grid.trans(pos);
    }
}
