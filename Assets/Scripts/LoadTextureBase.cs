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

public abstract class LoadTextureBase : MonoBehaviour
{
    protected static async Task<byte[]> LoadFromStreamingAssets(string filename) {
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
