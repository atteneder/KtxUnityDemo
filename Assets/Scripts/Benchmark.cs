using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Networking;
using BasisUniversalUnity;

public class Benchmark : MonoBehaviour
{
    [SerializeField]
    private string filePath;

    [SerializeField]
    private int count = 10;

    [SerializeField]
    Renderer prefab;

    byte[] data;

    // Start is called before the first frame update
    IEnumerator Start() {
        var url = BasisUniversalTexture.GetStreamingAssetsUrl(filePath);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        if(!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogErrorFormat("Error loading {0}: {1}",url,webRequest.error);
            yield break;
        }
        data = webRequest.downloadHandler.data;
        LoadBatch();
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space) && data!=null) {
            LoadBatch();
        }
    }

    void LoadBatch() {
        Profiler.BeginSample("LoadBatch");
        for (int i = 0; i < count; i++)
        {
            var b = Object.Instantiate<Renderer>(prefab);
            b.transform.position = new Vector3( Random.value, Random.value, Random.value ) * 3 - Vector3.one * 1.5f;
            b.material.mainTexture = BasisUniversal.LoadBytes(data);
        }
        Profiler.EndSample();
    }
}
