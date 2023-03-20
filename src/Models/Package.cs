namespace PackUpNtBack.Models
{
    public class Package
    {
        /// <summary>
        /// The name of the package
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// The most up-to-date version found
        /// </summary>
        string CurrentVersion { get; set; }
        /// <summary>
        /// The version currently used by the repository
        /// </summary>
        string RepoVersion { get; set; }
        /// <summary>
        /// A link to the source of the package, if available
        /// </summary>
        string? Source { get; set; }

    }

}