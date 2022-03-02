using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CommandsService.Controllers
{
    [ApiController]
    [Route("api/c/platforms/{platformId}/[controller]")]
    public class CommandsController: ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsByPlatform(int platformId)
        {
            Console.WriteLine($"--> Getting Commands by Platform: {platformId}");

            if (!_repository.PlatformExists(platformId))
                return NotFound();       

            var commands = _repository.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"--> Getting Command: {commandId} for Platform: {platformId}");

            if (!_repository.PlatformExists(platformId))
                return NotFound();
            

            var command = _repository.GetCommand(platformId, commandId);

            if (command == null)
                return NotFound();

            return Ok(_mapper.Map<CommandReadDto>(command));
        }
    }
}
