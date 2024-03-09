using dotnet_etcd;
using k8s;
using k8s.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Etcd
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // FOR YAMLS
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: "docker-desktop");
            var kubeClient = new Kubernetes(config);
            var typeMap = new Dictionary<String, Type>
            {
                { "v1/Service", typeof(V1Service) },
                { "apps/v1/StatefulSet", typeof(V1StatefulSet) }
            };

            var basePath = Path.Combine(Environment.CurrentDirectory, @"ManifestsV2");

            // Create SA
            var saToCreate = await KubernetesYaml.LoadFromFileAsync<V1ServiceAccount>($"{basePath}/sa.yaml");
            var sas = await kubeClient.CoreV1.ListNamespacedServiceAccountAsync("interconnect");
            var existingSA = sas.Items.FirstOrDefault(i => i.Name().Equals(saToCreate.Name()));
            if (existingSA == null)
            {
                await kubeClient.CoreV1.CreateNamespacedServiceAccountAsync(saToCreate, "interconnect");
            }

            // Create stateful set
            var statefulsets = await kubeClient.AppsV1.ListNamespacedStatefulSetAsync("interconnect");
            var statefulsetToCreate = await KubernetesYaml.LoadFromFileAsync<V1StatefulSet>($"{basePath}/statefulset.yaml");
            var existingStatefulService = statefulsets.Items.FirstOrDefault(i => i.Name().Equals(statefulsetToCreate.Name()));
            if (existingStatefulService == null)
            {
                await kubeClient.AppsV1.CreateNamespacedStatefulSetAsync(statefulsetToCreate, "interconnect");
            }

            var services = await kubeClient.CoreV1.ListNamespacedServiceAsync("interconnect");

            // Create headless service
            var headlessServiceToCreate = await KubernetesYaml.LoadFromFileAsync<V1Service>($"{basePath}/service-headless.yaml");
            var existingHeadlessService = services.Items.FirstOrDefault(i => i.Name().Equals(headlessServiceToCreate.Name()));
            if (existingHeadlessService == null)
            {
                await kubeClient.CoreV1.CreateNamespacedServiceAsync(headlessServiceToCreate, "interconnect");
            }

            // Create node port service
            var nodeportServiceToCreate = await KubernetesYaml.LoadFromFileAsync<V1Service>($"{basePath}/service-nodeport.yaml");
            var existingNodeportService = services.Items.FirstOrDefault(i => i.Name().Equals(nodeportServiceToCreate.Name()));
            if (existingNodeportService == null)
            {
                await kubeClient.CoreV1.CreateNamespacedServiceAsync(nodeportServiceToCreate, "interconnect");
            }
            
            var statefulsetWatched = kubeClient.AppsV1.ListNamespacedStatefulSetWithHttpMessagesAsync("interconnect", watch: true);
            await foreach (var (type, item) in statefulsetWatched.WatchAsync<V1StatefulSet, V1StatefulSetList>())
            {
                if (item.Status.AvailableReplicas == 3)
                {
                    break;
                }
            }

            EtcdClient client = new EtcdClient("http://localhost:30379,http://localhost:30380");
            //  Put key "foo/bar" with value "barfoo" with authenticated token
            client.Put("foo/bar", "barfoo");

            var test = client.GetVal("foo/bar");
            Console.WriteLine(test);
        }
    }
}
