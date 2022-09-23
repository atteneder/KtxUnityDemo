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

using System.Threading.Tasks;
using KtxUnity;
using UnityEngine;
using UnityEngine.Networking;

public abstract class LoadFileAdvancedDemo : MonoBehaviour
{
    [SerializeField]
    Material targetMaterial;

    [SerializeField]
    string fileName;
    
    /// <summary>
    /// Index of the image to import.
    /// </summary>
    public uint imageIndex;
        
    /// <summary>
    /// Face (in case of cubemap) or slice (in case of 3D texture) to import
    /// </summary>
    public uint faceSlice;
        
    /// <summary>
    /// Lowest mipmap level to import (where 0 is the highest resolution).
    /// Lower mipmap levels (higher resolution) are being discarded.
    /// Useful to limit texture resolution.
    /// </summary>
    public uint mipmapLevelLowerLimit;
        
    /// <summary>
    /// If true, a mipmap chain (if present) is imported.
    /// </summary>
    public bool importMipMapChain = true;
        
    /// <summary>
    /// If true, texture will be sampled
    /// in linear color space (sRGB otherwise)
    /// </summary>
    public bool linear;

    async void Start() {
        
        // Create KTX texture instance
        var basisTexture = CreateTextureBase();
        
        var managedData = await LoadFromStreamingAssets(fileName);

        if (managedData == null) return;
        
        using (var data = new ManagedNativeArray(managedData)) {
            
            var result = basisTexture.Load(data.nativeArray);

            if (result != ErrorCode.Success) return;
            
            result = await basisTexture.Transcode(
                linear,
                imageIndex,
                faceSlice,
                mipmapLevelLowerLimit,
                importMipMapChain
                );
            
            if (result != ErrorCode.Success) return;

            var textureResult = await basisTexture.CreateTexture(
                imageIndex,
                mipmapLevelLowerLimit,
                faceSlice,
                importMipMapChain
                );

            if (textureResult.errorCode == ErrorCode.Success) {
                // Use texture. For example, apply texture to a material
                targetMaterial.mainTexture = textureResult.texture;
                
                // Optional: Support arbitrary texture orientation by flipping the texture if necessary
                var scale = targetMaterial.mainTextureScale;
                scale.x = textureResult.orientation.IsXFlipped() ? -1 : 1;
                scale.y = textureResult.orientation.IsYFlipped() ? -1 : 1;
                targetMaterial.mainTextureScale = scale;
            }
            
            basisTexture.Dispose();
        }
        

    }
    
    protected async Task<byte[]> LoadFromStreamingAssets(string filename) {
        var url = TextureBase.GetStreamingAssetsUrl(filename);
        var webRequest = UnityWebRequest.Get(url);
        var asyncOp = webRequest.SendWebRequest();
        while (!asyncOp.isDone) {
            await Task.Yield();
        }

        if (!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogError(webRequest.error);
            return null;
        }

        return webRequest.downloadHandler.data;
    }
    
    protected abstract TextureBase CreateTextureBase();
}
