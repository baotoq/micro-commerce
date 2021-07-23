import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { filter } from 'rxjs/operators';
import { authConfig } from './auth.config';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'MicroCommerce';
  userProfile = {};

  constructor(private oauthService: OAuthService, private http: HttpClient) {
    this.oauthService.configure(authConfig);
    this.oauthService.loadDiscoveryDocumentAndLogin();

    this.oauthService.events
      .pipe(filter((e) => e.type === 'token_received'))
      .subscribe(async (u) => this.userProfile = await this.oauthService.loadUserProfile());
  }

  get claims() {
    const claims = this.oauthService.getIdentityClaims() as any;
    if (!claims) return null;
    return claims;
  }

  get() {
    this.http.get('http://localhost:16000/i/api/localApi').subscribe();
  }

  logout() {
    this.oauthService.logOut();
  }

  refresh() {
    this.oauthService.refreshToken();
  }
}
