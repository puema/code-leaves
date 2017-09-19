using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;
using Utilities;
using Logger = Utilities.Logger;

namespace Data
{
    public class SonarQubeService : Singleton<SonarQubeService>
    {
        public const string SonarQubeServerUrl = "https://www.qaware.de/sonarqube/";
        public const string Token = "d507a619e0f8a2304e70fcd0776204236342b625";

        private const string keyWithErrorChild = "com.bmw.ispi.air.central:air-common-validation:" +
                                                 "src/test/java/com/bmw/ispi/air/central/common/validation/ast";

        private const int pageSize = 500;
        private const string metricKeys = SonarQubeMetric.Coverage;
        private static readonly string sortFields = string.Join(", ", SonarQubeSortField.Path, SonarQubeSortField.Name);

        private static int successfullCalls;
        private static int totalCalls;

        public IEnumerator AddMetrics(SoftwareArtefact artefact)
        {
            var baseComponentKey = artefact.Key;

            var components = new List<SonarQubeComponent>();

            var page = 1;
            while (true)
            {
                var coroutine =
                    this.StartCoroutine<SonarQubeTree>(GetSonarQubeMeasures(baseComponentKey, page.ToString()));
                yield return coroutine.coroutine;

                components.AddRange(coroutine.value.components);

                page++;

                if (page > Math.Ceiling((double) coroutine.value.paging.total / coroutine.value.paging.pageSize)) break;
            }

            MapMetricToArtefact(ref artefact, components);

            WriteToFile(artefact);
        }

        private static void MapMetricToArtefact(ref SoftwareArtefact artefact, List<SonarQubeComponent> components)
        {
            foreach (var a in artefact.Traverse(a => a.Children))
            {
                if (a.Children.Count != 0) continue;
                var component = components.Find(c => c.key == a.Key);

                if (component == null) continue;
                foreach (var metric in component.measures)
                {
                    a.Metrics = new List<Metric>
                    {
                        new Metric
                        {
                            Key = metric.metric,
                            Value = (float) Math.Round(metric.value)
                        }
                    };
                }
            }
        }

        public IEnumerator GetStructure(string baseComponentKey)
        {
            var coroutine = this.StartCoroutine<SoftwareArtefact>(ComposeElements(baseComponentKey));
            yield return coroutine.coroutine;
            Logger.Log(coroutine.value);
            Debug.Log("Total calls: " + totalCalls);
            Debug.Log("Successfull calls: " + successfullCalls);
            // All requests are finished
//            WriteToFile(coroutine.value);
            yield return coroutine.value;
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
            SoftwareArtefact element = MapSonarQubeComponentToSoftwareArtefact(tree.baseComponent);

            // Store child coroutines to be able to join them
            List<Coroutine<SoftwareArtefact>> childCoroutines = tree.components
                .Select(component => this.StartCoroutine<SoftwareArtefact>(ComposeElements(component.key))).ToList();

            yield return CoroutineUtils.WaitForAll(childCoroutines);

            element.Children = childCoroutines.Select(c => c.value).Where(value => value != null).ToList();

            // Element and all children are finished
            yield return element;
        }

        private static IEnumerator GetSonarQubeMeasures(string baseComponentKey, string page)
        {
            const string strategy = SonarQubeStrategy.Leaves;
            var url =
                $"{SonarQubeServerUrl}api/measures/component_tree?baseComponentKey={baseComponentKey}" +
                $"&ps={pageSize}&p={page}&strategy={strategy}&metricKeys={metricKeys}";

            var www = new WWW(url, null, GetHeaders());
            yield return www;

            if (www.error != "")
            {
                HandleError(baseComponentKey, www);
            }
            else
            {
                var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
                yield return sonarQubeTree;
            }
        }

        private static IEnumerator GetSonarQubeTree(string sonarQubeComponentKey)
        {
            const string strategy = SonarQubeStrategy.Children;
            var url =
                $"{SonarQubeServerUrl}api/components/tree?baseComponentKey={sonarQubeComponentKey}" +
                $"&ps={pageSize}&s={sortFields}&strategy={strategy}";

            var www = new WWW(url, null, GetHeaders());
            yield return www;

            if (www.error != "")
            {
                HandleError(sonarQubeComponentKey, www);
            }
            else
            {
                var sonarQubeTree = JsonConvert.DeserializeObject<SonarQubeTree>(www.text);
                yield return sonarQubeTree;
            }
        }

        private static void HandleError(string sonarQubeComponentKey, WWW www)
        {
            Debug.LogWarning("SonarQube HTTP Error with Key: \"" + sonarQubeComponentKey + "\"\n" +
                             "Error code: " + www.error + "\n" +
                             "Text: " + www.text);
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {"Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Token + ":"))}
            };
        }

        private static void WriteToFile(SoftwareArtefact element)
        {
            var json = JsonConvert.SerializeObject(element, Formatting.Indented);
            Debug.Log("Writing to file...");
            File.WriteAllText("Assets/StreamingAssets/AirStructure2.json", json);
        }

        private static SoftwareArtefact MapSonarQubeComponentToSoftwareArtefact(SonarQubeComponent component)
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
}