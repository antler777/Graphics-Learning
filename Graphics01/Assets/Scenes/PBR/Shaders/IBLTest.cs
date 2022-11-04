using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class IBLTest : MonoBehaviour
{
    public Cubemap cubemap;
    public bool isDebug = false;
    public RenderTexture rt0;
    public RenderTexture rt1;
    [Range(4, 1024)]
    public int Count;

    public string path = "IBL/Precompute";
    
    private Material material;
    private void Create()
    {
        if (material == null)
        {
            material = new Material(Shader.Find("Hidden/IBLMaker_CubeMap"));
        }
        rt0 = new RenderTexture(cubemap.width * 2, cubemap.width, 0, RenderTextureFormat.ARGBFloat);
        rt1 = new RenderTexture(cubemap.width * 2, cubemap.width, 0, RenderTextureFormat.ARGBFloat);
        rt0.wrapMode = TextureWrapMode.Repeat;
        rt1.wrapMode = TextureWrapMode.Repeat;
        rt0.Create();
        rt1.Create();
        Graphics.Blit(cubemap, rt0, material, 0);
        material.SetTexture("_CubeTex", cubemap);
        for (int i = 0; i < Count; i++)
        {
            EditorUtility.DisplayProgressBar("", "", 1f / Count);
            Vector3 n = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.0000001f, 1f),
                    Random.Range(-1f, 1f)
                );
            while (n.magnitude > 1)
                n = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.0000001f, 1f),
                        Random.Range(-1f, 1f)
                    );
            n = n.normalized;
            material.SetVector("_RandomVector", new Vector4(
                n.x, n.y, n.z,
                1f / (i + 2)
                ));
            Graphics.Blit(rt0, rt1, material, 1);
            // 翻转
            var t = rt0;
            rt0 = rt1;
            rt1 = t;
        }
        Graphics.Blit(cubemap, rt1, material, 0);
        EditorUtility.ClearProgressBar();
        // 保存
        Texture2D texture = new Texture2D(cubemap.width * 2, cubemap.width, TextureFormat.ARGB32, true);
        var k = RenderTexture.active;
        RenderTexture.active = rt0;
        texture.ReadPixels(new Rect(0, 0, rt0.width, rt0.height), 0, 0);
        RenderTexture.active = k;
        byte[] bytes = texture.EncodeToPNG();
        System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(Application.dataPath, path) + "/" + texture.name + "_irradiance.png", System.IO.FileMode.Create);
        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
        bw.Write(bytes);
        fs.Close();
        bw.Close();
    }
}
