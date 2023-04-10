
namespace PackUpNtBack{
public class EmailBuilder
{
    ResponseBuilder responseBuilder;
    
    public EmailBuilder(ResponseBuilder build)
    {
        responseBuilder = build;
    }

    public string makeEmail()
        {
        
                string emailTemplate = @"
        <!DOCTYPE html>
        <html>
          <head>
            <meta charset='UTF-8'>
            <title>Out-of-Date Packages</title>
          </head>
          <body>
            <h1>Out-of-Date Packages in Your Github Repos</h1>
            <p>The following repositories have out-of-date packages:</p>
            <ul>
              {0}
            </ul>
          </body>
        </html>";

                string repoList = "";

             
                
                    string packageList = "";

                    foreach (var Package in responseBuilder.packages)
                    {
                        packageList += $"<li>{Package.Name} (Version in Repo: {Package.RepoVersion}, Latest Version: {Package.CurrentVersion})</li>";
                    }

                    repoList += $"<li><strong>{responseBuilder.repoName}:</strong><ul>{packageList}</ul></li>";
                

                return string.Format(emailTemplate, repoList);
            }
        }
    }
