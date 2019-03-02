using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderUpdater : MonoBehaviour
{
    private SkinnedMeshRenderer render;
    private MeshCollider meshCollider;

    private void Start()
    {
        render = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    void Update()
    {
        for(int i=0; i < render.sharedMesh.blendShapeCount; i++)
        {
            float blendWeight = render.GetBlendShapeWeight(i);
            render.SetBlendShapeWeight(i, blendWeight);
        }
        Mesh bakeMesh = new Mesh();
        render.BakeMesh(bakeMesh);
        meshCollider.sharedMesh = bakeMesh;
    }
}