using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

public class Food : MonoBehaviour, ISnakeCollidable 
{
    public bool alive;
    public bool dead;
    public String guid;

    public Vector3 pos;

    public Transform spr;
    public Transform shadow;
    // Start is called before the first frame update
    void Start()
    {
        alive = true;
        dead = false;
        spr.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(!alive && !dead) {
            dead = true;
            StartCoroutine(DestroyPart());
        } 

        spr.localPosition = new Vector3(0, Mathf.Abs(0.3f*Mathf.Sin(Time.time)),0);
        spr.localScale = Vector3.one * (0.05f*Mathf.Sin(Time.time*3) + 1.1f);

        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime*5);
    }

    IEnumerator DestroyPart() {
        var tmp = PhotonNetwork.Instantiate("Bloom", transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        PhotonNetwork.Destroy(tmp.GetComponent<PhotonView>());
        FoodManager.Instance.Remove(guid);
    }

    public void collide(Snake snek)
    {
        snek.AddTail();
        alive = false;
        spr.GetComponent<SpriteRenderer>().enabled = false;
        shadow.GetComponent<SpriteRenderer>().enabled = false;
    }
}
