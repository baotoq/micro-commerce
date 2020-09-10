import { UserManager } from "oidc-client";

const webBaseUri = window.location.origin;

const oidcSettings = {
  authority: process.env.REACT_APP_IDENTITY_URI,
  client_id: "react-web",
  redirect_uri: `${webBaseUri}/authentication/login-callback`,
  post_logout_redirect_uri: `${webBaseUri}/authentication/logout-callback`,
  response_type: "code",
  scope: "catalog-api IdentityServerApi openid profile roles",
  automaticSilentRenew: true,
  includeIdTokenInSilentRenew: true,
};

class AuthService {
  userManager;

  constructor() {
    this.userManager = new UserManager(oidcSettings);
  }

  getUserAsync() {
    return this.userManager.getUser();
  }

  loginAsync() {
    return this.userManager.signinRedirect();
  }

  completeLoginAsync(url) {
    return this.userManager.signinCallback(url);
  }

  renewTokenAsync() {
    return this.userManager.signinSilent();
  }

  logoutAsync() {
    return this.userManager.signoutRedirect();
  }

  async completeLogoutAsync(url) {
    return await this.userManager.signoutCallback(url);
  }
}

export default new AuthService();
