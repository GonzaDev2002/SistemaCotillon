using System.Security.Cryptography;

namespace Cotillon.Data;

/// <summary>
/// Hashing de PIN con PBKDF2-SHA256. El PIN nunca se guarda en texto plano.
/// </summary>
public static class PinHasher
{
    private const int TamanioSalt = 16;
    private const int TamanioHash = 32;
    private const int Iteraciones = 100_000;

    public static (string Hash, string Salt) Hashear(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
            throw new ArgumentException("El PIN no puede estar vacío.", nameof(pin));

        var salt = RandomNumberGenerator.GetBytes(TamanioSalt);
        var hash = Derivar(pin, salt);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verificar(string pin, string hashAlmacenado, string saltAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(pin) ||
            string.IsNullOrWhiteSpace(hashAlmacenado) ||
            string.IsNullOrWhiteSpace(saltAlmacenado))
            return false;

        byte[] salt;
        byte[] esperado;

        try
        {
            salt = Convert.FromBase64String(saltAlmacenado);
            esperado = Convert.FromBase64String(hashAlmacenado);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Derivar(pin, salt);

        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }

    private static byte[] Derivar(string pin, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iteraciones, HashAlgorithmName.SHA256, TamanioHash);
}
