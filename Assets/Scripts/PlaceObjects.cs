using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlaceObjects : MonoBehaviour
{

    public LayerMask layer;
    public float rotateSpeed = 60f;

    private void Start()
    {
        PositionObject();
    }


    private void Update()
    {
        PositionObject();

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(gameObject.GetComponent<PlaceObjects>());
            Debug.LogWarning("Построено");
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.up * 90);
        }

    }

    private void PositionObject()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, layer))
        {
            Vector3 a = hit.point;
            int b, c = 0;
            if (a.x - (int)a.x > 0.5)
                b = (int)a.x + 1;
            else
                b = (int)a.x;
            if (a.z - (int)a.z > 0.5)
                c = (int)a.z + 1;
            else
                c = (int)a.z;
            a.x = b;
            a.z = c;
            transform.position = a;
        }
    }
}
