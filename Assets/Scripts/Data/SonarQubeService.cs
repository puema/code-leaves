using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using NUnit.Framework.Internal.Execution;
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
    public string BaseComponentKey = "com.bmw.ispi.air.central:air-common";
    
    private string keyWithErrorChild = "com.bmw.ispi.air.central:air-common-validation:src/test/java/com/bmw/ispi/air/central/common/validation/ast";

    private const int pageSize = 500;
    private const string sortFields = "path, name";
    private const string strategy = "children";

    private Package root;
    private readonly Queue<string> queue = new Queue<string>();
    private readonly List<CompositionElement> compositionElements = new List<CompositionElement>();

    private IEnumerator Start()
    {
        HierarchyElement element = null;
        var coroutine = StartCoroutine(ComposeElements(BaseComponentKey, e => { element = e; }));
        yield return coroutine;
        
        // All requests are finished
        WriteToFile(element);
    }

    private IEnumerator ComposeElements(string sonarQubeComponentKey, Action<HierarchyElement> callback)
    {
        // Print key to see progress
        Debug.Log(sonarQubeComponentKey);
        SonarQubeTree tree = null;
        var coroutine = StartCoroutine(GetSonarQubeTree(sonarQubeComponentKey, t => tree = t ));
        yield return coroutine;
        
        // For now in case of an internal SonarQube error, the requested component is discarded comletely
        if (tree == null) yield break;
        
        // Map the requested component to our abstract software model
        HierarchyElement element = MapSonarQubeComponentToHierarchyElement(tree.baseComponent);
        
        // Recursive call for all children
        foreach (var component in tree.components)
        {
            HierarchyElement child = null;
            var childCoroutine = StartCoroutine(ComposeElements(component.key, e => child = e));
            yield return childCoroutine;
            element.Children.Add(child);
        }
        
        // Element and all children are finished
        callback(element);
    }

    private IEnumerator ProvideItemsInQueue(Action callback)
    {
        while (queue.Count != 0)
        {
            var key = queue.Dequeue();
            Debug.Log(key);
            var coroutine = StartCoroutine(GetSonarQubeTree(key, t =>
            {
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

        if (www.error != "")
        {
            Debug.LogWarning("SonarQube HTTP Error with Key: \"" + sonarQubeComponentKey + "\"");
            callback(null);
        }
        else
        {
            var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
            callback(sonarQubeTree);
        }
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

    private static void WriteToFile(HierarchyElement element)
    {
        var json = JsonConvert.SerializeObject(element, Formatting.Indented);
        Debug.Log("Writing to file...");
        File.WriteAllText("Assets/StreamingAssets/AirStructure2.json", json);
    }

    private IEnumerator GetSonarQubeTree_back(string sonarQubeComponentKey, Action<HierarchyElement> callback)
    {
        var www = CreateWebRequest(sonarQubeComponentKey);
        yield return www;

        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
        var parent = MapSonarQubeComponentToHierarchyElement(sonarQubeTree.baseComponent);

        callback(parent);
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