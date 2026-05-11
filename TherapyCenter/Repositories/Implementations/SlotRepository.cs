using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Models;
using TherapyCenter.Repositories.Interfaces;

namespace TherapyCenter.Repositories.Implementations
{
    public class SlotRepository : ISlotRepository
    {
        private readonly AppDbContext _context;

        public SlotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Slot> AddAsync(Slot slot)
        {
            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task<List<Slot>> GetAllAsync()
        {
            return await _context.Slots.ToListAsync();
        }

        public async Task<List<Slot>> GetByDoctorAndDateAsync(int doctorId, DateOnly date)
        {
            return await _context.Slots
                .Where(s => s.DoctorId == doctorId && s.Date == date)
                .ToListAsync();
        }

        public async Task<Slot?> GetByIdAsync(int id)
        {
            return await _context.Slots.FindAsync(id);
        }

        public async Task UpdateAsync(Slot slot)
        {
            _context.Slots.Update(slot);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Slot slot)
        {
            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();
        }
    }
}