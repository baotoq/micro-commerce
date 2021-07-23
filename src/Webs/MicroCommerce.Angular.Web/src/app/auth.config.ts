import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '@environments';

export const authConfig: AuthConfig = {
  issuer: environment.uri.authority,
  redirectUri: window.location.origin + '/index.html',
  clientId: 'angular-web',
  responseType: 'code',
  scope: 'openid profile email offline_access IdentityServerApi catalog-api',
  showDebugInformation: true,
};
