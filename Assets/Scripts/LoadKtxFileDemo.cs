﻿// Copyright (c) 2019-2021 Andreas Atteneder, All Rights Reserved.

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
using Unity.Collections;
using UnityEngine;

public class LoadKtxFileDemo : MonoBehaviour
{
    public Material targetMaterial;

    async void Start() {
        
        // Create KTX texture instance
        var texture = new KtxTexture();
        
        // Linear color sampling. Needed for non-color value textures (e.g. normal maps) 
        bool linearColor = true;
        
        // Load file from Streaming Assets folder (relative path)
        var result = await texture.LoadFromStreamingAssets("trout.ktx",linearColor);
        
        // Alternative: Load from URL
        // var result = await texture.LoadFromUrl("https://myserver.com/trout.ktx", linearColor);
        
        // Alternative: Load from memory
        // var result = await texture.LoadFromBytes(nativeArray, linearColor);

        if (result != null) {
            // Use texture. For example, apply texture to a material
            targetMaterial.mainTexture = result.texture;
            
            // Optional: Support arbitrary texture orientation by flipping the texture if necessary
            var scale = targetMaterial.mainTextureScale;
            scale.x = result.orientation.IsXFlipped() ? -1 : 1;
            scale.y = result.orientation.IsYFlipped() ? -1 : 1;
            targetMaterial.mainTextureScale = scale;
        }
    }
}
