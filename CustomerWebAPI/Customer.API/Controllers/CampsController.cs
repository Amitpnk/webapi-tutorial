using AutoMapper;
using Customer.API.Data;
using Customer.API.Data.Entities;
using Customer.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Customer.API.Controllers
{
    [RoutePrefix("api/camps")]
    public class CampsController : ApiController
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository campRepository, IMapper mapper)
        {
            _repository = campRepository;
            _mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(bool includeTalks = false)
        {
            try
            {
                // Decoupling dal
                var result = await _repository.GetAllCampsAsync(includeTalks);

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

        [Route("{moniker}", Name = "GetCamp")]
        public async Task<IHttpActionResult> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                // Decoupling dal
                var result = await _repository.GetCampAsync(moniker, includeTalks);

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
            return BadRequest(ModelState);
        }

    }
}
