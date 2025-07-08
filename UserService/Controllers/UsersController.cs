using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repository;
        public UsersController(IUserRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _repository.GetAllAsync();
            return Ok(users.Select(u => new UserDTO { Id = u.Id, Name = u.Name, Email = u.Email }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetById(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(new UserDTO { Id = user.Id, Name = user.Name, Email = user.Email });
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> Create(UserDTO userDto)
        {
            var user = new User { Name = userDto.Name, Email = userDto.Email };
            var created = await _repository.AddAsync(user);
            var result = new UserDTO { Id = created.Id, Name = created.Name, Email = created.Email };
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDTO>> Update(int id, UserDTO userDto)
        {
            if (id != userDto.Id) return BadRequest();
            var user = new User { Id = userDto.Id, Name = userDto.Name, Email = userDto.Email };
            var updated = await _repository.UpdateAsync(user);
            var result = new UserDTO { Id = updated.Id, Name = updated.Name, Email = updated.Email };
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
} 