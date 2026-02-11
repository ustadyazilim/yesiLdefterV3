using System;
using Tkn_Registry;
using Tkn_Variable;

namespace Tkn_UstadAPI
{
    /// <summary>
    /// API Configuration Helper
    /// NOTE(@Janberk): Centralized configuration management for API settings.
    /// Stores API base URL and JWT key in Windows Registry for runtime configuration.
    /// </summary>
    public static class tApiConfig
    {
        private const string REGISTRY_KEY_API_BASE_URL = "ApiBaseUrl";
        private const string REGISTRY_KEY_JWT_KEY = "JwtKey";
        // Default values (fallback if not in registry or environment)
        // NOTE: Prefer environment variables; these are non-secret placeholders to avoid startup crashes.
        // Env vars:
        //   USTAD_API_BASE_URL (e.g., http://localhost:5001)
        //   USTAD_JWT_KEY      (must match API Jwt:Key)
        private static readonly string DEFAULT_API_BASE_URL =
            Environment.GetEnvironmentVariable("USTAD_API_BASE_URL") ?? "http://143.198.228.153:5000/";
        private static readonly string DEFAULT_JWT_KEY =
            Environment.GetEnvironmentVariable("USTAD_JWT_KEY") ?? string.Empty;
        /// <summary>
        /// Get API base URL from registry or return default
        /// </summary>
        public static string GetApiBaseUrl()
        {
            try
            {
                var reg = new tRegistry();
                //var value = reg.getRegistryValue(REGISTRY_KEY_API_BASE_URL);
                var value = DEFAULT_API_BASE_URL;
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return value.ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading API base URL from registry: {ex.Message}");
            }
            
            return DEFAULT_API_BASE_URL;
        }
        /// <summary>
        /// Set API base URL in registry
        /// </summary>
        public static void SetApiBaseUrl(string apiBaseUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiBaseUrl))
                {
                    throw new ArgumentException("API base URL cannot be empty", nameof(apiBaseUrl));
                }
                var reg = new tRegistry();
                reg.SetUstadRegistry(REGISTRY_KEY_API_BASE_URL, apiBaseUrl.Trim());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing API base URL to registry: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Get JWT key from registry or return default
        /// NOTE(@Janberk): JWT key is used for encrypting/decrypting connection strings.
        /// This is NOT a password but an encryption key - it must match the API's JWT key.
        /// </summary>
        public static string GetJwtKey()
        {
            try
            {
                var reg = new tRegistry();
                var value = reg.getRegistryValue(REGISTRY_KEY_JWT_KEY);
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return value.ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading JWT key from registry: {ex.Message}");
            }
            
            return DEFAULT_JWT_KEY;
        }
        /// <summary>
        /// Set JWT key in registry
        /// NOTE(@Janberk): JWT key must match the API's JWT key for encryption/decryption to work.
        /// Registry path: HKEY_CURRENT_USER\Software\Üstad\YesiLdefter\JwtKey
        /// </summary>
        public static void SetJwtKey(string jwtKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jwtKey))
                {
                    throw new ArgumentException("JWT key cannot be empty", nameof(jwtKey));
                }
                if (jwtKey.Length < 32)
                {
                    throw new ArgumentException("JWT key must be at least 32 characters long", nameof(jwtKey));
                }
                var reg = new tRegistry();
                reg.SetUstadRegistry(REGISTRY_KEY_JWT_KEY, jwtKey.Trim());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing JWT key to registry: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize default API configuration if not already set
        /// NOTE(@Janberk): Call this during application startup to ensure defaults are set
        /// </summary>
        public static void InitializeDefaults()
        {
            try
            {
                var reg = new tRegistry();
                var apiUrl = reg.getRegistryValue(REGISTRY_KEY_API_BASE_URL);
                if (apiUrl == null || string.IsNullOrWhiteSpace(apiUrl.ToString()))
                {
                    SetApiBaseUrl(DEFAULT_API_BASE_URL);
                }
                var jwtKey = reg.getRegistryValue(REGISTRY_KEY_JWT_KEY);
                if (jwtKey == null || string.IsNullOrWhiteSpace(jwtKey.ToString()))
                {
                    SetJwtKey(DEFAULT_JWT_KEY);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing API configuration defaults: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset API base URL to default (localhost:5001)
        /// Use this to override any production URL stored in registry
        /// </summary>
        public static void ResetApiBaseUrlToDefault()
        {
            SetApiBaseUrl(DEFAULT_API_BASE_URL);
        }
    }
}

