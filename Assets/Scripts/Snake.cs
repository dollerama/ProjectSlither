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
    public Cam cam;

    public float speed;
    private Grid grid;
    private ServerPlayer serverplayer;

    public GameObject body;
    public int bodyCount;
    public List<GameObject> parts;
    private float life = 0;
    public int teamId;

    public Transform spr;

    // Start is called before the first frame update

    void Start() {
        grid = GameObject.FindObjectOfType<Grid>();
        serverplayer = GameObject.FindObjectOfType<ServerPlayer>();

        if(photonView.IsMine) {
            cam = GameObject.FindObjectOfType<Cam>();

            for(int i=0; i < bodyCount; i++) {
                var tmp = PhotonNetwork.Instantiate(body.name, pos + Vector2.down * (i+1), Quaternion.identity);
                parts.Add(tmp);
            }

            UpdateSprites();

            findTeam();

            cam.transform.position = pos;
            cam.follow = pos;
        }
    }

    private void tryAddVel(Vector2 val) {
        if(localVel.Count <= 3 && vel != val*-1) {
            if (localVel.Count > 0 && localVel.Last() != val || localVel.Count == 0) {
                localVel.Add(val);
            }
        }
    }

    void findTeam() {
        if(teamId == -1) {
            var smallestTeam = serverplayer.teams.OrderBy(x => x.playerCount).Select(x => x.teamId).First();
            teamId = smallestTeam;
            serverplayer.GetTeam(smallestTeam).addPlayer();
            pos = serverplayer.GetTeam(smallestTeam).transform.position;
            cam.transform.position = pos;
            cam.follow = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!serverplayer.Ready) return;

        if(photonView.IsMine) { 
            life += Time.deltaTime;
        
            if(Input.GetKey(KeyCode.UpArrow)) {
                tryAddVel(Vector2.up);
            } 
            if(Input.GetKey(KeyCode.DownArrow)) {
                tryAddVel(Vector2.down);
            } 
            if(Input.GetKey(KeyCode.LeftArrow)) {
                tryAddVel(Vector2.left);
            } 
            if(Input.GetKey(KeyCode.RightArrow)) {
                tryAddVel(Vector2.right);
            } 

            pos += vel*Time.deltaTime*speed; 

            if(localVel.Count > 0) {
                var tmp = localVel.ElementAt(0);
                if(vel != tmp*-1 && vel != tmp) {
                    vel = tmp;
                }
                localVel.RemoveAt(0);
            }

            cam.follow = pos + (vel*5);

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
                if(a.Length > 0 && life > 1f) {
                    foreach (var b in a) {
                        if(b.GetComponent<SnakePart>() || b.tag == "Wall") {
                            //if(b.GetComponent<PhotonView>())
                            //    b.GetComponent<PhotonView>().TransferOwnership(photonView.Owner);
                                
                            foreach(var p in parts) {
                                //p.GetComponent<PhotonView>().TransferOwnership(photonView.Owner);
                                PhotonNetwork.Destroy(p.GetComponent<PhotonView>());
                            }
                            
                            if(serverplayer.GetTeam(teamId).alive) { 
                                var tm = PhotonNetwork.Instantiate("SnakeHead", serverplayer.GetTeam(teamId).transform.position, Quaternion.identity);
                                tm.GetComponent<Snake>().teamId = teamId;
                                tm.GetComponent<Snake>().pos = serverplayer.GetTeam(teamId).transform.position;
                            } else {
                                serverplayer.GetTeam(teamId).removePlayer();
                            }

                            PhotonNetwork.Destroy(GetComponent<PhotonView>());
                            return;
                        }
                        if(b.GetComponent<ISnakeCollidable>() != null) {
                            if(b.GetComponent<PhotonView>())
                                b.GetComponent<PhotonView>().TransferOwnership(photonView.Owner);
                            b.GetComponent<ISnakeCollidable>().collide(this);
                        }
                    }
                }

                UpdateSprites();
            }
        }

        transform.position = grid.trans(pos);
    }

    public void UpdateSprites() {
        for(int i=1; i < bodyCount; i++) {
            var current = parts[bodyCount-i].GetComponent<SnakePart>().pos;
            var after = parts[bodyCount-i-1].GetComponent<SnakePart>().pos;
            var before = bodyCount-i+1 < bodyCount ? parts[bodyCount-i+1].GetComponent<SnakePart>().pos : current;

            if(before.y > current.y) {
                parts[bodyCount-i].GetComponent<SnakePart>().a_up = true;
                parts[bodyCount-i].GetComponent<SnakePart>().a_down = false;
            }
            else if(before.y < current.y) {
                parts[bodyCount-i].GetComponent<SnakePart>().a_up = false;
                parts[bodyCount-i].GetComponent<SnakePart>().a_down = true;
            }
            else {
                parts[bodyCount-i].GetComponent<SnakePart>().a_up = false;
                parts[bodyCount-i].GetComponent<SnakePart>().a_down = false;
            }
            if(before.x > current.x) {
                parts[bodyCount-i].GetComponent<SnakePart>().a_right = true;
                parts[bodyCount-i].GetComponent<SnakePart>().a_left = false;
            }
            else if(before.x < current.x) {
                parts[bodyCount-i].GetComponent<SnakePart>().a_right = false;
                parts[bodyCount-i].GetComponent<SnakePart>().a_left = true;
            }
            else {
                parts[bodyCount-i].GetComponent<SnakePart>().a_right = false;
                parts[bodyCount-i].GetComponent<SnakePart>().a_left = false;
            }


            if(after.y > current.y) {
                parts[bodyCount-i].GetComponent<SnakePart>().b_up = true;
                parts[bodyCount-i].GetComponent<SnakePart>().b_down = false;
            }
            else if(after.y < current.y) {
                parts[bodyCount-i].GetComponent<SnakePart>().b_up = false;
                parts[bodyCount-i].GetComponent<SnakePart>().b_down = true;
            }
            else {
                parts[bodyCount-i].GetComponent<SnakePart>().b_up = false;
                parts[bodyCount-i].GetComponent<SnakePart>().b_down = false;
            }
            if(after.x > current.x) {
                parts[bodyCount-i].GetComponent<SnakePart>().b_right = true;
                parts[bodyCount-i].GetComponent<SnakePart>().b_left = false;
            }
            else if(after.x < current.x) {
                parts[bodyCount-i].GetComponent<SnakePart>().b_right = false;
                parts[bodyCount-i].GetComponent<SnakePart>().b_left = true;
            }
            else {
                parts[bodyCount-i].GetComponent<SnakePart>().b_right = false;
                parts[bodyCount-i].GetComponent<SnakePart>().b_left = false;
            }
        } 

        var current1 = parts[0].GetComponent<SnakePart>().pos;
        var after1 = pos;
        var before1 =  parts[1].GetComponent<SnakePart>().pos;

        if(after1.y > current1.y) {
            spr.rotation = Quaternion.Euler(0, 0, 0);
            parts[0].GetComponent<SnakePart>().b_up = true;
            parts[0].GetComponent<SnakePart>().b_down = false;
        }
        else if(after1.y < current1.y) {
            spr.rotation = Quaternion.Euler(0, 0, 180);
            parts[0].GetComponent<SnakePart>().b_up = false;
            parts[0].GetComponent<SnakePart>().b_down = true;
        }
        else {
            parts[0].GetComponent<SnakePart>().b_up = false;
            parts[0].GetComponent<SnakePart>().b_down = false;
        }
        if(after1.x > current1.x) {
            spr.rotation = Quaternion.Euler(0, 0, -90);
            parts[0].GetComponent<SnakePart>().b_right = true;
            parts[0].GetComponent<SnakePart>().b_left = false;
        }
        else if(after1.x < current1.x) {
            spr.rotation = Quaternion.Euler(0, 0, 90);
            parts[0].GetComponent<SnakePart>().b_right = false;
            parts[0].GetComponent<SnakePart>().b_left = true;
        }
        else {
            parts[0].GetComponent<SnakePart>().b_right = false;
            parts[0].GetComponent<SnakePart>().b_left = false;
        }

        if(before1.y > current1.y) {
            parts[0].GetComponent<SnakePart>().a_up = true;
            parts[0].GetComponent<SnakePart>().a_down = false;
        }
        else if(before1.y < current1.y) {
            parts[0].GetComponent<SnakePart>().a_up = false;
            parts[0].GetComponent<SnakePart>().a_down = true;
        }
        else {
            parts[0].GetComponent<SnakePart>().a_up = false;
            parts[0].GetComponent<SnakePart>().a_down = false;
        }
        if(before1.x > current1.x) {
            parts[0].GetComponent<SnakePart>().a_right = true;
            parts[0].GetComponent<SnakePart>().a_left = false;
        }
        else if(before1.x < current1.x) {
            parts[0].GetComponent<SnakePart>().a_right = false;
            parts[0].GetComponent<SnakePart>().a_left = true;
        }
        else {
            parts[0].GetComponent<SnakePart>().a_right = false;
            parts[0].GetComponent<SnakePart>().a_left = false;
        }
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
        if(bodyCount > 2) {
            PhotonNetwork.Destroy(parts[bodyCount-1].GetComponent<PhotonView>());
            parts.RemoveAt(bodyCount-1);
            bodyCount -= 1;
            return true;
        } else {
            return false;
        }
    }
}
