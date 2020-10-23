// Copyright (c) 2019 Andreas Atteneder, All Rights Reserved.

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

public class BasisImageLoader : TextureFileLoader<BasisUniversalTexture>
{

    protected override void ApplyTexture(Texture2D texture, TextureOrientation orientation)
    {
        Vector2 pos = new Vector2(0,0);
        Vector2 size = new Vector2(texture.width, texture.height);

        if(orientation.IsXFlipped()) {
            pos.x = size.x;
            size.x *= -1;
        }

        if(orientation.IsYFlipped()) {
            pos.y = size.y;
            size.y *= -1;
        }

        GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(pos, size), Vector2.zero);
    }
}
