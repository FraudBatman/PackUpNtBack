using Postgrest.Attributes;
using Postgrest.Models;

namespace PackUpNtBack.Models
{
    [Table("package_info")]
    public class Package : BaseModel
    {
        [Column("response_id")]
        public ulong? ResponseId { get; set; }
        /// <summary>
        /// The name of the package
        /// </summary>
        [Column("package_name")]
        public string Name { get; set; }
        /// <summary>
        /// The most up-to-date version found
        /// </summary>
        [Column("current_version")]
        public string CurrentVersion { get; set; }
        /// <summary>
        /// The version currently used by the repository
        /// </summary>
        [Column("repo_version")]
        public string RepoVersion { get; set; }
        /// <summary>
        /// A link to the source of the package, if available
        /// </summary>
        [Column("source_url")]
        public string? Source { get; set; }

        public Package(string name, string repoVersion, string currentVersion, string source = null)
        {
            Name = name;
            RepoVersion = repoVersion;
            CurrentVersion = currentVersion;
            Source = source;
        }
        public Package()
        {

        }
    }

}