using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WilderMinds.MetaWeblog;

namespace Miniblog.Core.Services
{
    public class MetaWeblogService : IMetaWeblogProvider
    {
        private readonly IBlogService _blog;
        private readonly IConfiguration _config;
        private readonly IUserServices _userServices;
        private readonly IHttpContextAccessor _context;

        public MetaWeblogService(IBlogService blog, IConfiguration config, IHttpContextAccessor context, IUserServices userServices)
        {
            _blog = blog;
            _config = config;
            _userServices = userServices;
            _context = context;
        }

        public string AddPost(string blogid, string username, string password, Post post, bool publish)
        {
            this.ValidateUser(username, password);

            var newPost = new Models.Post
            {
                Title = post.title,
                Slug = !string.IsNullOrWhiteSpace(post.wp_slug) ? post.wp_slug : Models.Post.CreateSlug(post.title),
                Content = post.description,
                IsPublished = publish,
                Categories = post.categories
            };

            if (post.dateCreated != DateTime.MinValue)
            {
                newPost.PubDate = post.dateCreated;
            }

            _blog.SavePost(newPost).GetAwaiter().GetResult();

            return newPost.ID;
        }

        public bool DeletePost(string key, string postid, string username, string password, bool publish)
        {
            this.ValidateUser(username, password);

            var post = _blog.GetPostById(postid).GetAwaiter().GetResult();

            if (post != null)
            {
                _blog.DeletePost(post).GetAwaiter().GetResult();
                return true;
            }

            return false;
        }

        public bool EditPost(string postid, string username, string password, Post post, bool publish)
        {
            this.ValidateUser(username, password);

            var existing = _blog.GetPostById(postid).GetAwaiter().GetResult();

            if (existing != null)
            {
                existing.Title = post.title;
                existing.Slug = post.wp_slug;
                existing.Content = post.description;
                existing.IsPublished = publish;
                existing.Categories = post.categories;

                if (post.dateCreated != DateTime.MinValue)
                {
                    existing.PubDate = post.dateCreated;
                }

                _blog.SavePost(existing).GetAwaiter().GetResult();

                return true;
            }

            return false;
        }

        public CategoryInfo[] GetCategories(string blogid, string username, string password)
        {
            this.ValidateUser(username, password);

            return _blog.GetCategories().GetAwaiter().GetResult()
                           .Select(cat =>
                               new CategoryInfo
                               {
                                   categoryid = cat,
                                   title = cat
                               })
                           .ToArray();
        }

        public Post GetPost(string postid, string username, string password)
        {
            this.ValidateUser(username, password);

            var post = _blog.GetPostById(postid).GetAwaiter().GetResult();

            if (post != null)
            {
                return this.ToMetaWebLogPost(post);
            }

            return null;
        }

        public Post[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
        {
            this.ValidateUser(username, password);

            return _blog.GetPosts(numberOfPosts).GetAwaiter().GetResult().Select(this.ToMetaWebLogPost).ToArray();
        }

        public BlogInfo[] GetUsersBlogs(string key, string username, string password)
        {
            this.ValidateUser(username, password);

            var request = _context.HttpContext.Request;
            string url = request.Scheme + "://" + request.Host;

            return new[] { new BlogInfo {
                blogid ="1",
                blogName = _config["blog:name"] ?? nameof(MetaWeblogService),
                url = url
            }};
        }

        public MediaObjectInfo NewMediaObject(string blogid, string username, string password, MediaObject mediaObject)
        {
            this.ValidateUser(username, password);
            byte[] bytes = Convert.FromBase64String(mediaObject.bits);
            string path = _blog.SaveFile(bytes, mediaObject.name).GetAwaiter().GetResult();

            return new MediaObjectInfo { url = path };
        }

        public UserInfo GetUserInfo(string key, string username, string password)
        {
            this.ValidateUser(username, password);
            throw new NotImplementedException();
        }

        public int AddCategory(string key, string username, string password, NewCategory category)
        {
            this.ValidateUser(username, password);
            throw new NotImplementedException();
        }

        private void ValidateUser(string username, string password)
        {
            if (_userServices.ValidateUser(username, password)==false)
            {
                throw new MetaWeblogException("Unauthorized");
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, username));

            _context.HttpContext.User = new ClaimsPrincipal(identity);
        }

        private Post ToMetaWebLogPost(Models.Post post)
        {
            var request = _context.HttpContext.Request;
            string url = request.Scheme + "://" + request.Host;

            return new Post
            {
                postid = post.ID,
                title = post.Title,
                wp_slug = post.Slug,
                permalink = url + post.GetLink(),
                dateCreated = post.PubDate,
                description = post.Content,
                categories = post.Categories.ToArray()
            };
        }

        public Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetPostAsync(string postid, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
        {
            throw new NotImplementedException();
        }

        public Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditPostAsync(string postid, string username, string password, Post post, bool publish)
        {
            throw new NotImplementedException();
        }

        public Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
        {
            throw new NotImplementedException();
        }

        public Task<Tag[]> GetTagsAsync(string blogid, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
        {
            throw new NotImplementedException();
        }

        public Task<Page> GetPageAsync(string blogid, string pageid, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages)
        {
            throw new NotImplementedException();
        }

        public Task<Author[]> GetAuthorsAsync(string blogid, string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page, bool publish)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid)
        {
            throw new NotImplementedException();
        }
    }
}
