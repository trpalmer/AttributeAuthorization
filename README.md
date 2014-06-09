# AttributeAuthorization
A simple flexible .NET library for attribute based authorization for WebApi projects using [AttributeRouting](https://github.com/mccalltd/AttributeRouting).

Grab the source, or use [NuGet](https://www.nuget.org/packages/AttributeAuthorization/):
````
Install-Package AttributeAuthorization
````

## What It Does
AttributeAuthorization allows you to use attributes on your WebApi methods for authorization of API endpoints.

````c#
[POST("file")]
[AuthorizedFor("file:write")]
public HttpResponseMessage PostUploadFile(FileData data)
{
  ....
}
````

And then easily test if the current caller is allowed access to that method:
````c#
if (!authorization.IsAllowed(Request))
{
	return Request.CreateResponse(HttpStatusCode.Forbidden, "You do not have access to this method");
}
````

## Features
* Use attributes to define and document permissions on API methods.
* Support for auto-expanded parent:child permissions where access to the parent allows access to the child.
* Support for public methods where authorization is not required.
* Secure by default. Default route, non-attributed, mixed public/private methods are not allowed by default. Behavior is easily controlled.
* Works with OAuth, API Key or other authorization strategies.
* [MIT License](http://opensource.org/licenses/MIT)

## What It Doesn't Do
Make any assumptions about your security method. You plug in the method you need to determine the authorization carried with a request.
