using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderUpdater : MonoBehaviour
{
    public int blendKeyIndex = 0;
    // public float blendAmount = 100;

    private SkinnedMeshRenderer render;
    private MeshCollider meshCollider;

    private void Start()
    {
        render = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        float blendWeight = render.GetBlendShapeWeight(blendKeyIndex);
        render.SetBlendShapeWeight(blendKeyIndex, blendWeight);
        Mesh bakeMesh = new Mesh();
        render.BakeMesh(bakeMesh);
        meshCollider.sharedMesh = bakeMesh;
    }
}