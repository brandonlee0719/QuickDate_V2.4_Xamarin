using System.Collections.Generic;

namespace QuickDate.PaymentGoogle
{
    public static class InAppBillingGoogle 
    {
        public static readonly string ProductId = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAw5bcF5KkLtEPwqOGalW/iZVFw5AYQ1AWK3GVa8roSyzcGC2B429I3g0LVDWeFjKlxq0U4uN6RdVZVTFNUvaxDrr0QmyXzsv+OHzD/+ERAPWBxcD5Iq38O6A+aZTbM6LqiG2X6k6Ucfyu10u+m4aXlOQ76xDlS3EO98wPJF1Nyl/Y7LqVfMxyUjvEb3WeAdIF2amd5wgRAZEVy/CFiYkspBw7ayvnU2sykgBtdTuBumXp/iY6iS/cGb+LgsV4RUU7nx1/Kw4H3cVXhGwNhiZDxVc5bpbfRTJVoEfKZzfNIdFEzGOwIvWE5DnfGLUUHnAvHn2XJk+F8S9KVWuSAWrN0wIDAQAB";
        public static readonly List<string> ListProductSku = new List<string> // ID Product
        {
            "bagofcredits",
            "boxofcredits",
            "chestofcredits",
            "membershipweekly",
            "membershipmonthly",
            "membershipyearly",
            "membershiplifetime",
        };
    }
}