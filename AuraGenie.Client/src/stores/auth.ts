import { defineStore } from "pinia";
import { onMounted, ref } from "vue";
import { tokenRequest, msalInstance } from "@/auth/msal/msalConfig";
import {
  type SilentRequest,
  InteractionType,
  type PopupRequest,
  type RedirectRequest,
  type AccountInfo,
} from "@azure/msal-browser";

export const useAuthStore = defineStore("auth", () => {
  const account = ref<AccountInfo>();



  function getUserGroups(): string[] {
    var groups = account?.value?.idTokenClaims?.groups;
    return groups ? (groups as string[]) : [];
  }

  async function getAccessToken(): Promise<string> {
    const promise = new Promise<string>((resolve, reject) => {
      const silentRequest: SilentRequest = {
        ...tokenRequest,
        account: msalInstance.getAllAccounts()[0],
      };
      const authPromise = msalInstance.acquireTokenSilent(silentRequest);
      authPromise
        .then((authResult) => {
          resolve(authResult.accessToken);
        })
        .catch((error) => {
          // If the error is due to the user being required to login again, redirect them to the login page
          // This happens when the refresh token has expired
          if (error.errorCode === "interaction_required") {
            const request = {
              redirectStartPage: window.location.href,
            } as RedirectRequest;
            return msalInstance.loginRedirect(request);
          }
          console.error(error);
          // Error if no internet: endpoints_resolution_error
        });
    });
    return promise;
  }

  async function isAuthenticated(
    interactionType?: InteractionType,
    loginRequest?: PopupRequest | RedirectRequest
  ): Promise<boolean> {
    // If your application uses redirects for interaction, handleRedirectPromise must be called and awaited on each page load before determining if a user is signed in or not
    const instance = msalInstance;
    return instance
      .handleRedirectPromise()
      .then(() => {
        const accounts = instance.getAllAccounts();
        if (accounts.length > 0) {
          account.value = accounts[0];
          return true;
        }
        account.value = undefined;

        // User is not signed in and attempting to access protected route. Sign them in.
        if (interactionType === InteractionType.Popup) {
          return instance
            .loginPopup(loginRequest)
            .then(() => {
              return true;
            })
            .catch(() => {
              return false;
            });
        } else if (interactionType === InteractionType.Redirect) {
          return instance
            .loginRedirect(loginRequest)
            .then((res) => {
              return true;
            })
            .catch(() => {
              return false;
            });
        }

        return false;
      })
      .catch((e) => {
        console.error("Error in isAuthenticated", e);
        return false;
      });
  }

  async function logOut() {
    await msalInstance.logoutRedirect();
  }

  const states = { account };

  const functions = {
    isAuthenticated,
    getUserGroups,
    getAccessToken,
    logOut,
  };

  return { ...states, ...functions };
});
