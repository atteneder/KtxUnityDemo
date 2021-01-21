// Copyright (c) 2019-2021 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using KtxUnity;
using UnityEngine.UI;

public class TestBasisUniversalFileLoader : TextureFileLoader<BasisUniversalTestTexture>
{
    public BasisUniversalTestTexture overrideTexture;

    protected override async void Start() {
        var result = await LoadFromStreamingAssets(overrideTexture);
        ApplyTexture(result);
    }

    protected override void ApplyTexture(TextureResult result) {
        var rendererComponent = GetComponent<Renderer>();
        if(rendererComponent!=null && rendererComponent.sharedMaterial!=null) {
            var material = rendererComponent.material;
            material.mainTexture = result.texture;
            // Optional: Support arbitrary texture orientation by flipping the texture if necessary
            var scale = material.mainTextureScale;
            scale.x = result.orientation.IsXFlipped() ? -1 : 1;
            scale.y = result.orientation.IsYFlipped() ? -1 : 1;
            rendererComponent.material.mainTextureScale = scale;
        }
    }
}
