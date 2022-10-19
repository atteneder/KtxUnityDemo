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
using UnityEngine;
using UnityEngine.TestTools;

public class LoadTextureTest {

    Benchmark m_Benchmark;
    
    [UnityTest]
    [TextureTestCase("*.ktx2")]
    public IEnumerator Ktx(string filePath, bool mipmap) {
        yield return GenericFrames(filePath, mipmap: mipmap);
    }
    
    [UnityTest]
    [TextureTestCase("*.jpg")]
    public IEnumerator Jpg(string filePath, bool mipmap) {
        yield return GenericFrames(filePath, mipmap: mipmap);
    }
    
    [UnityTest]
    [TextureTestCase("*.png")]
    public IEnumerator Png(string filePath, bool mipmap) {
        yield return GenericFrames(filePath, true, mipmap);
    }
    
    IEnumerator GenericFrames(string filePath, bool alpha = false, bool mipmap = false) {
        yield return PreLoadBuffer(filePath);
        yield return TestHelper.LoadTextureInternal(m_Benchmark,1, alpha, mipmap);
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
