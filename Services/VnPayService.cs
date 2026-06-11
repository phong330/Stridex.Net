using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace StridexFinal_CSharp.Services;

public class VnPayService
{
    private readonly IConfiguration _config;

    public VnPayService(IConfiguration config)
    {
        _config = config;
    }

    public string CreatePaymentUrl(HttpContext context, int orderId, decimal amount)
    {
        var vnpUrl = _config["VnPay:BaseUrl"]!.Trim();
        var returnUrl = _config["VnPay:ReturnUrl"]!.Trim();
        var tmnCode = _config["VnPay:TmnCode"]!.Trim();
        var hashSecret = _config["VnPay:HashSecret"]!.Trim();

        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", tmnCode },
            { "vnp_Amount", ((long)(amount * 100)).ToString() },
            { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", $"Thanh toan don hang {orderId}" },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", returnUrl },
            { "vnp_TxnRef", orderId.ToString(CultureInfo.InvariantCulture) }
        };

        var hashData = string.Join("&", vnpParams.Select(x =>
            $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

        var query = string.Join("&", vnpParams.Select(x =>
            $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

        var secureHash = HmacSHA512(hashSecret, hashData);

        return $"{vnpUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateSignature(IQueryCollection query)
    {
        var hashSecret = _config["VnPay:HashSecret"]!.Trim();
        var vnpSecureHash = query["vnp_SecureHash"].ToString();

        var vnpParams = new SortedDictionary<string, string>();

        foreach (var item in query)
        {
            if (!item.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                && !item.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                vnpParams.Add(item.Key, item.Value.ToString());
            }
        }

        var hashData = string.Join("&", vnpParams.Select(x =>
            $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

        var checkHash = HmacSHA512(hashSecret, hashData);

        return checkHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSHA512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(inputBytes);

        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}