using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;
using Logger = Utilities.Logger;

public class SonarQubeService : Singleton<SonarQubeService>
{
    public string SonarQubeServerUrl = "https://www.qaware.de/sonarqube/";
    public string Token = "d507a619e0f8a2304e70fcd0776204236342b625";
    public string BaseComponentKey = "com.bmw.ispi.air.central:air-awpos";

    private string keyWithErrorChild =
        "com.bmw.ispi.air.central:air-common-validation:src/test/java/com/bmw/ispi/air/central/common/validation/ast";

    private const int pageSize = 500;
    private const string sortFields = "path, name";
    private const string strategy = "children";

    private int successfullCalls;
    private int totalCalls;

    private IEnumerator Start()
    {
        var coroutine = this.StartCoroutine<SoftwareArtefact>(ComposeElements(BaseComponentKey));
        yield return coroutine.coroutine;
        Logger.Log(coroutine.value);
        Debug.Log("Total calls: " + totalCalls);
        Debug.Log("Successfull calls: " + successfullCalls);
        // All requests are finished
        WriteToFile(coroutine.value);
    }

    private IEnumerator ComposeElements(string sonarQubeComponentKey)
    {
        totalCalls++;
        // Print key to see progress
        Debug.Log(sonarQubeComponentKey);
        var coroutine = this.StartCoroutine<SonarQubeTree>(GetSonarQubeTree(sonarQubeComponentKey));
        yield return coroutine.coroutine;
        var tree = coroutine.value;

        // For now in case of an internal SonarQube error, the requested component is discarded comletely
        if (tree == null) yield break;

        successfullCalls++;

        // Map the requested component to our abstract software model
        SoftwareArtefact element = MapSonarQubeComponentToHierarchyElement(tree.baseComponent);

        // Store child coroutines to be able to join them
        List<Coroutine<SoftwareArtefact>> childCoroutines = tree.components
            .Select(component => this.StartCoroutine<SoftwareArtefact>(ComposeElements(component.key))).ToList();

        yield return CoroutineUtils.WaitForAll(childCoroutines);

        element.Children = childCoroutines.Select(c => c.value).Where(value => value != null).ToList();
        
        // Element and all children are finished
        yield return element;
    }

    private IEnumerator GetSonarQubeTree(string sonarQubeComponentKey)
    {
        var www = CreateWebRequest(sonarQubeComponentKey);
        yield return www;

        if (www.error != "")
        {
            Debug.LogWarning("SonarQube HTTP Error with Key: \"" + sonarQubeComponentKey + "\"\n" +
                             "Error code: " + www.error + "\n"+
                             "Text: " + www.text);
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

    private static void WriteToFile(SoftwareArtefact element)
    {
        var json = JsonConvert.SerializeObject(element, Formatting.Indented);
        Debug.Log("Writing to file...");
        File.WriteAllText("Assets/StreamingAssets/AirStructure2.json", json);
    }

    private static SoftwareArtefact MapSonarQubeComponentToHierarchyElement(SonarQubeComponent component)
    {
        if (component == null) return null;

        if (component.qualifier == SonarQubeQualifier.TRK ||
            component.qualifier == SonarQubeQualifier.BRC ||
            component.qualifier == SonarQubeQualifier.DIR)
        {
            return new SoftwareArtefact
            {
                Name = component.name,
                Key = component.key,
                Children = new List<SoftwareArtefact>()
            };
        }
        return new SoftwareArtefact
        {
            Name = component.name,
            Key = component.key,
            Children = new List<SoftwareArtefact>()
        };
    }
}