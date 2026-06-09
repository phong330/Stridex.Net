using Microsoft.Data.SqlClient;
using StridexFinal_CSharp.Models;

namespace StridexFinal_CSharp.Repositories;

public class SanPhamRepository
{
    private readonly Db _db;

    public SanPhamRepository(Db db)
    {
        _db = db;
    }

    public async Task<List<SanPham>> GetAllAsync(string? tuKhoa = null, string? loai = null, string? sort = null)
    {
        var list = new List<SanPham>();
        var sql = @"SELECT Id, Ten, Loai, Gia, Hinh, Mota, NoiBat FROM SanPham WHERE 1=1";

        if (!string.IsNullOrWhiteSpace(tuKhoa))
            sql += " AND Ten LIKE @TuKhoa";
        if (!string.IsNullOrWhiteSpace(loai))
            sql += " AND Loai = @Loai";

        sql += sort switch
        {
            "gia-tang" => " ORDER BY Gia ASC",
            "gia-giam" => " ORDER BY Gia DESC",
            "ten" => " ORDER BY Ten ASC",
            _ => " ORDER BY NoiBat DESC, Id DESC"
        };

        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
            cmd.Parameters.AddWithValue("@TuKhoa", $"%{tuKhoa}%");
        if (!string.IsNullOrWhiteSpace(loai))
            cmd.Parameters.AddWithValue("@Loai", loai);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(Map(rd));
        return list;
    }

    public async Task<List<SanPham>> GetNoiBatAsync(int take = 8)
    {
        var list = new List<SanPham>();
        const string sql = @"SELECT TOP (@Take) Id, Ten, Loai, Gia, Hinh, Mota, NoiBat
                             FROM SanPham ORDER BY NoiBat DESC, Id DESC";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Take", take);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(Map(rd));
        return list;
    }

    public async Task<SanPham?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, Ten, Loai, Gia, Hinh, Mota, NoiBat FROM SanPham WHERE Id=@Id";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var rd = await cmd.ExecuteReaderAsync();
        return await rd.ReadAsync() ? Map(rd) : null;
    }

    public async Task<List<string>> GetLoaiAsync()
    {
        var list = new List<string>();
        const string sql = "SELECT DISTINCT Loai FROM SanPham WHERE Loai IS NOT NULL AND Loai<>'' ORDER BY Loai";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(rd.GetString(0));
        return list;
    }

    public async Task<int> AddAsync(SanPham sp)
    {
        const string getIdSql = "SELECT ISNULL(MAX(Id),0) + 1 FROM SanPham";
        const string sql = @"INSERT INTO SanPham(Id, Ten, Loai, Gia, Hinh, Mota, NoiBat)
                             VALUES(@Id, @Ten, @Loai, @Gia, @Hinh, @Mota, @NoiBat)";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var getId = new SqlCommand(getIdSql, conn);
        var id = Convert.ToInt32(await getId.ExecuteScalarAsync());
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        AddParams(cmd, sp);
        await cmd.ExecuteNonQueryAsync();
        return id;
    }

    public async Task UpdateAsync(SanPham sp)
    {
        const string sql = @"UPDATE SanPham SET Ten=@Ten, Loai=@Loai, Gia=@Gia, Hinh=@Hinh, Mota=@Mota, NoiBat=@NoiBat WHERE Id=@Id";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", sp.Id);
        AddParams(cmd, sp);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM SanPham WHERE Id=@Id";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddParams(SqlCommand cmd, SanPham sp)
    {
        cmd.Parameters.AddWithValue("@Ten", sp.Ten ?? "");
        cmd.Parameters.AddWithValue("@Loai", sp.Loai ?? "");
        cmd.Parameters.AddWithValue("@Gia", sp.Gia);
        cmd.Parameters.AddWithValue("@Hinh", sp.Hinh ?? "");
        cmd.Parameters.AddWithValue("@Mota", sp.MoTa ?? "");
        cmd.Parameters.AddWithValue("@NoiBat", sp.NoiBat);
    }

    private static SanPham Map(SqlDataReader rd) => new()
    {
        Id = Convert.ToInt32(rd["Id"]),
        Ten = rd["Ten"]?.ToString() ?? "",
        Loai = rd["Loai"]?.ToString() ?? "",
        Gia = rd["Gia"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["Gia"]),
        Hinh = rd["Hinh"]?.ToString() ?? "",
        MoTa = rd["Mota"]?.ToString() ?? "",
        NoiBat = rd["NoiBat"] != DBNull.Value && Convert.ToBoolean(rd["NoiBat"])
    };
}
