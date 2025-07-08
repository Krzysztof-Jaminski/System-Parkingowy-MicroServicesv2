using Microsoft.AspNetCore.Mvc;
using ReservationService.Models;
using ReservationService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ReservationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _userServiceUrl;
        private readonly string _promotionServiceUrl;
        public ReservationsController(IReservationRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _userServiceUrl = configuration["ExternalServices:UserService"];
            _promotionServiceUrl = configuration["ExternalServices:PromotionService"];
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDTO>>> GetAll()
        {
            try
            {
                var reservations = await _repository.GetAllAsync();
                return Ok(reservations.Select(r => new ReservationDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ParkingSpot = r.ParkingSpot,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    PromotionCode = r.PromotionCode,
                    Cost = r.Cost
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDTO>> GetById(int id)
        {
            try
            {
                var r = await _repository.GetByIdAsync(id);
                if (r == null) return NotFound();
                return Ok(new ReservationDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ParkingSpot = r.ParkingSpot,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    PromotionCode = r.PromotionCode,
                    Cost = r.Cost
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDTO>> Create([FromBody] ReservationDTO dto)
        {
            try
            {
                // Synchroniczne sprawdzenie Usera
                var client = _httpClientFactory.CreateClient();
                var userResp = await client.GetAsync($"{_userServiceUrl}/api/users/{dto.UserId}");
                if (!userResp.IsSuccessStatusCode) return BadRequest("User not found");
                // Synchroniczne sprawdzenie promocji (jeśli podano)
                decimal baseRate = 10m;
                decimal discount = 0m;
                if (!string.IsNullOrEmpty(dto.PromotionCode))
                {
                    var promoResp = await client.GetAsync($"{_promotionServiceUrl}/api/promotions/code/{dto.PromotionCode}");
                    if (!promoResp.IsSuccessStatusCode) return BadRequest("Promotion not found");
                    var promo = await promoResp.Content.ReadFromJsonAsync<PromotionDTO>();
                    var hours = (dto.EndTime - dto.StartTime).TotalHours;
                    if (promo.MinHours > 0 && hours < promo.MinHours)
                        return BadRequest($"Reservation does not meet promotion minimum hours: {promo.MinHours}");
                    discount = (decimal)promo.DiscountPercent;
                }
                var totalHours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
                var cost = baseRate * totalHours * (1 - discount / 100);
                var reservation = new Reservation
                {
                    UserId = dto.UserId,
                    ParkingSpot = dto.ParkingSpot,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    PromotionCode = dto.PromotionCode,
                    Cost = cost
                };
                var created = await _repository.AddAsync(reservation);
                dto.Id = created.Id;
                dto.Cost = created.Cost;
                return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ReservationDTO>> Update(int id, [FromBody] ReservationDTO dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest();
                // Synchroniczne sprawdzenie Usera
                var client = _httpClientFactory.CreateClient();
                var userResp = await client.GetAsync($"{_userServiceUrl}/api/users/{dto.UserId}");
                if (!userResp.IsSuccessStatusCode) return BadRequest("User not found");
                // Synchroniczne sprawdzenie promocji (jeśli podano)
                decimal baseRate = 10m;
                decimal discount = 0m;
                if (!string.IsNullOrEmpty(dto.PromotionCode))
                {
                    var promoResp = await client.GetAsync($"{_promotionServiceUrl}/api/promotions/code/{dto.PromotionCode}");
                    if (!promoResp.IsSuccessStatusCode) return BadRequest("Promotion not found");
                    var promo = await promoResp.Content.ReadFromJsonAsync<PromotionDTO>();
                    var hours = (dto.EndTime - dto.StartTime).TotalHours;
                    if (promo.MinHours > 0 && hours < promo.MinHours)
                        return BadRequest($"Reservation does not meet promotion minimum hours: {promo.MinHours}");
                    discount = (decimal)promo.DiscountPercent;
                }
                var totalHours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
                var cost = baseRate * totalHours * (1 - discount / 100);
                var reservation = new Reservation
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    ParkingSpot = dto.ParkingSpot,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    PromotionCode = dto.PromotionCode,
                    Cost = cost
                };
                var updated = await _repository.UpdateAsync(reservation);
                dto.Cost = cost;
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
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