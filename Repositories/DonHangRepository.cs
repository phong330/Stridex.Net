using Microsoft.Data.SqlClient;
using StridexFinal_CSharp.Models;

namespace StridexFinal_CSharp.Repositories;

public class DonHangRepository
{
    private readonly Db _db;

    public DonHangRepository(Db db)
    {
        _db = db;
    }

    public async Task<int> TaoDonHangAsync(int nguoiDungId, List<GioHangItem> gioHang)
    {
        var maDon = "SDX" + DateTime.Now.ToString("yyyyMMddHHmmss");
        var tongTien = gioHang.Sum(x => x.ThanhTien);

        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var tran = await conn.BeginTransactionAsync();
        try
        {
            const string insertDh = @"INSERT INTO DonHang(MaDonHang, NguoiDungId, NgayDat, TongTien, TrangThai)
                                      OUTPUT INSERTED.Id VALUES(@Ma, @NguoiDungId, GETDATE(), @TongTien, N'Chờ xác nhận')";
            await using var cmd = new SqlCommand(insertDh, conn, (SqlTransaction)tran);
            cmd.Parameters.AddWithValue("@Ma", maDon);
            cmd.Parameters.AddWithValue("@NguoiDungId", nguoiDungId);
            cmd.Parameters.AddWithValue("@TongTien", tongTien);
            var donHangId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            foreach (var item in gioHang)
            {
                const string insertCt = @"INSERT INTO ChiTietDonHang(DonHangId, SanPhamId, SoLuong, DonGia)
                                          VALUES(@DonHangId, @SanPhamId, @SoLuong, @DonGia)";
                await using var ct = new SqlCommand(insertCt, conn, (SqlTransaction)tran);
                ct.Parameters.AddWithValue("@DonHangId", donHangId);
                ct.Parameters.AddWithValue("@SanPhamId", item.SanPhamId);
                ct.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                ct.Parameters.AddWithValue("@DonGia", item.Gia);
                await ct.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();
            return donHangId;
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    public async Task<List<DonHang>> GetByNguoiDungAsync(int nguoiDungId)
    {
        var list = new List<DonHang>();
        const string sql = @"SELECT Id, MaDonHang, NguoiDungId, NgayDat, TongTien, TrangThai
                             FROM DonHang WHERE NguoiDungId=@NguoiDungId ORDER BY NgayDat DESC";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@NguoiDungId", nguoiDungId);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(MapDonHang(rd));
        return list;
    }

    public async Task<List<DonHang>> GetAllAsync()
    {
        var list = new List<DonHang>();
        const string sql = @"SELECT dh.Id, dh.MaDonHang, dh.NguoiDungId, dh.NgayDat, dh.TongTien, dh.TrangThai, nd.HoTen
                             FROM DonHang dh LEFT JOIN NguoiDung nd ON dh.NguoiDungId=nd.Id
                             ORDER BY dh.NgayDat DESC";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var dh = MapDonHang(rd);
            dh.TenNguoiDung = rd["HoTen"]?.ToString();
            list.Add(dh);
        }
        return list;
    }

    public async Task<DonHang?> GetDetailAsync(int id)
    {
        DonHang? donHang = null;
        const string sqlDh = @"SELECT dh.Id, dh.MaDonHang, dh.NguoiDungId, dh.NgayDat, dh.TongTien, dh.TrangThai, nd.HoTen
                               FROM DonHang dh LEFT JOIN NguoiDung nd ON dh.NguoiDungId=nd.Id WHERE dh.Id=@Id";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using (var cmd = new SqlCommand(sqlDh, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                donHang = MapDonHang(rd);
                donHang.TenNguoiDung = rd["HoTen"]?.ToString();
            }
        }
        if (donHang == null) return null;

        const string sqlCt = @"SELECT ct.Id, ct.DonHangId, ct.SanPhamId, ct.SoLuong, ct.DonGia, sp.Ten, sp.Hinh
                               FROM ChiTietDonHang ct LEFT JOIN SanPham sp ON ct.SanPhamId=sp.Id
                               WHERE ct.DonHangId=@Id";
        await using (var cmd = new SqlCommand(sqlCt, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                donHang.ChiTiet.Add(new ChiTietDonHang
                {
                    Id = Convert.ToInt32(rd["Id"]),
                    DonHangId = rd["DonHangId"] == DBNull.Value ? null : Convert.ToInt32(rd["DonHangId"]),
                    SanPhamId = rd["SanPhamId"] == DBNull.Value ? null : Convert.ToInt32(rd["SanPhamId"]),
                    SoLuong = rd["SoLuong"] == DBNull.Value ? 0 : Convert.ToInt32(rd["SoLuong"]),
                    DonGia = rd["DonGia"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["DonGia"]),
                    TenSanPham = rd["Ten"]?.ToString() ?? "Sản phẩm",
                    Hinh = rd["Hinh"]?.ToString() ?? ""
                });
            }
        }
        return donHang;
    }

    public async Task UpdateTrangThaiAsync(int id, string trangThai)
    {
        const string sql = "UPDATE DonHang SET TrangThai=@TrangThai WHERE Id=@Id";
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        await cmd.ExecuteNonQueryAsync();
    }

    private static DonHang MapDonHang(SqlDataReader rd) => new()
    {
        Id = Convert.ToInt32(rd["Id"]),
        MaDonHang = rd["MaDonHang"]?.ToString() ?? "",
        NguoiDungId = rd["NguoiDungId"] == DBNull.Value ? null : Convert.ToInt32(rd["NguoiDungId"]),
        NgayDat = rd["NgayDat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rd["NgayDat"]),
        TongTien = rd["TongTien"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["TongTien"]),
        TrangThai = rd["TrangThai"]?.ToString() ?? "Chờ xác nhận"
    };
}
