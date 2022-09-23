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

using KtxUnity;
using UnityEngine;
using UnityEngine.UI;

public class LoadBasisFileSpriteDemo : MonoBehaviour
{
    async void Start() {
        
        // Create a basis universal texture instance
        var texture = new BasisUniversalTexture();
        
        // Load file from Streaming Assets folder
        var result = await texture.LoadFromStreamingAssets("dachstein.basis");

        if (result != null) {
            // Calculate correct size
            var pos = new Vector2(0,0);
            var size = new Vector2(result.texture.width, result.texture.height);

            // Flip Sprite, if required
            if(result.orientation.IsXFlipped()) {
                pos.x = size.x;
                size.x *= -1;
            }

            if(result.orientation.IsYFlipped()) {
                pos.y = size.y;
                size.y *= -1;
            }

            // Create a Sprite and assign it to the Image
            GetComponent<Image>().sprite = Sprite.Create(result.texture, new Rect(pos, size), Vector2.zero);
            
            // Preserve aspect ratio:
            // Flipping the sprite by making the size x or y negative (above) breaks Image's `Preserve Aspect` feature
            // You can/have to calculate the RectTransform size yourself. Example:
            
            // Calculate correct size and assign it to the RectTransform
            const float scale = 0.5f; // Set this to whatever size you need it - best make it a serialized class field
            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(result.texture.width*scale, result.texture.height*scale);
        }
    }
}
