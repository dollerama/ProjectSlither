using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon.StructWrapping;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;

public class Snake : MonoBehaviourPunCallbacks
{
    public int playerID;
    public Vector2 pos;
    public Vector2 localPos;
    public Vector2 prevPos;
    public Vector2 localPrevPos;
    public Vector2 vel;
    public List<Vector2> localVel;

    public float speed;
    private Grid grid;
    private ServerPlayer serverplayer;

    public GameObject body;
    public int bodyCount;
    public List<GameObject> parts;
    private float life = 0;

    // Start is called before the first frame update

    void Start() {
        grid = GameObject.FindObjectOfType<Grid>();
        serverplayer = GameObject.FindObjectOfType<ServerPlayer>();

        GetComponent<PhotonView>().RPC("SetPos", RpcTarget.All, new Vector2(transform.position.x, transform.position.y)); 
        if(photonView.IsMine) {
            for(int i=0; i < bodyCount; i++) {
                var tmp = PhotonNetwork.Instantiate(body.name, pos + Vector2.down * (i+1), Quaternion.identity);
                parts.Add(tmp);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!serverplayer.Ready) return;

        life += Time.deltaTime;
        if(photonView.IsMine) {
            if(Input.GetKey(KeyCode.UpArrow)) {
                if(localVel.Count < 3 && vel != Vector2.down) localVel.Add(Vector2.up);
            } 
            if(Input.GetKey(KeyCode.DownArrow)) {
                if(localVel.Count < 3 && vel != Vector2.up) localVel.Add(Vector2.down);
            } 
            if(Input.GetKey(KeyCode.LeftArrow)) {
                if(localVel.Count < 3 && vel != Vector2.right) localVel.Add(Vector2.left);
            } 
            if(Input.GetKey(KeyCode.RightArrow)) {
                if(localVel.Count < 3 && vel != Vector2.left) localVel.Add(Vector2.right);
            } 
            if(localVel.Count > 0) {
                GetComponent<PhotonView>().RPC("SetVel", RpcTarget.All, localVel.ElementAt(0));
                localVel.RemoveAt(0);
            }
            GetComponent<PhotonView>().RPC("SetPos", RpcTarget.All, pos + vel*Time.deltaTime*speed); 

            if(Vector2.Distance( grid.trans(pos), grid.trans(prevPos)) >= grid.size.x) {
                

                if(bodyCount > 1) {
                    for(int i=1; i < bodyCount; i++) {
                        if(Vector2.Distance(parts[bodyCount-i].GetComponent<SnakePart>().pos, parts[bodyCount-i-1].GetComponent<SnakePart>().pos) > grid.size.x/2) {
                            parts[bodyCount-i].GetComponent<SnakePart>().SetPos(grid.trans(parts[bodyCount-i-1].GetComponent<SnakePart>().pos));
                        }
                    } 
                }

                if(bodyCount > 1) {
                    parts[1].GetComponent<SnakePart>().SetPos(parts[0].GetComponent<SnakePart>().pos);
                }
                if(bodyCount > 0) {
                    parts[0].GetComponent<SnakePart>().SetPos(prevPos);
                }

                prevPos = grid.trans(pos);
                pos = prevPos;

                var a = Physics2D.OverlapBoxAll(prevPos, Vector2.one/4f, 0f);
                if(a.Length > 0 && life > 1) {
                    foreach (var b in a) {
                        if((b.GetComponent<Snake>() && b.GetComponent<Snake>().playerID == playerID) || b.GetComponent<SnakePart>()) {
                            foreach(var p in parts) {
                                PhotonNetwork.Destroy(p.GetComponent<PhotonView>());
                            }
                            PhotonNetwork.Destroy(GetComponent<PhotonView>());
                            PhotonNetwork.Instantiate("SnakeHead", Vector3.zero, Quaternion.identity);
                        }
                        if(b.GetComponent<ISnakeCollidable>() != null) {
                            b.GetComponent<ISnakeCollidable>().collide(this);
                        }
                    }
                }
            }

            GameObject.FindObjectOfType<Cam>().follow = pos;
        }

        transform.position = grid.trans(pos);
    }

    public void AddTail() { 
        if(photonView.IsMine) {
            for(int i = 0; i < parts.Count; i++) {
                parts[i].transform.DOPunchScale(Vector3.one*1.05f, 0.35f).SetDelay(i*0.07f).Play();
            }

            var p = Vector2.zero;
            if(bodyCount <= 1) {
                p = pos-vel;
            }
            else {
                p = parts[bodyCount-2].GetComponent<SnakePart>().pos;
            }
            var tmp = PhotonNetwork.Instantiate(body.name, p, Quaternion.identity);
            parts.Add(tmp);
            bodyCount++;
        }
    }

    public bool RemoveTail() {
        if(bodyCount > 0) {
            PhotonNetwork.Destroy(parts[bodyCount-1].GetComponent<PhotonView>());
            parts.RemoveAt(bodyCount-1);
            bodyCount -= 1;
            return true;
        } else {
            return false;
        }
    }

    [PunRPC]
    void SetVel(Vector2 newV) {
        vel = newV;
    }

    [PunRPC]
    void SetPos(Vector2 newP)
    {
        pos = newP;
    }
}
