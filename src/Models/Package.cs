using Postgrest.Attributes;
using Postgrest.Models;

namespace PackUpNtBack.Models
{
    [Table("package_info")]
    public class Package
    {
        /// <summary>
        /// The name of the package
        /// </summary>
        [Column("package_name")]
        string Name { get; set; }
        /// <summary>
        /// The most up-to-date version found
        /// </summary>
        [Column("current_version")]
        string CurrentVersion { get; set; }
        /// <summary>
        /// The version currently used by the repository
        /// </summary>
        [Column("repo_version")]
        string RepoVersion { get; set; }
        /// <summary>
        /// A link to the source of the package, if available
        /// </summary>
        [Column("source_url")]
        string? Source { get; set; }

    }

}