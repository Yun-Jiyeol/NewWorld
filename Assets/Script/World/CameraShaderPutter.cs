using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaderPutter : MonoBehaviour
{
    public Material CamMaterial;
    public bool isUsingEffect = true;

    private void Start()
    {
        ResetMaterial();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (isUsingEffect)
        {
            Graphics.Blit(source, destination, CamMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void ResetMaterial()
    {
        CamMaterial.SetFloat(((SystemName)0).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)2).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)4).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)5).ToString(), 0.5f);
        CamMaterial.SetFloat(((SystemName)6).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)7).ToString(), 4f);
        CamMaterial.SetFloat(((SystemName)8).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)9).ToString(), 254f);
        CamMaterial.SetFloat(((SystemName)10).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)11).ToString(), 1f);
        CamMaterial.SetFloat(((SystemName)12).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)13).ToString(), 1f);
        CamMaterial.SetFloat(((SystemName)14).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)15).ToString(), 0.1f);
        CamMaterial.SetFloat(((SystemName)16).ToString(), 0f);
        CamMaterial.SetFloat(((SystemName)17).ToString(), 0.2f);
    }
}
