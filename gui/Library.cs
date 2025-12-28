using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GenshinImpact_WishOnStreamGUI
{
    public class Character
    {
        public string CharacterName { get; set; }
        public string Element { get; set; }
        public int Star { get; set; }
    }

    public class CharacterListInStar : List<Character>
    {
        public Character this[string name]
        {
            get
            {
                Character character = FindCharacterByName(name);
                return character;
            }
        }
        public int StarValue { get; set; }
        public int PullRate { get; set; }

        public void Add(string name)
        {
            Character character = FindCharacterByName(name);
            if (character == null)
            {
                character = new();
                character.CharacterName = name;
                Add(character);
            }
        }
        public void SortList()
        {
            _characters = this.OrderBy(c => c.CharacterName).ToList();
            Clear();
            foreach (Character character in _characters)
                Add(character);
        }
        private Character FindCharacterByName(string characterName)
        {
            return this.FirstOrDefault(c => c.CharacterName == characterName);
        }

        private List<Character> _characters = new();
    }

    public class StarList : SortedDictionary<int, CharacterListInStar>
    {
        public new CharacterListInStar this[int starValue]
        {
            get
            {
                CharacterListInStar charList = FindCharList(starValue);
                if (charList != null)
                    return charList;
                else
                    throw new Exception("Unable to find star collection");
            }
        }
        public Character this[string characterName]
        {
            get
            {
                Character character = new();

                foreach (KeyValuePair<int, CharacterListInStar> charList in this)
                {
                    CharacterListInStar characters = charList.Value;
                    Character thischar = characters[characterName];
                    if (thischar != null)
                    {
                        character = thischar;
                        break;
                    }
                }
                return character;
            }
        }
        public void AddStar(int starValue)
        {
            if (!ContainsKey(starValue))
            {
                CharacterListInStar charList = new();
                charList.StarValue = starValue;
                Add(starValue, charList);
            }
            _starList = this;
        }

        private CharacterListInStar FindCharList(int starValue)
        {
            return _starList[starValue];
        }
        private SortedDictionary<int, CharacterListInStar> _starList = new();
    }

    public static class Interwebs
    {
        public static readonly HttpClient httpClient = new();
    }

    class Images
    {
        public static Bitmap Load(string imagename)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream stream = _assembly.GetManifestResourceStream("GenshinImpact_WishOnStreamGUI.img." + imagename + ".png");
            Bitmap _bitmap = new(stream);
            return _bitmap;
        }
    }

    // thanks to Nick Charlton
    // https://thoughtbot.com/blog/using-httplistener-to-build-a-http-server-in-csharp
    class HttpServer
    {
        public const string localhostAddress = "http://localhost:8275/";
        public event Action<AuthPayload> AuthCompleted;
        private HttpListener _listener;

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(localhostAddress);
            _listener.Start();
            _listener.BeginGetContext(ListenerCallback, null);
        }

        public void Stop()
        {
            if (_listener?.IsListening == true)
            {
                _listener.Stop();
                _listener.Close();
            }
        }

        // thanks to https://www.codeproject.com/Tips/485182/Create-a-local-server-in-Csharp
        private void ListenerCallback(IAsyncResult result)
        {
            if (!_listener.IsListening)
                return;

            HttpListenerContext context = _listener.EndGetContext(result);

            // add CORS headers to allow the browser to send from the remote backend to localhost
            const string domain = "https://genshin-twitch.sidestreamnetwork.net/";
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // handle CORS preflight
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                context.Response.Close();
                _listener.BeginGetContext(ListenerCallback, null);
                return;
            }

            // only accept POST from /update-creds
            if (context.Request.HttpMethod != "POST" || context.Request.Url.AbsolutePath != "/update-creds")
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                _listener.BeginGetContext(ListenerCallback, null);
                return;
            }

            using StreamReader reader = new(context.Request.InputStream, context.Request.ContentEncoding);
            string body = reader.ReadToEnd();
            JObject jsonObj = JObject.Parse(body);

            AuthPayload payload = new()
            {
                ChannelName = jsonObj["channel_name"]?.ToString(),
                ChannelId = jsonObj["channel_id"]?.ToString(),
                BroadcasterType = jsonObj["broadcaster_type"]?.ToString(),
                Token = jsonObj["access_token"]?.ToString(),
                Redeems = jsonObj["redeems"] != null
                    ? jsonObj["redeems"].ToObject<List<string>>()
                    : []
            };

            // rewrite local_creds.js file with payload
            RewriteLocalCreds(payload);

            // notify listeners
            AuthCompleted?.Invoke(payload);

            byte[] response = Encoding.UTF8.GetBytes("OK");
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = response.Length;
            context.Response.OutputStream.Write(response, 0, response.Length);
            context.Response.Close();

            Stop();
        }

        public void RewriteLocalCreds(AuthPayload payload)
        {
            string pathSettings = Path.Combine("js", "local_creds.js");
            string js = File.ReadAllText(pathSettings);

            js = ReplaceVar(js, "channelName", $"'{payload.ChannelName}'");
            js = ReplaceVar(js, "channelID", $"'{payload.ChannelId}'");
            js = ReplaceVar(js, "localToken", $"'{payload.Token.Replace("'", @"\'")}'");

            File.WriteAllText(pathSettings, js);
        }

        string ReplaceVar(string source, string name, string value)
        {
            var pattern = $@"var\s+{name}\s*=\s*.*?;";
            return Regex.Replace(source, pattern, $"var {name} = {value};");
        }

        public class AuthPayload
        {
            public string ChannelName { get; set; }
            public string ChannelId { get; set; }
            public string BroadcasterType { get; set; }
            public string Token { get; set; }
            public List<string> Redeems { get; set; }
        }
    }

    public class UserInfo
    {
        string _name;
        string _id;
        string _token;
        string _redeem;
        bool _redeemEnabled;
        int _duration;
        string _twitchCommandPrefix;
        bool _twitchCommandEnabled;
        string _broadcasterType;
        List<string> _rewards;
        public UserInfo()
        {
            _name = "";
            _id = "";
            _token = "";
            _redeem = "";
            _redeemEnabled = false;
            _duration = 8000;
            _twitchCommandPrefix = "";
            _twitchCommandEnabled = false;
            _broadcasterType = "";
            _rewards = [];
        }
        public UserInfo(string name, string id)
        {
            _name = name;
            _id = id;
            _token = "";
            _redeem = "";
            _redeemEnabled = false;
            _duration = 8000;
            _twitchCommandPrefix = "";
            _twitchCommandEnabled = false;
            _broadcasterType = "";
            _rewards = [];
        }
        public string Name => _name;
        public string ID => _id;
        public string Redeem { get => _redeem; set => _redeem = value; }
        public bool RedeemEnabled { get => _redeemEnabled; set => _redeemEnabled = value; }
        public int Duration { get => _duration; set => _duration = value; }
        public string TwitchCommandPrefix { get => _twitchCommandPrefix; set => _twitchCommandPrefix = value; }
        public bool TwitchCommandEnabled { get => _twitchCommandEnabled; set => _twitchCommandEnabled = value; }
        public string BroadcasterType { get => _broadcasterType; set => _broadcasterType = value; }
        public string Token { get => _token; set => _token = value; }
        public List<string> Rewards { get => _rewards; set => _rewards = value;  }
        public async Task<List<string>> GetCustomRewards()
        {
            List<string> rewards = [];

            Task timeout = Task.Delay(3000);
            string url = $"https://genshin-twitch.sidestreamnetwork.net/api/redemptions?broadcaster_id={_id}";
            Task<HttpResponseMessage> request = Interwebs.httpClient.GetAsync(url);
            await Task.WhenAny(timeout, request);

            try
            {
                HttpResponseMessage response = request.Result;
                if (response.IsSuccessStatusCode)
                {
                    string page = await response.Content.ReadAsStringAsync();
                    rewards = JsonConvert.DeserializeObject<List<string>>(page);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return rewards;
        }
    }

    #region JSON classes
    class VersionClass
    {
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("download_url")]
        public string DownloadURL { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
    class CharacterElementPair
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("element")]
        public string Element { get; set; }
    }
    #endregion

}
