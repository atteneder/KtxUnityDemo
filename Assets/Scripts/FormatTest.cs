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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using KtxUnity;

public class FormatTest : MonoBehaviour
{
    [SerializeField] string[] ktxFiles = null;

    [SerializeField] TestKtxFileLoader prefab = null;
    [SerializeField] TestBasisUniversalFileLoader basisuPrefab = null;

    [SerializeField] float buttonWidth = 500;
    [SerializeField] float buttonHeight = 70;
    [SerializeField] float yGap = 5;

    List<GraphicsFormat> graphicsFormats;

    string file;
    GameObject currentGo;
    Vector2 scrollPos;

    void OnGUI() {
        float barWidth = Screen.width * 0.025f;
        GUI.skin.verticalScrollbar.fixedWidth = barWidth;
        GUI.skin.verticalScrollbarThumb.fixedWidth = barWidth;

        if(file==null) {
            BeginScrollView(ktxFiles.Length,barWidth);
            TextureGUI(barWidth);
        } else {
            BeginScrollView(graphicsFormats.Count,barWidth);
            FormatGUI(barWidth);
        }
        GUI.EndScrollView();
    }

    void Start() {
        // TranscodeFormatHelper.Init();
#if KTX_VERBOSE
        // TranscodeFormatHelper.GetSupportedTextureFormats ( out graphicsFormats, out textureFormats );
#endif

        graphicsFormats = new List<GraphicsFormat>();
        graphicsFormats.Add(GraphicsFormat.RGBA_DXT1_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGBA_DXT1_UNorm);
        graphicsFormats.Add(GraphicsFormat.RGBA_DXT5_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGBA_DXT5_UNorm);
        graphicsFormats.Add(GraphicsFormat.RGBA_BC7_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGBA_BC7_UNorm);
        graphicsFormats.Add(GraphicsFormat.RGB_ETC2_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGB_ETC_UNorm);
        graphicsFormats.Add(GraphicsFormat.RGBA_ETC2_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGBA_ETC2_UNorm);
        graphicsFormats.Add(GraphicsFormat.R_EAC_UNorm); // Also supports SNorm
        graphicsFormats.Add(GraphicsFormat.RG_EAC_UNorm); // Also supports SNorm
        graphicsFormats.Add(GraphicsFormat.RGB_PVRTC_4Bpp_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGB_PVRTC_4Bpp_UNorm);
        graphicsFormats.Add(GraphicsFormat.RGBA_ASTC4X4_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGBA_ASTC4X4_UNorm);
        graphicsFormats.Add(GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB);
        graphicsFormats.Add(GraphicsFormat.RGBA_PVRTC_4Bpp_UNorm);
        graphicsFormats.Add(GraphicsFormat.R8G8B8_SRGB); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R8G8B8_UNorm); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R8G8B8_UInt); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R8G8B8A8_SRGB); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R8G8B8A8_UNorm); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R8G8B8A8_SNorm); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R8G8B8A8_UInt); // Also supports SNorm, UInt, SInt
        graphicsFormats.Add(GraphicsFormat.R4G4B4A4_UNormPack16);
        graphicsFormats.Add(GraphicsFormat.R5G6B5_UNormPack16);
        graphicsFormats.Add(GraphicsFormat.B5G6R5_UNormPack16);
    }

    void BeginScrollView(int count, float barWidth) {
        scrollPos = GUI.BeginScrollView(
            new Rect(Screen.width-buttonWidth,0,buttonWidth,Screen.height),
            scrollPos,
            new Rect(0,0,buttonWidth-barWidth, (buttonHeight+yGap) * count)
        );
    }

    void TextureGUI(float barWidth) {
        float y = yGap;
        foreach(var f in ktxFiles) {
            if( GUI.Button( new Rect(0,y,buttonWidth-barWidth,buttonHeight),f)) {
                file = f;
                scrollPos = Vector2.zero;
            }
            y += buttonHeight + yGap;
        }
    }

    void FormatGUI(float barWidth) {
        float y = yGap;
        if( GUI.Button( new Rect(0,y,buttonWidth-barWidth,buttonHeight),"choose texture")) {
            scrollPos = Vector2.zero;
            file = null;
        }
        y += buttonHeight + yGap;

        GUI.color = new Color(.5f,.5f,1);

        float aWidth = (buttonWidth-barWidth)/2;

        foreach(var f in graphicsFormats) {
            var label = $"{f}";
            if( GUI.Button( new Rect(0,y,aWidth,buttonHeight),label)) {
                LoadTextureBasis(file+".basis", f);
            }
            if( GUI.Button( new Rect(aWidth,y,aWidth,buttonHeight),label)) {
                LoadTextureKtx(file+".ktx2", f);
            }
            y += buttonHeight + yGap;
        }
    }

    void LoadTextureBasis(string file, GraphicsFormat gf ) {
        if (currentGo!=null) {
            Destroy(currentGo);
        }
        var txt = new BasisUniversalTexture();

        var testLoader = Instantiate(basisuPrefab);
        testLoader.overrideTexture = txt;
        testLoader.transcodeFormat = gf;
        testLoader.filePath = file;
        currentGo = testLoader.gameObject;
    }

    void LoadTextureKtx(string file, GraphicsFormat gf ) {
        if (currentGo!=null) {
            Destroy(currentGo);
        }
        var txt = new KtxTexture();

        var testLoader = Instantiate(prefab);
        testLoader.overrideTexture = txt;
        testLoader.transcodeFormat = gf;
        testLoader.filePath = file;
        currentGo = testLoader.gameObject;
    }
}
