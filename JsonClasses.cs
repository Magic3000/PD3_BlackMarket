using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD3_BlackMarket
{
    public class Item
    {
        public string id = default;
        public string @namespace = default;
        public string clazz = default;
        public string type = default;
        public string status = default;
        public string sku = default;
        public string userId = default;
        public string itemId = default;
        public string itemNamespace = default;
        public string name = default;
        public string[] features = default;
        public string source = default;
        public string grantedAt = default;
        public string createdAt = default;
        public string updatedAt = default;
    }

    public class Favor
    {
        public int price;
        public string itemId;
    }

    public class NamespaceRoles
    {
        public string roleId = default;
        public string @namespace = default;
    }

    public class PD3Token
    {
        public string access_token = default;
        public string auth_trust_id = default;
        public string[] bans = default;
        public string display_name = default;
        public int expires_in = default;
        public bool is_comply = default;
        public int jflgs = default;
        public string @namespace = default;
        public NamespaceRoles[] namespace_roles = default;
        public string[] permissions = default;
        public string platform_id = default;
        public string platform_user_id = default;
        public long refresh_expires_in = default;
        public string refresh_token = default;
        public string[] roles = default;
        public string scope = default;
        public string token_type = default;
        public string user_id = default;
        public string xuid = default;
    }

    public class Credentials
    {
        public string username = default;
        public string password = default;
    }
}
