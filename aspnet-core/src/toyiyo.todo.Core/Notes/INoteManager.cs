using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Notes
{
    public interface INoteManager
    {
        Task<Note> Get(Guid id);
        Task<List<Note>> GetAll(GetAllNotesInput input);
        Task<int> GetAllCount(GetAllNotesInput input);
        Task<Note> Create(Note inputNote);
        Task<Note> Update(Note inputNote);
        Task Delete(Guid id, User user);
    }
}
