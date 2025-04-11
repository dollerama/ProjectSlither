using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class Chaser : MonoBehaviourPun, ISnakeCollidable
{
    public float RadiusVal;
    public float jitter;
    public float speed;
    public float spawnRate;
    public int life;
    private Grid grid;
    private bool Activate;
    private bool Alive;
    private Vector2 vel;
    private Vector2 pos;
    private Vector2 prevPos;
    private float timer;
    private float timer2;
    private float frames;

    public Transform spr;
    public Transform backing;
    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.FindObjectOfType<Grid>();
        GetComponent<CircleCollider2D>().radius = RadiusVal;
        pos = transform.position;
        Alive = true;

        var r = Random.Range(0, 4);
        if(r == 0) {
            vel = Vector2.up;        
        } else if(r == 1) {
            vel = Vector2.down;
        } else if(r == 2) {
            vel = Vector2.left;
        } else if(r == 3) {
            vel = Vector2.right;
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, RadiusVal);
    }

    // Update is called once per frame
    void Update()
    {
        if(Activate && photonView.IsMine) {
            backing.Rotate(Vector3.forward*Time.deltaTime*10);
            spr.localPosition = new Vector3(0, Mathf.Abs(0.3f*Mathf.Sin(Time.time)),0);

            pos += vel * speed * Time.deltaTime;
            frames += Time.deltaTime;

            if(Vector2.Distance( grid.trans(pos), grid.trans(prevPos)) >= grid.size.x) {
                transform.DOMove(grid.trans(pos), frames);
                frames = 0;

                if(Random.Range(jitter, 1.0f) > 1.0-jitter) {
                    while(true) {
                        var r = Random.Range(0, 4);
                    
                        if(r == 0) {
                            if(vel != Vector2.down) {
                                vel = Vector2.up; 
                                break;   
                            }  
                        } else if(r == 1) {
                            if(vel != Vector2.up) {
                                vel = Vector2.down;
                                break;   
                            }  
                        } else if(r == 2) {
                            if(vel != Vector2.right) {
                                vel = Vector2.left;
                                break;   
                            }  
                        } else if(r == 3) {
                            if(vel != Vector2.left) {
                                vel = Vector2.right;
                                break;   
                            }  
                        }
                    }
                }

                prevPos = pos;
                timer+=1;

                if(timer >= spawnRate) {
                    timer = 0;
                    life -= 1;
                    
                    FoodManager.Instance.Spawn(pos, (Vector3)grid.trans(pos));
                }
            }

            if(life < 0 && Alive) {
                StartCoroutine(Kill());
            }
        }
    }

    IEnumerator Kill() {
        Alive = false;
        transform.DOScale(0, 1);
        yield return new WaitForSeconds(1f);
        PhotonNetwork.Destroy(photonView);
    }

    public void collide(Snake snek)
    {
        if(!Activate) {
            Activate = true;
            GetComponent<CircleCollider2D>().radius = 0;
        }
    }
}
