using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class Spawner : MonoBehaviourPunCallbacks
{
    public float RadiusVal;
    public int Count;
    private Grid grid;
    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.FindObjectOfType<Grid>();
        GameObject.FindObjectOfType<ServerPlayer>().OnReadyUp.AddListener(() => {
            for(int i=0; i < Count; i++) {
                Debug.Log("hhh");
                var pos = Random.insideUnitCircle*RadiusVal;
                var tmp = PhotonNetwork.Instantiate("Food", pos, Quaternion.identity);
                DOTween.To(()=> tmp.transform.position, x=> tmp.transform.position = x, (Vector3)grid.trans(pos), 1f);
            }
        });
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, RadiusVal);
    }
}
