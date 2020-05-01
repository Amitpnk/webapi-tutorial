using AutoMapper;
using Customer.API.Data;
using Customer.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Customer.API.Controllers
{
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
}
