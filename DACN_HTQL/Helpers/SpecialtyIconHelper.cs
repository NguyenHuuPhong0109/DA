using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DACN_HTQL.Helpers;

public static class SpecialtyIconHelper
{
    public static string GetIconPath(string? tenChuyenKhoa)
    {
        if (string.IsNullOrWhiteSpace(tenChuyenKhoa))
            return "/images/specialties/mac-dinh.svg";

        var normalized = RemoveDiacritics(tenChuyenKhoa.Trim().ToLowerInvariant());

        if (normalized.Contains("noi tong quat") || normalized.Contains("tong quat") || normalized.Contains("noi khoa"))
            return "/images/specialties/noi-tong-quat.svg";

        if (normalized.Contains("tai mui hong") || normalized.Contains("tmh"))
            return "/images/specialties/tai-mui-hong.svg";

        if (normalized.Contains("da lieu"))
            return "/images/specialties/da-lieu.svg";

        if (normalized.Contains("nhi khoa") || normalized.Contains("nhi"))
            return "/images/specialties/nhi-khoa.svg";

        if (normalized.Contains("tim mach") || normalized.Contains("tim"))
            return "/images/specialties/tim-mach.svg";

        if (normalized.Contains("co xuong khop") || normalized.Contains("xuong khop") || normalized.Contains("khop"))
            return "/images/specialties/co-xuong-khop.svg";

        if (normalized.Contains("tieu hoa") || normalized.Contains("gan mat") || normalized.Contains("da day"))
            return "/images/specialties/tieu-hoa-gan-mat.svg";

        if (normalized.Contains("san phu khoa") || normalized.Contains("phu khoa") || normalized.Contains("san khoa") || normalized.Contains("san"))
            return "/images/specialties/san-phu-khoa.svg";

        // Kiểm tra răng hàm mặt trước mắt để tránh trùng từ "mặt"
        if (normalized.Contains("rang ham mat") || normalized.Contains("nha khoa") || normalized.Contains("rang"))
            return "/images/specialties/rang-ham-mat.svg";

        if (normalized == "mat" || normalized.Contains("nhan khoa") || Regex.IsMatch(normalized, @"\bmat\b"))
            return "/images/specialties/mat.svg";

        return "/images/specialties/mac-dinh.svg";
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }
}
