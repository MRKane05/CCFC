using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class TerrainHeightmapImporter : MonoBehaviour {
    public Terrain terrain;
    public string fileName = "path_to_raw_file.png"; // Path to your RAW file
    public string splatFileName = "splatmap filename.png";
    public int heightmapWidth = 1024; // Width of the raw heightmap
    public int heightmapHeight = 1024; // Height of the raw heightmap
    public int cutSize = 257; // Size of the section to cut
    public int splatCutSize = 2048;
    public Vector2Int startIndex = new Vector2Int(0, 0); // Starting point for the cut
    public Texture2D heightmapTexture;
    public Texture2D cutHeightmap;

    public Texture2D splatmapTexture;
    public Texture2D cutSplatMap;
    public Texture2D tempCutSplatMap;

    public bool bDoHeightmapLoad = false;

    public bool bGrabRandomCut = false;
    public bool bApplySplats = false;

    void Start()
    {
        LoadRandomHeightmapCut();
    }

    void Update()
    {
        if (bDoHeightmapLoad)
        {
            bDoHeightmapLoad = false;
            LoadHeightmap();
        }

        if (bGrabRandomCut)
        {
            bGrabRandomCut = false;
            LoadRandomHeightmapCut();
        }

        if (bApplySplats)
        {
            bApplySplats = false;
            ApplySplatmap();
        }
    }


    void LoadRandomHeightmapCut()
    {
        startIndex = new Vector2Int(Random.Range(0, heightmapWidth - cutSize-2), Random.Range(0, heightmapWidth - cutSize-2));
        LoadHeightmap();    //Do the data load
    }

    void LoadHeightmap()
    {
        PNGImageLoader(fileName);
        PNGSplatImageLoader(splatFileName);
        float[,] cutHeightmap = CutHeightmapFromPNG(startIndex.x, startIndex.y, cutSize);
        ApplyHeightmapToTerrain(cutHeightmap);

        CutSpatMap(startIndex.x, startIndex.y, splatCutSize);
        StartCoroutine(DoTextureApply());
    }
    IEnumerator DoTextureApply()
    {
        yield return null;  //Wait for the resizing to finish
        ApplySplatmap();
    }

    void PNGImageLoader(string fileName)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        //Converts desired path into byte array
        byte[] pngBytes = System.IO.File.ReadAllBytes(path);

        heightmapTexture = new Texture2D(2, 2);
        heightmapTexture.LoadImage(pngBytes);

    }

    void PNGSplatImageLoader(string fileName)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        //Converts desired path into byte array
        byte[] pngBytes = System.IO.File.ReadAllBytes(path);

        splatmapTexture = new Texture2D(2, 2);
        splatmapTexture.LoadImage(pngBytes);

    }

    float[,] LoadHeightmapFromRaw(string fileName, int width, int height)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        byte[] data = File.ReadAllBytes(path);
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Debug.Log("File Data Length: " + fileData.Length);
        float[,] heightmap = new float[height, width];

        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ushort value = (ushort)(data[index++] | data[index++] << 8); //I think the data stride is 3
                heightmap[y, x] = value / 65535f; // Normalize to 0-1 range
            }
        }

        return heightmap;
    }

    public static Texture2D ResizeTexture(Texture2D sourceTexture, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(sourceTexture, rt);

        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    public void CutSpatMap(int startX, int startY, int size)
    {
        tempCutSplatMap = new Texture2D(size, size, TextureFormat.RGBA32, false);
        if (splatmapTexture == null)
        {
            Debug.LogError("Splatmap texture is null!");
            //return null;
        }
        else
        {
            int width = splatmapTexture.width;
            int height = splatmapTexture.height;

            //cutSplatMap = new Texture2D(cutSize, cutSize);

            for (int y = 0; y < cutSize; y++)
            {
                for (int x = 0; x < cutSize; x++)
                {
                    Color pixel = splatmapTexture.GetPixel(startX + x, startY + y);
                    tempCutSplatMap.SetPixel(y, x, pixel);
                }
            }
            tempCutSplatMap.Apply();
        }


        // Get the required terrain splatmap resolution
        int terrainWidth = terrain.terrainData.alphamapWidth;
        int terrainHeight = terrain.terrainData.alphamapHeight;
        Debug.Log("AlphaMapWidth: " + terrainWidth + " Height: " + terrainHeight);
        // Create a resized texture
        cutSplatMap = ResizeTexture(tempCutSplatMap, terrainWidth, terrainHeight);
    }

    public float[,] CutHeightmapFromPNG(int startX, int startY, int size)
    {
        if (heightmapTexture == null)
        {
            Debug.LogError("Heightmap texture is null!");
            return null;
        }

        int width = heightmapTexture.width;
        int height = heightmapTexture.height;

        if (startX + cutSize > width || startY + cutSize > height)
        {
            Debug.LogError("Cut dimensions exceed texture bounds!");
            return null;
        }

        float minVal = Mathf.Infinity;
        float maxVal = 0f;

        float[,] heightmap = new float[cutSize, cutSize];
        cutHeightmap = new Texture2D(cutSize, cutSize);
        
        for (int y = 0; y < cutSize; y++)
        {
            for (int x = 0; x < cutSize; x++)
            {
                Color pixel = heightmapTexture.GetPixel(startX + x, startY + y);
                heightmap[y, x] = pixel.r; // Assuming grayscale PNG where red channel holds height
                minVal = Mathf.Min(pixel.r, minVal);
                maxVal = Mathf.Max(maxVal, pixel.r);

                cutHeightmap.SetPixel(y, x, Color.Lerp(Color.black, Color.white, pixel.r));
            }
        }
        cutHeightmap.Apply();
        Debug.Log("Max: " + maxVal + " Min: " + minVal);
        return heightmap;
    }

    public float[,] CutSplatFromPNG(int startX, int startY, int size)
    {
        if (heightmapTexture == null)
        {
            Debug.LogError("Heightmap texture is null!");
            return null;
        }

        int width = heightmapTexture.width;
        int height = heightmapTexture.height;

        if (startX + cutSize > width || startY + cutSize > height)
        {
            Debug.LogError("Cut dimensions exceed texture bounds!");
            return null;
        }

        float minVal = Mathf.Infinity;
        float maxVal = 0f;

        float[,] heightmap = new float[cutSize, cutSize];
        cutHeightmap = new Texture2D(cutSize, cutSize);

        for (int y = 0; y < cutSize; y++)
        {
            for (int x = 0; x < cutSize; x++)
            {
                Color pixel = heightmapTexture.GetPixel(startX + x, startY + y);
                heightmap[y, x] = pixel.r; // Assuming grayscale PNG where red channel holds height
                minVal = Mathf.Min(pixel.r, minVal);
                maxVal = Mathf.Max(maxVal, pixel.r);

                cutHeightmap.SetPixel(y, x, Color.Lerp(Color.black, Color.white, pixel.r));
            }
        }
        cutHeightmap.Apply();
        Debug.Log("Max: " + maxVal + " Min: " + minVal);
        return heightmap;
    }

    void ApplyHeightmapToTerrain(float[,] heightmap)
    {
        terrain.terrainData.heightmapResolution = cutSize;
        terrain.terrainData.SetHeights(0, 0, heightmap);
    }

    public void ApplySplatmap()
    {
        if (terrain == null)
        {
            Debug.LogError("Target terrain is null!");
            return;
        }

        int width = cutSplatMap.width;
        int height = cutSplatMap.height;

        if (terrain.terrainData.alphamapWidth != width || terrain.terrainData.alphamapHeight != height)
        {
            Debug.LogError("Splatmap size does not match terrain size!");
            return;
        }
        
        float[,,] splatmapData = new float[height, width, 4];
        float min = Mathf.Infinity;
        float max = 0;

        //This is giving the same values for every channel
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = cutSplatMap.GetPixel(x, y);

                splatmapData[y, x, 0] = (float)pixel.r;
                splatmapData[y, x, 1] = (float)pixel.g;
                splatmapData[y, x, 2] = (float)pixel.b; //PROBLEM: Water will need to be specially handled so that it has a smooth stepped edge on it against everything else
                splatmapData[y, x, 3] = 0f;
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("Applied Texture Details");
    }
}