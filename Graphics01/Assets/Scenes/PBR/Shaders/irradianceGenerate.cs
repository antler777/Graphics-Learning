using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class irradianceGenerate : MonoBehaviour
{
    public Cubemap cubemap;

    public Texture2D irrdiance;

    private Material currentmaterial;

    public RenderTexture rt0;
    public RenderTexture rt1;
    [Range(4, 1024)] public int Count;
    private void Start()
    {
        if (!currentmaterial)
        {
            currentmaterial = transform.GetComponent<Renderer>().sharedMaterial;
            if (!currentmaterial)
            {
                Debug.LogWarning("Cannot find a material on:" + transform.name);
            }
        }

        if (currentmaterial)
        {
            irrdiance = GenerateParabola();
            currentmaterial.SetTexture("_irrdiance",irrdiance);
        }
    }

    private Texture2D GenerateParabola()
    {
        Texture2D irrdiance = new Texture2D(cubemap.width,cubemap.height);

        for (int x = 0; x < cubemap.width; x++)
        {
            for (int y = 0; y < cubemap.height; y++)
            {
                //Color pixelcolor = sampler2(cubemap);
            }
        }

        return irrdiance;
    }
}
