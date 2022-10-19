// Copyright (c) 2019-2022 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using KtxUnity;
using UnityEngine;

public abstract class LoadMultipleTexturesDemo : LoadTextureBase
{
    [SerializeField]
    Material templateMaterial;

    [SerializeField]
    string fileName;
    
    [SerializeField]
    Renderer[] targets;
        
    /// <summary>
    /// Lowest mipmap level to import (where 0 is the highest resolution).
    /// Lower mipmap levels (higher resolution) are being discarded.
    /// Useful to limit texture resolution.
    /// </summary>
    [SerializeField]
    uint mipmapLevelLowerLimit;
        
    /// <summary>
    /// If true, a mipmap chain (if present) is imported.
    /// </summary>
    [SerializeField]
    bool importMipMapChain = true;
        
    /// <summary>
    /// If true, texture will be sampled
    /// in linear color space (sRGB otherwise)
    /// </summary>
    [SerializeField]
    bool linear;

    async void Start() {
        
        // Create KTX texture instance
        var texture = CreateTextureBase();
        
        var managedData = await LoadFromStreamingAssets(fileName);

        if (managedData == null) return;
        
        using (var data = new ManagedNativeArray(managedData)) {
            
            var result = texture.Open(data.nativeArray);

            if (result != ErrorCode.Success) return;

            for (var layer = 0u; layer < targets.Length; layer++) {
                var textureResult = await texture.LoadTexture2D(
                    linear,
                    layer,
                    0,
                    mipmapLevelLowerLimit,
                    importMipMapChain
                    );
                
                if (textureResult.errorCode != ErrorCode.Success) continue;

                var material = Instantiate(templateMaterial);
                // Use texture. For example, apply texture to a material
                material.mainTexture = textureResult.texture;
                
                // Optional: Support arbitrary texture orientation by flipping the texture if necessary
                var scale = material.mainTextureScale;
                scale.x = textureResult.orientation.IsXFlipped() ? -1 : 1;
                scale.y = textureResult.orientation.IsYFlipped() ? -1 : 1;
                material.mainTextureScale = scale;

                targets[layer].material = material;
            }
            
            texture.Dispose();
        }
    }
}
