# customerapp-webapi
 Creating webapi entity-framework automapper autofac dependency-injection

## Table of Contents

- [Sending Feedback](#sending-feedback)
- [About Web API](#about-entity-framework-core)
- [Sample application with each labs](#sample-application-with-each-steps)
    - Creating Web API application and configuration
        - [Step 1 - Create Application and setting up with Entity framework](#step-1---create-application-and-setting-up-with-entity-framework)
    - [Creating Get API](#creating-get-api)
        - [Step 2 - Status code](#step-2---status-code)
        - [Step 3 - Using code](#step-3---using-code)
        - [Step 4 - Using GET collections](#step-4---using-get-collections)
        - [Step 5 - Using Models instead of entities](#step-5---using-models-instead-of-entities)
        - [Step 6 - Serialization in Web API](#step-6---serialization-in-web-api)
        - [Step 7 - Getting Individual items](#step-7---getting-individual-items)
        - [Step 8 - Returning related data](#step-8---returning-related-data)
        - [Step 9 - Using query string](#step-9---using-query-string)
        - [Step 10 - Implementing search](#step-10---implementing-search)
    - [Modifying Data](#modifying-data)
        - [Step 11 - URI Design](#step-11---uri-design)
        - [Step 12 - Model binding](#step-12---model-binding)
        - [Step 13 - Implementng POST](#step-13---implementng-post)
        - [Step 14 - Adding Model validation](#step-14---adding-model-validation)
        - [Step 15 - Implementng PUT](#step-15---implementing-put)
        - [Step 16 - Implementng DELETE](#step-16---implementing-delete)
    - [Association with API](#modifying-data)


## Sending Feedback

For feedback can drop mail to my email address amit.naik8103@gmail.com or you can create [issue](https://github.com/Amitpnk/angular-application/issues/new)

## About Web API

* ASP.NET Web API is a framework for building HTTP services that can be accessed from any client including browsers and mobile devices. It is an ideal platform for building RESTful applications on the .NET Framework

## Creating Web API application and configuration

### Step 1 - Create Application and setting up with Entity framework

* Create Web API application (Framework 4.7.2)
* Create Entity framework 
    * Refer to [Entityframework repo](https://github.com/Amitpnk/customerapp-entityframework)
    * Register Autofac configuration
    * Create Repository pattern

## Creating Get API

### Step 2 - Status code

|Code|Description|Code|Description|
|---|---|---|---|
|**200**|OK|**400**|Bad request|
|**201**|Created|**401**|Not authorized|
|202|Accepted|**403**|Forbidden|
| ||**404**|Not found|
|302|Found|405|Method not allowed|
|**304**|Not modified|409|Conflict|
|307|Temp redirect| ||
|308|Perm redirect|**500**|Internal error|

* Bold status code is widely used

### Step 3 - Using code

using status code

```c#
public IHttpActionResult get()
{
    var x = true;
    if (x)
    {
        return Ok(new { Name = "Amit", Occupation = "CEO" });
    }
    else
    {
        return BadRequest("Bad request");
    }
}
```

### Step 4 - Using GET collections

Using GET collections

```c#
public class CampsController : ApiController
{
    private readonly ICampRepository repository;

    public CampsController(ICampRepository campRepository)
    {
        repository = campRepository;
    }
    public async Task<IHttpActionResult> Get()
    {
        try
        {
            var result = await repository.GetAllCampsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            // TODO Add logging
            return InternalServerError(ex);
        }

    }
}
```

### Step 5 - Using Models instead of entities

Giving limited property to end user

Add Models/CampModel.cs file

```c#
public class CampModel
{
    public string Name { get; set; }
    public string Moniker { get; set; }
    public DateTime EventDate { get; set; } = DateTime.MinValue;
    public int Length { get; set; } = 1;
}
```

* Install AutoMapper via nuget
* Add Models/CampMappingProfile.cs file

```c#
public class CampMappingProfile : Profile
{
    public CampMappingProfile()
    {
        CreateMap<Camp, CampModel>();
    }
}
```

Register automapper in autofac file

```c#
private static void RegisterServices(ContainerBuilder bldr)
{
    var config = new MapperConfiguration(c =>
    {
        c.AddProfile(new CampMappingProfile());
    });
    bldr.RegisterInstance(config.CreateMapper())
        .As<IMapper>()
        .SingleInstance();

    // ...
}
```

Implement automapper to centrailise configuration

```c#
public class CampsController : ApiController
{
    private readonly ICampRepository _repository;
    private readonly IMapper _mapper;

    public CampsController(ICampRepository campRepository, IMapper mapper)
    {
        _repository = campRepository;
        _mapper = mapper;
    }
    public async Task<IHttpActionResult> Get()
    {
        try
        {
            // Decoupling dal
            var result = await _repository.GetAllCampsAsync();

            // Centralising configuration
            var mappedResult = _mapper.Map<IEnumerable<CampModel>>(result);
            return Ok(mappedResult);
        }
        catch (Exception ex)
        {
            // TODO Add logging
            return InternalServerError(ex);
        }

    }
}
```

### Step 6 - Serialization in Web API

In case, if we need camel case json then add below code to webapiconfig file

```c#
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Web API configuration and services
        AutofacConfig.Register();

        // Change case of JSON
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();
     
        // ...
    }
}
```

### Step 7 - Getting Individual items

Comment route configuration in webapiconfig.cs file

```c#
public static void Register(HttpConfiguration config)
{
    // ... 

    //config.Routes.MapHttpRoute(
    //    name: "DefaultApi",
    //    routeTemplate: "api/{controller}/{id}",
    //    defaults: new { id = RouteParameter.Optional }
    //);
}```

```c#
[RoutePrefix("api/camps")]
public class CampsController : ApiController
{
    // ...

    [Route("{moniker}")]
    public async Task<IHttpActionResult> Get(string moniker)
    {
        try
        {
            // Decoupling dal
            var result = await _repository.GetCampAsync(moniker);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CampModel>(result));
        }
        catch (Exception ex)
        {
            // TODO Add logging
            return InternalServerError(ex);
        }

    }
}
```

### Step 8 - Returning related data

In case, we need location
Add Location prefix to location property in CampModel

```c#
public class CampModel
{
    public string Name { get; set; }
    public string Moniker { get; set; }
    public DateTime EventDate { get; set; } = DateTime.MinValue;
    public int Length { get; set; } = 1;

    // In case if your property name is not added with prefix 
    public string VenueName { get; set; }
    public string LocationAddress1 { get; set; }
    public string LocationAddress2 { get; set; }
    public string LocationAddress3 { get; set; }
    public string LocationCityTown { get; set; }
    public string LocationStateProvince { get; set; }
    public string LocationPostalCode { get; set; }
    public string LocationCountry { get; set; }

}
```

### Step 9 - Using query string

Create speaker and talk model class, because to avoid data class to expose in api side

```c#
public class SpeakerModel
{
    public int SpeakerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string Company { get; set; }
    public string CompanyUrl { get; set; }
    public string BlogUrl { get; set; }
    public string Twitter { get; set; }
    public string GitHub { get; set; }

}
```

```c#
public class TalkModel
{
    public int TalkId { get; set; }
    public string Title { get; set; }
    public string Abstract { get; set; }
    public int Level { get; set; }
    public SpeakerModel Speaker { get; set; }

}
```

Register speaker and talk in automapper

```c#
public class CampMappingProfile : Profile
{
    public CampMappingProfile()
    {
        CreateMap<Camp, CampModel>()
            .ForMember(c => c.Venue,
            opt => opt.MapFrom(m => m.Location.VenueName));
        CreateMap<Speaker, SpeakerModel>();
        CreateMap<Talk, TalkModel>();
    }
}
```

Add query string, if we need to add talk data then to pass it as query string


```c#
[RoutePrefix("api/camps")]
public class CampsController : ApiController
{
    // ...

    [Route()]
    public async Task<IHttpActionResult> Get(bool includeTalks = false)
    {
        try
        {
            // Decoupling dal
            var result = await _repository.GetAllCampsAsync(includeTalks);

        // ...

    }

    [Route("{moniker}")]
    public async Task<IHttpActionResult> Get(string moniker, bool includeTalks = false)
    {
        try
        {
            // Decoupling dal
            var result = await _repository.GetCampAsync(moniker, includeTalks);

        // ...

    }
}

```

passing querystring in postman
```
http://localhost:56556/api/camps?includeTalks=true
http://localhost:56556/api/camps/ATL2018?includeTalks=true
```

### Step 10 - Implementing search

Searching API
* As there is more the 2 get method, we have to use verbs (i.e. HttpGet)

```c#

[Route("searchByDate/{eventDate:datetime}")]
[HttpGet]
public async Task<IHttpActionResult> SearchByEventDate(DateTime eventDate, bool includeTalks = false)
{
    try
    {
        // Decoupling dal
        var result = await _repository.GetAllCampsByEventDate(eventDate, includeTalks);
        
        return Ok(_mapper.Map<CampModel[]>(result));
    }
    catch (Exception ex)
    {
        // TODO Add logging
        return InternalServerError(ex);
    }
}

```

passing querystring in postman
```
http://localhost:56556/api/camps/searchByDate/2018-10-18
```

## Modifying Data

### Step 11 - URI Design

|Resource return type| GET | POST | PUT | DELETE |
|---|---|---|---|---|
|/customers|List|New item|Status code only|Status code only(Error)|
|/customers/123|Item|Status code only(Error)|Updated item|Status code only|

### Step 12 - Model binding

```c#
[Route()]
public async Task<IHttpActionResult> Post(CampModel model)
{
    return   null;
}
```

### Step 13 - Implementng POST

```c#
[Route()]
public async Task<IHttpActionResult> Post(CampModel model)
{
    try
    {
        if (ModelState.IsValid)
        {
            // Mapping CampModel to Camp
            var camp = _mapper.Map<Camp>(model);

            // Insert to DB
            _repository.AddCamp(camp);

            // Commit to DB
            if (await _repository.SaveChangesAsync())
            {
                // Get the inserted CampModel
                var newModel = _mapper.Map<CampModel>(camp);

                // Pass to Route with new value
                return CreatedAtRoute("GetCamp",
                    new { moniker = newModel.Moniker }, newModel);
            }
        }
    }
    catch (Exception ex)
    {
        // TODO Add logging
        return InternalServerError(ex);
    }
    return BadRequest();
}
```

Add Name to route, incase if we need to redirect

```c#
[Route("{moniker}", Name = "GetCamp")]
public async Task<IHttpActionResult> Get(string moniker, bool includeTalks = false)
{
    // ...
}
 ```

Add ReverseMap in case of binding CampModel to Camp

```c#
public CampMappingProfile()
{
    CreateMap<Camp, CampModel>()
        .ForMember(c => c.Venue, opt => opt.MapFrom(m => m.Location.VenueName))
        .ReverseMap();
    // ...
}
```

### Step 14 - Adding Model validation

```c#
public class CampModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Moniker { get; set; }
    [Required]
    public DateTime EventDate { get; set; } = DateTime.MinValue;
    [Range(1,30)]
    public int Length { get; set; } = 1;

    // ...
}
```

```c#
[Route()]
public async Task<IHttpActionResult> Post(CampModel model)
{
    try
    {
        if (await _repository.GetCampAsync(model.Moniker) != null)
        {
            ModelState.AddModelError("Moniker", "Moniker in use");
        }

        if (ModelState.IsValid)
        {
            // ...
        }
    }

    return BadRequest(ModelState);
}
```

### Step 15 - Implementing PUT

```c#
[Route("{moniker}")]
public async Task<IHttpActionResult> Put(string moniker, CampModel model)
{
    try
    {
        // check moniker in DB
        Camp camp = await _repository.GetCampAsync(moniker);
        // if it is not found, send 404
        if (camp == null)
        {
            return NotFound();
        }
        // automapper map campModel to camp EF model
        _mapper.Map(model, camp);

        if (await _repository.SaveChangesAsync())
        {
            return Ok(_mapper.Map<CampModel>(camp));
        }
        else
        {
            return InternalServerError();
        }
    }
    catch (Exception ex)
    {
        // TODO Add logging
        return InternalServerError(ex);
    }
    
}
```

calling from postman
```
http://localhost:56556/api/camps/ATL2019

// with JSON object as
{
    "name": "New Code Camp",
    "moniker": "ATL2019",
    "eventDate": "2019-10-18T00:00:00",
    "length": 1,
    "talks": [],
    "venue": "New address2",
    "locationAddress1": null,
    "locationAddress2": null,
    "locationAddress3": null,
    "locationCityTown": null,
    "locationStateProvince": null,
    "locationPostalCode": null,
    "locationCountry": "IN"
}
```

### Step 15 - Implementing DELETE

```c#
[Route("{moniker}")]
public async Task<IHttpActionResult> Delete(string moniker, CampModel model)
{
    try
    {
        // check moniker in DB
        Camp camp = await _repository.GetCampAsync(moniker);
        // if it is not found, send 404
        if (camp == null)
        {
            return NotFound();
        }
        // Delete camp 
        _repository.DeleteCamp(camp);

        if (await _repository.SaveChangesAsync())
        {
            return Ok();
        }
        else
        {
            return InternalServerError();
        }
    }
    catch (Exception ex)
    {
        // TODO Add logging
        return InternalServerError(ex);
    }
}
```

calling from postman
```
http://localhost:56556/api/camps/ATL2020
```

## Association with API

For calling association data, we will use below URL design

```
/api/camps/alt2020/talks
/api/camps/alt2020/talks/1
```

### Step 16 - Creating association controller

```c#
[RoutePrefix("api/camps/{moniker}/talks")]
public class TalksController : ApiController
{
    private readonly ICampRepository _repository;
    private readonly IMapper _mapper;

    public TalksController(ICampRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [Route("{id:int}")]
    public async Task<IHttpActionResult> Get(string moniker, int id)
    {
        try
        {
            var result = await _repository.GetTalkByMonikerAsync(moniker, id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<TalkModel>(result));
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}
```

calling from postman
```
http://localhost:56556/api/camps/ATL2018/talks/1
```

## Functional API

In case we need to some below operation, then we can use Functional API's
* print job, or 
* sending email 
* starting some operation or service
* clearing cache then we can use functional api

We have to avoid functional api for Reporting because functional api should not have returning data

### Step 17 - Creating functional api

```c#
public class OpertaionsController : ApiController
{
    [HttpOptions]
    [Route("api/sendemail")]
    public IHttpActionResult SendEmail()
    {
        try
        {
            // TODO: Sending email logic

            return Ok();
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}
```

## API Versioning

Below are some examples we can implement API versioning


* URI Path (recommended) <br/>
https://foo.org/api/v2/customers

* Query string <br/>
https://foo.org/api/customers?v=2.0

* Versioning with headers<br/>
GET /api/camps <br/>
HTTP/1.1 <br/>
Host: localhost:4433 <br/>
Content-type: application/json
<b>x-version: 2.0</b>

* Versioning with Accept header<br/>
GET /api/camps <br/>
HTTP/1.1 <br/>
Host: localhost:4433 <br/>
Content-type: application/json
<b>Accept: application/json;version=2.
0</b>

* Versioning with Content type (recommended)<br/>
GET /api/camps <br/>
HTTP/1.1 <br/>
Host: localhost:4433 <br/>
<b>Content-type: application/vnd.yourapp.camp.v1+json</b>
<b>Accept: application/vnd.yourapp.camp.v1+json</b>


### Step 18 - Introducing versioning in Global 

Add Microsoft.AspNet.WebApi.Versioning via nuget

```c#
public static void Register(HttpConfiguration config)
{
    // Web API configuration and services
    AutofacConfig.Register();

    config.AddApiVersioning(cfg =>
    {
        // Versioning 1.1
        cfg.DefaultApiVersion = new Microsoft.Web.Http.ApiVersion(1, 1);
        // No need to pass in query string
        cfg.AssumeDefaultVersionWhenUnspecified = true;
        // Version can be seen in headers
        cfg.ReportApiVersions = true;
    });

    // ...
}
```

### Step 19 - Versioning in Controller

Removing versioning line for config file

```c#
public static void Register(HttpConfiguration config)
{
    // Web API configuration and services
    AutofacConfig.Register();

    config.AddApiVersioning(cfg =>
    {
        // Versioning 1.1
        // cfg.DefaultApiVersion = new Microsoft.Web.Http.ApiVersion(1, 1);
        // No need to pass in query string
        cfg.AssumeDefaultVersionWhenUnspecified = true;
        // Version can be seen in headers
        cfg.ReportApiVersions = true;
    });

    // ...
}
```

```c#
[ApiVersion("1.1")]
[RoutePrefix("api/camps")]
public class CampsController : ApiController
{
    // ...
}
```

### Step 20 - URL Versioning

```c#
public static void Register(HttpConfiguration config)
{
    // Web API configuration and services
    AutofacConfig.Register();

    config.AddApiVersioning(cfg =>
    {
        // No need to pass in query string
        cfg.AssumeDefaultVersionWhenUnspecified = true;
        // Version can be seen in headers 
        cfg.ReportApiVersions = true;
        // URL vesion 
        cfg.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    // Change case of JSON
    config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
        new CamelCasePropertyNamesContractResolver();


    var constraintResolver = new DefaultInlineConstraintResolver()
    {
        ConstraintMap =
        {
            ["apiVersion"] = typeof(ApiVersionRouteConstraint)
        }
    };

    // Web API routes
    config.MapHttpAttributeRoutes(constraintResolver);
}
```

```c#
[ApiVersion("2.0")]
[RoutePrefix("api/v{version:apiVersion}/camps")]
public class Camps2Controller : ApiController
{
    // ...
}
```

```
http://localhost:56556/api/v2/camps
```
