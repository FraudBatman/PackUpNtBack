using Postgrest.Attributes;
using Postgrest.Models;

namespace PackUpNtBack.Models
{
    [Table("identities")]
    public class SupabaseIdentity : BaseModel
    {
        [PrimaryKey("id")]
        public int ID { get; set; }

        [PrimaryKey("provider")]
        public string Provider { get; set; }

        [Column("user_id")]
        public string UserID { get; set; }

        [Column("identity_data")]
        public string IdentityData { get; set; }
    }
}