/**
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/samples/msal-browser-samples/vue3-sample-app/src/authConfig.ts
 */
import { LogLevel, PublicClientApplication } from "@azure/msal-browser";

/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 */
export const msalConfig = {
  auth: {
    // 'Application (client) ID' of app registration in Azure portal - this value is a GUID
    clientId: import.meta.env.VITE_AZURE_AD_CLIENTID,
    // Full directory URL, in the form of https://login.microsoftonline.com/<tenant-id>
    authority: import.meta.env.VITE_AZURE_AD_AUTHORITY,
    // Full redirect URL, in form of http://localhost:3000
    // Must be registered as a SPA redirectURI on your app registration
    redirectUri: import.meta.env.VITE_AZURE_AD_REDIRECT_URI,
  },
  cache: {
    cacheLocation: "sessionStorage", // This configures where your cache will be stored
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  },
  system: {
    loggerOptions: {
      loggerCallback: (
        level: LogLevel,
        message: string,
        containsPii: boolean
      ) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Info:
            console.info(message);
            return;
          case LogLevel.Verbose:
            console.debug(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
          default:
            return;
        }
      },
      logLevel: LogLevel.Warning,
    },
  },
};

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit:
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const loginRequest = {
  scopes: [
    "openid",
    "email",
    import.meta.env.VITE_AZURE_AD_SCOPE,
    "group.read.all",
    "user.read",
  ],
};

/**
 * Add here the scopes to request when obtaining an access token for MS Graph API. For more information, see:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/resources-and-scopes.md
 */
export const tokenRequest = {
  scopes: [
    "openid",
    "email",
    import.meta.env.VITE_AZURE_AD_SCOPE,
  ],
  forceRefresh: false, // Set this to "true" to skip a cached token and go to the server to get a new token
};

export const msalInstance = new PublicClientApplication(msalConfig);
await msalInstance.initialize();