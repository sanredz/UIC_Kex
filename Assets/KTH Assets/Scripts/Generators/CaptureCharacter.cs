using UnityEngine;
using System.Collections;
using System.IO;

public class CaptureCharacter : MonoBehaviour
{
    public Texture2D charTexture;
    public int textureWidth = 256;
    public bool saveTextures = true;
    private int numberOfAngles;
    private int numberOfFrames;
    private Texture2D texture;
    private Texture2D normalMap;
    private Texture2D textureAtlas;
    private Texture2D normalAtlas;
    private Transform cameraPivot;
    private Animator animator;
    private Transform myTransform;
    private Transform midGeoTransform;
    private SkinnedMeshRenderer myRenderer;
    private Camera mainCamera;
    private int textureHeight;
    private int frameWidth;
    private int frameHeight;
    private int atlasWidth;
    private int atlasHeight;
    private int totalFrames;
    private int frame;
    private int indexY;
    private int indexX;
    private Rect captureRect;
    private float currentNormalizedTime;
    private Color normalColor;
    private Color[] blankFrame;
    private Color[] blankNormalFrame;
    private Material textureMaterial;
    private Material normalMaterial;

    void Start()
    {   
        numberOfAngles = Settings.numberOfAngles;
        numberOfFrames = Settings.numberOfFrames;

        myTransform = gameObject.transform;

        midGeoTransform = transform.Find("MidGeo");
		//myRenderer = midGeoTransform.GetComponent<SkinnedMeshRenderer>();
		SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
		myRenderer = smr;


        mainCamera = Camera.main;

        cameraPivot = GameObject.Find("CameraPivot").transform;
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        
        textureHeight = textureWidth / 2;
        frameWidth = textureWidth / 8;
        frameHeight = textureHeight / 2;
        atlasWidth = textureWidth * numberOfAngles / 4;
        atlasHeight = textureHeight * numberOfAngles;	
        int framesInRow = textureWidth / frameWidth;
        int framesInColumn = textureHeight / frameHeight;
        totalFrames = framesInRow * framesInColumn;
        captureRect = new Rect(Screen.width / 2 - Screen.height / 4, 0, Screen.height / 2, Screen.height);

        int numPixels = frameWidth * frameHeight;
        blankFrame = new Color[numPixels];
        blankNormalFrame = new Color[numPixels];
        normalColor = new Color(0.5f, 0.5f, 1f, 1f);
        for (int i = 0; i < numPixels; i++) {
            blankFrame [i] = Color.clear;
            blankNormalFrame [i] = normalColor;
        }
        textureMaterial = new Material(Shader.Find("Diffuse"));
//		textureMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        normalMaterial = new Material(Shader.Find("Custom/DisplayNormals"));
        textureMaterial.mainTexture = charTexture;
		
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        normalMap = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        textureAtlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.ARGB32, false);
        normalAtlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.ARGB32, false);

        mainCamera.backgroundColor = Color.clear;
        myRenderer.material = textureMaterial;
        Debug.Log("Making texture with dimensions " + textureWidth + "x" + textureHeight);
        StartCoroutine(CaptureFrames());
    }
    
    private IEnumerator CaptureFrames()
    {
        float startTime = Time.time;
        for (indexX = 0; indexX < numberOfAngles / 4; indexX++) {
            for (indexY = 0; indexY < numberOfAngles; indexY++) {
                for (frame = 0; frame < numberOfFrames; frame++) {
                    for (int i = 0; i < 2; i++) {
                        yield return new WaitForEndOfFrame();
                        Capture((i == 0) ? texture : normalMap);
                        myRenderer.material = i == 0 ? normalMaterial : textureMaterial;
                    }
                    UpdateAnimation();
                }
                FillTextures();
                AddToAtlas(texture, textureAtlas);
                AddToAtlas(normalMap, normalAtlas);
                AllocateNewTextures();
                RotateCameraY();
                currentNormalizedTime = 0f;
                frame = 0;
            }
            RotateCameraX();
            currentNormalizedTime = 0f;
            indexY = 0;
        }
        Debug.Log("Capture done, applying to atlases.");
        textureAtlas.Apply();
        normalAtlas.Apply();
        string path = Application.dataPath + "/Resources/GeneratedTextures/";
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        SaveToPNG(Path.Combine(path, textureWidth + "textureAtlas.png"), textureAtlas);
        SaveToPNG(Path.Combine(path, textureWidth + "normalAtlas.png"), normalAtlas);
        Debug.Log("Generation finished! Time to generate: " + (Time.time - startTime) + " s.");
    }

    private void AddToAtlas(Texture2D tex, Texture2D atlas)
    {
        atlas.SetPixels(indexX * textureWidth, atlasHeight - (indexY + 1) * textureHeight,
		                textureWidth, textureHeight, tex.GetPixels());
    }
	
    private void AllocateNewTextures()
    {
        texture = null;
        normalMap = null;
        Resources.UnloadUnusedAssets();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        normalMap = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
    }
	
    private void FillTextures()
    {
        while (frame < totalFrames) {
            SetFrameInTexture(texture, blankFrame);
            SetFrameInTexture(normalMap, blankFrame);
            frame++;
        }

        // Set blank pixels to normal map color.
        // We could do this before capturing but we don't want
        // to get seizures.
        for (int x = 0; x < textureWidth; x++) {
            for (int y = 0; y < textureHeight; y++) {
                if (normalMap.GetPixel(x, y).a == 0f) {
                    normalMap.SetPixel(x, y, normalColor);
                }
            }
        }
        texture.Apply(false);
        normalMap.Apply(false);
    }
    
    private void Capture(Texture2D tex)
    {
        Texture2D textureFrame = new Texture2D(Screen.height / 2, Screen.height, TextureFormat.ARGB32, false);
        textureFrame.ReadPixels(captureRect, 0, 0, false);
        TextureScale.Bilinear(textureFrame, frameWidth, frameHeight); 
        SetFrameInTexture(tex, textureFrame.GetPixels());
    }

    private void SetFrameInTexture(Texture2D texture, Color[] pixels)
    {
        int columns = textureWidth / frameWidth;		
        int x = frameWidth * (frame % columns);
        int y = textureHeight - frameHeight * ((frame / columns) + 1);
        texture.SetPixels(x, y, frameWidth, frameHeight, pixels);
    }

    private void UpdateAnimation()
    {
        currentNormalizedTime += (float)(1f / numberOfFrames);
        animator.ForceStateNormalizedTime(currentNormalizedTime);
        midGeoTransform.position = new Vector3(0f, 0f, 0f);
        myTransform.position = new Vector3(0f, 0f, 0f);
    }
    
    private void RotateCameraY()
    {
        cameraPivot.eulerAngles -= Vector3.up * (360f / numberOfAngles);
    }
    
    private void RotateCameraX()
    {
        Vector3 e = cameraPivot.eulerAngles;
        cameraPivot.eulerAngles = new Vector3(e.x, 0, 0);
        cameraPivot.eulerAngles -= Vector3.right * (75 / ((numberOfAngles / 4) - 1));
    }
    
    public void SaveToPNG(string path, Texture2D tex)
    {
#if UNITY_WEBPLAYER
		Debug.Log("No WriteAllBytes on Webplayer.");
		
#else
        File.WriteAllBytes(path, tex.EncodeToPNG());
#endif
    }
}