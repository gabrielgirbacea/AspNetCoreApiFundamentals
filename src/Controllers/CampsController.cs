using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampDto[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsAsync(includeTalks);
                return mapper.Map<CampDto[]>(results);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampDto>> Get(string moniker)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker);
                if (result == null)
                {
                    return NotFound();
                }

                return mapper.Map<CampDto>(result);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampDto[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any())
                {
                    return NotFound();
                }

                return mapper.Map<CampDto[]>(results);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampDto>> Post(CampDto campDto)
        {
            try
            {
                var existing = await repository.GetCampAsync(campDto.Moniker);
                if (existing != null)
                {
                    return BadRequest("Moniker in Use");
                }

                var location = linkGenerator.GetPathByAction(
                    "Get",
                    "Camps",
                    new { moniker = campDto.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                // Create a new Camp
                var camp = mapper.Map<Camp>(campDto);
                repository.Add(camp);

                if (await repository.SaveChangesAsync())
                {
                    return Created(location, mapper.Map<CampDto>(camp));

                }

            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }


        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampDto>> Put(string moniker, CampDto campDto)
        {
            try
            {
                var oldCamp = await repository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }

                mapper.Map(campDto, oldCamp);


                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<CampDto>(oldCamp);

                }

            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await repository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }

                repository.Delete(oldCamp);

                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

    }

}
