using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Helpers
{
    // BẮT BUỘC phải có chữ "public static" ở đây
    public static class Session
    {
        public static string MaNhanVien { get; set; }
        public static string HoTen { get; set; }
        public static string ChucVu { get; set; }

        public static void Clear()
        {
            MaNhanVien = "";
            HoTen = "";
            ChucVu = "";
        }
    }
}