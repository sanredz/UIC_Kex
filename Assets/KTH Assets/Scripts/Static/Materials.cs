using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Materials
{
    private static int numberOfAngles = Settings.numberOfAngles;
    private static int numberOfColors = Settings.numberOfColors;
    private static int numberOfFrames = Settings.numberOfFrames;
    private const int numberOfQualities = 2;
    public const int MediumQuality = 1;
    public const int LowQuality = 0;
    public static Color[] colors;
    private static Material[] meshMaterials;
    private static Material[,] impostorMaterials;
    private static Vector2[,,][] uvs;
	public static Texture texture;
	


	static Materials(){
		// Set up colors
		colors = new Color[numberOfColors];

		for (int i = 0; i < numberOfColors; i++) {
			colors [i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
		}

//		colors = new Color[] {Color.green, Color.cyan, Color.blue, Color.magenta, Color.red, Color.yellow, new Color(146,0,255,1), new Color(255,195,0,1), new Color(167,255,0,1)};
		
		// Set up materials
		impostorMaterials = new Material[numberOfColors, numberOfQualities];
		meshMaterials = new Material[numberOfColors];       
		for (int color = 0; color < Settings.numberOfColors; color++) {
			impostorMaterials [color, LowQuality] = MakeMaterial(color, LowQuality);
			impostorMaterials [color, MediumQuality] = MakeMaterial(color, MediumQuality);
			meshMaterials [color] = MakeMaterial(color);
		}
		
		// Set up UVs
		uvs = new Vector2[numberOfAngles / 4, numberOfAngles, numberOfFrames][];
		for (int x = 0; x < numberOfAngles / 4; x++) {
			for (int y = 0; y < numberOfAngles; y++) {
				for (int frame = 0; frame < numberOfFrames; frame++) {
					uvs [x, y, frame] = CalculateUV(x, y, frame);
				}
			}
		}
	}

    public static Material GetMaterial(int color)
    {
        return meshMaterials [color];
    }

    public static Material GetMaterial(int color, int quality)
    {
        return impostorMaterials [color, quality];
    }

    public static Vector2[] GetUV(int indexX, int indexY, int i)
    {
        return uvs [indexX, indexY, i];
    }

    private static Material MakeMaterial(int color)
    {
        Material mat = new Material(Shader.Find("ShirtColor/Diffuse"));
        mat.SetColor("_ShirtColor", colors [color]);
		texture = (Texture) Resources.Load("CMan0010-M4-Body-D");
		if(texture){
			mat.SetTexture("_MainTex", texture);
		}
        else{
			Debug.Log("Materials: No texture found, going with default");
			mat.SetTexture("_MainTex", (Texture)Resources.Load("ff"));
		}
        return mat;
    }

	public static Material MakeMaterial(Color c)
	{
		Material mat = new Material(Shader.Find("ShirtColor/Diffuse"));
		mat.SetColor("_ShirtColor", c);
		texture = (Texture) Resources.Load("CMan0010-M4-Body-D");
		if(texture){
			mat.SetTexture("_MainTex", texture);
		}
		else{
			Debug.Log("Materials: No texture found, going with default");
			mat.SetTexture("_MainTex", (Texture)Resources.Load("ff"));
		}
		return mat;
	}

    private static Material MakeMaterial(int color,int quality)
    {
        Material mat = new Material(Shader.Find("ShirtColor/Transparent"));
        mat.SetColor("_ShirtColor", colors [color]);
        string q = (quality == MediumQuality) ? "1024" : "256";
        mat.SetTexture("_MainTex", (Texture)Resources.Load("GeneratedTextures/" + q + "textureAtlas"));
        mat.SetTexture("_BumpMap", (Texture)Resources.Load("GeneratedTextures/" + q + "normalAtlas"));
        return mat;
    }

    private static Vector2[] CalculateUV(int indexX, int indexY, int i)
    {
        //TODO: Should probably not be hard coded.
        float frameWidth = 1f / 32f;
        float frameHeight = 1f / 32f;
        float angleWidth = 1f / 4f;
        float angleHeight = 1f / 16f;
        int numFramesHorizontal = 8;

        float x = indexX * angleWidth;
        float y = 1 - (indexY * angleHeight);

        float dx = frameWidth * (i % numFramesHorizontal);
        float dy = frameHeight * (i / numFramesHorizontal);
        Vector2 topLeft = new Vector2(x + dx, y - dy);
        Vector2 topRight = new Vector2(x + dx + frameWidth, y - dy);
        Vector2 bottomLeft = new Vector2(x + dx, y - dy - frameHeight);
        Vector2 bottomRight = new Vector2(x + dx + frameWidth, y - dy - frameHeight);

        return new Vector2[] { 
            bottomRight,
            topLeft,
            bottomLeft,
            topRight};
    }
}