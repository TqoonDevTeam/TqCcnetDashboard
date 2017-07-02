using Octokit;

namespace TqLib.Utils
{
    public class GitHubUtil
    {
        private const string HeaderValue = "Awesome-Octocat-App-TqoonDevTeam";
        private Credentials _Credentials;
        private ProductHeaderValue _Header;

        public GitHubClient Client { get; private set; }

        public GitHubUtil() : this(null, null)
        {
        }

        public GitHubUtil(string token) : this(token, null)
        {
        }

        public GitHubUtil(string userid, string userpw)
        {
            if (!string.IsNullOrEmpty(userid) && string.IsNullOrEmpty(userpw))
            {
                _Credentials = new Credentials(userid);
            }
            else if (!string.IsNullOrEmpty(userid) && !string.IsNullOrEmpty(userpw))
            {
                _Credentials = new Credentials(userid, userpw);
            }
            else
            {
                _Credentials = null;
            }
            _Header = new ProductHeaderValue(HeaderValue);
            Client = GitHubClient();
        }

        private GitHubClient GitHubClient()
        {
            var client = new GitHubClient(_Header);
            if (_Credentials != null)
            {
                client.Credentials = _Credentials;
            }
            return client;
        }
    }
}