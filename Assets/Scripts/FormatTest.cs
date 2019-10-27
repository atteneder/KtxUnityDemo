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

    List<TranscodeFormatTuple> graphicsFormats;
    List<KeyValuePair<TextureFormat,TranscodeFormat>> textureFormats;

    string file;
    GameObject currentGo;
    Vector2 scrollPos;

    void Start() {
        TranscodeFormatHelper.Init();
        TranscodeFormatHelper.GetSupportedTextureFormats ( out graphicsFormats, out textureFormats );
    }

    void OnGUI() {
        float barWidth = Screen.width * 0.025f;
        GUI.skin.verticalScrollbar.fixedWidth = barWidth;
        GUI.skin.verticalScrollbarThumb.fixedWidth = barWidth;

        if(file==null) {
            BeginScrollView(ktxFiles.Length,barWidth);
            TextureGUI(barWidth);
        } else {
            BeginScrollView(graphicsFormats.Count+textureFormats.Count,barWidth);
            FormatGUI(barWidth);
        }
        GUI.EndScrollView();
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
            var label = string.Format("{0}/{1}",f.format,f.transcodeFormat);
            if( GUI.Button( new Rect(0,y,aWidth,buttonHeight),label)) {
                LoadTextureBasis(file+".basis",f.transcodeFormat,f.format);
            }
            if( GUI.Button( new Rect(aWidth,y,aWidth,buttonHeight),label)) {
                LoadTextureKtx(file+".ktx",f.transcodeFormat,f.format);
            }
            y += buttonHeight + yGap;
        }
    }

    void LoadTextureBasis(string file, TranscodeFormat transF, GraphicsFormat gf ) {
        if (currentGo!=null) {
            Destroy(currentGo);
        }
        var txt = new BasisUniversalTestTexture();
        txt.graphicsFormat = gf;
        txt.transF = transF;

        var testLoader = Object.Instantiate<TestBasisUniversalFileLoader>(basisuPrefab);
        testLoader.overrideTexture = txt;
        testLoader.filePath = file;
        currentGo = testLoader.gameObject;
    }

    void LoadTextureKtx(string file, TranscodeFormat transF, GraphicsFormat gf ) {
        if (currentGo!=null) {
            Destroy(currentGo);
        }
        var txt = new KtxTestTexture();
        txt.graphicsFormat = gf;
        txt.transF = transF;

        var testLoader = Object.Instantiate<TestKtxFileLoader>(prefab);
        testLoader.overrideTexture = txt;
        testLoader.filePath = file;
        currentGo = testLoader.gameObject;
    }
}
