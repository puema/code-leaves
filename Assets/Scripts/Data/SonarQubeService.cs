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

public class SonarQubeService : Singleton<SonarQubeService>
{
    public string SonarQubeServerUrl = "https://www.qaware.de/sonarqube/";
    public string Token = "d507a619e0f8a2304e70fcd0776204236342b625";
    public string BaseComponentKey = "com.bmw.ispi.air.central:air-awpos";
    
    private string keyWithErrorChild = "com.bmw.ispi.air.central:air-common-validation:src/test/java/com/bmw/ispi/air/central/common/validation/ast";

    private const int pageSize = 500;
    private const string sortFields = "path, name";
    private const string strategy = "children";

    private IEnumerator Start()
    {
        var coroutine = this.StartCoroutine<HierarchyElement>(ComposeElements(keyWithErrorChild));
        yield return coroutine.coroutine;
        Logger.Json(coroutine.value);
        // All requests are finished
        WriteToFile(coroutine.value);
        
//        var routine = this.StartCoroutine<int>(TestTypedRoutine());
//        yield return routine.coroutine;
//        Debug.Log(routine.value);
        
//        CoroutineWithData routine = new CoroutineWithData(this, TestRoutineWithData());
//        yield return routine.coroutine;
//        Debug.Log(routine.result);

        
    }

    private IEnumerator ComposeElements(string sonarQubeComponentKey)
    {
        // Print key to see progress
        Debug.Log(sonarQubeComponentKey);
        var coroutine = this.StartCoroutine<SonarQubeTree>(GetSonarQubeTree(sonarQubeComponentKey));
        yield return coroutine.coroutine;
        var tree = coroutine.value;
        
        // For now in case of an internal SonarQube error, the requested component is discarded comletely
        if (tree == null) yield break;
        
        // Map the requested component to our abstract software model
        HierarchyElement element = MapSonarQubeComponentToHierarchyElement(tree.baseComponent);
        
        // Recursive call for all children
        foreach (var component in tree.components)
        {
            var childCoroutine = this.StartCoroutine<HierarchyElement>(ComposeElements(component.key));
            yield return childCoroutine.coroutine;
            if (childCoroutine.value != null) element.Children.Add(childCoroutine.value);
        }
//        
        // Element and all children are finished
        yield return element;
    }

    private IEnumerator GetSonarQubeTree(string sonarQubeComponentKey)
    {
        var www = CreateWebRequest(sonarQubeComponentKey);
        yield return www;

        if (www.error != "")
        {
            Debug.LogWarning("SonarQube HTTP Error with Key: \"" + sonarQubeComponentKey + "\"");
        }
        else
        {
            var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
            yield return sonarQubeTree;
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
}
