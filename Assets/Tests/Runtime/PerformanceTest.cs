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
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

[Category("Performance")]
public class PerformanceTest {

    Benchmark m_Benchmark;
    
    [UnityTest]
    [Performance]
    [TextureTestCase("colorgrid-8k*", "Performance")]
    public IEnumerator ColorGrid8K(string filePath) {
        yield return GenericFrames(filePath, 10);
    }
    
    IEnumerator GenericFrames(string filePath, int count) {
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
            yield return TestHelper.LoadTextureInternal(m_Benchmark, count, time, allocated, reserved);
        }
        Cleanup();
    }

    IEnumerator PreLoadBuffer(string filePath)
    {
        m_Benchmark = new Benchmark();
        yield return m_Benchmark.LoadData(filePath);
    }

    void Cleanup() {
        m_Benchmark.Dispose();
    }
}
