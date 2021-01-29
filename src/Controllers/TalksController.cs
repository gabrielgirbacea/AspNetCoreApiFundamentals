using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/[controller]")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkDto[]>> Get(string moniker)
        {
            try
            {
                var talks = await repository.GetTalksByMonikerAsync(moniker, true);
                return mapper.Map<TalkDto[]>(talks);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkDto>> Get(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);
                
                if(talk == null)
                {
                    return NotFound();
                }
                
                return mapper.Map<TalkDto>(talk);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkDto>> Post(string moniker, TalkDto talkDto)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return BadRequest("Camp does not exist");
                }

                var talk = mapper.Map<Talk>(talkDto);
                talk.Camp = camp;

                if (talkDto.Speaker == null)
                {
                    return BadRequest("Speaker ID is required");
                }

                var speaker = await repository.GetSpeakerAsync(talkDto.Speaker.SpeakerId);
                if (speaker == null)
                {
                    return BadRequest("Speaker could not be found");
                }

                talk.Speaker = speaker;

                repository.Add(talk);

                if (await repository.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction(
                        HttpContext,
                        "Get",
                        values: new { moniker, id = talk.TalkId });

                    return Created(url, mapper.Map<TalkDto>(talk));
                }
                else
                {

                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult<TalkDto>> Put(string moniker, int id, TalkDto talkDto)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null)
                {
                    return BadRequest("Cannot find talk.");
                }

                if (talkDto.Speaker != null)
                {
                    var speaker = await repository.GetSpeakerAsync(talkDto.Speaker.SpeakerId);

                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<TalkDto>(talk);
                }
                else
                {
                    return BadRequest("Failed to update database.");
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var oldTalk = await repository.GetTalkByMonikerAsync(moniker, id);
                if (oldTalk == null)
                {
                    return NotFound($"Could not find talk with id {id} for camp with moniker of {moniker}");
                }

                repository.Delete(oldTalk);

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
