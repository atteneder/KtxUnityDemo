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
using UnityEngine.SceneManagement;

public class SceneSelection : MonoBehaviour
{
    [SerializeField] float width = 300;
    [SerializeField] float height = 100;
    [SerializeField] float yGap = 10;

    void OnGUI() {
        float y = yGap;
        if( GUI.Button( new Rect(0,y,width,height),"Main scene")) {
            SceneManager.LoadScene("Main",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"Format Test scene")) {
            SceneManager.LoadScene("FormatTestScene",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"BasisU scene")) {
            SceneManager.LoadScene("SampleSceneBasisUniversal",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"BasisU UASTC scene")) {
            SceneManager.LoadScene("SampleSceneBasisUniversalUASTC",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"BasisU Canvas scene")) {
            SceneManager.LoadScene("SampleSceneBasisUniversalCanvas",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"KTX scene")) {
            SceneManager.LoadScene("SampleSceneKtx",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"KTX UASTC scene")) {
            SceneManager.LoadScene("SampleSceneKtxUASTC",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"KTX UASTC Z scene")) {
            SceneManager.LoadScene("SampleSceneKtxUASTCZ",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"KTX Normal scene")) {
            SceneManager.LoadScene("SampleSceneKtxNormal",LoadSceneMode.Single);
        }
        y += height + yGap;

        if( GUI.Button( new Rect(0,y,width,height),"Benchmark scene")) {
            SceneManager.LoadScene("BenchmarkScene",LoadSceneMode.Single);
        }
    }
}
