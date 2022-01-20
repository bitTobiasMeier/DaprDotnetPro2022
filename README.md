# Dinner for Dapr
Sample application for the dapr article series in the dotnetpro magazine 2/2022 - 4/2022


Deployment to Azure Kubernetes Services
_______________________

1. Create a kubernetes cluster and connect your local docker installation to kubernetes
2. Init the cluster for dapr: dapr init -k
1. Create namespace: kubectl create namespace dinnerdemo
2. deploy redis with helm:

*helm repo add bitnami https://charts.bitnami.com/bitnami --namespace dinnerdemo*

*helm repo update*

*helm install redis bitnami/redis*

3. Go to directory deploy\components and deploy the dapr components with kubectl apply -f . -n dinnerdemo
4. Create docker containers for all services and adjust the image names in the yaml files
5. go to directory deploy and deploy the services with kubectl apply -f . -n dinnerdemo



