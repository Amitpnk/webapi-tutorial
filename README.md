# customerapp-webapi
 Creating webapi entity-framework automapper autofac dependency-injection

## Table of Contents

- [Sending Feedback](#sending-feedback)
- [About Web API](#about-entity-framework-core)
- [Sample application with each labs](#sample-application-with-each-steps)
    - Creating WCF application
        - [Step 1 - Create Application](#step-1---create-application)
    - Creating Get API
        - [Step 2 - Adding Data contract](#step-2---adding-data-contract)
        - [Step 3 - Adding Service contract and Operation contract in interface](#step-3---adding-service-contract-and-operation-contract-in-interface)
        - [Step 4 - Adding EntityFramework and DbContext](#step-4---adding-entityframework-and-dbcontext)
        - [Step 5 - Service Implementation](#step-5---service-implementation)
    - Hosting Services

## Sending Feedback

For feedback can drop mail to my email address amit.naik8103@gmail.com or you can create [issue](https://github.com/Amitpnk/angular-application/issues/new)

## About Web API

* Windows Communication Foundation 
    * Basically it is platform for building distributed service-orieted application
        * Defines services and host for those services
        * Defines clients to connect to services
* SOAP Messaging
    * It is XML based protocol information at a wire-level
    * WCF is messaging system 



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

### Step 6 - SerialiZation in Web API

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

