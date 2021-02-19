using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class VATGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private GameObject targetToBake;
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private float y_offset = 0f;
    [SerializeField] private int frameCount;
    [SerializeField] private Space space = Space.Self;
    [SerializeField] private string dirToSave;

    [Header("Debug Settings")] [SerializeField]
    private Texture2D textureToPrint;

    private Mesh _mesh;

    private const string illegalChars = "|";

    /*
     * Generate is use to create a vertex animated texture of a gameobject with an animation clip that has either a
     * child GO with either a Meshfilter or a SkinnedMeshRenderer component
     * 
     */
    [ContextMenu("Generate")]
    public void Generate()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = targetToBake.GetComponentInChildren<SkinnedMeshRenderer>();

        if (!skinnedMeshRenderer)
        {
            MeshFilter meshFilter = targetToBake.GetComponentInChildren<MeshFilter>();

            if (!meshFilter)
            {
                Debug.LogError("No SkinnedMeshRenderer or MeshFilter found!");
                return;
            }

            Debug.Log("Generating Texture from MeshFilter");
            Generate(meshFilter);
            return;
        }

        Debug.Log("Generating Texture from SkinnedMeshRenderer");
        Generate(skinnedMeshRenderer);
    }

    private void Generate(MeshFilter meshFilter)
    {
        // texture with dimensions [vertexcount, frameCount] with color as vertex position
        
        // store the vertex count as width
        int width = meshFilter.sharedMesh.vertexCount;

        // Note the RGBAhalf color format and the lack of linear color (uses sRBG) and no mipmaps
        Texture2D texture = new Texture2D(width, frameCount, TextureFormat.RGBAHalf, false, false);

        _mesh = new Mesh();

        for (int y = 0; y < frameCount; y++)
        {
            // subtract 1 so that final t value = 1.0f
            float t = y / (frameCount - 1.0f);

            // updates the clip to the time for the current frame
            animationClip.SampleAnimation(targetToBake, t * animationClip.length);

            // stores the mesh in its current state (might not be needed need to check)
            _mesh = meshFilter.sharedMesh;
            

            for (var x = 0; x < width; x++)
            {
                Vector3 position = _mesh.vertices[x];

                // position += y_offset * Vector3.up;

                Matrix4x4 TRSMat = Matrix4x4.TRS(
                    meshFilter.transform.localPosition, 
                    meshFilter.transform.localRotation,
                    meshFilter.transform.localScale
                );
                position = TRSMat.MultiplyPoint3x4(position);
                
                if (space == Space.World) position = targetToBake.transform.TransformPoint(position);
                texture.SetPixel(x, y, new Color(position.x, position.y, position.z, 1));
            }
        }

        texture.Apply();
        
        SaveTexture(texture);
    }

    private void Generate(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        int width = skinnedMeshRenderer.sharedMesh.vertexCount;

        Texture2D texture = new Texture2D(width, frameCount, TextureFormat.RGBAHalf, false, false);

        _mesh = new Mesh();

        for (int y = 0; y < frameCount; y++)
        {
            // subtract 1 so that final t value = 1.0f
            float t = y / (frameCount - 1.0f);

            // updates the clip to the needed frame
            animationClip.SampleAnimation(targetToBake, t * animationClip.length);
            

            // Bake the skinnedmeshrenderer in its current state to a mesh for vertex information extraction
            
            skinnedMeshRenderer.BakeMesh(_mesh);

            for (var x = 0; x < width; x++)
            {
                Vector3 position = _mesh.vertices[x];
                
                Matrix4x4 TRSMat = Matrix4x4.TRS(
                    skinnedMeshRenderer.transform.localPosition, 
                    skinnedMeshRenderer.transform.localRotation,
                    skinnedMeshRenderer.transform.localScale);
                position = TRSMat.MultiplyPoint3x4(position);

                if (space == Space.World) position = targetToBake.transform.TransformPoint(position);
                texture.SetPixel(x, y, new Color(position.x, position.y + y_offset, position.z, 1));
            }
        }

        texture.Apply();
        
        SaveTexture(texture);
    }

    /*
     * Save the generated Texture to an exr file.
     * Auto generate the name based on the animation clip and the gameobject name (checking for OS characters)
     */
    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToEXR(Texture2D.EXRFlags.None);

        string filename = targetToBake.name + "-" + animationClip.name + ".exr";
        foreach (char c in filename)
        {
            if (illegalChars.Contains(c.ToString()))
            {
                Debug.Log("Replacing illegal chars");
                filename = filename.Replace(c.ToString(), "");
            }
        }

        string fullName = Application.dataPath + dirToSave + filename;

        if (File.Exists(fullName))
        {
            File.Delete(fullName);
            Debug.Log("Deleting Old Texture");
        }
        Debug.Log("Saving Texture to "+ fullName);
        File.WriteAllBytes(fullName, bytes);
        UnityEditor.AssetDatabase.Refresh();

        DestroyImmediate(texture);
    }

    [ContextMenu("Print Texture")]
    public void PrintTexture()
    {
        PrintTexture(textureToPrint);
    }

    /*
     * Used to debug the position values of the texture
     * NOTE: need to set the texture to print and the frame count 
     */
    private void PrintTexture(Texture2D printTex)
    {
        if (!printTex)
        {
            Debug.LogError("No texture provided");
            return;
        }

        if (frameCount == 0)
        {
            Debug.Log("No framecount as no texture generated. Either generate a texture or set a frame count");
        }
        
        for (int y = 0; y < frameCount; y++)
        {
            float t = y / (frameCount - 1.0f);

            String rowString = "\nFrame " + (y + 1) + " / " + printTex.height + ": \n";

            for (int x = 0; x < printTex.width; x++)
            {
                rowString += "Vertex: " + x + ": position: ( " + printTex.GetPixel(x, y).r + "," +
                             printTex.GetPixel(x, y).g + "," + printTex.GetPixel(x, y).b + " )";
            }
            Debug.Log(rowString);
        }
    }
    
    #endif
}
