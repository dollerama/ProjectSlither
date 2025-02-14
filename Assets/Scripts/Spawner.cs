using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private float timer = 0;
    public string spawnName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 5) {
            timer = 0;
            PhotonNetwork.Instantiate(spawnName, new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0f), Quaternion.identity);
        }
    }
}
