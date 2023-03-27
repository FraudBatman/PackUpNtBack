using Postgrest.Attributes;

namespace PackUpNtBack.Models
{
    [Table("repo_settings")]
    public class RepoSettings
    {
        [Column("repo_id")]
        ulong RepoId { get; set; }

        [Column("excluded_packages")]
        string ExcludedPackages { get; set; }

        [Column("notification_frequency")]
        string NotificationFrequency { get; set; }

        [Column("notification_day")]
        string NotificationDay { get; set; }
    }
}