///////////////////////////////////////////////////////////////////////////////
// SAMPLE: Encryption and decryption using DPAPI functions.
//
// To run this sample, create a new Visual C# project using the Console
// Application template and replace the contents of the Class1.cs file
// with the code below.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//
// Copyright (C) 2003 Obviex(TM). All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

/// <summary>
/// Encrypts and decrypts data using DPAPI functions.
/// http://www.obviex.com/samples/dpapi.aspx
/// http://www.rsdn.ru/forum/pda/2975877.flat
/// </summary>
public class DPAPI
{
    // Wrapper for DPAPI CryptProtectData function.
    [DllImport("Coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CryptProtectData(ref DATA_BLOB pPlainText,
        string szDescription,
        IntPtr pEntropy,
        IntPtr pReserved,
        IntPtr pPrompt,
        int dwFlags,
        ref DATA_BLOB pCipherText);

    // Wrapper for DPAPI CryptUnprotectData function.
    [DllImport("Coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CryptUnprotectData(ref DATA_BLOB pCipherText,
        ref string pszDescription,
        IntPtr pEntropy,
        IntPtr pReserved,
        IntPtr pPrompt,
        int dwFlags,
        ref DATA_BLOB pPlainText);

    // BLOB structure used to pass data to DPAPI functions.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct DATA_BLOB
    {
        public int cbData;
        public IntPtr pbData;
    }

    // Prompt structure to be used for required parameters.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CRYPTPROTECT_PROMPTSTRUCT
    {
        public int cbSize;
        public int dwPromptFlags;
        public IntPtr hwndApp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string szPrompt;
    }

    // DPAPI key initialization flags.
    private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
    private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

    /// <summary>
    /// Initializes empty prompt structure.
    /// </summary>
    /// <param name="ps">
    /// Prompt parameter (which we do not actually need).
    /// </param>
    private static void InitPrompt(ref CRYPTPROTECT_PROMPTSTRUCT ps)
    {
        //ps.cbSize = 128;
        ps.dwPromptFlags = 0;
        ps.hwndApp = IntPtr.Zero;
        ps.szPrompt = null;
        ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
    }

    /// <summary>
    /// Initializes a BLOB structure from a byte array.
    /// </summary>
    /// <param name="data">
    /// Original data in a byte array format.
    /// </param>
    /// <param name="blob">
    /// Returned blob structure.
    /// </param>
    private static void InitBLOB(byte[] data, ref DATA_BLOB blob)
    {
        // Use empty array for null parameter.
        if (data == null)
            data = new byte[0];

        // Allocate memory for the BLOB data.
        blob.pbData = Marshal.AllocHGlobal(data.Length);

        // Make sure that memory allocation was successful.
        if (blob.pbData == IntPtr.Zero)
            throw new Exception(
                "Unable to allocate data buffer for BLOB structure.");

        // Specify number of bytes in the BLOB.
        blob.cbData = data.Length;

        // Copy data from original source to the BLOB structure.
        Marshal.Copy(data, 0, blob.pbData, data.Length);
    }

    // Flag indicating the type of key. DPAPI terminology refers to
    // key types as user store or machine store.
    public enum KeyType { UserKey = 1, MachineKey };

    // It is reasonable to set default key type to user key.
    private const KeyType DefaultKeyType = KeyType.UserKey;

    /// <summary>
    /// Calls DPAPI CryptProtectData function to encrypt a plaintext
    /// string value with a user-specific key. This function does not
    /// specify data description and additional entropy.
    /// </summary>
    /// <param name="plainText">
    /// Plaintext data to be encrypted.
    /// </param>
    /// <returns>
    /// Encrypted value in a base64-encoded format.
    /// </returns>
    public static string Encrypt(string plainText)
    {
        return Encrypt(DefaultKeyType, plainText, String.Empty);
    }

    /// <summary>
    /// Calls DPAPI CryptProtectData function to encrypt a plaintext
    /// string value. This function does not specify data description
    /// and additional entropy.
    /// </summary>
    /// <param name="keyType">
    /// Defines type of encryption key to use. When user key is
    /// specified, any application running under the same user account
    /// as the one making this call, will be able to decrypt data.
    /// Machine key will allow any application running on the same
    /// computer where data were encrypted to perform decryption.
    /// Note: If optional entropy is specifed, it will be required
    /// for decryption.
    /// </param>
    /// <param name="plainText">
    /// Plaintext data to be encrypted.
    /// </param>
    /// <returns>
    /// Encrypted value in a base64-encoded format.
    /// </returns>
    public static string Encrypt(KeyType keyType, string plainText)
    {
        return Encrypt(keyType, plainText, String.Empty);
    }

    /// <summary>
    /// Calls DPAPI CryptProtectData function to encrypt a plaintext
    /// string value.
    /// </summary>
    /// <param name="keyType">
    /// Defines type of encryption key to use. When user key is
    /// specified, any application running under the same user account
    /// as the one making this call, will be able to decrypt data.
    /// Machine key will allow any application running on the same
    /// computer where data were encrypted to perform decryption.
    /// Note: If optional entropy is specifed, it will be required
    /// for decryption.
    /// </param>
    /// <param name="plainText">
    /// Plaintext data to be encrypted.
    /// </param>
    /// <param name="description">
    /// Optional description of data to be encrypted. If this value is
    /// specified, it will be stored along with encrypted data and
    /// returned as a separate value during decryption.
    /// </param>
    /// <returns>
    /// Encrypted value in a base64-encoded format.
    /// </returns>
    public static string Encrypt(KeyType keyType, string plainText, string description) 
    {
        // Make sure that parameters are valid.
        if (plainText == null) 
            plainText = String.Empty;

        // Call encryption routine and convert returned bytes into
        // a base64-encoded value.
        //return Convert.ToBase64String(Encrypt(keyType, Encoding.UTF8.GetBytes(plainText), Encoding.UTF8.GetBytes(entropy), description));

        var buffer = Encrypt(keyType, Encoding.UTF8.GetBytes(plainText), description);
        var result = new StringBuilder();
        foreach (var b in buffer)
        {
            result.Append(string.Format("{0:X2}", b));
        }
        return result.ToString();

        //test
        //string descr;
        //var pwd = "0100000000000000000000000000000000000000000000000800000072006400700000000168000010000000100000001AB4D76B84A783AE005FF0459573F14B00000000048000001000000010000000ACC516896DED92B57E3F8DC1CC00B4E50E000000ED1CB74E1CF86F11E5489F92B62A14000000D9AADF1B262E4BC863AF962CBDD476DE7F59DA80";
        //var buf = new Crypto().ConvertHexStringToByte(pwd);
        //var test = Decrypt(buf, null, out descr);
    }

    public static string RdpEncrypt(string plainText)
    {
        if (plainText == null)
            plainText = String.Empty;

        var buffer = Encrypt(KeyType.UserKey, ConvertToRdp(Encoding.UTF8.GetBytes(plainText)), "rdp");
        
        var result = new StringBuilder();
        foreach (var b in buffer)
        {
            result.Append(string.Format("{0:X2}", b));
        }

        return result.ToString();
    }

    private static byte[] ConvertToRdp(byte[] buffer)
    {
        var result = new List<byte>();
        foreach (var b in buffer)
        {
            result.Add(b);
            result.Add(0);
        }
        result.Add(0);
        result.Add(0);
        return result.ToArray();
    }

    /// <summary>
    /// Calls DPAPI CryptProtectData function to encrypt an array of
    /// plaintext bytes.
    /// </summary>
    /// <param name="keyType">
    /// Defines type of encryption key to use. When user key is
    /// specified, any application running under the same user account
    /// as the one making this call, will be able to decrypt data.
    /// Machine key will allow any application running on the same
    /// computer where data were encrypted to perform decryption.
    /// Note: If optional entropy is specifed, it will be required
    /// for decryption.
    /// </param>
    /// <param name="plainTextBytes">
    /// Plaintext data to be encrypted.
    /// </param>
    /// <param name="description">
    /// Optional description of data to be encrypted. If this value is
    /// specified, it will be stored along with encrypted data and
    /// returned as a separate value during decryption.
    /// </param>
    /// <returns>
    /// Encrypted value.
    /// </returns>
    public static byte[] Encrypt(KeyType keyType, byte[] plainTextBytes, string description)
    {
        // Make sure that parameters are valid.
        if (plainTextBytes == null) 
            plainTextBytes = new byte[0];
        if (description == null) 
            description = String.Empty;

        // Create BLOBs to hold data.
        var plainTextBlob = new DATA_BLOB();
        var cipherTextBlob = new DATA_BLOB();

        // We only need prompt structure because it is a required
        // parameter.
        var prompt = new CRYPTPROTECT_PROMPTSTRUCT();
        InitPrompt(ref prompt);

        try
        {
            // Convert plaintext bytes into a BLOB structure.
            try
            {
                InitBLOB(plainTextBytes, ref plainTextBlob);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Cannot initialize plaintext BLOB.", ex);
            }

            // Disable any types of UI.
            int flags = CRYPTPROTECT_UI_FORBIDDEN;

            // When using machine-specific key, set up machine flag.
            if (keyType == KeyType.MachineKey)
                flags |= CRYPTPROTECT_LOCAL_MACHINE;

            // Call DPAPI to encrypt data.
            //var success = CryptProtectData(ref plainTextBlob, description, ref entropyBlob, IntPtr.Zero, ref prompt, flags, ref cipherTextBlob);
            var success = CryptProtectData(ref plainTextBlob, description, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, flags, ref cipherTextBlob);

            // Check the result.
            if (!success)
            {
                // If operation failed, retrieve last Win32 error.
                int errCode = Marshal.GetLastWin32Error();

                // Win32Exception will contain error message corresponding
                // to the Windows error code.
                throw new Exception(
                    "CryptProtectData failed.", new Win32Exception(errCode));
            }

            // Allocate memory to hold ciphertext.
            var cipherTextBytes = new byte[cipherTextBlob.cbData];

            // Copy ciphertext from the BLOB to a byte array.
            Marshal.Copy(cipherTextBlob.pbData, cipherTextBytes, 0, cipherTextBlob.cbData);

            // Return the result.
            return cipherTextBytes;
        }
        catch (Exception ex)
        {
            throw new Exception("DPAPI was unable to encrypt data.", ex);
        }
        // Free all memory allocated for BLOBs.
        finally
        {
            if (plainTextBlob.pbData != IntPtr.Zero)
                Marshal.FreeHGlobal(plainTextBlob.pbData);

            if (cipherTextBlob.pbData != IntPtr.Zero)
                Marshal.FreeHGlobal(cipherTextBlob.pbData);
        }
    }

    /// <summary>
    /// Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    /// This function does not use additional entropy and does not
    /// return data description.
    /// </summary>
    /// <param name="cipherText">
    /// Encrypted data formatted as a base64-encoded string.
    /// </param>
    /// <returns>
    /// Decrypted data returned as a UTF-8 string.
    /// </returns>
    /// <remarks>
    /// When decrypting data, it is not necessary to specify which
    /// type of encryption key to use: user-specific or
    /// machine-specific; DPAPI will figure it out by looking at
    /// the signature of encrypted data.
    /// </remarks>
    public static string Decrypt(string cipherText)
    {
        string description;
        return Decrypt(cipherText, out description);
    }

    /// <summary>
    /// Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    /// </summary>
    /// <param name="cipherText">
    /// Encrypted data formatted as a base64-encoded string.
    /// </param>
    /// <param name="description">
    /// Returned description of data specified during encryption.
    /// </param>
    /// <returns>
    /// Decrypted data returned as a UTF-8 string.
    /// </returns>
    /// <remarks>
    /// When decrypting data, it is not necessary to specify which
    /// type of encryption key to use: user-specific or
    /// machine-specific; DPAPI will figure it out by looking at
    /// the signature of encrypted data.
    /// </remarks>
    public static string Decrypt(string cipherText, out string description)
    {
        var decryptresult = Decrypt(Convert.FromBase64String(cipherText), out description);
        if (decryptresult == null)
            return null;

        return Encoding.UTF8.GetString(decryptresult, 0, decryptresult.Length);
    }

    /// <summary>
    /// Calls DPAPI CryptUnprotectData to decrypt ciphertext bytes.
    /// </summary>
    /// <param name="cipherTextBytes">
    /// Encrypted data.
    /// </param>
    /// <param name="description">
    /// Returned description of data specified during encryption.
    /// </param>
    /// <returns>
    /// Decrypted data bytes.
    /// </returns>
    /// <remarks>
    /// When decrypting data, it is not necessary to specify which
    /// type of encryption key to use: user-specific or
    /// machine-specific; DPAPI will figure it out by looking at
    /// the signature of encrypted data.
    /// </remarks>
    public static byte[] Decrypt(byte[] cipherTextBytes, out string description)
    {
        // Create BLOBs to hold data.
        var plainTextBlob = new DATA_BLOB();
        var cipherTextBlob = new DATA_BLOB();

        // We only need prompt structure because it is a required
        // parameter.
        var prompt = new CRYPTPROTECT_PROMPTSTRUCT();
        InitPrompt(ref prompt);

        // Initialize description string.
        description = String.Empty;

        try
        {
            // Convert ciphertext bytes into a BLOB structure.
            try
            {
                InitBLOB(cipherTextBytes, ref cipherTextBlob);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Cannot initialize ciphertext BLOB.", ex);
            }

            // Disable any types of UI. CryptUnprotectData does not
            // mention CRYPTPROTECT_LOCAL_MACHINE flag in the list of
            // supported flags so we will not set it up.
            //var flags = CRYPTPROTECT_UI_FORBIDDEN;

            // Call DPAPI to decrypt data.
            //bool success = CryptUnprotectData(ref cipherTextBlob,
            //                                  ref description,
            //                                  ref entropyBlob,
            //                                      IntPtr.Zero,
            //                                  ref prompt,
            //                                      flags,
            //                                  ref plainTextBlob);

            bool success = CryptUnprotectData(ref cipherTextBlob,
                ref description,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                CRYPTPROTECT_UI_FORBIDDEN,
                ref plainTextBlob);

            // Check the result.
            if (!success)
            {
                // If operation failed, retrieve last Win32 error.
                int errCode = Marshal.GetLastWin32Error();

                // Win32Exception will contain error message corresponding
                // to the Windows error code.
                throw new Exception(
                    "CryptUnprotectData failed.", new Win32Exception(errCode));
            }

            // Allocate memory to hold plaintext.
            var plainTextBytes = new byte[plainTextBlob.cbData];

            // Copy ciphertext from the BLOB to a byte array.
            Marshal.Copy(plainTextBlob.pbData,
                         plainTextBytes,
                         0,
                         plainTextBlob.cbData);

            // Return the result.
            return plainTextBytes;
        }
        catch (Exception ex)
        {
            throw new Exception("DPAPI was unable to decrypt data.", ex);
        }
        // Free all memory allocated for BLOBs.
        finally
        {
            if (plainTextBlob.pbData != IntPtr.Zero)
                Marshal.FreeHGlobal(plainTextBlob.pbData);

            if (cipherTextBlob.pbData != IntPtr.Zero)
                Marshal.FreeHGlobal(cipherTextBlob.pbData);
        }
    }
}
