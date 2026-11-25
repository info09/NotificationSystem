namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Cung cấp khả năng băm và xác minh credential.
/// </summary>
public interface ICredentialHasher
{
    /// <summary>
    /// Tạo giá trị băm cho credential dạng plaintext.
    /// </summary>
    /// <param name="plainText">Giá trị plaintext.</param>
    /// <returns>Giá trị băm.</returns>
    string Hash(string plainText);

    /// <summary>
    /// Xác minh credential plaintext với giá trị băm.
    /// </summary>
    /// <param name="hashedValue">Giá trị băm.</param>
    /// <param name="plainText">Giá trị plaintext.</param>
    /// <returns><c>true</c> nếu khớp; ngược lại <c>false</c>.</returns>
    bool Verify(string hashedValue, string plainText);
}
