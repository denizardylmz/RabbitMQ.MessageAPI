using MessageAPI.Abstractions;
using MessageAPI.Infrastructure.Interceptors;

namespace MessageAPI.API.Infrastructure
{
    public class HttpCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;


        public HttpCurrentUser(IHttpContextAccessor  httpContextAccessor)
        {
                _httpContextAccessor = httpContextAccessor;
        }

        public string? Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}
