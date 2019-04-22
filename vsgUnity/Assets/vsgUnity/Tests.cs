using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using vsgUnity.Native;

public class Tests : MonoBehaviour
{
    public GameObject meshGameObject;

    // Start is called before the first frame update
    void Start()
    {
        NativeLog.InstallDebugLogCallback();

        Vector2[] points = new Vector2[] { Vector2.zero, Vector2.left, Vector2.right };
        float[] xvalues = NativeTests.GetXValues(points);

        if(xvalues.Length != points.Length)
        {
            Debug.LogError("Returned xvalues array does not match length of points array!");
            return;
        }

        for(int i = 0; i < points.Length; i++)
        {
            Debug.Log("source x = " + points[i].x + " returned x = " + xvalues[i] + " match = " + (points[i].x == xvalues[i] ? "true" : "false"));
        }

        MeshFilter filter = meshGameObject.GetComponent<MeshFilter>();
        Debug.Log("Got mesh with " + filter.mesh.vertexCount + " verticies");
        NativeTests.ConvertMesh(filter.mesh);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
