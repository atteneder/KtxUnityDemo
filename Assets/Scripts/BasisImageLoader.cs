﻿// Copyright (c) 2019-2022 Andreas Atteneder, All Rights Reserved.

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

class BasisImageLoader : TextureFileLoader<BasisUniversalTexture>
{

    [SerializeField]
    float scale = 1.0f;

    protected override void ApplyTexture(TextureResult result)
    {
        Vector2 pos = new Vector2(0,0);
        Vector2 size = new Vector2(result.texture.width, result.texture.height);

        if(result.orientation.IsXFlipped()) {
            pos.x = size.x;
            size.x *= -1;
        }

        if(result.orientation.IsYFlipped()) {
            pos.y = size.y;
            size.y *= -1;
        }

        GetComponent<Image>().sprite = Sprite.Create(result.texture, new Rect(pos, size), Vector2.zero);

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(result.texture.width*scale, result.texture.height*scale);
    }
}
