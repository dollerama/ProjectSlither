using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2 size;

    public Vector2 trans(Vector2 p) {
        return new Vector2(Mathf.Round(p.x / size.x) * size.x, Mathf.Round(p.y / size.y) * size.y); 
    }
}
