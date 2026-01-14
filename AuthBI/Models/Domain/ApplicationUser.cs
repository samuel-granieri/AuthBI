using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;


namespace AuthBI.Models.Domain
{
    public class ApplicationUser: MongoIdentityUser<Guid>
    {
        public string TenantId { get; set; }
    }
}
