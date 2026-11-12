using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace WPF_study01.Services
{
    /// <summary>
    /// MySQL 历史记录服务
    /// </summary>
    public class MySQLHistoryService : IHistoryService
    {
        // 连接字符串：改3个地方--用户名、密码、端口
        private const string ConnStr = AppConfig.MysqlConnStr;

        public void SaveRecord(bool isPassed,int defectCount,string imagePath = null)
        {
            // using = 用完自动释放，相当于 Python 的 with
            using var conn = new MySqlConnection(ConnStr);
            conn.Open();

            // ★ SqlParameter 防 SQL 注入——永远别自己拼字符串
            string sql = @"INSERT INTO detection_records
                         (Timestamp, IsPassed, DefectCount, ImagePath)
                         VALUES (@ts, @passed, @count, @path)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ts", DateTime.Now);
            cmd.Parameters.AddWithValue("@passed", isPassed ? 1 : 0);
            cmd.Parameters.AddWithValue("@count", defectCount);
            cmd.Parameters.AddWithValue("@path", (object)imagePath ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
        // 等 SaveRecord 跑通了再开
        // public List<DetectionRecord> GetAllRecords() { ... }
        public List<DetectionRecord> GetAllRecords()
        {
            var records = new List<DetectionRecord>();

            using var conn = new MySqlConnection(ConnStr);
            conn.Open();

            // ORDER BY Id DESC：最新记录排最上面
            string sql = "SELECT Id, Timestamp, IsPassed, DefectCount, ImagePath " +
                         "FROM detection_records ORDER BY Id DESC LIMIT 100";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();   // ★ 新东西：结果集读取器

            while (reader.Read())   // 一行一行读，读完返回false退出循环
            {
                records.Add(new DetectionRecord
                {
                    Id = reader.GetInt32("Id"),
                    Timestamp = reader.GetDateTime("Timestamp"),
                    IsPassed = reader.GetBoolean("IsPassed"),
                    DefectCount = reader.GetInt32("DefectCount"),
                    ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath"))
                                ? null : reader.GetString("ImagePath")
                });
            }
            return records;
        }
    }
}
