param appName string
param environment string
param location string
param apiImage string

@secure()
param etsyClientId string

@secure()
param etsyClientSecret string

@secure()
param etsyStateSigningSecret string

@secure()
param tokenEncryptionKey string

var suffix = uniqueString(resourceGroup().id, appName, environment)
var normalized = toLower(replace(appName, '-', ''))
var acrName = take('${normalized}${environment}${suffix}', 50)
var stateStorageName = take('st${normalized}${environment}${suffix}', 24)
var frontendStorageName = take('web${normalized}${environment}${suffix}', 24)
var logAnalyticsName = 'log-${appName}-${environment}'
var appInsightsName = 'appi-${appName}-${environment}'
var containerEnvName = 'cae-${appName}-${environment}'
var apiAppName = 'ca-${appName}-api-${environment}'
var identityName = 'id-${appName}-api-${environment}'
var blobContainerName = 'printhub'
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${stateStorage.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${stateStorage.listKeys().keys[0].value}'
var frontendEndpoint = frontendStorage.properties.primaryEndpoints.web

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, identity.id, 'acrpull')
  scope: acr
  properties: {
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

resource stateStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: stateStorageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource stateBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: stateStorage
  name: 'default'
}

resource stateContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: stateBlobService
  name: blobContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource frontendStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: frontendStorageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource frontendBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: frontendStorage
  name: 'default'
}

resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource apiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: identity.id
        }
      ]
      secrets: [
        {
          name: 'storage-connection-string'
          value: storageConnectionString
        }
        {
          name: 'etsy-client-secret'
          value: etsyClientSecret
        }
        {
          name: 'etsy-state-signing-secret'
          value: etsyStateSigningSecret
        }
        {
          name: 'token-encryption-key'
          value: tokenEncryptionKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'AZURE_STORAGE_CONNECTION_STRING'
              secretRef: 'storage-connection-string'
            }
            {
              name: 'Storage__ContainerName'
              value: blobContainerName
            }
            {
              name: 'Etsy__ClientId'
              value: etsyClientId
            }
            {
              name: 'Etsy__ClientSecret'
              secretRef: 'etsy-client-secret'
            }
            {
              name: 'Etsy__StateSigningSecret'
              secretRef: 'etsy-state-signing-secret'
            }
            {
              name: 'TokenEncryption__Key'
              secretRef: 'token-encryption-key'
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: frontendEndpoint
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
      }
    }
  }
  dependsOn: [
    acrPull
    stateContainer
    frontendBlobService
  ]
}

output apiContainerAppName string = apiApp.name
output apiFqdn string = apiApp.properties.configuration.ingress.fqdn
output acrName string = acr.name
output acrLoginServer string = acr.properties.loginServer
output frontendStorageAccountName string = frontendStorage.name
output frontendEndpoint string = frontendEndpoint
