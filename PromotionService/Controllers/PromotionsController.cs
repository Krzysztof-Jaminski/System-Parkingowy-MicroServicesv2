using Microsoft.AspNetCore.Mvc;
using PromotionService.Models;
using PromotionService.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PromotionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly PromotionRepository _repo;
        public PromotionsController(PromotionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionDTO>>> GetAll()
        {
            var promotions = await _repo.GetAllAsync();
            return Ok(promotions.Select(p => new PromotionDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DiscountPercent = p.DiscountPercent,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                MinHours = p.MinHours
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionDTO>> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(new PromotionDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DiscountPercent = p.DiscountPercent,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                MinHours = p.MinHours
            });
        }

        [HttpPost]
        public async Task<ActionResult<PromotionDTO>> Create(PromotionDTO dto)
        {
            var p = new Promotion
            {
                Name = dto.Name,
                Description = dto.Description,
                DiscountPercent = dto.DiscountPercent,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                MinHours = dto.MinHours
            };
            var created = await _repo.AddAsync(p);
            dto.Id = created.Id;
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PromotionDTO dto)
        {
            if (id != dto.Id) return BadRequest();
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound();
            p.Name = dto.Name;
            p.Description = dto.Description;
            p.DiscountPercent = dto.DiscountPercent;
            p.ValidFrom = dto.ValidFrom;
            p.ValidTo = dto.ValidTo;
            p.MinHours = dto.MinHours;
            await _repo.UpdateAsync(p);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<PromotionDTO>> GetByCode(string code)
        {
            var promotions = await _repo.GetAllAsync();
            var p = promotions.FirstOrDefault(x => x.Name == code);
            if (p == null) return NotFound();
            return Ok(new PromotionDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DiscountPercent = p.DiscountPercent,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                MinHours = p.MinHours
            });
        }
    }
} 