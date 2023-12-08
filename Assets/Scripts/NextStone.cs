using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStone : MonoBehaviour
{
    private Stone stone;
    public Vector2 direction;

    private void Awake()
    {
        stone = GetComponentInParent<Stone>();
    }

    private void OnMouseDown()
    {
        stone.x += (int)direction.x;
        stone.y += (int)direction.y;
        Vector3 position = stone.GetComponent<Transform>().position;
        stone.GetComponent<Transform>().position = gameObject.transform.position;
    }
}
