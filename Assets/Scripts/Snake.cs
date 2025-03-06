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
    public int teamId;

    // Start is called before the first frame update

    void Start() {
        grid = GameObject.FindObjectOfType<Grid>();
        serverplayer = GameObject.FindObjectOfType<ServerPlayer>();

        if(photonView.IsMine) {
            for(int i=0; i < bodyCount; i++) {
                var tmp = PhotonNetwork.Instantiate(body.name, pos + Vector2.down * (i+1), Quaternion.identity);
                parts.Add(tmp);
            }
        }
    }

    private void tryAddVel(Vector2 val) {
        if(localVel.Count <= 3 && vel != val*-1) {
            if (localVel.Count > 0 && localVel.Last() != val || localVel.Count == 0) {
                localVel.Add(val);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!serverplayer.Ready) return;

        if(photonView.IsMine) {
            if(teamId == -1) {
                var smallestTeam = serverplayer.teams.OrderBy(x => x.playerCount).Select(x => x.teamId).First();
                teamId = smallestTeam;
                serverplayer.GetTeam(smallestTeam).playerCount++;
                pos = serverplayer.GetTeam(smallestTeam).transform.position;
            }

            life += Time.deltaTime;
        
            if(Input.GetKey(KeyCode.UpArrow)) {
                tryAddVel(Vector2.up);
            } 
            else if(Input.GetKey(KeyCode.DownArrow)) {
                tryAddVel(Vector2.down);
            } 
            else if(Input.GetKey(KeyCode.LeftArrow)) {
                tryAddVel(Vector2.left);
            } 
            else if(Input.GetKey(KeyCode.RightArrow)) {
                tryAddVel(Vector2.right);
            } 

            if(localVel.Count > 0) {
                vel = localVel.ElementAt(0);
                localVel.RemoveAt(0);
            }
            pos += vel*Time.deltaTime*speed; 

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
                        if((b.GetComponent<Snake>() && b.GetComponent<Snake>().playerID == playerID) || b.GetComponent<SnakePart>() || b.tag == "Wall") {
                            foreach(var p in parts) {
                                PhotonNetwork.Destroy(p.GetComponent<PhotonView>());
                            }
                            
                            if(serverplayer.GetTeam(teamId).alive) { 
                                var tm = PhotonNetwork.Instantiate("SnakeHead", serverplayer.GetTeam(teamId).transform.position, Quaternion.identity);
                                tm.GetComponent<Snake>().teamId = teamId;
                                tm.GetComponent<Snake>().pos = serverplayer.GetTeam(teamId).transform.position;
                            }

                            PhotonNetwork.Destroy(GetComponent<PhotonView>());
                        }
                        if(b.GetComponent<ISnakeCollidable>() != null) {
                            b.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                            b.GetComponent<ISnakeCollidable>().collide(this);
                        }
                    }
                }
            }

            GameObject.FindObjectOfType<Cam>().follow = pos;
        }

        transform.position = grid.trans(pos);
    }

    public override void OnLeftRoom()
    {
        serverplayer.GetTeam(teamId).playerCount--;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(vel);
            stream.SendNext(pos);
            stream.SendNext(teamId);
        }
        else
        {
            vel = (Vector2)stream.ReceiveNext();
            pos = (Vector2)stream.ReceiveNext();
            teamId = (int)stream.ReceiveNext();
        }
    }

    public void AddTail() { 
        if(photonView.IsMine) {
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
}
