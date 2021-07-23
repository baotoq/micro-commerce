import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

import { OAuthModule } from 'angular-oauth2-oidc';

import { AppRoutingModule } from '@app/app-routing.module';

import { environment } from '@environments';

import { SharedModule } from '@shared/shared.module';

import { AppComponent } from '@app/app.component';
import { HomeComponent } from '@app/home/home.component';

@NgModule({
  declarations: [AppComponent, HomeComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    OAuthModule.forRoot({
      resourceServer: {
        allowedUrls: [environment.uri.gateway],
        sendAccessToken: true,
      },
    }),
    SharedModule.forRoot(),
    AppRoutingModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
