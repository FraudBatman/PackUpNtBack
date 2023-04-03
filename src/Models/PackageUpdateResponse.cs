using Postgrest.Attributes;
using Postgrest.Models;

namespace PackUpNtBack.Models
{
    [Table("backend_reponse")]
    public class PackageUpdateResponse : BaseModel
    {
        [PrimaryKey("id")]
        public ulong? ID { get; set; }

        [Column("repo_id")]
        public ulong? RepoId { get; set; }

        /// <summary>
        /// The type of the repo (.NET, NPM, etc.)
        /// </summary>

        [Column("repo_type")]
        public string RepoType { get; set; }
    }


    // If we want to move from string in the future -NO
    // public enum RepoType
    // {
    //     DotNet,
    //     Node,
    // }
}