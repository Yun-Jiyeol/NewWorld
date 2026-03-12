using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaderPutter : MonoBehaviour
{
    public Material CamMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, CamMaterial);
    }
}
