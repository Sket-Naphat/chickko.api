using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace chickko.api.Configurations
{
    // รับ DateOnly จาก JSON ได้หลาย format: "yyyy-MM-dd" (ปกติ), ISO datetime เต็ม
    // (เช่นจาก JS Date.toISOString()) และ empty string (เฉพาะ nullable -> null)
    // กันเคส frontend ส่ง date มาไม่ตรง format เป๊ะแล้วโดน 400 ก่อนถึง action method ด้วยซ้ำ
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";
        private static readonly TimeZoneInfo ThaiTimeZone = ResolveThaiTimeZone();

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => ParseOrThrow(reader.GetString());

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));

        internal static DateOnly ParseOrThrow(string? value)
        {
            if (TryParse(value, out var result))
                return result;
            throw new JsonException($"Unable to convert \"{value}\" to DateOnly. Expected format: {Format}.");
        }

        internal static bool TryParse(string? value, out DateOnly result)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (DateOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return true;

                // ยอมรับ ISO datetime เต็ม เช่น "2026-07-01T00:00:00.000Z" (JS Date.toISOString())
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                {
                    if (dt.Kind == DateTimeKind.Utc)
                        dt = TimeZoneInfo.ConvertTimeFromUtc(dt, ThaiTimeZone); // กันวันที่เพี้ยนข้ามวันตอนแปลง UTC -> ไทย
                    result = DateOnly.FromDateTime(dt);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static TimeZoneInfo ResolveThaiTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } // Windows
            catch
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok"); } // Linux
                catch { return TimeZoneInfo.CreateCustomTimeZone("Thai", TimeSpan.FromHours(7), "Thailand Time", "Thailand Time"); }
            }
        }
    }

    public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateOnlyJsonConverter.ParseOrThrow(value);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
            else
                writer.WriteNullValue();
        }
    }
}
