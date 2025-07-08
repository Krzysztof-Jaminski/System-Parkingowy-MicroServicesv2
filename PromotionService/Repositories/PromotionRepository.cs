using Microsoft.EntityFrameworkCore;
using PromotionService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PromotionService.Repositories
{
    public class PromotionRepository
    {
        private readonly PromotionDbContext _context;
        public PromotionRepository(PromotionDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetAllAsync() => await _context.Promotions.ToListAsync();
        public async Task<Promotion> GetByIdAsync(int id) => await _context.Promotions.FindAsync(id);
        public async Task<Promotion> AddAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }
        public async Task<bool> UpdateAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Promotions.FindAsync(id);
            if (entity == null) return false;
            _context.Promotions.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 