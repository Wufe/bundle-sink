[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $ApiKey
)


rm -rf ./bin/Release

dotnet pack -c Release

cd ./bin/Release

dotnet nuget push BundleSink*.nupkg --api-key $ApiKey --source https://api.nuget.org/v3/index.json