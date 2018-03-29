if not exist .\bin mkdir .\bin
nuget.exe pack .\src\AttributeAuthorization\AttributeAuthorization.csproj -Prop Configuration=Release -OutputDirectory .\bin 