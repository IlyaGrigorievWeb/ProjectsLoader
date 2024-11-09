namespace ProjectsLoader.Models
{
    public class GitHubApiResponse
    {
        public int stargazers_count { get; set; }
        public DateTime created_at { get; set; }
        public string default_branch { get; set; }
    }
}
