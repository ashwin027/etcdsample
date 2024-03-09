Pulled: registry-1.docker.io/bitnamicharts/etcd:9.15.2
Digest: sha256:13eab1cf897edae00b9d74c1a5f01b20f9a202d8090e4745d11b83a0f523cafe
NAME: etcd
LAST DEPLOYED: Sat Mar  9 12:49:48 2024
NAMESPACE: interconnect
STATUS: deployed
REVISION: 1
TEST SUITE: None
NOTES:
CHART NAME: etcd
CHART VERSION: 9.15.2
APP VERSION: 3.5.12

** Please be patient while the chart is being deployed **

etcd can be accessed via port 2379 on the following DNS name from within your cluster:

    etcd.interconnect.svc.cluster.local

To create a pod that you can use as a etcd client run the following command:

    kubectl run etcd-client --restart='Never' --image docker.io/bitnami/etcd:3.5.12-debian-12-r10 --env ROOT_PASSWORD=$(kubectl get secret --namespace interconnect etcd -o jsonpath="{.data.etcd-root-password}" | base64 -d) --env ETCDCTL_ENDPOINTS="etcd.interconnect.svc.cluster.local:2379" --namespace interconnect --command -- sleep infinity

Then, you can set/get a key using the commands below:

    kubectl exec --namespace interconnect -it etcd-client -- bash
    etcdctl --user root:$ROOT_PASSWORD put /message Hello
    etcdctl --user root:$ROOT_PASSWORD get /message

To connect to your etcd server from outside the cluster execute the following commands:

    kubectl port-forward --namespace interconnect svc/etcd 2379:2379 &
    echo "etcd URL: http://127.0.0.1:2379"

 * As rbac is enabled you should add the flag `--user root:$ETCD_ROOT_PASSWORD` to the etcdctl commands. Use the command below to export the password:

    export ETCD_ROOT_PASSWORD=$(kubectl get secret --namespace interconnect etcd -o jsonpath="{.data.etcd-root-password}" | base64 -d)

WARNING: There are "resources" sections in the chart not set. Using "resourcesPreset" is not recommended for production. For production installations, please set the following values according to your workload needs:
  - disasterRecovery.cronjob.resources
  - resources
+info https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/