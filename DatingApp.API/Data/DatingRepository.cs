using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userID, int recipientID)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerID == userID && u.LikeeID == recipientID);
        }

        public async Task<User> GetUser(int ID)
        {
            var user = await _context.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.ID == ID);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(x => x.Photos)
                .OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.ID != userParams.UserID);
            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserID, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.ID));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserID, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.ID));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDateOfBirth = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDateOfBirth = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDateOfBirth && u.DateOfBirth <= maxDateOfBirth);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int ID, bool likers)
        {
            var user = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.ID == ID);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeID == ID).Select(i => i.LikerID);
            }
            else
            {
                return user.Likees.Where(u => u.LikerID == ID).Select(i => i.LikeeID);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int ID)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.ID == ID);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                 .Include(u => u.Sender).ThenInclude(p => p.Photos)
                 .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                 .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientID == messageParams.UserID
                         && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderID == messageParams.UserID
                         && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientID == messageParams.UserID 
                        && u.RecipientDeleted == false && u.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages,
                    messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userID, int recipientID)
        {
                 var messages = await _context.Messages
                 .Include(u => u.Sender).ThenInclude(p => p.Photos)
                 .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                 .Where(m => m.RecipientID == userID && m.RecipientDeleted == false && m.SenderID == recipientID
                     || m.RecipientID == recipientID && m.SenderID == userID && m.SenderDeleted == false)
                 .OrderByDescending(m => m.MessageSent)
                 .ToListAsync();
                 
                 return messages;
        }
    }
}