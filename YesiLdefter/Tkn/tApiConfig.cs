using System;
using System.IO;
using System.Text;
using Tkn_Registry;
using Tkn_Variable;
using Newtonsoft.Json.Linq;

namespace Tkn_UstadAPI
{
    /// <summary>
    /// API Configuration Helper
    /// NOTE(@Janberk): Centralized configuration management for API settings.
    /// Stores API base URL and JWT key in Windows Registry for runtime configuration.
    /// Also supports reading from appsettings.Production.json for production deployments.
    /// </summary>
    public static class tApiConfig
    {
        private const string REGISTRY_KEY_API_BASE_URL = "ApiBaseUrl";
        private const string REGISTRY_KEY_JWT_KEY = "JwtKey";
        private const string PRODUCTION_SETTINGS_FILE = "appsettings.Production.json";
        private const string DEFAULT_SETTINGS_FILE = "appsettings.json";
        
        // Default values (fallback if not in registry or environment)
        // NOTE: Production server is set as default for now until dev mode is configured
        // Env vars:
        //   USTAD_JWT_KEY      (must match API Jwt:Key)
        // JWT Key from Development settings: UstadSecretKeyForJWTTokenGeneration2026SecureKey32Chars
        private static readonly string DEFAULT_API_BASE_URL =
            Environment.GetEnvironmentVariable("USTAD_API_BASE_URL") ?? "http://143.198.228.153:5000";
        private static readonly string DEFAULT_JWT_KEY =
            Environment.GetEnvironmentVariable("USTAD_JWT_KEY") ?? "UstadSecretKeyForJWTTokenGeneration2026SecureKey32Chars";
        
        /// <summary>
        /// Load API base URL from settings file (appsettings.json or appsettings.Production.json)
        /// </summary>
        private static string LoadSettingsApiBaseUrl()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Try production settings first, then default settings
            string[] settingsFiles = { PRODUCTION_SETTINGS_FILE, DEFAULT_SETTINGS_FILE };
            
            foreach (string settingsFile in settingsFiles)
            {
                try
                {
                    string settingsPath = Path.Combine(baseDir, settingsFile);
                    
                    if (File.Exists(settingsPath))
                    {
                        string jsonContent = File.ReadAllText(settingsPath, Encoding.UTF8);
                        JObject settings = JObject.Parse(jsonContent);
                        
                        // Try to get Api.BaseUrl
                        string apiBaseUrl = settings["Api"]?["BaseUrl"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(apiBaseUrl))
                        {
                            System.Diagnostics.Debug.WriteLine($"Loaded API base URL from {settingsFile}: {apiBaseUrl}");
                            return apiBaseUrl.Trim();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading settings file {settingsFile}: {ex.Message}");
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Load JWT key from settings file (appsettings.json or appsettings.Production.json)
        /// </summary>
        private static string LoadSettingsJwtKey()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Try production settings first, then default settings
            string[] settingsFiles = { PRODUCTION_SETTINGS_FILE, DEFAULT_SETTINGS_FILE };
            
            foreach (string settingsFile in settingsFiles)
            {
                try
                {
                    string settingsPath = Path.Combine(baseDir, settingsFile);
                    
                    if (File.Exists(settingsPath))
                    {
                        string jsonContent = File.ReadAllText(settingsPath, Encoding.UTF8);
                        JObject settings = JObject.Parse(jsonContent);
                        
                        // Try to get Jwt.Key
                        string jwtKey = settings["Jwt"]?["Key"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(jwtKey))
                        {
                            System.Diagnostics.Debug.WriteLine($"Loaded JWT key from {settingsFile}");
                            return jwtKey.Trim();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading settings file {settingsFile}: {ex.Message}");
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Get API base URL from registry, production settings, or return default
        /// Priority: Registry > Production Settings > Environment Variable > Default
        /// </summary>
        public static string GetApiBaseUrl()
        {
            // 1. Try registry first (highest priority - user override)
            try
            {
                var reg = new tRegistry();
                var value = reg.getRegistryValue(REGISTRY_KEY_API_BASE_URL);
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return value.ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading API base URL from registry: {ex.Message}");
            }
            
            // 2. Try settings files (production or default)
            string settingsUrl = LoadSettingsApiBaseUrl();
            if (!string.IsNullOrWhiteSpace(settingsUrl))
            {
                return settingsUrl;
            }
            
            // 3. Fall back to environment variable or default
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
        /// Get JWT key from registry, settings files, or return default
        /// NOTE(@Janberk): JWT key is used for encrypting/decrypting connection strings.
        /// This is NOT a password but an encryption key - it must match the API's JWT key.
        /// Priority: Registry > Settings Files > Environment Variable > Default
        /// </summary>
        public static string GetJwtKey()
        {
            // 1. Try registry first (highest priority - user override)
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
            
            // 2. Try settings files
            string settingsJwtKey = LoadSettingsJwtKey();
            if (!string.IsNullOrWhiteSpace(settingsJwtKey))
            {
                return settingsJwtKey;
            }
            
            // 3. Fall back to environment variable or default
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
    }
}

