using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;

public class CompositionElement
{
    public string Key { get; set; }
    public string Parent { get; set; }
    public HierarchyElement Element { get; set; }
}

public class SonarQubeService : Singleton<SonarQubeService>
{
    public string SonarQubeServerUrl = "https://www.qaware.de/sonarqube/";
    public string Token = "d507a619e0f8a2304e70fcd0776204236342b625";
    public string BaseComponentKey = "com.bmw.ispi.air.central:air";

    private const int pageSize = 500;
    private const string sortFields = "path, name";
    private const string strategy = "children";

    private Package root;
    private readonly Queue<string> queue = new Queue<string>();
    private readonly List<string> compositionElements = new List<string>();

    private IEnumerator Start()
    {
        queue.Enqueue(BaseComponentKey);
        var coroutine = StartCoroutine(ProvideItemsInQueue(() =>
        {
            Logger.Json(compositionElements);

        }));
        ComposeElements();
        yield return coroutine;
    }

    private void ComposeElements()
    {
    }

    private IEnumerator ProvideItemsInQueue(Action callback)
    {
        while (queue.Count != 0)
        {
            Logger.Json(queue);
            var key = queue.Dequeue();
            var coroutine = StartCoroutine(GetSonarQubeTree(key, t =>
            {
                compositionElements.Add(t.baseComponent.key);
                foreach (var component in t.components)
                {
                    queue.Enqueue(component.key);
                }
            }));
            yield return coroutine;
        }
        callback();
    }

    private IEnumerator GetSonarQubeTree(string sonarQubeComponentKey, Action<SonarQubeTree> callback)
    {
        var www = CreateWebRequest(sonarQubeComponentKey);
        yield return www;

        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);

        callback(sonarQubeTree);
    }

    private WWW CreateWebRequest(string sonarQubeComponentKey)
    {
        var url =
            $"{SonarQubeServerUrl}api/components/tree?baseComponentKey={sonarQubeComponentKey}" +
            $"&ps={pageSize}&s={sortFields}&strategy={strategy}";

        var headers = new Dictionary<string, string>
        {
            {"Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Token + ":"))}
        };

        var www = new WWW(url, null, headers);
        return www;
    }

    private IEnumerator GetSonarQubeTree_back(string sonarQubeComponentKey, Action<HierarchyElement> callback)
    {
        var www = CreateWebRequest(sonarQubeComponentKey);
        yield return www;

        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
        var parent = MapSonarQubeComponentToHierarchyElement(sonarQubeTree.baseComponent);

        callback(parent);

        foreach (var component in sonarQubeTree.components)
        {
            StartCoroutine(GetSonarQubeTree_back(component.key, element => { parent.Children.Add(element); }));
        }
    }

    private async Task<HierarchyElement> DoStuff(string sonarQubeComponentKey)
    {
        ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

        var webRequest = (HttpWebRequest) WebRequest.Create(
            $"{SonarQubeServerUrl}api/components/tree?baseComponentKey={sonarQubeComponentKey}" +
            $"&ps={pageSize}&s={sortFields}&strategy={strategy}");

        webRequest.Method = "GET";
        webRequest.Headers.Add("Authorization",
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Token + ":")));

        var response = await webRequest.GetResponseAsync();

        var reader = new StreamReader(response.GetResponseStream());
        var json = reader.ReadToEnd();
        Debug.Log(json);

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

    private IEnumerator WaitABit()
    {
        yield return new WaitForSeconds(60);
        var json = JsonConvert.SerializeObject(root, Formatting.Indented);
        Debug.Log("Writing to file...");
        File.WriteAllText("Assets/StreamingAssets/AirStructure.json", json);
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