using System;

public class OrderNumberGenerator
{
    /// <summary>
    /// Generates a sequential order number using GUID and timestamp
    /// </summary>
    /// <param name="prefix">Optional prefix for the order number</param>
    /// <returns>A timestamp-based order number with GUID portion</returns>
    public static string GenerateSequentialOrderNumber(string prefix = "ORD-")
    {
        // Get current timestamp
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        // Get last 6 digits of GUID for uniqueness
        string guidPart = Guid.NewGuid().ToString("N").Substring(0, 6);

        return $"{prefix}{timestamp}-{guidPart}";
    }
}