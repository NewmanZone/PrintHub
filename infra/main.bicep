targetScope = 'subscription'

@description('Short app name used in Azure resource names.')
param appName string = 'printhub'

@allowed([
  'dev'
  'test'
  'prod'
])
@description('Deployment environment suffix.')
param environment string = 'prod'

@description('Azure region for all resources.')
param location string = 'eastus2'

@description('Container image used during initial provisioning. The deploy workflow updates this to the ACR-built PrintHub API image.')
param apiImage string = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

@secure()
@description('Etsy OAuth client id.')
param etsyClientId string

@secure()
@description('Etsy OAuth client secret.')
param etsyClientSecret string

@secure()
@description('Dedicated state-signing secret for Etsy OAuth CSRF protection.')
param etsyStateSigningSecret string

@secure()
@description('Base64 AES key used to encrypt stored Etsy OAuth credentials.')
param tokenEncryptionKey string

var resourceGroupName = 'rg-${appName}-${environment}'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

module app './modules/app.bicep' = {
  name: 'printhub-app-${environment}'
  scope: rg
  params: {
    appName: appName
    environment: environment
    location: location
    apiImage: apiImage
    etsyClientId: etsyClientId
    etsyClientSecret: etsyClientSecret
    etsyStateSigningSecret: etsyStateSigningSecret
    tokenEncryptionKey: tokenEncryptionKey
  }
}

output resourceGroupName string = rg.name
output apiContainerAppName string = app.outputs.apiContainerAppName
output apiFqdn string = app.outputs.apiFqdn
output acrName string = app.outputs.acrName
output acrLoginServer string = app.outputs.acrLoginServer
output frontendStorageAccountName string = app.outputs.frontendStorageAccountName
output frontendEndpoint string = app.outputs.frontendEndpoint
