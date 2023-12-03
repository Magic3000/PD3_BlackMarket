using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.ConsoleColor;

namespace PD3_BlackMarket
{

    internal class Program
    {
        public static Random random = new Random();

        private static string url_token = "https://nebula.starbreeze.com/iam/v3/oauth/token";
        private static string buy_url => $"https://nebula.starbreeze.com/platform/public/namespaces/pd3/users/{userId}/orders";
        private static string save_data_url => $"https://nebula.starbreeze.com/cloudsave/v1/namespaces/pd3/users/{userId}/records/progressionsavegame";
        private static string entitlementUrl => $"https://nebula.starbreeze.com/platform/public/namespaces/pd3/users/{userId}/entitlements?limit=2147483647";
        private static string oops => $"https://nebula.starbreeze.com/platform/public/namespaces/pd3/users/{userId}/entitlements/";
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFabcdef0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string HidePassword(string password) => new string('*', password.Length);

        public static string token;
        public static string userId;
        public static string credentialsPath = "credentials.txt";
        static void Main(string[] args)
        {
            Credentials creds = new Credentials();
            void EnterCredentials()
            {
                $"Enter nebula login: "._sout(Green);
                creds.username = Console.ReadLine();
                $"Enter nebula password: "._sout(Green);
                creds.password = Console.ReadLine();
                File.WriteAllText(credentialsPath, JsonConvert.SerializeObject(creds));
            }
            if (!File.Exists(credentialsPath) || string.IsNullOrEmpty(File.ReadAllText(credentialsPath)))
            {
                File.Create(credentialsPath).Close();
                EnterCredentials();
            }
            else
            {
                creds = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(credentialsPath));
                $"Username: {creds.username}"._sout(Green);
                $"Password: {HidePassword(creds.password)}"._sout(Green);
                $"Enter new credentials? y/n"._sout(Green);
                var newCreds = Console.ReadLine();
                if (newCreds == "y")
                {
                    EnterCredentials();
                    $"Username: {creds.username}"._sout(Green);
                    $"Password: {Enumerable.Repeat("*", creds.password.Length)}"._sout(Green);
                }
            }

            var tokenHeader = new Dictionary<string, string>();
            tokenHeader["Host"] = "nebula.starbreeze.com";
            tokenHeader["Content-Type"] = "application/x-www-form-urlencoded";
            tokenHeader["Authorization"] = "Basic MGIzYmZkZjVhMjVmNDUyZmJkMzNhMzYxMzNhMmRlYWI6";
            tokenHeader["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.5735.289 Electron/25.8.3 Safari/537.36";
            tokenHeader["Accept"] = "*/*";
            tokenHeader["Sec-Fetch-Site"] = "cross-site";
            tokenHeader["Sec-Fetch-Mode"] = "cors";
            tokenHeader["Sec-Fetch-Dest"] = "empty";
            tokenHeader["Accept-Encoding"] = "gzip, deflate, br";
            tokenHeader["Accept-Language"] = "en-US";

            var dataToken = new Dictionary<string, string>();
            dataToken["username"] = creds.username;
            dataToken["password"] = creds.password;
            dataToken["grant_type"] = "password";
            var cliend_id = RandomString(32);
            $"Generated client_id for login: {cliend_id}"._sout(Cyan);
            dataToken["client_id"] = cliend_id;
            dataToken["extend_exp"] = "true";

            MyWebRequest loginReq = new MyWebRequest(url_token, RequestMethod.POST,
                            tokenHeader, "application/x-www-form-urlencoded", dataToken);

            var response = loginReq.GetResponse<PD3Token>();
            token = response.access_token;
            userId = response.user_id;
            $"Logged in as {response.display_name}"._sout(Green);
            $"UserId: {response.user_id}"._sout(Cyan);
            $"\nToken: {token}"._sout(Red);
            $"\nRefreshToken: {response.refresh_token}"._sout(Yellow);

            var buyHeader = new Dictionary<string, string>();
            buyHeader["Accept-Encoding"] = "deflate, gzip";
            buyHeader["Content-Type"] = "application/json";
            buyHeader["Accept"] = "application/json";
            buyHeader["Authorization"] = $"Bearer {token}";
            buyHeader["Namespace"] = "pd3";
            buyHeader["Game-Client-Version"] = "1.0.0.1";
            buyHeader["AccelByte-SDK-Version"] = "21.0.3";
            buyHeader["AccelByte-OSS-Version"] = "0.8.11";
            buyHeader["User-Agent"] = "PAYDAY3/++UE4+Release-4.27-CL-0 Windows/10.0.19045.1.256.64bit";

            var buyData = new Dictionary<string, string>();
            buyData["itemId"] = null;
            buyData["quantity"] = "1";
            buyData["price"] = null;
            buyData["discountedPrice"] = null;
            buyData["region"] = "SE";
            buyData["language"] = "en-US";
            buyData["returnUrl"] = "http://127.0.0.1";

            var pd3_offsets = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("Payday3_offsets.json"));
            var pd3_rare_items = JsonConvert.DeserializeObject<Item[]>((JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("items.json")))["items"].ToString());

            bool go = true;
            while (go)
            {
                $"\nEsc - exit\n1 - Buy C-Stacks\n2 - Custom buy\n3 - Favors\n4 - Pre-Order/Silver/Gold/Collectors edition content\n5 - Remove some stuff"._sout(Yellow);
                var command = Console.ReadKey().Key;
                $"\n"._sout();
                if (command == ConsoleKey.Escape)
                {
                    go = false;
                }
                else if (command == ConsoleKey.D1)
                {
                    $"Enter amount of C-Stacks (10 c-stacks for 90k): "._sout(Green);
                    if (int.TryParse(Console.ReadLine(), out int buyCount))
                    {
                        buyData["itemId"] = "dd693796e4fb4e438971b65eecf6b4b7";     //large one, 10 c-stacks for 90k
                        buyData["price"] = "90000";
                        buyData["discountedPrice"] = "90000";
                        buyData["currencyCode"] = "CASH";
                        for (int i = 0; i < buyCount; i++)
                        {
                            MyWebRequest cStackReq = new MyWebRequest(buy_url, RequestMethod.POST, buyHeader, "application/json", JsonConvert.SerializeObject(buyData));
                            var resp = cStackReq.GetResponse();
                            $"{i + 1}) Buying status: {resp}\n"._sout(Yellow);
                        }
                    }
                }
                else if (command == ConsoleKey.D2)
                {
                    $"Enter itemID: "._sout(Green);
                    var itemId = Console.ReadLine();
                    $"Enter price: "._sout(Green);
                    var price = Console.ReadLine();
                    $"Enter discountedPrice: "._sout(Green);
                    var discountedPrice = Console.ReadLine();
                    $"Enter currencyCode: "._sout(Green);
                    var currencyCode = Console.ReadLine();
                    $"Enter amount of c-scacks: "._sout(Green);
                    if (int.TryParse(Console.ReadLine(), out int buyCount))
                    {
                        buyData["itemId"] = itemId;
                        buyData["price"] = price;
                        buyData["discountedPrice"] = discountedPrice;
                        buyData["currencyCode"] = currencyCode;
                        for (int i = 0; i < buyCount; i++)
                        {
                            MyWebRequest cStackReq = new MyWebRequest(buy_url, RequestMethod.POST, buyHeader, "application/json", JsonConvert.SerializeObject(buyData));
                            var resp = cStackReq.GetResponse();
                            $"{i + 1}) Buying status: {resp}\n"._sout(Yellow);
                        }
                    }
                }
                else if (command == ConsoleKey.D3)
                {
                    var Heistfav = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(pd3_offsets["Heistfav"].ToString());
                    var heist_list = new Dictionary<int, (Favor, string)>();
                    var counter = 1;
                    foreach (var heistDict in Heistfav)
                    {
                        foreach (var heist in heistDict)
                        {
                            var heistName = heist.Key;
                            $"HeistName: {heistName}"._sout();
                            var heistfavors = JsonConvert.DeserializeObject<Dictionary<string, Favor>[]>(heist.Value.ToString());
                            foreach (var favorDict in heistfavors)
                            {
                                foreach (var favor in favorDict)
                                {
                                    heist_list.Add(counter, (favor.Value, favor.Key));
                                    $"\t{counter++}) Favor: {favor.Key} - {favor.Value.price}$"._sout();
                                }
                            }
                        }
                    }

                    $"Select what you want to buy: "._sout();
                    var toBuy = heist_list[int.Parse(Console.ReadLine())];
                    $"You buying {toBuy.Item2}"._sout();
                    $"Enter amount of favors: "._sout(Green);
                    if (int.TryParse(Console.ReadLine(), out int buyCount))
                    {
                        buyData["itemId"] = toBuy.Item1.itemId;
                        buyData["price"] = toBuy.Item1.price.ToString();
                        buyData["discountedPrice"] = toBuy.Item1.price.ToString();
                        buyData["currencyCode"] = "CASH";
                        for (int i = 0; i < buyCount; i++)
                        {
                            MyWebRequest favorReq = new MyWebRequest(buy_url, RequestMethod.POST, buyHeader, "application/json", JsonConvert.SerializeObject(buyData));
                            var resp = favorReq.GetResponse();
                            $"{i + 1}) Buying status: {resp}\n"._sout(Yellow);
                        }
                    }
                }
                else if (command == ConsoleKey.D4)
                {
                    var raritiesDict = new Dictionary<string, (string, int)>();        //name, id-price
                    raritiesDict["Venomous Verdigris (Pre-Order mask)"] = ("e8a13085ff9543cd9976c6e275197cbd", 0);
                    raritiesDict["Obsidian Glitz (Pre-Order suit)"] = ("175b6b75241e42e4b023c4d4b1950e99", 0);
                    raritiesDict["Cotton Stripes (Pre-Order gloves)"] = ("5b65aab6f6354c66aea0da248e9452d6", 0);

                    raritiesDict["PD2 gloves (Pre-Order gloves)"] = ("638bf75ba8394a508e05b52117eceee4", 0);

                    raritiesDict["Dark Sterling (Silver-Edition mask)"] = ("668da63985ca46d381195bdb827ba1aa", 0);

                    raritiesDict["Skull of Liberty (Gold-Edition mask)"] = ("8c874bff3f474a1faaffb5ef8db9a69d", 0);
                    raritiesDict["Golden Slate (Gold-Edition gloves)"] = ("f30a96c01c54492fb648f130106160da", 0);

                    raritiesDict["Solidus (Collectors-Edition mask)"] = ("94d9475ef8e34819938f78161b041db6", 0);

                    raritiesDict["Echelon suit (Infamous suit)"] = ("533872e42ab84d00ba32355ea2de792f", 0);
                    raritiesDict["OG American Dream (Infamous mask)"] = ("865b127704cd40fe9284cc1f4023886f", 0);
                    raritiesDict["Old Faithful (Infamous mask)"] = ("22353e0584ef4a2596e4630759a29bf7", 0);
                    raritiesDict["fluer-de-lys (Infamous mask)"] = ("8959E686397C4BB1AF7C4CB3D8C36EAA", 0);

                    raritiesDict["Coulrophobia (Nebula mask)"] = ("737A2739479C49F184B5A1C6C1C6F4C1", 0);
                    raritiesDict["Black Claret (Nebula mask)"] = ("e6b9bcc883124f5a86611cc7617da8f8", 0);

                    var counter = 1;
                    foreach (var someItem in raritiesDict)
                    {
                        $"{counter++}) {someItem.Key}"._sout(Green);
                    }

                    $"Select what you want to buy: "._sout();
                    var toBuy = raritiesDict.ElementAt(int.Parse(Console.ReadLine()) - 1);
                    $"You buying {toBuy.Key}\tPress any key to proceed"._sout();
                    Console.ReadKey();
                    buyData["itemId"] = toBuy.Value.Item1;
                    buyData["price"] = toBuy.Value.Item2.ToString();
                    buyData["discountedPrice"] = toBuy.Value.Item2.ToString();
                    buyData["currencyCode"] = "CASH";
                    MyWebRequest favorReq = new MyWebRequest(buy_url, RequestMethod.POST, buyHeader, "application/json", JsonConvert.SerializeObject(buyData));
                    var resp = favorReq.GetResponse();
                    $"Buying status: {resp}\n"._sout(Yellow);
                }
                else if (command == ConsoleKey.D5)
                {
                    MyWebRequest inventoryReq = new MyWebRequest(entitlementUrl, RequestMethod.GET, buyHeader, "application/json", "");
                    var resp = inventoryReq.GetResponse();
                    $"Fetching inventory: {resp}\n"._sout(Yellow);
                    var respDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(resp);
                    var entitlements = JsonConvert.DeserializeObject<Item[]>(respDict["data"].ToString());
                    int counter = 1;
                    foreach (var entitlement in entitlements)
                    {
                        $"{counter++}) {entitlement.createdAt} {entitlement.name} ({entitlement.itemId}) remove-id: {entitlement.id}"._sout(pd3_rare_items.Any(x => x.itemId == entitlement.itemId.ToUpper()) ? Red : DarkYellow);
                    }

                    $"P.S. Rare items has red color"._sout(Cyan);
                    bool removing = true;
                    while (removing)
                    {
                        "Enter id to remove: "._sout(Green);
                        var idToRemove = Console.ReadLine();
                        $"Are you sure you want to remove {entitlements.First(x => x.id == idToRemove).name} ({idToRemove}) ? y/n"._sout(Red);
                        var choice = Console.ReadKey().Key;
                        $"\n"._sout();
                        if (choice == ConsoleKey.Y)
                        {
                            var removeJson = new Dictionary<string, string>();
                            removeJson["useCount"] = "1";
                            removeJson["options"] = null;
                            MyWebRequest removeReq = new MyWebRequest($"{oops}{idToRemove}/decrement", RequestMethod.PUT, buyHeader, "application/json", JsonConvert.SerializeObject(removeJson));
                            var removeResp = removeReq.GetResponse();
                            $"Item removed: {removeResp}"._sout(Cyan);
                        }
                        else
                        {
                            "Removing cancelled"._sout(Green);
                        }
                    }
                }
            }

            $"\nExiting.. press any key"._sout(Red);
            Console.ReadKey();
        }
    }
}
