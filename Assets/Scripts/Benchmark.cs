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
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Networking;
using Unity.Collections;
using BasisUniversalUnity;

public class Benchmark : MonoBehaviour
{
    [SerializeField]
    private string filePath;

    [SerializeField]
    private int count = 10;

    [SerializeField]
    Renderer prefab;

    NativeArray<byte> data;

    // Start is called before the first frame update
    IEnumerator Start() {
        var url = BasisUniversalTexture.GetStreamingAssetsUrl(filePath);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        if(!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogErrorFormat("Error loading {0}: {1}",url,webRequest.error);
            yield break;
        }
        data = new NativeArray<byte>(webRequest.downloadHandler.data,Allocator.Persistent);
        LoadBatch();
    }

    // Update is called once per frame
    void Update() {
        if(data!=null &&
        (
            Input.GetKeyDown(KeyCode.Space)
            || (Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began)
        )) {
            LoadBatch();
        }
    }

    void LoadBatch() {
        Profiler.BeginSample("LoadBatch");
        for (int i = 0; i < count; i++)
        {
            var bt = new BasisUniversalTexture();
            bt.onTextureLoaded += ApplyTexture;
            bt.LoadFromBytes(data,this);
        }
        Profiler.EndSample();
    }

    void ApplyTexture(Texture2D texture) {
        Profiler.BeginSample("ApplyTexture");
        if (texture==null) return;
        var b = Object.Instantiate<Renderer>(prefab);
        b.transform.position = new Vector3( Random.value, Random.value, Random.value ) * 3 - Vector3.one * 1.5f;
        b.material.mainTexture = texture;
        Profiler.EndSample();
    }

    void OnDestroy() {
        if(data!=null) {
            data.Dispose();
        }
    }
}
