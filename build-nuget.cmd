if not exist .\bin mkdir .\bin
.\tools\nuget.exe pack .\src\AttributeAuthorization\AttributeAuthorization.csproj -Prop Configuration=Release -OutputDirectory .\bin