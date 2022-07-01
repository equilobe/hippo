using Hippo.Application.Common.Models;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Hippo.Infrastructure.Identity
{
    public sealed class PasswordHasher
    {
        public static byte Version => 1;
        public int SaltSize { get; } = 128 / 8; // 128 bits
        public HashAlgorithmName HashAlgorithmName { get; set; } = HashAlgorithmName.SHA256;

        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            byte[] salt = GenerateSalt(SaltSize);
            byte[] hash = HashPasswordWithSalt(password, salt);

            var inArray = new byte[1 + SaltSize + hash.Length];
            inArray[0] = Version;
            Buffer.BlockCopy(salt, 0, inArray, 1, SaltSize);
            Buffer.BlockCopy(hash, 0, inArray, 1 + SaltSize, hash.Length);

            return Convert.ToBase64String(inArray);
        }

        public Result VerifyHashedPassword(string hashedPassword, string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (hashedPassword == null)
                return Result.Failure(new string[] { "Password is null." });

            Span<byte> numArray = Convert.FromBase64String(hashedPassword);
            if (numArray.Length < 1)
                return Result.Failure(new string[] { "Password is empty." });

            var salt = numArray.Slice(1, SaltSize).ToArray();
            var bytes = numArray[(1 + SaltSize)..].ToArray();

            var hash = HashPasswordWithSalt(password, salt);

            if (FixedTimeEquals(hash, bytes))
                return Result.Success();

            return Result.Failure(new string[] { "Password is incorrect." });
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            byte[] hash;

            if(HashAlgorithmName.Name == null)
            {
                throw new Exception("No HashAlgorithmName found.");
            }

            using (var hashAlgorithm = HashAlgorithm.Create(HashAlgorithmName.Name))
            {
                byte[] input = Encoding.UTF8.GetBytes(password);
                hashAlgorithm?.TransformBlock(salt, 0, salt.Length, salt, 0);
                hashAlgorithm?.TransformFinalBlock(input, 0, input.Length);

                if (hashAlgorithm?.Hash == null)
                {
                    throw new Exception("No Hash found.");
                }

                hash = hashAlgorithm.Hash;
            }

            return hash;
        }

        private static byte[] GenerateSalt(int byteLength)
        {
            var cryptoServiceProvider = new Random();
            var data = new byte[byteLength];

            cryptoServiceProvider.NextBytes(data);

            return data;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            int length = left.Length;
            int accum = 0;

            for (int index = 0; index < length; index++)
            {
                accum |= left[index] - right[index];
            }

            return accum == 0;
        }
    }
}
