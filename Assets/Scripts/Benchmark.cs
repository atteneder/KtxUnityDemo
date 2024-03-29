﻿// Copyright (c) 2019-2022 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Networking;
using KtxUnity;

public class Benchmark : IDisposable
{
    public enum ImageType {
        None,
        Ktx,
        PNG,
        JPG
    }

    public delegate void TextureResultDelegate(TextureResult result);
    public event TextureResultDelegate OnTextureLoaded;

    byte[] m_DataArray;
    ManagedNativeArray m_Data;
    public ImageType currentType = ImageType.None;

    CancellationTokenSource m_CancellationTokenSource;

    public IEnumerator LoadData(string filePath) {
        if(filePath.EndsWith(".ktx2")) {
            currentType = ImageType.Ktx;
        } else if(filePath.EndsWith(".png")) {
            currentType = ImageType.PNG;
        } else if(filePath.EndsWith(".jpg")) {
            currentType = ImageType.JPG;
        } else {
            Debug.LogError("Unknown image type");
            yield break;
        }
        m_Data?.Dispose();
        m_DataArray = null;
        var url = TextureBase.GetStreamingAssetsUrl(filePath);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        if(!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogErrorFormat("Error loading {0}: {1}",url,webRequest.error);
            yield break;
        }

        m_DataArray = webRequest.downloadHandler.data;
        m_Data = new ManagedNativeArray(m_DataArray);
    }

    public async Task<float> LoadBatch(int count, bool alpha = false, bool mipmaps = false, bool imageSharp = false) {
        var startTime = Time.realtimeSinceStartup;
        if (currentType == ImageType.Ktx) {
            var tasks = new Task[count];
            for (var i = 0; i < count; i++) {
                var bt = new KtxTexture();
                tasks[i] = LoadAndApply(bt);
            }
            await Task.WhenAll(tasks);
        }
        else {
            for (var i = 0; i < count; i++) {
                var texture = new Texture2D(
                    2, 2,
                    alpha ? TextureFormat.RGBA32 : TextureFormat.RGB24,
                    mipmaps
                    );
                texture.LoadImage(m_DataArray, true);
                OnTextureLoaded?.Invoke(new TextureResult(texture, TextureOrientation.UnityDefault));
            }
        }
        
        return Time.realtimeSinceStartup-startTime;
    }

    async Task LoadAndApply(TextureBase ktx) {
        var result = await ktx.LoadFromBytes(m_Data.nativeArray);
        OnTextureLoaded?.Invoke(result);
    }

    public void NeverEndingStory() {
        m_CancellationTokenSource = new CancellationTokenSource();
        NeverEndingStoryLoop();
    }

    async void NeverEndingStoryLoop() {
        if(currentType==ImageType.Ktx) {
            while(!m_CancellationTokenSource.IsCancellationRequested)
            {
                var bt = new KtxTexture();
                var result = await bt.LoadFromBytes(m_Data.nativeArray);
                if (m_CancellationTokenSource.IsCancellationRequested) break;
                OnTextureLoaded?.Invoke(result);
                await Task.Yield();
            }
        } else {
            while(!m_CancellationTokenSource.IsCancellationRequested)
            {
                var texture = new Texture2D(2,2, TextureFormat.RGB24, false);
                texture.LoadImage(m_DataArray,true);
                if (m_CancellationTokenSource.IsCancellationRequested) break;
                OnTextureLoaded?.Invoke(new TextureResult(texture,TextureOrientation.UnityDefault));
                await Task.Yield();
            }
        }
    }

    public void Dispose () {
        m_CancellationTokenSource?.Cancel();
        m_Data?.Dispose();
    }
}
