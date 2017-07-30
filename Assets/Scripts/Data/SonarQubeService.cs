using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class SonarQubeService : Singleton<SonarQubeService>
{
    public string SonarQubeServerUrl = "https://www.qaware.de/sonarqube/";
    public string Token = "d507a619e0f8a2304e70fcd0776204236342b625";
    public string BaseComponentKey = "com.bmw.ispi.air.central:air";

    private const int pageSize = 500;
    private const string sortFields = "path, name";
    private const string strategy = "children";

    private Package root;

    private async void Start()
    {
        StartCoroutine(GetSonarQubeTree(BaseComponentKey, element => { root = (Package) element; }));
        StartCoroutine(WaitABit());
    }

    private IEnumerator WaitABit()
    {
        yield return new WaitForSeconds(60);
        var json = JsonConvert.SerializeObject(root, Formatting.Indented);
        Debug.Log("Writing to file...");
        File.WriteAllText("Assets/StreamingAssets/AirStructure.json", json);
    }

    private IEnumerator GetSonarQubeTree(string sonarQubeComponentKey, Action<HierarchyElement> callback)
    {
        var url =
            $"{SonarQubeServerUrl}api/components/tree?baseComponentKey={sonarQubeComponentKey}" +
            $"&ps={pageSize}&s={sortFields}&strategy={strategy}";

        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Token + ":"))
        };

        var www = new WWW(url, null, headers);
        yield return www;

        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
        if (sonarQubeComponentKey ==
            "com.bmw.ispi.air.central:air-da-document:src/main/java/com/bmw/ispi/air/central/da/document/DocumentStorage.java"
        )
        {
            
            Logger.Json(sonarQubeTree);
        }
        var parent = MapSonarQubeComponentToHierarchyElement(sonarQubeTree.baseComponent);
        
        callback(parent);
        
        foreach (var component in sonarQubeTree.components)
        {
            StartCoroutine(GetSonarQubeTree(component.key, element =>
            {
                parent.Children.Add(element);
            }));
        }
    }

    private async Task<HierarchyElement> DoStuff(string sonarQubeComponentKey)
    {
        var webRequest = (HttpWebRequest) WebRequest.Create(
            $"{SonarQubeServerUrl}api/components/tree?baseComponentKey={sonarQubeComponentKey}" +
            $"&ps={pageSize}&s={sortFields}&strategy={strategy}");

        webRequest.Method = "GET";
        webRequest.Headers.Add("Authorization",
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Token + ":")));

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        // allows for validation of SSL conversations
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        var response = await webRequest.GetResponseAsync();

        var reader = new StreamReader(response.GetResponseStream());
        var json = reader.ReadToEnd();
        Debug.Log(json);

        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(json);

        Logger.Json(sonarQubeTree);
        return MapSonarQubeComponentToHierarchyElement(sonarQubeTree.baseComponent);
    }

    private HierarchyElement WebClientRequest(string sonarQubeComponentKey)
    {
        var webClient = new WebClient();
        webClient.Headers.Add("Authorization",
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Token + ":")));
        var json = webClient.DownloadString(
            $"{SonarQubeServerUrl}api/components/tree?baseComponentKey={sonarQubeComponentKey}" +
            $"&ps={pageSize}&s={sortFields}&strategy={strategy}");

        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(json);

        Logger.Json(sonarQubeTree);
        return MapSonarQubeComponentToHierarchyElement(sonarQubeTree.baseComponent);
    }

    private static HierarchyElement MapSonarQubeComponentToHierarchyElement(SonarQubeComponent component)
    {
        if (component == null) return null;
        
        if (component.qualifier == SonarQubeQualifier.TRK ||
            component.qualifier == SonarQubeQualifier.BRC ||
            component.qualifier == SonarQubeQualifier.DIR)
        {
            return new Package
            {
                Name = component.name,
                Key = component.key,
                Children = new List<HierarchyElement>()
            };
        }
        return new Class
        {
            Name = component.name,
            Key = component.key,
            Children = new List<HierarchyElement>()
        };
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; }
    public object result;
    private readonly IEnumerator target;

    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}