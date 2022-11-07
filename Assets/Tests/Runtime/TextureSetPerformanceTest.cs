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
using System.IO;
using System.Threading.Tasks;
using KtxUnity;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

[Category("Performance")]
public class TextureSetPerformanceTest {
    const int k_MeasurementCount = 10;
    [UnityTest,Performance]
    public IEnumerator PNGImageConversion() {
        var buffers = LoadBuffers("dt","*.png");
        var time = new SampleGroup("TextureLoadTime");
        
        for (var i = 0; i < k_MeasurementCount; i++) {
            using (Measure.Frames().Scope()) {
                var startTime = Time.realtimeSinceStartup;
                LoadTexturesImageConversion(buffers, false, false);
                Measure.Custom(time,(Time.realtimeSinceStartup-startTime)*1000);
                // Wait one frame so Scope captures it.
                yield return null;
            }
        }
    }

    [UnityTest, Performance]
    public IEnumerator KtxEtc1s() {
        yield return Ktx("dt", "*-etc1s.ktx2");
    }
    
    [UnityTest, Performance]
    public IEnumerator KtxZuastc() {
        yield return Ktx("dt", "*-zuastc.ktx2");
    }
    
    [UnityTest, Performance]
    public IEnumerator KtxEtc1sMipmap() {
        yield return Ktx("dt", "*-etc1s-mipmap.ktx2");
    }
    
    [UnityTest, Performance]
    public IEnumerator KtxZuastcMipmap() {
        yield return Ktx("dt", "*-zuastc-mipmap.ktx2");
    }

    IEnumerator Ktx(string path, string searchPattern) {
        var buffers = LoadBuffers(path,searchPattern);
        var time = new SampleGroup("TextureLoadTime");
        for (var i = 0; i < k_MeasurementCount; i++) {
            using (Measure.Frames().Scope()) {
                var startTime = Time.realtimeSinceStartup;
                var task = LoadTexturesKtx(buffers);
                while (!task.IsCompleted) {
                    yield return null;
                }
                Measure.Custom(time, (Time.realtimeSinceStartup - startTime) * 1000);
            }
        }
    }

    static byte[][] LoadBuffers(string path, string searchPattern) {
        Profiler.BeginSample("LoadBuffers");
        var dir = Path.Combine(Application.streamingAssetsPath, path);
        var files = Directory.GetFiles(dir, searchPattern);
        Assert.AreEqual(259, files.Length);

        // Load images into buffer
        var buffers = new byte[files.Length][];
        for (var i = 0; i < files.Length; i++) {
            var file = files[i];
            buffers[i] = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, file));
        }
        Profiler.EndSample();
        return buffers;
    }

    static async Task LoadTexturesKtx(IReadOnlyList<byte[]> buffers) {
        var tasks = new Task<Texture2D>[buffers.Count];
        for (var i = 0; i < buffers.Count; i++) {
            tasks[i] = LoadTextureKtx(buffers[i]);
        }
        await Task.WhenAll(tasks);
    }

    static async Task<Texture2D> LoadTextureKtx(byte[] buffer) {
        var ktx = new KtxTexture();
        using var buf = new ManagedNativeArray(buffer);
        var result = await ktx.LoadFromBytes(buf.nativeArray);
        var texture = result.texture;
        Assert.NotNull(texture);
        Assert.Greater(texture.width,64);
        Assert.Greater(texture.height,64);
        return texture;
    }
    
    static void LoadTexturesImageConversion(IEnumerable<byte[]> buffers, bool alpha, bool mipmaps) {
        foreach (var t in buffers) {
            LoadTextureImageConversion(t, alpha, mipmaps);
        }
    }

    static void LoadTextureImageConversion(byte[] data, bool alpha, bool mipmaps) {
        Profiler.BeginSample("LoadTextureImageConversion");
        var texture = new Texture2D(
            2, 2,
            alpha ? TextureFormat.RGBA32 : TextureFormat.RGB24,
            mipmaps
        );
        texture.LoadImage(data, true);
        
        Assert.Greater(texture.width,64);
        Assert.Greater(texture.height,64);
        Profiler.EndSample();
    }
}
