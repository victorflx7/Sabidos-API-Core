using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // ADICIONADO: Para usar Task

namespace SabidosAPI_Core.Services
{
    public class FlashcardService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public FlashcardService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<FlashcardResponseDto>> GetAllFlashcardsAsync(string? userUId = null)
        {
            var query = _context.Flashcards.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(userUId))
                query = query.Where(p => p.AuthorUid == userUId);

            var flashcards = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return _mapper.Map<List<FlashcardResponseDto>>(flashcards);
        }

        public async Task<FlashcardResponseDto?> GetFlashcardByIdAsync(int flashcardId)
        {
            var flashcard = await _context.Flashcards
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == flashcardId);
            if (flashcard == null) return null;
            return _mapper.Map<FlashcardResponseDto>(flashcard);
        }

        public async Task<int> GetFlashcardsCountByUserAsync(string authorUid)
        {
            return await _context.Flashcards.CountAsync(e => e.AuthorUid == authorUid);
        }

        public async Task<FlashcardResponseDto> CreateFlashcardAsync(FlashcardCreateUpdateDto flashcardDto, string authorUid, string nameAuthor)
        {
            var flashcard = _mapper.Map<Flashcard>(flashcardDto); // CORREÇÃO: Mapeando para Flashcard
            _context.Flashcards.Add(flashcard); // CORREÇÃO: Usando DbSet Flashcards

            // Atribuições que o service deve garantir
            flashcard.AuthorUid = authorUid;
            flashcard.AuthorName = nameAuthor;
            flashcard.CreatedAt = DateTime.UtcNow;
            flashcard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<FlashcardResponseDto>(flashcard);
        }

        public async Task<FlashcardResponseDto?> UpdateFlashcardAsync(int flashcardId, FlashcardCreateUpdateDto dto)
        {
            var flashcard = await _context.Flashcards
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == flashcardId);

            if (flashcard == null) return null;

            _mapper.Map(dto, flashcard);
            flashcard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<FlashcardResponseDto>(flashcard);
        }


        public async Task<bool> DeleteFlashcardAsync(int flashcardId)
        {
            var flashcard = await _context.Flashcards.FirstOrDefaultAsync(p => p.Id == flashcardId);
            if (flashcard == null) return false;

            _context.Flashcards.Remove(flashcard);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}