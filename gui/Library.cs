using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
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

    public class AuthThings(MainWindow mainwindow)
    {
        MainWindow _mainwindow = mainwindow;
        public string wisherPath = "";
        public UserInfo user;
        public const string CLIENT_ID = "rs83ihxx7l4k7jjeprsiz03ofvly8g";
        List<string> connectionErrors;

        // thanks to Philippe
        // https://stackoverflow.com/questions/29801195/adding-headers-when-using-httpclient-getasync
        public async void GetUserInfo(string access_token)
        {
            connectionErrors = new();
            bool userHasValidToken = await ValidateToken(access_token);

            // need a check if the reset/disconnect button was clicked
            if (!userHasValidToken)
            {
                await AcquireToken(access_token);

                user.Rewards = await GetCustomRewards();
                _mainwindow.SetUserInfo(user);
                SaveCreds(user);
                _mainwindow.DisplayConnectionErrors(connectionErrors);
            }
            else
            {
                user.Rewards = await GetCustomRewards();
                _mainwindow.SetUserInfo(user);
                SaveCreds(user);
                _mainwindow.DisplayConnectionErrors(connectionErrors);
            }

        }

        private async Task<bool> ValidateToken(string token)
        {
            using HttpRequestMessage requestMessage = new(HttpMethod.Get, "https://id.twitch.tv/oauth2/validate");
            requestMessage.Headers.Authorization = new("Bearer", token);
            try
            {
                HttpResponseMessage validateResponse = await Interwebs.httpClient.SendAsync(requestMessage);
                if (validateResponse.IsSuccessStatusCode)
                {
                    Task<string> validatePage = validateResponse.Content.ReadAsStringAsync();
                    TwitchToken receivedTokenInfo = JsonConvert.DeserializeObject<TwitchToken>(validatePage.Result);
                    user = new(receivedTokenInfo.Username, receivedTokenInfo.User_ID)
                    {
                        Token = token
                    };
                    long timenow = DateTimeOffset.Now.ToUnixTimeSeconds();
                    long tokenExpiresInSeconds = int.Parse(receivedTokenInfo.TokenExpiresIn);
                    user.TokenExpiry = timenow + tokenExpiresInSeconds;
                    return true;
                }
            }
            catch
            {
                connectionErrors.Add("Unable to validate existing token.");
            }
            return false;
        }

        private async Task AcquireToken(string token)
        {
            using HttpRequestMessage requestMessage = new(HttpMethod.Get, "https://id.twitch.tv/oauth2/userinfo");
            requestMessage.Headers.Authorization = new("Bearer", token);
            try
            {
                HttpResponseMessage claimResponse = await Interwebs.httpClient.SendAsync(requestMessage);
                if (claimResponse.IsSuccessStatusCode)
                {
                    Task<string> claimPage = claimResponse.Content.ReadAsStringAsync();
                    TwitchClaims claimResult = JsonConvert.DeserializeObject<TwitchClaims>(claimPage.Result);

                    UserInfo userInfo = new(claimResult.Username, claimResult.User_ID)
                    {
                        Token = token,
                        TokenExpiry = int.Parse(claimResult.TokenExpiry)
                    };
                    if (user.ID == claimResult.User_ID)
                    {
                        userInfo.Redeem = user.Redeem;
                        userInfo.Duration = user.Duration;
                    }

                    user = userInfo;
                }
            }
            catch
            {
                connectionErrors.Add("Unable to acquire or refresh Twitch token.");
            }
        }

        public async void RevokeToken()
        {
            if (user.Token != "")
            {
                List<KeyValuePair<string, string>> data = new()
                {
                    new KeyValuePair<string, string>("client_id", CLIENT_ID),
                    new KeyValuePair<string, string>("token", user.Token),
                };
                FormUrlEncodedContent content = new(data);
                try
                {
                    await Interwebs.httpClient.PostAsync("https://id.twitch.tv/oauth2/revoke", content);
                    _mainwindow.userTokenized = false;
                }
                catch
                {
                    connectionErrors.Add("Unable to revoke token.");
                }
            }

            SaveCreds(user, true, true);
        }

        private async Task<List<string>> GetCustomRewards()
        {
            List<string> rewards = new();
            using HttpRequestMessage redeemRequest = new(HttpMethod.Get, "https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id=" + user.ID);
            redeemRequest.Headers.Add("Client-ID", CLIENT_ID);
            redeemRequest.Headers.Authorization = new ("Bearer", user.Token);

            try
            {
                HttpResponseMessage redeemResponse = await Interwebs.httpClient.SendAsync(redeemRequest);
                string responseBody = await redeemResponse.Content.ReadAsStringAsync();
                UserResponse userData = JsonConvert.DeserializeObject<UserResponse>(responseBody);

                if (userData != null && userData.Data != null)
                {
                    int ctr = 0;
                    foreach (UserData item in userData.Data)
                    {
                        rewards.Add(item.Title);
                        ctr++;
                    }
                }

            }
            catch
            {
                connectionErrors.Add("Unable to fetch custom rewards.");
            }
            return rewards;
        }

        public string SaveCreds(UserInfo userInfo, bool saveToFile = false, bool revoke = false)
        {
            string pathSettings = Path.Combine(wisherPath, "js/local_creds.js");
            string errors = "";

            if (!File.Exists(pathSettings))
                MessageBox.Show("The \"local_creds.js\" file was not found in the \"" + wisherPath + "\"js folder.\nOne will be created for you.");

            if (!revoke)
            {
                if (userInfo.Name == "")
                    errors += " - Username was blank. Please connect using the Twitch button.\n";
                if (saveToFile && (userInfo.Redeem == ""))
                    errors += " - The Channel Point Redeem is not set. Please set this or make sure you have access to Twitch channel point rewards (Twitch Affiiate, etc.).";
            }
            else
            {
                userInfo = new();
                user = userInfo;
            }

            if (errors == "")
            {
                errors = "User settings saved successfully!";
                _mainwindow.UpdateSettingsPanel(userInfo);
                using StreamWriter writer = new(pathSettings);
                writer.WriteLine("var channelName = \'" + userInfo.Name + "\';");
                writer.WriteLine("var channelID = \'" + userInfo.ID + "\';");
                writer.WriteLine("var localToken = \'" + userInfo.Token + "\';");
                writer.WriteLine("var localTokenExpiry = " + userInfo.TokenExpiry + ";");
                writer.WriteLine("var redeemTitle = \'" + userInfo.Redeem + "\';");
                writer.WriteLine("var animation_duration = " + userInfo.Duration + ";");
            }
            else
            {
                errors = "User Settings errors:\n" + errors;
            }

            return errors;
        }
    }

    // Define a class to represent the structure of the JSON response
    public class UserResponse
    {
        public UserData[] Data { get; set; }
    }

    public class UserData
    {
        public string Title { get; set; }
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

        private HttpListener _listener;

        private MainWindow _mainwindow;

        public void Start(MainWindow mainwindow)
        {
            _mainwindow = mainwindow;
            _listener = new HttpListener();
            _listener.Prefixes.Add(localhostAddress);
            _listener.Start();
            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        // thanks to https://www.codeproject.com/Tips/485182/Create-a-local-server-in-Csharp
        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                var context = _listener.EndGetContext(result);

                string htmlContent = "<html><body><script>" +
                    "var params = new URLSearchParams(window.location.hash.substring(1));" +
                    "var token = params.get('access_token');" +
                    "var newurl = \"" + localhostAddress + "?access_token=\" + token;" +
                    "if (token != null) window.location.href = newurl;" +
                    "</script></body></html>";
                string access_token = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query).Get("access_token");
                string starRow = "";
                string elementsRow = "";

                // if access_token is set, get user info associated with the token
                if (access_token != null)
                {
                    // create images
                    string starPath = "./img/star.svg";
                    string[] elements = ["Pyro", "Hydro", "Anemo", "Electro", "Dendro", "Cryo", "Geo"];
                    string elementsPath = "./img/elements/";
                    // process stars
                    if (File.Exists(starPath))
                    {
                        starRow = "<div class=\"star\">";
                        for (int i = 0; i < 5; i++)
                        {
                            starRow += File.ReadAllText(starPath);
                        }
                        starRow += "</div>";
                    }
                    // process elements
                    elementsRow = "<div class=\"elements\">";
                    foreach (string element in elements)
                    {
                        string elementToInsert = elementsPath + element + ".svg";
                        if (File.Exists(elementToInsert))
                        {
                            elementsRow += File.ReadAllText(elementToInsert);
                        }
                    }
                    elementsRow += "</div>";


                    _mainwindow.authVar.GetUserInfo(access_token);
                    htmlContent = "<style>" +
                        "#container { display: flex; height: 100vh; justify-content: center; align-items: center; font-family: sans-serif; }" +
                        "#contents { display: flex; flex-wrap: wrap; gap: 0; min-width: 400px; }" +
                        "#link_to_token {" +
                            "font-weight: bold; font-size: 1.2rem; text-align: center; display: block; padding: 1em; margin: 0 auto;" +
                            "background: #59f; color: #fff; text-decoration: none; width: 50%; text-shadow: 2px 2px 2px rgb(0 0 0 / 30%); border-radius: 15px;" +
                        "}" +
                        "h1, .elements { flex-basis: 100%; text-align: center; }" +
                        "h1 { margin: 0; color: #666; } " +
                        ".star svg { width: 40px; }" +
                        ".elements svg { width: 54px; filter: saturate(0%) brightness(60%); }" +
                        ".error { background: #f95; }" +
                        "</style>";
                    htmlContent += "<div id=\"container\">" +
                        "<div id=\"contents\">" +
                        "<h1>Genshin Impact: Wish On Stream</h1>" +
                        elementsRow +
                        "<div id=\"link_to_token\">" +
                        "You may now close this window.<br>Click on the \"Save\" button and refresh the Genshin Wisher browser source in your streaming software.<br>You may also close the Genshin Wisher app now." +
                        "</div>" +
                        "</div>" +
                        "</div>";
                }

                byte[] _responseArray = System.Text.Encoding.UTF8.GetBytes(htmlContent); // get the bytes to response
                context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length); // write bytes to the output stream
                context.Response.KeepAlive = false; // set the KeepAlive bool to false
                context.Response.Close(); // close the connection

                Receive();
            }
        }
    }

    public class UserInfo
    {
        string _name;
        string _id;
        string _token;
        long _expiry;
        string _redeem;
        int _duration;
        List<string> _rewards;
        public UserInfo()
        {
            _name = "";
            _id = "";
            _token = "";
            _expiry = 0;
            _redeem = "";
            _duration = 8000;
            _rewards = [];
        }
        public UserInfo(string name, string id)
        {
            _name = name;
            _id = id;
            _token = "";
            _expiry = 0;
            _redeem = "";
            _duration = 8000;
            _rewards = [];
        }
        public string Name => _name;
        public string ID => _id;
        public string Redeem { get => _redeem; set => _redeem = value; }
        public int Duration { get => _duration; set => _duration = value; }
        public string Token { get => _token; set => _token = value; }
        public long TokenExpiry { get => _expiry; set => _expiry = value; }
        public List<string> Rewards { get => _rewards; set => _rewards = value;  }
        public string CheckUser()
        {
            if ((_name == "") || (_id == "") || (_token == ""))
                return "Connect your Twitch account to begin.";

            if (!CheckExpiry())
                return "Token expired. Refresh your token by clicking on the \"Connect to Twitch\" button.";
            return "";
        }
        public bool CheckExpiry()
        {
            long rightNow = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (_expiry > 0 || rightNow < _expiry)
                return true;

            return false;
        }
    }

    #region JSON classes
    class TwitchClaims
    {
        [JsonProperty("exp")]
        public string TokenExpiry { get; set; }
        [JsonProperty("iat")]
        public string TokenIssued { get; set; }
        [JsonProperty("sub")]
        public string User_ID { get; set; }
        [JsonProperty("preferred_username")]
        public string Username { get; set; }
    }
    class TwitchToken
    {
        [JsonProperty("client_id")]
        public string Client_ID { get; set; }
        [JsonProperty("login")]
        public string Username { get; set; }
        [JsonProperty("scopes")]
        public List<string> Scopes { get; set; }
        [JsonProperty("user_id")]
        public string User_ID { get; set; }
        [JsonProperty("expires_in")]
        public string TokenExpiresIn { get; set; }
    }
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
