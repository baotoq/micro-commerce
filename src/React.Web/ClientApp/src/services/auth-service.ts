import { User, UserManager } from "oidc-client";

const oidcSettings = {
  authority: process.env.REACT_APP_BACKEND_URL,
  client_id: "react-web",
  redirect_uri: `${process.env.REACT_APP_URL}/authentication/login-callback`,
  post_logout_redirect_uri: `${process.env.REACT_APP_URL}/authentication/logout-callback`,
  response_type: "code",
  scope: "bshop-api openid profile",
  automaticSilentRenew: true,
  includeIdTokenInSilentRenew: true,
};

export class AuthService {
  public userManager: UserManager;

  constructor() {
    this.userManager = new UserManager(oidcSettings);
  }

  public getUserAsync(): Promise<User | null> {
    return this.userManager.getUser();
  }

  public loginAsync(): Promise<void> {
    return this.userManager.signinRedirect();
  }

  public completeLoginAsync(url: string): Promise<User> {
    return this.userManager.signinCallback(url);
  }

  public renewTokenAsync(): Promise<User> {
    return this.userManager.signinSilent();
  }

  public logoutAsync(): Promise<void> {
    return this.userManager.signoutRedirect();
  }

  public async completeLogoutAsync(url: string): Promise<void> {
    await this.userManager.signoutCallback(url);
  }
}
