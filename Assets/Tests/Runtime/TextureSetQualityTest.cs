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
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[Category("Quality")]
public class TextureSetQualityTest {

    [UnityTest,TextureQualityTestCase("*.png","dt")]
    public static IEnumerator TestKtxQuality(
        string originalPath,
        string ktxEtc1sPath,
        string ktxUastcPath
        ) 
    {
        Assert.NotNull(ktxEtc1sPath);
        Assert.NotNull(ktxUastcPath);

        var ktx1 = Path.Combine(Application.streamingAssetsPath, ktxEtc1sPath);
        var ktx2 = Path.Combine(Application.streamingAssetsPath, ktxUastcPath);
        
        Assert.IsTrue(File.Exists(ktx1), $"File {ktx1} not found");
        Assert.IsTrue(File.Exists(ktx2), $"File {ktx2} not found");
        
        
        
        // TODO: fix TextureQualityTestCase to work with regular Test (not just UnityTest)
        yield break;
    }
}
