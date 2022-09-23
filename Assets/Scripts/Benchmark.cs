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

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Networking;
using Unity.Collections;
using KtxUnity;

public class Benchmark : MonoBehaviour
{
    enum ImageType {
        None,
        KTX,
        PNG,
        JPG
    }

    [SerializeField]
    private string[] filePaths = null;

    [SerializeField]
    Renderer prefab = null;

    [SerializeField] float width = 300;
    [SerializeField] float height = 100;
    [SerializeField] float yGap = 10;

    NativeArray<byte> data;
    byte[] dataArray;
    ImageType currentType = ImageType.None;
    int total_count = 0;

    float start_time;
    float batch_time = -1;
    int batch_count = -1;

    float spread = 3;
    float step = -.001f;
    float distance = 10;
    float aspectRatio = 1.5f;

    // Render maximum of 50 at a time (the rest is still in memory)
    const int MAX_ITEMS = 50;
    Queue<Renderer> rendererQueue = new Queue<Renderer>(MAX_ITEMS);

    CancellationTokenSource cancellationTokenSource;

    // Start is called before the first frame update
    void Start() {
        aspectRatio = Screen.width/(float)Screen.height;
    }

    void OnGUI() {
        float x = Screen.width-width;
        float y = yGap;
        if(data.IsCreated) {
            if( GUI.Button( new Rect(x,y,width,height),"Change image")) {
                data.Dispose();
                currentType = ImageType.None;
            }
            y += height + yGap;

            if( GUI.Button( new Rect(x,y,width,height),"+1")) {
                LoadBatch(1);
            }
            y += height + yGap;

            if( GUI.Button( new Rect(x,y,width,height),"+10")) {
                LoadBatch(10);
            }
            y += height + yGap;

            if( GUI.Button( new Rect(x,y,width,height),"+500")) {
                LoadBatch(500);
            }
            y += height + yGap;

            if( GUI.Button( new Rect(x,y,width,height),"+endless")) {
                NeverEndingStory();
            }
            y += height + yGap;
        } else
        if (currentType == ImageType.None) {
            foreach(var filePath in filePaths) {
                if( GUI.Button( new Rect(x,y,width,height),filePath)) {
                    StartCoroutine(LoadData(filePath));
                }
                y += height + yGap;
            }
        }

        GUI.skin.label.fontSize = 100;
        GUI.Label(new Rect(0,Screen.height-height-yGap,width,height),total_count.ToString());

        if (batch_time>=0) {
            GUI.Label(new Rect(Screen.width-width,Screen.height-height-yGap,width,height),batch_time.ToString("0.000"));
        }
    }

    IEnumerator LoadData(string filePath) {
        if(filePath.EndsWith(".ktx2")) {
            currentType = ImageType.KTX;
        } else if(filePath.EndsWith(".png")) {
            currentType = ImageType.PNG;
        } else if(filePath.EndsWith(".jpg")) {
            currentType = ImageType.JPG;
        } else {
            Debug.LogError("Unknown image type");
            yield break;
        }
        if(data.IsCreated) {
            data.Dispose();
        }
        var url = TextureBase.GetStreamingAssetsUrl(filePath);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        if(!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogErrorFormat("Error loading {0}: {1}",url,webRequest.error);
            yield break;
        }
        data = new NativeArray<byte>(webRequest.downloadHandler.data,Allocator.Persistent);
        dataArray = data.ToArray();
    }

    async void LoadBatch(int count) {
        Profiler.BeginSample("LoadBatch");
        start_time = Time.realtimeSinceStartup;
        batch_count = count;
        batch_time = -1;
        if (currentType == ImageType.KTX) {
            var tasks = new Task[count];
            for (var i = 0; i < count; i++) {
                var bt = new KtxTexture();
                tasks[i] = LoadAndApply(bt);
            }
            Profiler.EndSample();
            await Task.WhenAll(tasks);
        }
        else {
            for (var i = 0; i < count; i++) {
                var texture = new Texture2D(2, 2);
                texture.LoadImage(data.ToArray(), true);
                ApplyTexture(new TextureResult(texture, TextureOrientation.UNITY_DEFAULT));
            }
            Profiler.EndSample();
        }
        
        batch_time = Time.realtimeSinceStartup-start_time;
        Debug.LogFormat("Batch load time: {0}", batch_time);
        batch_count = -1;
    }

    async Task LoadAndApply(TextureBase ktx) {
        var result = await ktx.LoadFromBytes(data);
        ApplyTexture(result);
    }

    void NeverEndingStory() {
        cancellationTokenSource = new CancellationTokenSource();
        NeverEndingStoryLoop();
    }

    async void NeverEndingStoryLoop() {
        start_time = Time.realtimeSinceStartup;
        if(currentType==ImageType.KTX) {
            while(!cancellationTokenSource.IsCancellationRequested)
            {
                var bt = new KtxTexture();
                var result = await bt.LoadFromBytes(data);
                if (cancellationTokenSource.IsCancellationRequested) break;
                ApplyTexture(result);
                batch_time = Time.realtimeSinceStartup-start_time;
                await Task.Yield();
            }
        } else {
            while(!cancellationTokenSource.IsCancellationRequested)
            {
                var texture = new Texture2D(2,2);
                texture.LoadImage(data.ToArray(),true);
                if (cancellationTokenSource.IsCancellationRequested) break;
                ApplyTexture(new TextureResult(texture,TextureOrientation.UNITY_DEFAULT));
                batch_time = Time.realtimeSinceStartup-start_time;
                await Task.Yield();
            }
        }
    }

    void ApplyTexture(TextureResult result) {
        Profiler.BeginSample("ApplyTexture");
        if (result==null) return;
        total_count++;
        // Debug.LogFormat("Added image {0}",total_count);
        var b = Object.Instantiate<Renderer>(prefab);
        b.transform.position = new Vector3(
            (Random.value-.5f)* spread * aspectRatio,
            (Random.value-.5f)* spread,
            distance
            );
        distance+=step;
        var material = b.material;
        material.mainTexture = result.texture;
        var scale = material.mainTextureScale;
        scale.x = result.orientation.IsXFlipped() ? -1 : 1;
        scale.y = result.orientation.IsYFlipped() ? -1 : 1;
        b.material.mainTextureScale = scale;

        rendererQueue.Enqueue(b);
        while(rendererQueue.Count>MAX_ITEMS) {
            var r = rendererQueue.Dequeue();
            if (r == null) return;
            r.enabled = false;
        }

        Profiler.EndSample();
    }

    void OnDestroy() {
        cancellationTokenSource?.Cancel();
        if(data.IsCreated) {
            data.Dispose();
        }
    }
}
