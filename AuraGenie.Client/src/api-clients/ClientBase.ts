import { useAuthStore } from "@/stores/auth";

export class ClientBase {
  private authStore: any;

  constructor() {
    this.authStore = useAuthStore();
  }

  public getBaseUrl(url: string, baseUrl: string | undefined): string {
    return import.meta.env.VITE_API_URL;
  }

  protected async transformOptions(options: RequestInit): Promise<RequestInit> {
    const promise = new Promise<RequestInit>(async (resolve, reject) => {
      try {
        if (await this.authStore.isAuthenticated()) {
          const tokenPromise = this.authStore.getAccessToken();
          tokenPromise.then((token: string) => {
            options.headers = {
              ...options.headers,
              Authorization: `Bearer ${token}`,
            };
            resolve(options);
          });
        } else {
          resolve(options);
        }
      } catch (error) {
        reject(error);
      }
    });
    return promise;
  }

  public transformResult(
    url: string,
    response: Response,
    processor: (response: Response) => any
  ): Promise<any> {
    return processor(response);
  }
}
