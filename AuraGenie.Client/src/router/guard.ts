import type { RouteLocationNormalized, Router } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import type { User } from "oidc-client-ts";
import { useMessageStore } from "@/stores/message";
import { InteractionType, type RedirectRequest } from "@azure/msal-browser";

export function registerGuard(router: Router) {
  router.beforeEach(
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    async (to: RouteLocationNormalized, from: RouteLocationNormalized) => {
      const authStore = useAuthStore();
      const messageStore = useMessageStore();
      if (to.name !== "signin-oidc" && !messageStore.initialized) {
        messageStore.initialize();
      }
      if (to.meta?.requiresAuth) {
        const request = {
          redirectStartPage: to.fullPath,
        } as RedirectRequest;
        const shouldProceed = await authStore.isAuthenticated(
          InteractionType.Redirect,
          request
        );
        return shouldProceed;
      }

      return true;
    }
  );
}
