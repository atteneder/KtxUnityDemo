using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasisUniversalUnity;

public class CustomBasisUrlLoader : BasisUrlLoader
{
   protected override void ApplyTexture(Texture2D texture) {
        var renderer = GetComponent<Renderer>();
        if(renderer!=null && renderer.sharedMaterial!=null) {
            renderer.material.mainTexture = texture;
        }
    }
}
