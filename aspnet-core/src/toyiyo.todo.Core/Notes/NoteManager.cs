using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Notes
{
    public class NoteManager : DomainService, INoteManager
    {
        private readonly IRepository<Note, Guid> _noteRepository;

        public NoteManager(IRepository<Note, Guid> noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<Note> Get(Guid id)
        {
            return await _noteRepository.GetAll()
                .Include(n => n.Author)
                .Include(n => n.Job)
                .Include(n => n.Replies)
                .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        [UnitOfWork]
        public async Task<List<Note>> GetAll(GetAllNotesInput input)
        {
            var query = GetAllNotesQueryable(input);
            
            return await query
                .Include(n => n.Author)
                .Include(n => n.Replies)
                .ThenInclude(r => r.Author)
                .OrderByDescending(n => n.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();
        }

        [UnitOfWork]
        public async Task<int> GetAllCount(GetAllNotesInput input)
        {
            return await GetAllNotesQueryable(input).CountAsync();
        }

        private IQueryable<Note> GetAllNotesQueryable(GetAllNotesInput input)
        {
            var query = _noteRepository.GetAll()
                .Where(n => n.JobId == input.JobId && n.ParentNoteId == null);

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(n => n.Content.Contains(input.Keyword));
            }

            return query;
        }

        [UnitOfWork]
        public async Task<Note> Create(Note inputNote)
        {
            return await _noteRepository.InsertAsync(inputNote);
        }

        public async Task<Note> Update(Note inputNote)
        {
            return await _noteRepository.UpdateAsync(inputNote);
        }

        public async Task Delete(Guid id, User user)
        {
            var note = await Get(id);
            if (note == null) throw new ArgumentNullException(nameof(id));

            // Call the domain logic to perform validations
            note = Note.Delete(note, user);

            await _noteRepository.DeleteAsync(note);
        }
    }
}
