using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Controllers.DTOs;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserID);

            // var userMapped = _mapper.Map<UserForDetailedDTO>(userFromRepo);

            // var camelCaseFormatter = new JsonSerializerSettings();
            // camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            // Response.Headers.Add("user", JsonConvert.SerializeObject(userMapped, camelCaseFormatter));
       
            userParams.UserID = currentUserID;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userParams.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{ID}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int ID)
        {
            var user = await _repo.GetUser(ID);

            var userToReturn = _mapper.Map<UserForDetailedDTO>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdateUser(int ID, UserForUpdateDTO userForUpdateDTO)
        {
            if (ID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _repo.GetUser(ID);

            _mapper.Map(userForUpdateDTO, userFromRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {ID} failed on save");
        }

    }
}