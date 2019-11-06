// Copyright (c) 2019 Andreas Atteneder, All Rights Reserved.

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
    private string[] filePaths;

    [SerializeField]
    Renderer prefab = null;

    [SerializeField] float width = 300;
    [SerializeField] float height = 100;
    [SerializeField] float yGap = 10;

    NativeArray<byte> data;
    byte[] dataArray;
    ImageType currentType = ImageType.None;
    int total_count = 0;

    float spread = 3;
    float step = -.001f;
    float distance = 10;
    float aspectRatio = 1.5f;

    // Render maximum of 50 at a time (the rest is still in memory)
    const int MAX_ITEMS = 50;
    Queue<Renderer> rendererQueue = new Queue<Renderer>(MAX_ITEMS);

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

            if( GUI.Button( new Rect(x,y,width,height),"+50")) {
                LoadBatch(50);
            }
            y += height + yGap;

            if( GUI.Button( new Rect(x,y,width,height),"+endless")) {
                StartCoroutine(NeverEndingStory());
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
    }

    IEnumerator LoadData(string filePath) {
        if(filePath.EndsWith(".ktx")) {
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

    void LoadBatch(int count) {
        Profiler.BeginSample("LoadBatch");
        for (int i = 0; i < count; i++)
        {
            if(currentType==ImageType.KTX) {
                var bt = new KtxTexture();
                bt.onTextureLoaded += ApplyTexture;
                bt.LoadFromBytes(data,this);
            } else {
                var texture = new Texture2D(2,2);
                texture.LoadImage(data.ToArray(),true);
                ApplyTexture(texture);
            }
        }
        Profiler.EndSample();
    }

    IEnumerator NeverEndingStory() {
        if(currentType==ImageType.KTX) {
            while(true)
            {
                var bt = new KtxTexture();
                bt.onTextureLoaded += ApplyTexture;
                bt.LoadFromBytes(data,this);
                yield return null;
            }
        } else {
            while(true)
            {
                var texture = new Texture2D(2,2);
                texture.LoadImage(data.ToArray(),true);
                ApplyTexture(texture);
                yield return null;
            }
        }
    }

    void ApplyTexture(Texture2D texture) {
        Profiler.BeginSample("ApplyTexture");
        if (texture==null) return;
        total_count++;
        Debug.LogFormat("Added image {0}",total_count);
        var b = Object.Instantiate<Renderer>(prefab);
        b.transform.position = new Vector3(
            (Random.value-.5f)* spread * aspectRatio,
            (Random.value-.5f)* spread,
            distance
            );
        distance+=step;
        b.material.mainTexture = texture;

        rendererQueue.Enqueue(b);
        while(rendererQueue.Count>MAX_ITEMS) {
            var r = rendererQueue.Dequeue();
            r.enabled = false;
        }

        Profiler.EndSample();
    }

    void OnDestroy() {
        if(data.IsCreated) {
            data.Dispose();
        }
    }
}
