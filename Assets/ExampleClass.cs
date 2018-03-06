using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    private ParticleSystem ps;
    public float startFrame = 0.0f;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetimeMultiplier = 2.0f;

        var tex = ps.textureSheetAnimation;
        tex.enabled = true;
        tex.numTilesX = 4;
        tex.numTilesY = 2;
    }

    void Update()
    {
        var tex = ps.textureSheetAnimation;
        tex.startFrame = startFrame;
    }

    void OnGUI()
    {
        startFrame = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), startFrame, 0.0f, 7.0f);
    }
}