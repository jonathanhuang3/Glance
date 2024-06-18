using UnityEngine;
using System.Linq;

public class QuickFOVandPPD : MonoBehaviour
{

    public RenderTexture renderTexture;
    private Texture2D tex;
    private void Start()
    {
        // Calculate pixels per degree and FOV of VR camera to write to console
        float aspectRatio = Camera.main.aspect;
        float horizontalFOV = Camera.main.fieldOfView * aspectRatio;
        float pixelsPerDegreeHorizontal = Screen.width / horizontalFOV;
        Debug.Log("Horizontal Pixels per degree: " + pixelsPerDegreeHorizontal + " And FOV: " + horizontalFOV); // 9.15 ppd and 104.2623 FOV
        Debug.Log("Vertical Pixels per degree: " + Screen.height / Camera.main.fieldOfView + " And FOV: " + Camera.main.fieldOfView); // 9.15 ppd and 60 FOV
        Debug.Log("Screen width: " + Screen.width + " Screen height: " + Screen.height + " All Camera Count: " + Camera.allCamerasCount); // 954 x 549 and 1

        tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        // Read pixels from the RenderTexture to the Texture2D
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        // Measure length of red pixels in each column of texture
        int textureWidth = tex.width;
        int textureHeight = tex.height;
        Color[] pixels = tex.GetPixels();

        int[] redPixelCounts = new int[textureWidth];
        int[] whitePixelCounts = new int[textureWidth];
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                Color pixel = pixels[y * textureWidth + x];
                if (pixel.r > 0.5f && pixel.g < 0.5f && pixel.b < 0.5f)
                {
                    redPixelCounts[x]++;
                }
                if (pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f)
                {
                    whitePixelCounts[x]++;
                }
            }
        }


        Debug.Log("Red pixels: " + string.Join(", ", redPixelCounts.Max()));
        Debug.Log("White pixels: " + whitePixelCounts.Where(x => x > 0).Max());
        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Scripts/SavedScreen.png", bytes);
    }
}