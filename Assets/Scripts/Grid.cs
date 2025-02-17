using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2 size;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 trans(Vector2 p) {
        return new Vector2(Mathf.Round(p.x / size.x) * size.x, Mathf.Round(p.y / size.y) * size.y); 
    }
}
