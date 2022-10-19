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

using System;
using System.Collections;
using KtxUnity;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;
using Object = UnityEngine.Object;

[Category("Performance")]
public class LoadTextureTest {
    
#if UNITY_EDITOR
    const int k_ImagesPerBatch = 50;
#else
    const int k_ImagesPerBatch = 500;
#endif
    
    [UnityTest]
    [Performance]
    public IEnumerator Ktx() {
        yield return Generic("trout.ktx2");
    }

    [UnityTest]
    [Performance]
    public IEnumerator Jpg() {
        yield return Generic("trout.jpg");
    }

    [UnityTest]
    [Performance]
    public IEnumerator KtxFrames() {
        yield return GenericFrames("trout.ktx2");
    }
    
    [UnityTest]
    [Performance]
    public IEnumerator JpgFrames() {
        yield return GenericFrames("trout.jpg");
    }

    IEnumerator Generic(string filePath) {
        yield return PreLoadBuffer(filePath);
        var time = new SampleGroup("TextureLoadTime");
        var allocated = new SampleGroup("TotalAllocatedMemory", SampleUnit.Megabyte);
        var reserved = new SampleGroup("TotalReservedMemory", SampleUnit.Megabyte);
        // Warmup
        yield return LoadTextureInternal(3);
        using (Measure.Scope())
        {
            yield return LoadTextureInternal(k_ImagesPerBatch,time,allocated,reserved);
        }
        Cleanup();
    }
    
    IEnumerator GenericFrames(string filePath) {
        yield return PreLoadBuffer(filePath);
        var time = new SampleGroup("TextureLoadTime");
        var allocated = new SampleGroup("TotalAllocatedMemory", SampleUnit.Megabyte);
        var reserved = new SampleGroup("TotalReservedMemory", SampleUnit.Megabyte);
        using (Measure.Frames()
                   .ProfilerMarkers("LoadBatch", "CreateTexture")
                   .DontRecordFrametime()
                   .WarmupCount(1)
                   .MeasurementCount(10)
                   .Scope()
              )
        {
            yield return LoadTextureInternal(k_ImagesPerBatch,time, allocated,reserved);
        }
        Cleanup();
    }
    
    IEnumerator LoadTextureInternal(int count, SampleGroup time = null, SampleGroup allocated = null, SampleGroup reserved = null) {
        var actualCount = 0;
        var resources = new Texture2D[count];
        
        void OnTextureLoaded(TextureResult result) {
            Assert.AreEqual( ErrorCode.Success,result.errorCode);
            Assert.IsNotNull(result.texture);
            resources[actualCount] = result.texture;
            actualCount++;
        };

        m_Benchmark.OnTextureLoaded += OnTextureLoaded;
        
        var task = m_Benchmark.LoadBatch(count);

        while (!task.IsCompleted) {
            yield return null;
        }

        if (time != null) {
            Measure.Custom(time,task.Result*1000);
        }
        
        m_Benchmark.OnTextureLoaded -= OnTextureLoaded;
        
        Assert.AreEqual(count, actualCount);

        if (allocated != null) {
            Measure.Custom(allocated, Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
        }

        if (reserved != null) {
            Measure.Custom(reserved, Profiler.GetTotalReservedMemoryLong() / 1048576f);
        }
        
        foreach (var resource in resources) {
            Object.Destroy(resource);
        }
        GC.Collect();
    }
    
    ManagedNativeArray m_BufferWrapped;
    Benchmark m_Benchmark;
    
    IEnumerator PreLoadBuffer(string filePath)
    {
        m_Benchmark = new Benchmark();
        yield return m_Benchmark.LoadData(filePath);
    }

    void Cleanup() {
        m_Benchmark.Dispose();
    }
}
