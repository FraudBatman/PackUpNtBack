namespace PackUpNtBack.Models
{
    public class PackageUpdateResponse
    {
        /// <summary>
        /// The origianl name of the repository
        /// </summary>
        string RepoName { get; set; }

        /// <summary>
        /// The type of the repo (.NET, NPM, etc.)
        /// </summary>
        string RepoType { get; set; }

        /// <summary>
        /// All packages that have been tested to be out-of-date. Doesn't account for user settings.
        /// </summary>
        Package[] Packages { get; set; }
    }


    // If we want to move from string in the future -NO
    // public enum RepoType
    // {
    //     DotNet,
    //     Node,
    // }
}