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
using UnityEngine;
using UnityEngine.Profiling;
using KtxUnity;
using ImageType = Benchmark.ImageType;

public class BenchmarkUI : MonoBehaviour
{
    [SerializeField]
    string[] filePaths;

    [SerializeField]
    Renderer prefab;

    [SerializeField] float width = 300;
    [SerializeField] float height = 100;
    [SerializeField] float yGap = 10;

    Benchmark m_Benchmark;
    ImageType m_CurrentType = ImageType.None;
    int m_TotalCount;

    float m_Spread = 3;
    float m_Step = -.001f;
    float m_Distance = 10;
    float m_AspectRatio = 1.5f;

    // Render maximum of 50 at a time (the rest is still in memory)
    const int k_MaxItems = 50;
    Queue<Renderer> m_RendererQueue = new Queue<Renderer>(k_MaxItems);

    // Start is called before the first frame update
    void Start() {
        m_AspectRatio = Screen.width/(float)Screen.height;
    }

    void OnGUI() {
        var x = Screen.width-width;
        var y = yGap;
        if(m_Benchmark!=null) {
            if( GUI.Button( new Rect(x,y,width,height),"Change image")) {
                m_Benchmark.OnTextureLoaded -= ApplyTexture;
                m_Benchmark.Dispose();
                m_Benchmark = null;
                m_CurrentType = ImageType.None;
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
        } else
        if (m_CurrentType == ImageType.None) {
            foreach(var filePath in filePaths) {
                if( GUI.Button( new Rect(x,y,width,height),filePath)) {
                    StartCoroutine(LoadData(filePath));
                }
                y += height + yGap;
            }
        }

        GUI.skin.label.fontSize = 100;
        GUI.Label(new Rect(0,Screen.height-height-yGap,width,height),m_TotalCount.ToString());
    }

    IEnumerator LoadData(string filePath) {
        m_Benchmark = new Benchmark();
        yield return m_Benchmark.LoadData(filePath);
        m_Benchmark.OnTextureLoaded += ApplyTexture;
    }

    async void LoadBatch(int count) {
        var batchTime = await m_Benchmark.LoadBatch(count);
        Debug.LogFormat("Batch load time: {0}", batchTime);
    }

    void NeverEndingStory() {
        m_Benchmark.NeverEndingStory();
    }

    void ApplyTexture(TextureResult result) {
        Profiler.BeginSample("ApplyTexture");
        if (result==null) return;
        m_TotalCount++;
        // Debug.LogFormat("Added image {0}",total_count);
        var b = Instantiate(prefab);
        b.transform.position = new Vector3(
            (Random.value-.5f)* m_Spread * m_AspectRatio,
            (Random.value-.5f)* m_Spread,
            m_Distance
            );
        m_Distance+=m_Step;
        var material = b.material;
        material.mainTexture = result.texture;
        var scale = material.mainTextureScale;
        scale.x = result.orientation.IsXFlipped() ? -1 : 1;
        scale.y = result.orientation.IsYFlipped() ? -1 : 1;
        b.material.mainTextureScale = scale;

        m_RendererQueue.Enqueue(b);
        while(m_RendererQueue.Count>k_MaxItems) {
            var r = m_RendererQueue.Dequeue();
            if (r == null) return;
            r.enabled = false;
        }

        Profiler.EndSample();
    }

    void OnDestroy() {
        m_Benchmark?.Dispose();
    }
}
