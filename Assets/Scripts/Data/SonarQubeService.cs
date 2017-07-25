using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;

public class SonarQubeService : Singleton<SonarQubeService>
{
    public string Token;
    public string BaseComponentKey;

    private void Start()
    {
        StartCoroutine(GetSonarQubeData());
    }

    IEnumerator GetSonarQubeData()
    {
        var url = "https://www.qaware.de/sonarqube/api/components/tree?baseComponentKey=" + BaseComponentKey + 
                           "&ps=500&s=path, name";
        var headers = new Dictionary<string, string>();
        headers["Authorization"] = "Basic " + 
                                   System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(Token + ":"));

        // Post a request to an URL with our custom headers
        var webRequest = new WWW(url, null, headers);
        yield return webRequest;

        if (webRequest.error != null)
        {
            Debug.Log("Web request to SonarQube failed:");
            Debug.Log(webRequest.text);
            yield break;
        }
        
        var test = JsonConvert.DeserializeObject<SonarQubeTree>(webRequest.text);
        
        // Show results as text
        Debug.Log(test.paging.total);
    }
}