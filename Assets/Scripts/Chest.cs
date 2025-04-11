using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

public class Chest : MonoBehaviourPunCallbacks, ISnakeCollidable
{
    public Transform Radius;
    public float RadiusVal;
    public int SpawnCount;
    public float Delay;
    public bool Activate;
    private Grid grid;

    public Transform leaves;

    public void collide(Snake snek)
    {
        if(!Activate && photonView.IsMine) {
            transform.DOPunchRotation(new Vector3(0, 0, 180), 0.75f);
            Radius.DOScale(RadiusVal,1f);
            leaves.DOScale(1, 1f);
            StartCoroutine(Spawn());
            Activate = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, RadiusVal);
    }

    private IEnumerator Spawn() {
        for(int i=0; i < SpawnCount; i++) {
            var pos = grid.trans(transform.position + new Vector3(Random.Range(-RadiusVal/2.0f,RadiusVal/2.0f), Random.Range(-RadiusVal/2.0f,RadiusVal/2.0f), 0));
            FoodManager.Instance.Spawn(transform.position, (Vector3)grid.trans(pos));
            transform.DOPunchScale(Vector3.one*0.1f, 0.2f);
            yield return new WaitForSeconds(Delay);
        }
        Radius.DOScale(0, 0.75f).OnComplete(() => {
            transform.DOPunchRotation(new Vector3(0, 0, 180), 0.75f);
            transform.DOScale(0.9f, 0.25f);
        });
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Activate);
        }
        else
        {
            Activate = (bool)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine) {
            Activate = false;
            grid = GameObject.FindObjectOfType<Grid>();
        }
    }
}
