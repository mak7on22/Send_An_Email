using Microsoft.Extensions.Caching.Memory;
using TDList.Models;

namespace TDList.Servises
{
    public class UserService
    {
        private readonly TDLContext context;
        private readonly IMemoryCache cache;

        public UserService(TDLContext context, IMemoryCache cache)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        //public async Task<IEnumerable<User>> GetUsers()
        //{
        //    return await context.Users.ToListAsync();
        //}
        //public async Task AddUser(User user)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException(nameof(user));
        //    await context.Users.AddAsync(user);
        //    await context.SaveChangesAsync();
        //    cache.Set(user.Id, user, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        //}

        //public async Task<User?> GetUser(int id)
        //{
        //    if (!cache.TryGetValue(id, out User? user))
        //    {
        //        user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        //        if (user != null)
        //            cache.Set(user.Id, user, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        //    }
        //    return user;
        //}
    }
}
