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
                var pos = Random.insideUnitCircle*RadiusVal;
                FoodManager.Instance.Spawn(pos, (Vector3)grid.trans(pos));
            }
        });
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, RadiusVal);
    }
}
