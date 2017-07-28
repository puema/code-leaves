using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using Newtonsoft.Json;
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

    private async void Start()
    {
//        StartCoroutine(GetSonarQubeTree(BaseComponentKey));
//        var package = await DoStuff(BaseComponentKey);
//        Logger.Json(package);
    }

    private IEnumerator GetSonarQubeTree(string sonarQubeComponentKey)
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

        handleResponse(www);
    }

    private async Task<Package> DoStuff(string sonarQubeComponentKey)
    {
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

        return MapSonarQubeTreeToPackage(sonarQubeTree);
    }

    private void handleResponse(WWW webRequest)
    {
        var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(webRequest.text);

        var element = MapSonarQubeComponentToPackage(sonarQubeTree.baseComponent);
        
        Logger.Json(element);
    }

    private static Package MapSonarQubeTreeToPackage(SonarQubeTree tree)
    {
        var package = new Package
        {
            Name = tree.baseComponent.name,
            Key = tree.baseComponent.key,
            Children = new List<HierarchyElement>()
        };

        foreach (var component in tree.components)
        {
            package.Children.Add(MapSonarQubeComponentToPackage(component));
        }
        return package;
    }

    private static Package MapSonarQubeComponentToPackage(SonarQubeComponent component)
    {
        return new Package
        {
            Name = component.name,
            Key = component.key,
            Children = new List<HierarchyElement>()
        };
    }
    
    public bool MyRemoteCertificateValidationCallback(System.Object sender,
        X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None) {
            for (int i=0; i<chain.ChainStatus.Length; i++) {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build ((X509Certificate2)certificate);
                if (!chainIsValid) {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }
}