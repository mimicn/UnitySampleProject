using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComputeShadeTest : MonoBehaviour
{
    public Text console;
    public RawImage destTex;
    public ComputeShader computeShader;
    public Texture2D texture;
    public RenderTexture RT;
    ComputeBuffer intComputeBuffer;
    private bool enableCS = false;
    // Use this for initialization
    void Start () {
        enableCS = SystemInfo.supportsComputeShaders;
        if (!enableCS)
        {
            if(console != null) { console.text = "not support ComputeShaders"; }
        }
        else
        {
            if (console != null) { console.text = "support ComputeShaders\n"; }
            texture = new Texture2D(64, 64);
            RT = new RenderTexture(256, 256, 0);
            RT.enableRandomWrite = true;
            RT.Create();

            computeShader.SetTexture
                (0, "Result", RT);
            computeShader.Dispatch(0, 256/8, 256/8, 1);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
