using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository
    {
        //private readonly DataContext _context;

        //public DatingRepository(DataContext context)
        //{
        //    _context = context;
        //}

        //public void Add<T>(T entity) where T : class
        //{
        //    _context.Add(entity);
        //}

        //public void Delete<T>(T entity) where T : class
        //{
        //    _context.Remove(entity);
        //}

        //// Get one user
        //public async Task<User> GetUser(int id)
        //{
        //    var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

        //    return user;
        //}

        //// Get all users in pagination
        //public async Task<PagedList<User>> GetUsers(UserParams userParams)
        //{
        //    var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
        //    users = users.Where(u => u.Id != userParams.UserId);

        //    if (!string.IsNullOrEmpty(userParams.Name))
        //    {
        //        users = users.Where(u => u.Name.ToLower().Contains(userParams.Name));
        //    }
        //    if (!string.IsNullOrEmpty(userParams.Verification))
        //    {
        //        users = users.Where(u => userParams.Verification == "true" ? u.Verified == true : u.Verified == false);
        //    }
        //    if (!string.IsNullOrEmpty(userParams.Status))
        //    {
        //        users = users.Where(u => u.Status == userParams.Status);
        //    }
        //    if (userParams.Gender != "any")
        //    {
        //        users = users.Where(u => u.Gender == userParams.Gender);
        //    }
        //    if (userParams.Likers)
        //    {
        //        var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);

        //        users = users.Where(u => userLikers.Contains(u.Id));
        //    }
        //    if (userParams.Likees)
        //    {
        //        var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);

        //        users = users.Where(u => userLikees.Contains(u.Id));
        //    }
        //    if (userParams.MinAge != 18 || userParams.MaxAge != 99)
        //    {
        //        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        //        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

        //        users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
        //    }
        //    if (!string.IsNullOrEmpty(userParams.OrderBy))
        //    {
        //        switch (userParams.OrderBy)
        //        {
        //            case "name":
        //                users = users.OrderBy(u => u.Name);
        //                break;
        //            case "gender":
        //                users = users.OrderByDescending(u => u.Gender);
        //                break;
        //            case "age":
        //                users = users.OrderByDescending(u => u.DateOfBirth);
        //                break;
        //            case "email":
        //                users = users.OrderByDescending(u => u.Email);
        //                break;
        //            case "phone":
        //                users = users.OrderByDescending(u => u.Phone);
        //                break;
        //            case "created":
        //                users = users.OrderByDescending(u => u.Created);
        //                break;
        //            case "verification":
        //                users = users.OrderByDescending(u => u.Verified);
        //                break;
        //            default:
        //                users = users.OrderByDescending(u => u.LastActive);
        //                break;
        //        }
        //    }

        //    return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        //}

        //// Get number of users by status
        //public async Task<int[]> GetNumberOfUsersByStatus()
        //{
        //    var activeCount = await _context.Users.Where(u => u.Status == "active" && u.Role != "admin").CountAsync();
        //    var disalbedCount = await _context.Users.Where(u => u.Status == "disabled" && u.Role != "admin").CountAsync();
        //    var deletedCount = await _context.Users.Where(u => u.Status == "deleted" && u.Role != "admin").CountAsync();

        //    return new int[] { activeCount, disalbedCount, deletedCount };
        //}

        //// Get likes from sender or recipient
        //private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        //{
        //    var user = await _context.Users.Include(x => x.Likers).Include(x => x.Likees).FirstOrDefaultAsync(u => u.Id == id);

        //    if (likers)
        //    {
        //        return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
        //    }
        //    else
        //    {
        //        return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
        //    }
        //}

        //// Check if any changes in databse
        //public async Task<bool> SaveAll()
        //{
        //    return await _context.SaveChangesAsync() > 0;
        //}

        //// Get all photos of a user
        //public async Task<Photo> GetPhoto(int id)
        //{
        //    var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

        //    return photo;
        //}

        //// Get main photo of a user
        //public async Task<Photo> GetMainPhotoForUser(int userId)
        //{
        //    return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        //}

        //// Return a like object if this user has liked another one
        //public async Task<Like> GetLike(int userId, int recipientId)
        //{
        //    return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        //}

        //public async Task<Message> GetMessage(int id)
        //{
        //    return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        //}

        //// GET: Get sent messages or received ones
        //public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        //{
        //    var messages = _context.Messages
        //        .Include(u => u.Sender).ThenInclude(p => p.Photos)
        //        .Include(u => u.Recipient).ThenInclude(p => p.Photos).AsQueryable();

        //    switch (messageParams.MessageContainer)
        //    {
        //        case "Inbox":
        //            messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
        //            break;
        //        case "Outbox":
        //            messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
        //            break;
        //        default:
        //            messages = messages.Where(u => u.RecipientId == messageParams.UserId
        //            && u.RecipientDeleted == false
        //            && u.IsRead == false);
        //            break;
        //    }

        //    messages = messages.OrderByDescending(d => d.MessageSent);

        //    return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        //}

        //// GET: Get message thread
        //public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        //{
        //    var messages = await _context.Messages
        //        .Include(u => u.Sender).ThenInclude(p => p.Photos)
        //        .Include(u => u.Recipient).ThenInclude(p => p.Photos)
        //        .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId
        //        || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
        //        .OrderByDescending(m => m.MessageSent).ToListAsync();

        //    return messages;
        //}
    }
}
