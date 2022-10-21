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

#if UNITY_EDITOR
    const int k_CountBase = 1;
#else
    const int k_CountBase = 3;
#endif

    const int k_Factor1K = 64; // 8k texture has 64 times the area of 1k texture
    const int k_Factor64 = 16384; // 8k texture has 64 times the area of 64x64 texture
    
    Benchmark m_Benchmark;
    
    [UnityTest,Performance,TextureTestCase("colorgrid-8k*", "Performance")]
    public IEnumerator ColorGrid8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, false, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("colorgrid-1k*", "Performance")]
    public IEnumerator ColorGrid1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, false, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("colorgrid-64*", "Performance")]
    public IEnumerator ColorGrid64(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor64*k_CountBase, false, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("singlecolor-1k*", "Performance")]
    public IEnumerator SingleColor1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, false, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("noise-rgb-1k*", "Performance")]
    public IEnumerator NoiseRgb1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, false, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("noise-rgba-1k*", "Performance")]
    public IEnumerator NoiseRgba1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("noise-rgb-8k*", "Performance")]
    public IEnumerator NoiseRgb8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, false, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("noise-rgba-8k*", "Performance")]
    public IEnumerator NoiseRgba8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("normal-noise*8k*", "Performance")]
    public IEnumerator NormalNoise8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("normal-noise*1k*", "Performance")]
    public IEnumerator NormalNoise1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("normal-smooth*8k*", "Performance")]
    public IEnumerator NormalSmooth8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("normal-smooth*1k*", "Performance")]
    public IEnumerator NormalSmooth1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath,k_Factor1K*k_CountBase, true, mipmap, imageSharp);
    }

    [UnityTest,Performance,TextureTestCase("vector-rgb-8k*", "Performance")]
    public IEnumerator VectorRgb8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("vector-rgb-1k*", "Performance")]
    public IEnumerator VectorRgb1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("vector-rgba-8k*", "Performance")]
    public IEnumerator VectorRgba8K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_CountBase, true, mipmap, imageSharp);
    }
    
    [UnityTest,Performance,TextureTestCase("vector-rgba-1k*", "Performance")]
    public IEnumerator VectorRgba1K(string filePath, bool mipmap, bool imageSharp = true) {
        yield return GenericFrames(filePath, k_Factor1K*k_CountBase, true, mipmap, imageSharp);
    }
    
    IEnumerator GenericFrames(
        string filePath,
        int count,
        bool alpha = false,
        bool mipmap = false,
        bool imageSharp = true
        ) {
        yield return PreLoadBuffer(filePath);
        var time = new SampleGroup("TextureLoadTime");
#if UNITY_EDITOR
        // Textures are only readable in the Editor
        var textureSize = new SampleGroup("TextureSize", SampleUnit.Byte);
        yield return TestHelper.GetTextureSize(m_Benchmark, textureSize, alpha, mipmap);
#endif
        using (Measure.Frames()
                   .ProfilerMarkers("LoadBatch", "CreateTexture")
                   .DontRecordFrametime()
                   .WarmupCount(1)
                   .MeasurementCount(10)
                   .Scope(time)
              )
        {
            yield return TestHelper.LoadTextureInternal(m_Benchmark, count, alpha, mipmap, imageSharp, time);
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
