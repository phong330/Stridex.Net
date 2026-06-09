using Microsoft.Data.SqlClient;
using StridexFinal_CSharp.Models;

namespace StridexFinal_CSharp.Repositories;

public class NguoiDungRepository
{
    private readonly Db _db;

    public NguoiDungRepository(Db db)
    {
        _db = db;
    }

    public async Task<NguoiDung?> DangNhapAsync(string email, string matKhau)
    {
        const string sql = "SELECT Id, HoTen, Email, MatKhau, VaiTro FROM NguoiDung WHERE Email=@Email AND MatKhau=@MatKhau";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email.Trim());
        cmd.Parameters.AddWithValue("@MatKhau", matKhau);
        await using var rd = await cmd.ExecuteReaderAsync();
        return await rd.ReadAsync() ? Map(rd) : null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(*) FROM NguoiDung WHERE Email=@Email";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email.Trim());
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    public async Task<int> RegisterAsync(DangKyViewModel model)
    {
        const string sql = @"INSERT INTO NguoiDung(HoTen, Email, MatKhau, VaiTro)
                             OUTPUT INSERTED.Id VALUES(@HoTen, @Email, @MatKhau, N'User')";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@HoTen", model.HoTen.Trim());
        cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
        cmd.Parameters.AddWithValue("@MatKhau", model.MatKhau);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<NguoiDung>> GetAllAsync()
    {
        var list = new List<NguoiDung>();
        const string sql = "SELECT Id, HoTen, Email, MatKhau, VaiTro FROM NguoiDung ORDER BY Id DESC";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(Map(rd));
        return list;
    }

    private static NguoiDung Map(SqlDataReader rd) => new()
    {
        Id = Convert.ToInt32(rd["Id"]),
        HoTen = rd["HoTen"]?.ToString() ?? "",
        Email = rd["Email"]?.ToString() ?? "",
        MatKhau = rd["MatKhau"]?.ToString() ?? "",
        VaiTro = rd["VaiTro"]?.ToString() ?? "User"
    };
}
