using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Project_QLTS_DNC.Converters
{
    public class CurrencyConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var value))
                return value;

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();

                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    // Loại bỏ các ký tự đặc biệt như $, ₫, dấu phẩy
                    var cleaned = stringValue
                        .Replace("$", "")  // Xóa ký tự $
                        .Replace("₫", "")  // Xóa ký tự ₫
                        .Replace(",", "")  // Xóa dấu phẩy
                        .Trim();

                    // Chuyển chuỗi đã làm sạch thành decimal
                    if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                        return parsed;
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            // Chuyển giá trị decimal thành chuỗi mà không có dấu phẩy, nếu giá trị là null thì mặc định 0
            writer.WriteNumberValue(value ?? 0);
        }
    }
}
