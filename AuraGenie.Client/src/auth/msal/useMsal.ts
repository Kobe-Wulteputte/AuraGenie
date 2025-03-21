// https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/samples/msal-browser-samples/vue3-sample-app/src/composition-api/useMsal.ts
import { type AccountInfo, InteractionStatus, PublicClientApplication } from '@azure/msal-browser';
import { getCurrentInstance, type Ref, toRefs } from 'vue';

export type MsalContext = {
    instance: PublicClientApplication;
    accounts: Ref<AccountInfo[]>;
    inProgress: Ref<InteractionStatus>;
};

export function useMsal(): MsalContext {
    const internalInstance = getCurrentInstance();
    if (!internalInstance) {
        throw 'useMsal() cannot be called outside the setup() function of a component';
    }
    const { instance, accounts, inProgress } = toRefs(
        internalInstance.appContext.config.globalProperties.$msal,
    );

    if (!instance || !accounts || !inProgress) {
        throw 'Please install the msalPlugin';
    }

    if (inProgress.value === InteractionStatus.Startup) {
        instance.value.handleRedirectPromise().catch(() => {
            // Errors should be handled by listening to the LOGIN_FAILURE event
            return;
        });
    }

    return {
        instance: instance.value,
        accounts,
        inProgress,
    };
}
