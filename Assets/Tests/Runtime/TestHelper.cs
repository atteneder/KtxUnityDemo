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
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;
using Assert = UnityEngine.Assertions.Assert;
using Object = UnityEngine.Object;

static class TestHelper {
    
    internal static IEnumerator LoadTextureInternal(
        Benchmark benchmark,
        int count,
        bool alpha = false,
        bool mipmap = false,
        SampleGroup time = null,
        SampleGroup allocated = null,
        SampleGroup reserved = null
        )
    {
        var actualCount = 0;
        var resources = new Texture2D[count];
        var formatNotSupportedBySystem = false;
        ErrorCode firstError = ErrorCode.Success;
        
        void OnTextureLoaded(TextureResult result) {
            if (result.errorCode == ErrorCode.FormatUnsupportedBySystem) {
                formatNotSupportedBySystem = true;
            }
            if (result.errorCode != ErrorCode.Success) {
                firstError = result.errorCode;
            }
            resources[actualCount] = result.texture;
            actualCount++;
        };

        benchmark.OnTextureLoaded += OnTextureLoaded;
        
        var task = benchmark.LoadBatch(count,alpha,mipmap);

        while (!task.IsCompleted) {
            yield return null;
        }

        if (time != null) {
            Measure.Custom(time,task.Result*1000);
        }
        
        benchmark.OnTextureLoaded -= OnTextureLoaded;

        if (formatNotSupportedBySystem) {
            NUnit.Framework.Assert.Ignore("Format is not supported by system");
        }
        
        Assert.AreEqual(count, actualCount);
        Assert.AreEqual( ErrorCode.Success,firstError);

        if (allocated != null) {
            Measure.Custom(allocated, Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
        }

        if (reserved != null) {
            Measure.Custom(reserved, Profiler.GetTotalReservedMemoryLong() / 1048576f);
        }
        
        foreach (var resource in resources) {
            Assert.IsNotNull(resource);
            Object.Destroy(resource);
        }
    }
    
    internal static IEnumerator GetTextureSize(
        Benchmark benchmark,
        SampleGroup sizeGroup,
        bool alpha = false,
        bool mipmap = false
        )
    {
        void OnTextureLoaded(TextureResult result) {
            Assert.AreEqual( ErrorCode.Success,result.errorCode);
            Assert.IsNotNull(result.texture);
            if (result.texture.isReadable) {
                var bytes = result.texture.GetRawTextureData();
                Measure.Custom(sizeGroup,bytes.Length);
            }
        }

        benchmark.OnTextureLoaded += OnTextureLoaded;
        
        var task = benchmark.LoadBatch(1,alpha,mipmap);

        while (!task.IsCompleted) {
            yield return null;
        }

        benchmark.OnTextureLoaded -= OnTextureLoaded;
    }
}
