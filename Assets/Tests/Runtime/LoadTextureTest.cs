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
using KtxUnity;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

[Category("Performance")]
public class LoadTextureTest {
    
    const int k_ImagesPerBatch = 500;
    
    [UnityTest]
    [Performance]
    public IEnumerator Ktx() {
        yield return PreLoadBuffer("trout.ktx2");
        // Warmup
        yield return LoadTextureInternal(3);
        using (Measure.Scope())
        {
            yield return LoadTextureInternal(k_ImagesPerBatch);
        }
        Cleanup();
    }

    [UnityTest]
    [Performance]
    public IEnumerator KtxFrames() {
        yield return PreLoadBuffer("trout.ktx2");
        using (Measure.Frames()
                   .WarmupCount(1)
                   .MeasurementCount(10)
                   .Scope()
              )
        {
            yield return LoadTextureInternal(k_ImagesPerBatch);
        }
        Cleanup();
    }
    
    [UnityTest]
    [Performance]
    public IEnumerator Jpg() {
        yield return PreLoadBuffer("trout.jpg");
        // Warmup
        yield return LoadTextureInternal(3);
        using (Measure.Scope())
        {
            yield return LoadTextureInternal(k_ImagesPerBatch);
        }
        Cleanup();
    }

    [UnityTest]
    [Performance]
    public IEnumerator JpgFrames() {
        yield return PreLoadBuffer("trout.jpg");
        using (Measure.Frames()
                   .WarmupCount(1)
                   .MeasurementCount(10)
                   .Scope()
              )
        {
            yield return LoadTextureInternal(k_ImagesPerBatch);
        }
        Cleanup();
    }

    IEnumerator LoadTextureInternal(int count) {
        var actualCount = 0;
        
        m_Benchmark.OnTextureLoaded += result => {
            Assert.AreEqual( ErrorCode.Success,result.errorCode);
            Assert.IsNotNull(result.texture);
            actualCount++;
        };
        
        var task = m_Benchmark.LoadBatch(count);

        while (!task.IsCompleted) {
            yield return null;
        }
        
        Assert.AreEqual(count, actualCount);
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
