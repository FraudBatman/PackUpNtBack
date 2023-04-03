using Postgrest.Attributes;
using Postgrest.Models;

namespace PackUpNtBack.Models
{
    [Table("backend_response")]
    public class PackageUpdateResponse
    {
        [PrimaryKey("id")]
        public ulong ID { get; set; }

        /// <summary>
        /// The origianl name of the repository
        /// </summary>
        public string RepoName { get; set; }

        [Column("repo_id")]
        public ulong? RepoId { get; set; }

        /// <summary>
        /// The type of the repo (.NET, NPM, etc.)
        /// </summary>

        [Column("repo_type")]
        public string RepoType { get; set; }

        /// <summary>
        /// All packages that have been tested to be out-of-date. Doesn't account for user settings.
        /// </summary>
        public Package[] Packages { get; set; }
    }


    // If we want to move from string in the future -NO
    // public enum RepoType
    // {
    //     DotNet,
    //     Node,
    // }
}