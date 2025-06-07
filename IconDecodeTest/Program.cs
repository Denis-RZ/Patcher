using System;

class Program
{
    static void Main()
    {
        // Example of an invalid Base64 string (missing padding "=") similar to the value that caused the crash
        const string invalid = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAHUlEQVR4nGP8z8Dwn4ECwESJ5lEDRg0YNWAwGQAAWG0CHvXMz6IAAAAASUVORK5CYII";
        Console.WriteLine($"Attempting to decode: {invalid}");
        try
        {
            var bytes = Convert.FromBase64String(invalid);
            Console.WriteLine($"Decoded {bytes.Length} bytes");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error decoding base64: {ex.Message}");
        }
    }
}
