using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class FoodManager : MonoBehaviour, IOnEventCallback
{
    public GameObject FoodPref;

    static FoodManager mInstance;

    private const byte spawnOp = 1;
    private const byte removeOp = 2;

    private Dictionary<String, GameObject> allFood = new Dictionary<string, GameObject>();

    public static FoodManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = GameObject.FindFirstObjectByType<FoodManager>();
            }
            return mInstance;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void Spawn(Vector3 position, Vector3 endPos) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(spawnOp, new object[] {position, endPos}, raiseEventOptions, SendOptions.SendReliable);
    }

    public void Remove(String id) {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; 
        PhotonNetwork.RaiseEvent(removeOp, id, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == spawnOp)
        {
            object[] data = (object[])photonEvent.CustomData;

            var newG = Guid.NewGuid();
            var tmp = Instantiate(FoodPref, (Vector3)data[0], Quaternion.identity);
            tmp.GetComponent<Food>().guid = newG.ToString();
            tmp.GetComponent<Food>().pos = (Vector3)data[1];
            allFood.Add(newG.ToString(), tmp);
        } 
        else if(eventCode == removeOp) {
            String data = (String)photonEvent.CustomData;
            if(allFood.ContainsKey(data)) {
                Destroy(allFood[data]);
                allFood.Remove(data);
            }
        }
    }
}
