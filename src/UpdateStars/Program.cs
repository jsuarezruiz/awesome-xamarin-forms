using Octokit;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UpdateStars
{
    class Program
    {
        const string AppName = "xamarin-update-stars";
        const string GitHubToken = "INSERT YOUR GITHUB TOKEN HERE";

        const string UrlRegex = @"https?:\/\/github.com\/([A-Za-z0-9-.]+\/[A-Za-z0-9-.]+)";

        static void Main(string[] args)
        {
            Console.WriteLine("Star Updater");

            Console.WriteLine("Initialize");

            // Initialize the GitHub client
            var client = new GitHubClient(new ProductHeaderValue(AppName));
            var tokenAuth = new Credentials(GitHubToken);
            client.Credentials = tokenAuth;

            // Read the README.md file
            var filePath = "../../../../../README.md";
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            Regex regex = new Regex(UrlRegex, RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                MatchCollection matches = regex.Matches(line);

                if (matches.Count > 0)
                {
                    // Get repository info (owner and name)
                    var url = matches[0].Value;
                    Uri.TryCreate(url, UriKind.Absolute, out Uri uri);

                    if (uri == null)
                        break;

                    var path = uri.PathAndQuery;
                    var t1 = path.Split("/");
                    var owner = t1[1];
                    var name = t1[2];

                    try
                    {
                        // Get repository stars
                        Repository repository = client.Repository.Get(owner, name).Result;
                        var stars = repository.StargazersCount;

                        // Update README.md
                        UpdateStars(filePath, line, name, stars);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                            Console.WriteLine($"An error has ocurred updating {name}. More information: {ex.InnerException.Message}");
                        else
                            Console.WriteLine($"An error has ocurred updating {name}. More information: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("README.md Updated!");
        }

        static void UpdateStars(string filePath, string line, string name, int stars)
        {
            if (stars == 0)
                return;

            Console.WriteLine($"Updating {name} to add {stars} stars");

            string text = File.ReadAllText(filePath);

            var newLine = line;

            // Remove stars (if exists)
            if (line.Contains("★"))
            {
                var starText = line.Substring(0, line.IndexOf(']'));
                var starIndex = starText.LastIndexOf(" ");
                var starCount = starText.Length - starIndex;
                newLine = line.Remove(starIndex, starCount);
            }

            // Add updated stars
            var index = newLine.IndexOf(']');
            newLine = newLine.Insert(index, $" ★{stars}");
            text = text.Replace(line, newLine);

            File.WriteAllText(filePath, text);
        }
    }
}